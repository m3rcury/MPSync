Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports System.ComponentModel
Imports System.Threading
Imports System.Data.SQLite

Public Class MPSync_process

    Private mps As New MPSync_settings
    Public Shared _db_client, _db_server, _thumbs_client, _thumbs_server, _databases(), _thumbs(), _watched_dbs() As String
    Public Shared _db_sync, _thumbs_sync, _db_direction, _db_sync_method, _thumbs_direction, _thumbs_sync_method As Integer
    Public Shared _db_pause, _thumbs_pause, db_complete, thumbs_complete, debug As Boolean
    Dim checked_databases, checked_thumbs, checked_watched As Boolean

    Public Shared Property p_Debug As Boolean
        Get
            Return debug
        End Get
        Set(value As Boolean)
            debug = value
        End Set
    End Property

    Public Shared Sub wait(ByVal seconds As Long, Optional ByVal verbose As Boolean = True)

        If verbose Then Log.Info("MPSync: process plugin sleeping " & seconds.ToString & " seconds.....")

        System.Threading.Thread.Sleep(seconds * 1000)

    End Sub

    Public Shared Sub CheckPlayerplaying(ByVal thread As String, ByVal seconds As Long)
        If (thread = "db" And _db_pause) Or (thread = "thumbs" And _thumbs_pause) Then
            If MediaPortal.Player.g_Player.Playing Then Log.Info("MPSync: pausing " & thread & " thread as player is playing.")
            Do While MediaPortal.Player.g_Player.Playing
                MPSync_process.wait(seconds, False)
            Loop
        End If
    End Sub

    Public Sub MPSyncProcess()

        Dim file As String = Config.getFile(Config.Dir.Config, "MPSync.xml")

        If Not FileIO.FileSystem.FileExists(file) Then Return

        Log.Info("MPSync: process plugin initialisation.")

        ' get configuratin from XML file

        Dim db_sync_value, thumbs_sync_value, databases, thumbs, watched_dbs, version As String

        Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(file)

            version = XMLreader.getValueAsString("Plugin", "version", "0")
            checked_databases = XMLreader.getValueAsString("Plugin", "databases", True)
            checked_thumbs = XMLreader.getValueAsString("Plugin", "thumbs", True)
            p_Debug = XMLreader.GetValueAsString("Plugin", "debug", False)
            checked_watched = XMLreader.GetValueAsString("DB Settings", "watched", False)

            _db_client = XMLreader.getValueAsString("DB Path", "client", Nothing)
            _db_server = XMLreader.getValueAsString("DB Path", "server", Nothing)
            _db_direction = XMLreader.getValueAsInt("DB Path", "direction", 0)
            _db_sync_method = XMLreader.getValueAsInt("DB Path", "method", 0)

            _db_sync = XMLreader.getValueAsInt("DB Settings", "sync periodicity", 15)
            db_sync_value = XMLreader.getValueAsString("DB Settings", "sync periodicity value", "minutes")
            _db_pause = XMLreader.getValueAsString("DB Settings", "pause while playing", False)
            databases = XMLreader.getValueAsString("DB Settings", "databases", Nothing)
            watched_dbs = XMLreader.getValueAsString("DB Settings", "watched databases", Nothing)

            _thumbs_client = XMLreader.getValueAsString("Thumbs Path", "client", Nothing)
            _thumbs_server = XMLreader.getValueAsString("Thumbs Path", "server", Nothing)
            _thumbs_direction = XMLreader.getValueAsInt("Thumbs Path", "direction", 0)
            _thumbs_sync_method = XMLreader.getValueAsInt("Thumbs Path", "method", 0)

            _thumbs_sync = XMLreader.getValueAsInt("Thumbs Settings", "sync periodicity", 15)
            thumbs_sync_value = XMLreader.getValueAsString("Thumbs Settings", "sync periodicity value", "minutes")
            _thumbs_pause = XMLreader.getValueAsString("Thumbs Settings", "pause while playing", False)
            thumbs = XMLreader.getValueAsString("Thumbs Settings", "thumbs", Nothing)

        End Using

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

            ' calculate actual sync periodicity in seconds
            If db_sync_value = "minutes" Then
                _db_sync = _db_sync * 60
            ElseIf db_sync_value = "hours" Then
                _db_sync = _db_sync * 3600
            End If

            ' check that both paths end with a "\"
            If InStrRev(_db_client, "\") <> Len(_db_client) Then _db_client = Trim(_db_client) & "\"
            If InStrRev(_db_server, "\") <> Len(_db_server) Then _db_server = Trim(_db_server) & "\"

            ' create the required work files and triggers to handle watched status synchronization

            For x As Integer = 0 To UBound(_watched_dbs)
                Create_Work_Tables(_db_client, _watched_dbs(x))
                Create_Work_Tables(_db_server, _watched_dbs(x))
                Create_Triggers(_db_client, _watched_dbs(x))
            Next

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

            If p_Debug Then Log.Debug("MPSync: Immediate Synchronization started")

            Dim autoEvent As New AutoResetEvent(False)

            ThreadPool.QueueUserWorkItem(AddressOf WorkMethod, autoEvent)

            autoEvent.WaitOne()

        Else
            WorkMethod(Nothing)
        End If

        Return

    End Sub

    Private Shared Function FieldExist(ByVal path As String, ByVal database As String, ByVal table As String, ByVal fields() As String) As Array

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim columns() As String = Nothing
        Dim exists() As Boolean
        Dim x As Integer = 0

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA table_info (" & table & ")"
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()
            ReDim Preserve columns(x)
            columns(x) = SQLreader(1)
            x += 1
        End While

        SQLconnect.Close()

        If x = 0 Then ReDim Preserve columns(0)

        ReDim exists(fields.Count - 1)

        For x = 0 To fields.Count - 1
            exists(x) = columns.Contains(fields(x))
        Next

        Return exists

    End Function

    Private Sub Create_Work_Tables(ByVal path As String, ByVal database As String)

        If p_Debug Then Log.Debug("MPSync: Creating work table mpsync in database " & database)

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        Dim SQL As String = Nothing

        Select Case database

            Case mps.i_watched(0).database
                SQL = "CREATE TABLE IF NOT EXISTS mpsync " & _
                      "(mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, id INTEGER, user TEXT, user_rating TEXT, watched INTEGER, " & _
                      "resume_part INTEGER, resume_time INTEGER, resume_data TEXT)"
            Case mps.i_watched(1).database
                SQL = "CREATE TABLE IF NOT EXISTS mpsync (mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, idTrack INTEGER, " & _
                      "iResumeAt INTEGER, dateLastPlayed TEXT)"
            Case mps.i_watched(2).database
                SQL = "CREATE TABLE IF NOT EXISTS mpsync " & _
                      "(mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, CompositeID TEXT, id INTEGER, EpisodeFilename TEXT, " & _
                      "watched INTEGER, myRating TEXT, StopTime TEXT, DateWatched TEXT, WatchedFileTimeStamp INTEGER, UnwatchedItems INTEGER, EpisodesUnWatched INTEGER)"
            Case mps.i_watched(3).database
                SQL = "CREATE TABLE IF NOT EXISTS mpsync " & _
                      "(mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, idMovie INTEGER, watched BOOL, timeswatched INTEGER, " & _
                      "iwatchedPercent INTEGER, idResume INTEGER, idFile INTEGER, stoptime INTEGER, resumeData BLOOB, idBookmark INTEGER, fPercentage TEXT)"
        End Select

        If SQL <> Nothing Then
            Try
                SQLcommand.CommandText = SQL
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                Log.Error("MPSync: Error executing " & SQL)
            End Try
        End If

    End Sub

    Private Sub Create_Triggers(ByVal path As String, ByVal database As String)

        If p_Debug Then Log.Debug("MPSync: Creating triggers in database " & database)

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        Dim SQL As String = Nothing

        Select Case database

            Case mps.i_watched(0).database
                SQL = "DROP TRIGGER IF EXISTS mpsync_update; " & _
                      "CREATE TRIGGER mpsync_update " & _
                      "AFTER UPDATE OF user_rating, watched, resume_part, resume_time, resume_data ON user_movie_settings " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE id = new.id; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, id, user, user_rating, watched, resume_part, resume_time, resume_data) " & _
                      "VALUES('user_movie_settings', datetime('now'), new.id, new.user, new.user_rating, new.watched, new.resume_part, new.resume_time, new.resume_data); " & _
                      "END"
            Case mps.i_watched(1).database
                SQL = "DROP TRIGGER IF EXISTS mpsync_update; " & _
                      "CREATE TRIGGER mpsync_update " & _
                      "AFTER UPDATE OF uResumeAt, dateLastPlayed ON tracks " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE idTrack = new.idTrack; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, idTrack, dateLastPlayed) " & _
                      "VALUES('tracks', datetime('now'), new.idTrack, new.dateLastPlayed); " & _
                      "END"
            Case mps.i_watched(2).database
                SQL = "DROP TRIGGER IF EXISTS mpsync_update1; " & _
                      "CREATE TRIGGER mpsync_update1 " & _
                      "AFTER UPDATE OF DateWatched, StopTime ON local_episodes " & _
                      "BEGIN  " & _
                      "DELETE FROM mpsync WHERE tablename = 'local_episodes' AND EpisodeFilename = new.EpisodeFilename; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, EpisodeFolename, CompositeID, DateWatched, StopTime) " & _
                      "VALUES('local_episodes', datetime('now'), new.EpisodeFilename, new.CompositeID, new.DateWatched, new.StopTime); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update2; " & _
                      "CREATE TRIGGER mpsync_update2 " & _
                      "AFTER UPDATE OF watched, myRating ON online_episodes " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'online_episodes' AND CompositeID = new.CompositeID; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, CompositeID, watched, myRating) " & _
                      "VALUES('online_episodes', datetime('now'), new.CompositeID, new.watched, new.myRating); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update3; " & _
                      "CREATE TRIGGER mpsync_update3 " & _
                      "AFTER UPDATE OF WatchedFileTimeStamp, UnwatchedItems, EpisodesUnWatched ON online_series " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'online_series' AND ID = new.ID; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, ID, WatchedFileTimeStamp, UnwatchedItems, EpisodesUnWatched) " & _
                      "VALUES('online_series', datetime('now'), new.ID, new.WatchedFileTimeStamp, new.UnwatchedItems, new.EpisodesUnWatched); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update4; " & _
                      "CREATE TRIGGER mpsync_update4 " & _
                      "AFTER UPDATE OF UnwatchedItems, EpisodesUnWatched ON season " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'season' AND ID = new.ID; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, ID, UnwatchedItems, EpisodesUnWatched) " & _
                      "VALUES('season', datetime('now'), new.ID, new.UnwatchedItems, new.EpisodesUnWatched); " & _
                      "END"
            Case mps.i_watched(3).database
                SQL = "DROP TRIGGER IF EXISTS mpsync_update1; " & _
                      "CREATE TRIGGER mpsync_update1 " & _
                      "AFTER UPDATE OF watched, timeswatched, iwatchedPercent ON movie " & _
                      "BEGIN  " & _
                      "DELETE FROM mpsync WHERE tablename = 'movie' AND idMovie = new.idMovie; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, idMovie, watched, timeswatched, iwatchedPercent) " & _
                      "VALUES('movie', datetime('now'), new.idMovie, new.watched, new.timeswatched, new.iwatchedPercent); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update2; " & _
                      "CREATE TRIGGER mpsync_update2 " & _
                      "AFTER UPDATE OF stoptime, resumeData ON resume " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'resume' AND idResume = new.idResume; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, idResume, idFile, stoptime, resumeData) " & _
                      "VALUES('resume', datetime('now'), new.idResume, new.idFile, new.stoptime, new.resumeData); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update3; " & _
                      "CREATE TRIGGER mpsync_update3 " & _
                      "AFTER UPDATE OF fPercentage ON bookmark " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'bookmark' AND idResume = new.idResume; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename, mps_lastupdated, idBookMark, idFile, fPercentage) " & _
                      "VALUES('bookmark', datetime('now'), new.idResume, new.idFile, new.fPercentage); " & _
                      "END"
        End Select

        If SQL <> Nothing Then
            Try
                SQLcommand.CommandText = SQL
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                Log.Error("MPSync: Error executing " & SQL)
            End Try
        End If

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
        Else
            db_complete = Not checked_databases
            thumbs_complete = Not checked_thumbs
            Do While Not (db_complete And thumbs_complete)
                wait(30, False)
            Loop
        End If

    End Sub

End Class
