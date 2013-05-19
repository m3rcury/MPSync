Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports System.ComponentModel
Imports System.Threading
Imports System.Data.SQLite

Public Class MPSync_process

    Private mps As New MPSync_settings
    Public Shared _db_client, _db_server, _thumbs_client, _thumbs_server, _databases(), _thumbs(), _watched_dbs(), _objects(), dbname(), session, sync_type As String
    Public Shared _db_sync, _thumbs_sync, _db_direction, _db_sync_method, _thumbs_direction, _thumbs_sync_method As Integer
    Public Shared _db_pause, _thumbs_pause, debug, check_watched As Boolean
    Public Shared dbinfo() As IO.FileInfo

    Dim checked_databases, checked_thumbs As Boolean
    Dim bw_threads As Integer

    Private Structure fieldnames
        Dim names As Array
    End Structure

    Public Shared Property p_Debug As Boolean
        Get
            Dim debug As Boolean
            Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))
                debug = XMLreader.GetValueAsString("Plugin", "debug", False)
            End Using
            Return debug
        End Get
        Set(value As Boolean)
            debug = value
        End Set
    End Property

    Public Shared Sub wait(ByVal seconds As Long, Optional ByVal verbose As Boolean = True, Optional ByVal thread As String = Nothing)

        If thread <> Nothing Then thread = thread & " "
        If verbose Then Log.Info("MPSync: " & thread & "process sleeping " & seconds.ToString & " seconds.....")

        System.Threading.Thread.Sleep(seconds * 1000)

    End Sub

    Public Shared Sub CheckPlayerplaying(ByVal thread As String, ByVal seconds As Long)
        If (thread = "db" And _db_pause) Or (thread = "thumbs" And _thumbs_pause) Then
            If MediaPortal.Player.g_Player.Playing Then Log.Info("MPSync: paimports " & thread & " thread as player is playing.")
            Do While MediaPortal.Player.g_Player.Playing
                MPSync_process.wait(seconds, False)
            Loop
        End If
    End Sub

    Public Shared Sub logStats(ByVal message As String, ByVal msgtype As String)

        Select Case msgtype
            Case "INFO"
                Log.Info(message)
            Case "ERROR"
                Log.Error(message)
        End Select

        Dim file As String = Config.GetFile(Config.Dir.Log, "mpsync.log")
        Dim fhandle As System.IO.FileStream

        If msgtype = "LOG" Then msgtype = "INFO"

        Dim info As Byte() = New System.Text.UTF8Encoding(True).GetBytes(DateTime.Now & " - [" & msgtype & "] " & message & vbCrLf)

        Try
            fhandle = IO.File.Open(file, IO.FileMode.Append, IO.FileAccess.Write, IO.FileShare.ReadWrite)
            fhandle.Write(info, 0, info.Length)
            fhandle.Close()
        Catch ex As Exception
        End Try

    End Sub

    Public Shared Sub MPSync_Launcher()

        ' create log file
        Dim file As String = Config.GetFile(Config.Dir.Log, "mpsync.log")

        If IO.File.Exists(Config.GetFile(Config.Dir.Log, "mpsync.bak")) Then IO.File.Delete(Config.GetFile(Config.Dir.Log, "mpsync.bak"))
        If IO.File.Exists(file) Then FileIO.FileSystem.RenameFile(file, "mpsync.bak")
        Dim fhandle As System.IO.FileStream = IO.File.Open(file, IO.FileMode.OpenOrCreate)
        fhandle.Close()

        Dim mps As New MPSync_process
        mps.MPSyncProcess()

    End Sub

    Public Sub MPSyncProcess()

        Dim file As String = Config.GetFile(Config.Dir.Config, "MPSync.xml")

        If Not FileIO.FileSystem.FileExists(file) Then Return

        Dim i_method As Array = {"Propagate both additions and deletions", "Propagate additions only", "Propagate deletions only"}
        Dim db_sync_value, thumbs_sync_value, databases, thumbs, watched_dbs, objects, version As String

        ' get configuratin from XML file
        Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(file)

            version = XMLreader.GetValueAsString("Plugin", "version", "0")
            checked_databases = XMLreader.GetValueAsString("Plugin", "databases", True)
            checked_thumbs = XMLreader.GetValueAsString("Plugin", "thumbs", True)
            p_Debug = XMLreader.GetValueAsString("Plugin", "debug", False)
            session = XMLreader.GetValueAsString("Plugin", "session ID", Nothing)
            sync_type = XMLreader.GetValueAsString("Plugin", "sync type", "Triggers")
            check_watched = XMLreader.GetValueAsString("DB Settings", "watched", False)

            _db_client = XMLreader.GetValueAsString("DB Path", "client", Nothing)
            _db_server = XMLreader.GetValueAsString("DB Path", "server", Nothing)
            _db_direction = XMLreader.GetValueAsInt("DB Path", "direction", 0)
            _db_sync_method = XMLreader.GetValueAsInt("DB Path", "method", 0)

            _db_sync = XMLreader.GetValueAsInt("DB Settings", "sync periodicity", 15)
            db_sync_value = XMLreader.GetValueAsString("DB Settings", "sync periodicity value", "minutes")
            _db_pause = XMLreader.GetValueAsString("DB Settings", "pause while playing", False)
            databases = XMLreader.GetValueAsString("DB Settings", "databases", Nothing)
            watched_dbs = XMLreader.GetValueAsString("DB Settings", "watched databases", Nothing)
            objects = XMLreader.GetValueAsString("DB Settings", "objects", Nothing)

            _thumbs_client = XMLreader.GetValueAsString("Thumbs Path", "client", Nothing)
            _thumbs_server = XMLreader.GetValueAsString("Thumbs Path", "server", Nothing)
            _thumbs_direction = XMLreader.GetValueAsInt("Thumbs Path", "direction", 0)
            _thumbs_sync_method = XMLreader.GetValueAsInt("Thumbs Path", "method", 0)

            _thumbs_sync = XMLreader.GetValueAsInt("Thumbs Settings", "sync periodicity", 15)
            thumbs_sync_value = XMLreader.GetValueAsString("Thumbs Settings", "sync periodicity value", "minutes")
            _thumbs_pause = XMLreader.GetValueAsString("Thumbs Settings", "pause while playing", False)
            thumbs = XMLreader.GetValueAsString("Thumbs Settings", "thumbs", Nothing)

        End Using

        If p_Debug Then
            logStats("MPSync: process plugin version " & version & " initialisation with DEBUG.", "INFO")
        Else
            logStats("MPSync: process plugin version " & version & " initialisation.", "INFO")
        End If

        If session = Nothing Then
            session = System.Guid.NewGuid.ToString()
            Using XMLwriter As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))
                XMLwriter.SetValue("Plugin", "session ID", session)
            End Using
        End If

        ' write settings to log file
        If checked_databases Then
            Select Case _db_direction
                Case 0
                    logStats("MPSync: DB - " & _db_client & " <-> " & _db_server, "INFO")
                Case 1
                    logStats("MPSync: DB - " & _db_client & " --> " & _db_server, "INFO")
                Case 2
                    logStats("MPSync: DB - " & _db_client & " <-- " & _db_server, "INFO")
            End Select
            logStats("MPSync: DB synchronization method - " & i_method(_db_sync_method), "INFO")
            If databases = Nothing Then
                logStats("MPSync: DBs selected - ALL", "INFO")
            Else
                logStats("MPSync: DBs selected - " & databases, "INFO")
            End If
            If check_watched Then
                If watched_dbs = Nothing Then
                    logStats("MPSync: watched/resume selected for ALL", "INFO")
                Else
                    logStats("MPSync: watched/resume selected for " & watched_dbs, "INFO")
                End If
            Else
                logStats("MPSync: watched/resume not selected", "INFO")
            End If
            If objects = Nothing Then
                logStats("MPSync: Objects selected - ALL", "INFO")
            Else
                logStats("MPSync: Objects selected - " & objects, "INFO")
            End If
        End If

        If checked_thumbs Then
            Select Case _thumbs_direction
                Case 0
                    logStats("MPSync: THUMBS - " & _thumbs_client & " <-> " & _thumbs_server, "INFO")
                Case 1
                    logStats("MPSync: THUMBS - " & _thumbs_client & " --> " & _thumbs_server, "INFO")
                Case 2
                    logStats("MPSync: THUMBS - " & _thumbs_client & " <-- " & _thumbs_server, "INFO")
            End Select
            logStats("MPSync: THUMBS synchronization method - " & i_method(_thumbs_sync_method), "INFO")
            If thumbs = Nothing Then
                logStats("MPSync: THUMBS selected - ALL", "INFO")
            Else
                logStats("MPSync: THUMBS selected - " & thumbs, "INFO")
            End If
        End If

        If _db_client <> Nothing And _db_server <> Nothing And checked_databases Then

            ' get list of databases to synchronise
            If databases <> Nothing Then
                _databases = Split(databases, "|")
            Else
                ReDim _databases(0)
                _databases(0) = "ALL"
            End If

            ' get list of watched databases to synchronise
            If watched_dbs <> Nothing Then
                _watched_dbs = Split(watched_dbs, "|")
            Else
                _watched_dbs = mps.getDatabase
            End If

            ' get list of objects to copy
            If objects <> Nothing Then
                _objects = Split(objects, "|")
            Else
                ReDim _objects(0)
                _objects(0) = "ALL"
            End If

            ' calculate actual sync periodicity in seconds
            If db_sync_value = "minutes" Then _db_sync = _db_sync * 60

            ' check that both paths end with a "\"
            If InStrRev(_db_client, "\") <> Len(_db_client) Then _db_client = Trim(_db_client) & "\"
            If InStrRev(_db_server, "\") <> Len(_db_server) Then _db_server = Trim(_db_server) & "\"

            If Not IO.Directory.Exists(_db_server) Then IO.Directory.CreateDirectory(_db_server)

            ' get last write time for all db3s
            If _db_direction = 1 Then
                getDBInfo(_db_client, _db_server)
            ElseIf _db_direction = 2 Then
                getDBInfo(_db_server, _db_client)
            End If

            ' create the required work files and triggers to handle watched status & synchronization
            If _db_direction = 1 Then
                checkTriggers("client>server")
            ElseIf _db_direction = 2 Then
                checkTriggers("server>client")
            End If

        End If

        If _thumbs_client <> Nothing And _thumbs_server <> Nothing And checked_thumbs Then

            ' get list of thumbs to synchronise
            If thumbs <> Nothing Then
                _thumbs = Split(thumbs, "|")
            Else
                ReDim _thumbs(0)
                _thumbs(0) = "ALL"
            End If

            ' calculate actual sync periodicity in seconds
            If thumbs_sync_value = "minutes" Then
                _thumbs_sync = _thumbs_sync * 60
            ElseIf thumbs_sync_value = "hours" Then
                _thumbs_sync = _thumbs_sync * 3600
            End If

            ' check that both paths end with a "\"
            If InStrRev(_thumbs_client, "\") <> Len(_thumbs_client) Then _thumbs_client = Trim(_thumbs_client) & "\"
            If InStrRev(_thumbs_server, "\") <> Len(_thumbs_server) Then _thumbs_server = Trim(_thumbs_server) & "\"

        End If

        If Not MPSync_settings.syncnow Then

            If p_Debug Then logStats("MPSync: Immediate Synchronization started", "DEBUG")

            Dim autoEvent As New AutoResetEvent(False)

            ThreadPool.QueueUserWorkItem(AddressOf WorkMethod, autoEvent)

            autoEvent.WaitOne()

        Else
            WorkMethod(Nothing)
        End If

    End Sub

    Public Shared Function TableExist(ByVal path As String, ByVal database As String, ByVal table As String, Optional ByVal dropifempty As Boolean = False) As Boolean

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim exist As Boolean = False

        SQLconnect.ConnectionString = "Data Source=" & path & database

        If Not dropifempty Then SQLconnect.ConnectionString = SQLconnect.ConnectionString & ";Read Only=True;"

        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='" & table & "'"
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()
            exist = True
        End While

        SQLreader.Close()

        If exist And dropifempty Then
            SQLcommand.CommandText = "SELECT COUNT(*) FROM " & table
            SQLreader = SQLcommand.ExecuteReader()
            SQLreader.Read()

            If Int(SQLreader(0)) = 0 Then
                SQLreader.Close()
                SQLcommand.CommandText = "DROP TABLE " & table
                SQLcommand.ExecuteNonQuery()
                exist = False
            End If
        End If

        SQLconnect.Close()

        Return exist

    End Function

    Private Function FieldExist(ByVal path As String, ByVal database As String, ByVal table As String, ByVal field As String) As Boolean

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim columns() As String = Nothing
        Dim x As Integer = 0

        SQLconnect.ConnectionString = "Data Source=" & path & database & ";Read Only=True;"
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA table_info (" & table & ")"
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()
            ReDim Preserve columns(x)
            columns(x) = LCase(SQLreader(1))
            x += 1
        End While

        SQLconnect.Close()

        If x = 0 Then ReDim Preserve columns(0)

        Return columns.Contains(LCase(field))

    End Function

    Private Function FieldList(ByVal path As String, ByVal database As String, ByVal table As String, ByVal fields As Array, Optional ByVal prefix As String = Nothing) As String

        Dim f_list As String = Nothing

        For x As Integer = 0 To UBound(fields)

            If FieldExist(path, database, table, fields(x)) Then
                f_list &= prefix & fields(x) & ","
            End If

        Next

        If f_list <> Nothing Then f_list = Left(f_list, Len(f_list) - 1)

        Return f_list

    End Function

    Private Sub getDBInfo(ByVal source As String, ByVal target As String)

        Dim x As Integer = 0
        Dim check As String = Nothing
        Dim bw_checkDB() As BackgroundWorker = Nothing

        For Each database As String In IO.Directory.GetFiles(source, "*.db3")

            If _databases.Contains(IO.Path.GetFileName(database)) Or _databases.Contains("ALL") Then

                ReDim Preserve dbname(x)
                ReDim Preserve dbinfo(x)
                dbname(x) = IO.Path.GetFileName(database)
                dbinfo(x) = My.Computer.FileSystem.GetFileInfo(database)

                ReDim Preserve bw_checkDB(x)
                bw_checkDB(x) = New BackgroundWorker
                bw_checkDB(x).WorkerSupportsCancellation = True
                AddHandler bw_checkDB(x).DoWork, AddressOf bw_checkDB_worker

                If Not bw_checkDB(x).IsBusy Then bw_checkDB(x).RunWorkerAsync(target & "|" & dbname(x) & "|" & database)
                x += 1
                bw_threads += 1

            End If
        Next

        Do While bw_threads > 0
            wait(10, False)
        Loop

    End Sub

    Private Sub bw_checkDB_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim check As String = "ok"
        Dim parm() As String = Split(e.Argument, "|")

        If IO.File.Exists(parm(0) & parm(1)) Then
            Dim SQLconnect As New SQLiteConnection()
            Dim SQLcommand As SQLiteCommand
            Dim SQLreader As SQLiteDataReader = Nothing
            SQLconnect.ConnectionString = "Data Source=" & parm(0) & parm(1)
            SQLconnect.Open()
            SQLcommand = SQLconnect.CreateCommand
            SQLcommand.CommandText = "PRAGMA integrity_check;"

            Try
                SQLreader = SQLcommand.ExecuteReader()
                check = SQLreader(0)
            Catch ex As Exception
                check = "error"
            End Try

            SQLconnect.Close()

            If check <> "ok" Then IO.File.Delete(parm(0) & parm(1))

        End If

        If Not IO.File.Exists(parm(0) & parm(1)) Then
            logStats("MPSync: Copying database " & parm(2) & " from source", "LOG")
            IO.File.Copy(parm(2), parm(0) & parm(1), True)
            Drop_Triggers(parm(0), parm(1), "mpsync_update|mpsync_watch")
            check = "copied file from source"
        End If

        logStats("MPSync: Checking integrity of database " & parm(1) & " - Status: " & check, "LOG")

        bw_threads -= 1

    End Sub

    Private Sub checkTriggers(ByVal mode As String)

        Dim x As Integer
        Dim bw_checkTriggers() As BackgroundWorker = Nothing

        For x = 0 To UBound(_watched_dbs)

            ReDim Preserve bw_checkTriggers(x)
            bw_checkTriggers(x) = New BackgroundWorker
            bw_checkTriggers(x).WorkerSupportsCancellation = True
            AddHandler bw_checkTriggers(x).DoWork, AddressOf bw_watchtriggers_worker

            If check_watched Then
                If Not bw_checkTriggers(x).IsBusy Then bw_checkTriggers(x).RunWorkerAsync("ADD|" & _watched_dbs(x))
            Else
                If Not bw_checkTriggers(x).IsBusy Then bw_checkTriggers(x).RunWorkerAsync("DROP|" & _watched_dbs(x))
            End If

            bw_threads += 1

        Next

        If sync_type = "Triggers" Then
            ' create remaining triggers to handle synchronization

            If mode = "client>server" Then
                For Each database As String In IO.Directory.GetFiles(_db_client, "*.db3")
                    If _databases.Contains(IO.Path.GetFileName(database)) Or _databases.Contains("ALL") Then

                        ReDim Preserve bw_checkTriggers(x)
                        bw_checkTriggers(x) = New BackgroundWorker
                        bw_checkTriggers(x).WorkerSupportsCancellation = True
                        AddHandler bw_checkTriggers(x).DoWork, AddressOf bw_worktriggers_worker

                        If Not bw_checkTriggers(x).IsBusy Then bw_checkTriggers(x).RunWorkerAsync("ADD|" & database)
                        x += 1
                        bw_threads += 1

                    End If
                Next
            End If

            For Each database As String In IO.Directory.GetFiles(_db_server, "*.db3")
                If _databases.Contains(IO.Path.GetFileName(database)) Or _databases.Contains("ALL") Then

                    ReDim Preserve bw_checkTriggers(x)
                    bw_checkTriggers(x) = New BackgroundWorker
                    bw_checkTriggers(x).WorkerSupportsCancellation = True
                    AddHandler bw_checkTriggers(x).DoWork, AddressOf bw_worktriggers_worker

                    If Not bw_checkTriggers(x).IsBusy Then bw_checkTriggers(x).RunWorkerAsync("ADD|" & database)
                    x += 1
                    bw_threads += 1

                End If
            Next
        End If

        ' remove triggers from local, if any
        If sync_type = "Timestamp" Or mode = "server>client" Then
            For Each database As String In IO.Directory.GetFiles(_db_client, "*.db3")

                ReDim Preserve bw_checkTriggers(x)
                bw_checkTriggers(x) = New BackgroundWorker
                bw_checkTriggers(x).WorkerSupportsCancellation = True
                AddHandler bw_checkTriggers(x).DoWork, AddressOf bw_worktriggers_worker

                If Not bw_checkTriggers(x).IsBusy Then bw_checkTriggers(x).RunWorkerAsync("DROP|" & database)
                x += 1
                bw_threads += 1

            Next
        End If

        Do While bw_threads > 0
            wait(10, False)
        Loop

    End Sub

    Private Sub bw_watchtriggers_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm() As String = Split(e.Argument, "|")

        If parm(0) = "ADD" Then
            Create_Watch_Tables(_db_client, parm(1))
            Create_Watch_Tables(_db_server, parm(1))
            Drop_Triggers(_db_client, parm(1), "mpsync_update|mpsync_watch")
            If _db_direction <> 1 Then
                Create_Watch_Triggers(_db_client, parm(1))
            End If
        ElseIf parm(0) = "DROP" Then
            Drop_Watch_Tables(_db_client, parm(1))
            Drop_Triggers(_db_client, parm(1), "mpsync_update|mpsync_watch")
        End If

        bw_threads -= 1

    End Sub

    Private Sub bw_worktriggers_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm() As String = Split(e.Argument, "|")

        If parm(0) = "DROP" Then Drop_Triggers(IO.Path.GetDirectoryName(parm(1)) & "\", IO.Path.GetFileName(parm(1)), "mpsync_work")
        If parm(0) = "ADD" Then Create_Sync_Triggers(parm(1))

        bw_threads -= 1

    End Sub

    Private Sub Create_Watch_Tables(ByVal path As String, ByVal database As String)

        If Not IO.File.Exists(path & database) Then Exit Sub

        logStats("MPSync: Creating/altering work table mpsync in database " & path & database, "LOG")

        Dim SQL As String = Nothing

        If TableExist(path, database, "mpsync", True) = False Then

            Select Case database

                Case mps.i_watched(0).database
                    SQL = "CREATE TABLE IF NOT EXISTS mpsync " & _
                          "(mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, mps_session TEXT, id INTEGER, user TEXT, " & _
                          "user_rating TEXT, watched INTEGER, resume_part INTEGER, resume_time INTEGER, resume_data TEXT, alternatecovers TEXT, coverfullpath TEXT, " & _
                          "coverthumbfullpath TEXT)"
                Case mps.i_watched(1).database
                    SQL = "CREATE TABLE IF NOT EXISTS mpsync (mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, mps_session TEXT, " & _
                          "idTrack INTEGER, iResumeAt INTEGER, dateLastPlayed TEXT)"
                Case mps.i_watched(2).database
                    SQL = "CREATE TABLE IF NOT EXISTS mpsync " & _
                          "(mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, mps_session TEXT, CompositeID TEXT, id INTEGER, " & _
                          "EpisodeFilename TEXT, watched INTEGER, myRating TEXT, StopTime TEXT, DateWatched TEXT, WatchedFileTimeStamp INTEGER, " & _
                          "UnwatchedItems INTEGER, EpisodesUnWatched INTEGER)"
                Case mps.i_watched(3).database
                    SQL = "CREATE TABLE IF NOT EXISTS mpsync " & _
                          "(mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, mps_session TEXT, idMovie INTEGER, watched BOOL, " & _
                          "timeswatched INTEGER, iwatchedPercent INTEGER, idResume INTEGER, idFile INTEGER, stoptime INTEGER, resumeData BLOOB, idBookmark INTEGER, " & _
                          "fPercentage TEXT)"

            End Select

        Else
            If FieldExist(path, database, "mpsync", "mps_session") = False Then SQL = "ALTER TABLE mpsync ADD COLUMN mps_session TEXT;"

            Select Case database

                Case mps.i_watched(0).database
                    If FieldExist(path, database, "mpsync", "id") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN id INTEGER;"
                    If FieldExist(path, database, "mpsync", "user") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN user TEXT;"
                    If FieldExist(path, database, "mpsync", "user_rating") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN user_rating TEXT;"
                    If FieldExist(path, database, "mpsync", "watched") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN watched INTEGER;"
                    If FieldExist(path, database, "mpsync", "resume_part") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN resume_part INTEGER;"
                    If FieldExist(path, database, "mpsync", "resume_data") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN resume_data TEXT;"
                    If FieldExist(path, database, "mpsync", "alternatecovers") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN alternatecovers TEXT;"
                    If FieldExist(path, database, "mpsync", "coverfullpath") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN coverfullpath TEXT;"
                    If FieldExist(path, database, "mpsync", "coverthumbfullpath") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN coverthumbfullpath TEXT;"
                Case mps.i_watched(1).database
                    If FieldExist(path, database, "mpsync", "idTrack") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN idTrack INTEGER;"
                    If FieldExist(path, database, "mpsync", "iResumeAt") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN iResumeAt INTEGER;"
                    If FieldExist(path, database, "mpsync", "dateLastPlayed") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN dateLastPlayed TEXT;"
                Case mps.i_watched(2).database
                    If FieldExist(path, database, "mpsync", "CompositeID") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN CompositeID TEXT;"
                    If FieldExist(path, database, "mpsync", "id") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN id INTEGER;"
                    If FieldExist(path, database, "mpsync", "EpisodeFilename") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN EpisoideFilename TEXT;"
                    If FieldExist(path, database, "mpsync", "watched") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN watched INTEGER;"
                    If FieldExist(path, database, "mpsync", "myRating") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN myRating TEXT;"
                    If FieldExist(path, database, "mpsync", "StopTime") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN StopTime TEXT;"
                    If FieldExist(path, database, "mpsync", "DateWatched") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN DateWatched TEXT;"
                    If FieldExist(path, database, "mpsync", "WatchedFileTimeStamp") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN WatchedFileTimeStamp INTEGER;"
                    If FieldExist(path, database, "mpsync", "UnwatchedItems") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN UnwatchedItems INTEGER;"
                    If FieldExist(path, database, "mpsync", "EpisodesUnWatched") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN EpisodesUnWatched INTEGER;"
                Case mps.i_watched(3).database
                    If FieldExist(path, database, "mpsync", "idMovie") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN idMovie INTEGER;"
                    If FieldExist(path, database, "mpsync", "watched") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN watched BOOL;"
                    If FieldExist(path, database, "mpsync", "timeswatched") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN timeswatched INTEGER;"
                    If FieldExist(path, database, "mpsync", "iwatchedPercent") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN iwatchedPercent INTEGER;"
                    If FieldExist(path, database, "mpsync", "idResume") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN idResume INTEGER;"
                    If FieldExist(path, database, "mpsync", "idFile") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN idFile INTEGER;"
                    If FieldExist(path, database, "mpsync", "stoptime") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN stoptime INTEGER;"
                    If FieldExist(path, database, "mpsync", "resumeData") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN resumeData BLOOB;"
                    If FieldExist(path, database, "mpsync", "idBookmark") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN idBookmark INTEGER;"
                    If FieldExist(path, database, "mpsync", "fPercentage") = False Then SQL = SQL & "ALTER TABLE mpsync ADD COLUMN fPercentage TEXT;"

            End Select

        End If

        If SQL <> Nothing Then

            Dim SQLconnect As New SQLiteConnection()
            Dim SQLcommand As SQLiteCommand

            SQLconnect.ConnectionString = "Data Source=" & path & database
            SQLconnect.Open()
            SQLcommand = SQLconnect.CreateCommand

            Try
                SQLcommand.CommandText = SQL
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                logStats("MPSync: Error executing '" & SQLcommand.CommandText & "' on database " & database, "ERROR")
            End Try

            SQLconnect.Close()

        End If

    End Sub

    Private Sub Drop_Watch_Tables(ByVal path As String, ByVal database As String)

        If Not IO.File.Exists(path & database) Then Exit Sub

        If p_Debug Then logStats("MPSync: Drop work table mpsync from database " & database, "DEBUG")

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        Try
            SQLcommand.CommandText = "DROP TABLE IF EXISTS mpsync"
            SQLcommand.ExecuteNonQuery()
        Catch ex As Exception
            logStats("MPSync: Error executing '" & SQLcommand.CommandText & "' from database " & database, "ERROR")
        End Try

        SQLconnect.Close()

    End Sub

    Private Sub Create_Watch_Triggers(ByVal path As String, ByVal database As String)

        If Not IO.File.Exists(path & database) Then Exit Sub

        logStats("MPSync: Creating triggers in database " & database, "LOG")

        Dim SQL As String = Nothing
        Dim tables() As String = Nothing
        Dim keys() As String = Nothing
        Dim fields() As fieldnames = Nothing
        Dim tblflds As String = Nothing
        Dim newflds As String = Nothing

        Select Case database

            Case mps.i_watched(0).database
                ReDim tables(1), keys(1), fields(1)

                tables(0) = "user_movie_settings"
                keys(0) = "id"
                fields(0).names = {"id", "user", "user_rating", "watched", "resume_part", "resume_time", "resume_data"}

                tables(1) = "movie_info"
                keys(1) = "id"
                fields(1).names = {"id", "alternatecovers", "coverfullpath", "coverthumbfullpath"}

            Case mps.i_watched(1).database
                ReDim tables(0), keys(0), fields(0)

                tables(0) = "tracks"
                keys(0) = "idTrack"
                fields(0).names = {"idTrack", "iResumeAt", "dateLastPlayed"}

            Case mps.i_watched(2).database
                ReDim tables(3), keys(3), fields(3)

                tables(0) = "local_episodes"
                keys(0) = "EpisodeFilename"
                fields(0).names = {"EpisodeFilename", "CompositeID", "DateWatched", "StopTime"}

                tables(1) = "online_episodes"
                keys(1) = "CompositeID"
                fields(1).names = {"CompositeID", "watched", "myRating"}

                tables(2) = "online_series"
                keys(2) = "id"
                fields(2).names = {"ID", "WatchedFileTimeStamp", "UnwatchedItems", "EpisodesUnWatched"}

                tables(3) = "season"
                keys(3) = "id"
                fields(3).names = {"ID", "UnwatchedItems", "EpisodesUnWatched"}

            Case mps.i_watched(3).database
                ReDim tables(2), keys(2), fields(2)

                tables(0) = "movie"
                keys(0) = "idMovie"
                fields(0).names = {"idMovie", "watched", "timeswatched", "iwatchedPercent"}

                tables(1) = "resume"
                keys(1) = "idResume"
                fields(1).names = {"idResume", "idFile", "stoptime", "resumeData"}

                tables(2) = "bookmark"
                keys(2) = "idResume"
                fields(2).names = {"idBookMark", "idFile", "fPercentage"}

        End Select

        ' build the SQL statement for the creation of the respective triggers

        For x As Integer = 0 To UBound(tables)

            If tables(x) <> Nothing Then

                If TableExist(path, database, tables(x)) Then

                    tblflds = FieldList(path, database, tables(x), fields(x).names)

                    If tblflds <> Nothing Then
                        newflds = FieldList(path, database, tables(x), fields(x).names, "new.")

                        SQL = SQL & "CREATE TRIGGER IF NOT EXISTS mpsync_watch_" & tables(x) & " " & _
                              "AFTER UPDATE OF " & tblflds & " ON " & tables(x) & " " & _
                              "BEGIN " & _
                              "DELETE FROM mpsync WHERE tablename='" & tables(x) & "' AND mps_session='" & session & "' AND " & keys(x) & "=new." & keys(x) & "; " & _
                              "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds & ") " & _
                              "VALUES('" & tables(x) & "',datetime('now','localtime'),'" & session & "'," & newflds & "); " & _
                              "END; "
                    End If
                End If
            End If
        Next

        If SQL <> Nothing Then

            Dim SQLconnect As New SQLiteConnection()
            Dim SQLcommand As SQLiteCommand

            SQLconnect.ConnectionString = "Data Source=" & path & database
            SQLconnect.Open()
            SQLcommand = SQLconnect.CreateCommand

            Try
                SQLcommand.CommandText = SQL
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                logStats("MPSync: Error executing '" & SQLcommand.CommandText & " on database " & database, "ERROR")
            End Try

            SQLconnect.Close()

        End If

    End Sub

    Private Sub Drop_Triggers(ByVal path As String, ByVal database As String, ByVal searchpattern As String, Optional ByVal table As String = Nothing)

        If Not IO.File.Exists(path & database) Then Exit Sub

        If p_Debug Then logStats("MPSync: Drop triggers in database " & database, "DEBUG")

        Dim SQL As String = Nothing
        Dim pattern() As String = Split(searchpattern, "|")

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        If table = Nothing Then
            SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type='trigger'"
        Else
            SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type='trigger' and tbl_name='" & table & "'"
        End If

        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()
            For x As Integer = 0 To UBound(pattern)
                If Left(SQLreader(0), Len(pattern(x))) = pattern(x) Then SQL = SQL & "DROP TRIGGER " & SQLreader(0) & "; "
            Next
        End While

        SQLreader.Close()

        If SQL <> Nothing Then
            Try
                SQLcommand.CommandText = SQL
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                logStats("MPSync: Error executing '" & SQLcommand.CommandText & "' on database " & database, "ERROR")
            End Try
        End If

        SQLconnect.Close()

    End Sub

    Private Sub Create_Sync_Triggers(ByVal database As String)

        If p_Debug Then logStats("MPSync: Check synchronization triggers in database " & database, "DEBUG")

        Dim x As Integer = -1
        Dim table() As String = Nothing
        Dim trigger() As String = Nothing
        Dim SQL() As String = Nothing
        Dim d_SQL, u_SQL, i_SQL As String
        Dim omit As Array = {"mpsync", "mpsync_trigger", "sqlite_sequence", "sqlite_stat1", "sqlite_stat2"}

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader

        SQLconnect.ConnectionString = "Data Source=" & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table'"
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()

            If Array.IndexOf(omit, SQLreader(0)) = -1 Then
                x += 1
                ReDim Preserve table(x)
                table(x) = SQLreader(0)
            End If

        End While

        SQLreader.Close()

        If x = -1 Then
            ReDim Preserve table(0)
            table(0) = Nothing
        End If

        SQLcommand.CommandText = "SELECT sql FROM sqlite_master WHERE type='trigger'"
        SQLreader = SQLcommand.ExecuteReader()

        x = -1

        While SQLreader.Read()

            If Array.IndexOf(omit, SQLreader(0)) = -1 Then
                x += 1
                ReDim Preserve trigger(x)
                trigger(x) = SQLreader(0)
            End If

        End While

        SQLreader.Close()

        If x = -1 Then
            ReDim Preserve trigger(0)
            trigger(0) = Nothing
        End If

        If TableExist(IO.Path.GetDirectoryName(database) & "\", IO.Path.GetFileName(database), "mpsync_trigger", True) = False Then

            Try
                SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS mpsync_trigger (id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, lastupdated TEXT)"
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                logStats("MPSync: Error executing '" & SQLcommand.CommandText & " on database " & database, "ERROR")
            End Try

        End If

        For x = 0 To UBound(table)

            u_SQL = "CREATE TRIGGER mpsync_work_u_" & table(x) & " " & _
                    "AFTER UPDATE ON " & table(x) & " " & _
                    "BEGIN " & _
                    "UPDATE mpsync_trigger SET lastupdated=datetime('now','localtime') WHERE tablename='" & table(x) & "'; " & _
                    "INSERT INTO mpsync_trigger(tablename,lastupdated) " & _
                    "SELECT '" & table(x) & "',datetime('now','localtime') " & _
                    "WHERE NOT EXISTS (SELECT 1 FROM mpsync_trigger WHERE tablename = '" & table(x) & "' LIMIT 1); " & _
                    "END"

            i_SQL = "CREATE TRIGGER mpsync_work_i_" & table(x) & " " & _
                    "AFTER INSERT ON " & table(x) & " " & _
                    "BEGIN " & _
                    "UPDATE mpsync_trigger SET lastupdated=datetime('now','localtime') WHERE tablename='" & table(x) & "'; " & _
                    "INSERT INTO mpsync_trigger(tablename,lastupdated) " & _
                    "SELECT '" & table(x) & "',datetime('now','localtime') " & _
                    "WHERE NOT EXISTS (SELECT 1 FROM mpsync_trigger WHERE tablename = '" & table(x) & "' LIMIT 1); " & _
                    "END"

            d_SQL = "CREATE TRIGGER mpsync_work_d_" & table(x) & " " & _
                    "AFTER DELETE ON " & table(x) & " " & _
                    "BEGIN " & _
                    "UPDATE mpsync_trigger SET lastupdated=datetime('now','localtime') WHERE tablename='" & table(x) & "'; " & _
                    "INSERT INTO mpsync_trigger(tablename,lastupdated) " & _
                    "SELECT '" & table(x) & "',datetime('now','localtime') " & _
                    "WHERE NOT EXISTS (SELECT 1 FROM mpsync_trigger WHERE tablename = '" & table(x) & "' LIMIT 1); " & _
                    "END"

            If trigger.Contains(u_SQL) And trigger.Contains(i_SQL) And trigger.Contains(d_SQL) Then
                u_SQL = Nothing
                i_SQL = Nothing
                d_SQL = Nothing
            Else
                u_SQL = u_SQL & "; "
                i_SQL = i_SQL & "; "
                d_SQL = d_SQL & "; "
            End If

            ReDim Preserve SQL(x)
            SQL(x) = u_SQL & i_SQL & d_SQL

        Next

        SQLconnect.Close()

        For x = 0 To UBound(SQL)
            If SQL(x) <> Nothing Then Drop_Triggers(IO.Path.GetDirectoryName(database) & "\", IO.Path.GetFileName(database), "mpsync_work", table(x))
        Next

        SQLconnect.ConnectionString = "Data Source=" & database
        SQLconnect.Open()

        For x = 0 To UBound(SQL)

            If SQL(x) <> Nothing Then

                logStats("MPSync: Creating synchronization triggers on table " & table(x) & " in database " & database, "LOG")

                Try
                    SQLcommand.CommandText = SQL(x)
                    SQLcommand.ExecuteNonQuery()
                Catch ex As Exception
                    logStats("MPSync: Error executing '" & SQLcommand.CommandText & " on table " & table(x) & " in database " & database & " with exception: " & ex.Message, "ERROR")
                End Try

            End If

        Next

        SQLconnect.Close()

    End Sub

    Private Sub WorkMethod(stateInfo As Object)

        Dim db As Boolean = True
        Dim thumbs As Boolean = True

        If _db_client <> Nothing And _db_server <> Nothing And checked_databases Then
            If p_Debug Then Log.Debug("MPSync: Started Database thread")
            Dim bw_db As New BackgroundWorker
            bw_db.WorkerSupportsCancellation = True
            AddHandler bw_db.DoWork, AddressOf MPSync_process_DB.bw_db_worker
            bw_db.RunWorkerAsync()
        Else
            db = False
        End If

        If _thumbs_client <> Nothing And _thumbs_server <> Nothing And checked_thumbs Then
            If p_Debug Then Log.Debug("MPSync: Started Thumbs thread")
            Dim bw_thumbs As New BackgroundWorker
            bw_thumbs.WorkerSupportsCancellation = True
            AddHandler bw_thumbs.DoWork, AddressOf MPSync_process_Thumbs.bw_thumbs_worker
            bw_thumbs.RunWorkerAsync()
        Else
            thumbs = False
        End If

        If Not MPSync_settings.syncnow Then
            If db = False And thumbs = False Then CType(stateInfo, AutoResetEvent).Set()
        End If

    End Sub

End Class
