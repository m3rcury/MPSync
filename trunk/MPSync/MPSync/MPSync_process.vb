﻿Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports System.ComponentModel
Imports System.Threading
Imports System.Data.SQLite

Public Class MPSync_process

    Private mps As New MPSync_settings
    Public Shared _db_client, _db_server, _thumbs_client, _thumbs_server, _databases(), _thumbs(), _watched_dbs() As String
    Public Shared _db_sync, _thumbs_sync, _db_direction, _db_sync_method, _thumbs_direction, _thumbs_sync_method As Integer
    Public Shared _db_pause, _thumbs_pause, db_complete, thumbs_complete, debug As Boolean
    Dim session As String
    Dim checked_databases, checked_thumbs, checked_watched As Boolean

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

    Public Sub MPSyncProcess()

        Dim file As String = Config.getFile(Config.Dir.Config, "MPSync.xml")

        If Not FileIO.FileSystem.FileExists(file) Then Return

        Log.Info("MPSync: process plugin initialisation.")

        ' get configuratin from XML file

        Dim db_sync_value, thumbs_sync_value, databases, thumbs, watched_dbs, version As String

        Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(file)

            version = XMLreader.GetValueAsString("Plugin", "version", "0")
            checked_databases = XMLreader.GetValueAsString("Plugin", "databases", True)
            checked_thumbs = XMLreader.GetValueAsString("Plugin", "thumbs", True)
            p_Debug = XMLreader.GetValueAsString("Plugin", "debug", False)
            session = XMLreader.GetValueAsString("Plugin", "session ID", Nothing)
            checked_watched = XMLreader.GetValueAsString("DB Settings", "watched", False)

            _db_client = XMLreader.GetValueAsString("DB Path", "client", Nothing)
            _db_server = XMLreader.GetValueAsString("DB Path", "server", Nothing)
            _db_direction = XMLreader.GetValueAsInt("DB Path", "direction", 0)
            _db_sync_method = XMLreader.GetValueAsInt("DB Path", "method", 0)

            _db_sync = XMLreader.GetValueAsInt("DB Settings", "sync periodicity", 15)
            db_sync_value = XMLreader.GetValueAsString("DB Settings", "sync periodicity value", "minutes")
            _db_pause = XMLreader.GetValueAsString("DB Settings", "pause while playing", False)
            databases = XMLreader.GetValueAsString("DB Settings", "databases", Nothing)
            watched_dbs = XMLreader.GetValueAsString("DB Settings", "watched databases", Nothing)

            _thumbs_client = XMLreader.GetValueAsString("Thumbs Path", "client", Nothing)
            _thumbs_server = XMLreader.GetValueAsString("Thumbs Path", "server", Nothing)
            _thumbs_direction = XMLreader.GetValueAsInt("Thumbs Path", "direction", 0)
            _thumbs_sync_method = XMLreader.GetValueAsInt("Thumbs Path", "method", 0)

            _thumbs_sync = XMLreader.GetValueAsInt("Thumbs Settings", "sync periodicity", 15)
            thumbs_sync_value = XMLreader.GetValueAsString("Thumbs Settings", "sync periodicity value", "minutes")
            _thumbs_pause = XMLreader.GetValueAsString("Thumbs Settings", "pause while playing", False)
            thumbs = XMLreader.GetValueAsString("Thumbs Settings", "thumbs", Nothing)

        End Using

        If session = Nothing Then
            session = System.Guid.NewGuid.ToString()
            Using XMLwriter As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))
                XMLwriter.SetValue("Plugin", "session ID", session)
            End Using
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

    Private Function TableExist(ByVal path As String, ByVal database As String, ByVal table As String, Optional ByVal dropifempty As Boolean = False) As Boolean

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim exist As Boolean = False

        SQLconnect.ConnectionString = "Data Source=" + path + database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type=""table"""
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()

            If SQLreader(0) = table Then
                exist = True
                Exit While
            End If

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

        Return columns.Contains(field)

    End Function

    Private Function FieldList(ByVal path As String, ByVal database As String, ByVal table As String, ByVal fields As Array, Optional ByVal prefix As String = Nothing) As String

        Dim f_list As String = Nothing

        For x As Integer = 0 To UBound(fields)

            If FieldExist(path, database, table, fields(x)) Then
                f_list &= prefix & fields(x) & ","
            End If

        Next

        Return Left(f_list, Len(f_list) - 1)

    End Function

    Private Sub Create_Work_Tables(ByVal path As String, ByVal database As String)

        If p_Debug Then Log.Debug("MPSync: Creating work table mpsync in database " & database)

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        Dim SQL As String = Nothing

        If TableExist(path, database, "mpsync", True) = False Then

            Select Case database

                Case mps.i_watched(0).database
                    SQL = "CREATE TABLE IF NOT EXISTS mpsync " & _
                          "(mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, mps_session TEXT, id INTEGER, user TEXT, " & _
                          "user_rating TEXT, watched INTEGER, resume_part INTEGER, resume_time INTEGER, resume_data TEXT)"
                Case mps.i_watched(1).database
                    SQL = "CREATE TABLE IF NOT EXISTS mpsync (mps_id INTEGER PRIMARY KEY AUTOINCREMENT, tablename TEXT, mps_lastupdated TEXT, mps_session TEXT" & _
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
            If FieldExist(path, database, "mpsync", "mps_session") = False Then
                SQL = "ALTER TABLE mpsync ADD COLUMN mps_session TEXT"
            End If
        End If

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
        Dim fields1, fields2, fields3, fields4 As Array
        Dim tblflds1, tblflds2, tblflds3, tblflds4 As String
        Dim newflds1, newflds2, newflds3, newflds4 As String

        Select Case database

            Case mps.i_watched(0).database
                fields1 = {"user", "user_rating", "watched", "resume_part", "resume_time", "resume_data"}
                tblflds1 = FieldList(path, database, "user_movie_settings", fields1)
                newflds1 = FieldList(path, database, "user_movie_settings", fields1, "new.")

                SQL = "DROP TRIGGER IF EXISTS mpsync_update; " & _
                      "CREATE TRIGGER mpsync_update " & _
                      "AFTER UPDATE OF " & tblflds1 & " ON user_movie_settings " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE id = new.id; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds1 & ") " & _
                      "VALUES('user_movie_settings',datetime('now'),'" & session & "'," & newflds1 & "); " & _
                      "END"
            Case mps.i_watched(1).database
                fields1 = {"idTrack", "iResumeAt", "dateLastPlayed"}
                tblflds1 = FieldList(path, database, "tracks", fields1)
                newflds1 = FieldList(path, database, "tracks", fields1, "new.")

                SQL = "DROP TRIGGER IF EXISTS mpsync_update; " & _
                      "CREATE TRIGGER mpsync_update " & _
                      "AFTER UPDATE OF " & tblflds1 & " ON tracks " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE idTrack = new.idTrack; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds1 & ") " & _
                      "VALUES('tracks',datetime('now'),'" & session & "'," & newflds1 & "); " & _
                      "END"
            Case mps.i_watched(2).database
                fields1 = {"EpisodeFilename", "CompositeID", "DateWatched", "StopTime"}
                tblflds1 = FieldList(path, database, "local_episodes", fields1)
                newflds1 = FieldList(path, database, "local_episodes", fields1, "new.")
                fields2 = {"CompositeID", "watched", "myRating"}
                tblflds2 = FieldList(path, database, "online_episodes", fields2)
                newflds2 = FieldList(path, database, "online_episodes", fields2, "new.")
                fields3 = {"ID", "WatchedFileTimeStamp", "UnwatchedItems", "EpisodesUnWatched"}
                tblflds3 = FieldList(path, database, "online_series", fields3)
                newflds3 = FieldList(path, database, "online_series", fields3, "new.")
                fields4 = {"ID", "UnwatchedItems", "EpisodesUnWatched"}
                tblflds4 = FieldList(path, database, "season", fields4)
                newflds4 = FieldList(path, database, "season", fields4, "new.")

                SQL = "DROP TRIGGER IF EXISTS mpsync_update1; " & _
                      "CREATE TRIGGER mpsync_update1 " & _
                      "AFTER UPDATE OF " & tblflds1 & " ON local_episodes " & _
                      "BEGIN  " & _
                      "DELETE FROM mpsync WHERE tablename = 'local_episodes' AND EpisodeFilename = new.EpisodeFilename; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds1 & ") " & _
                      "VALUES('local_episodes',datetime('now'),'" & session & "'," & newflds1 & "); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update2; " & _
                      "CREATE TRIGGER mpsync_update2 " & _
                      "AFTER UPDATE OF " & tblflds2 & " ON online_episodes " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'online_episodes' AND CompositeID = new.CompositeID; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds2 & ") " & _
                      "VALUES('online_episodes',datetime('now'),'" & session & "'," & newflds2 & "); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update3; " & _
                      "CREATE TRIGGER mpsync_update3 " & _
                      "AFTER UPDATE OF " & tblflds3 & " ON online_series " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'online_series' AND ID = new.ID; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds3 & ") " & _
                      "VALUES('online_series',datetime('now'),'" & session & "'," & newflds3 & "); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update4; " & _
                      "CREATE TRIGGER mpsync_update4 " & _
                      "AFTER UPDATE OF " & tblflds4 & " ON season " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'season' AND ID = new.ID; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds4 & ") " & _
                      "VALUES('season',datetime('now'),'" & session & "'," & newflds4 & "); " & _
                      "END"
            Case mps.i_watched(3).database
                fields1 = {"idMovie", "watched", "timeswatched", "iwatchedPercent"}
                tblflds1 = FieldList(path, database, "movie", fields1)
                newflds1 = FieldList(path, database, "movie", fields1, "new.")
                fields2 = {"idResume", "idFile", "stoptime", "resumeData"}
                tblflds2 = FieldList(path, database, "resume", fields2)
                newflds2 = FieldList(path, database, "resume", fields2, "new.")
                fields3 = {"idBookMark", "idFile", "fPercentage"}
                tblflds3 = FieldList(path, database, "bookmark", fields3)
                newflds3 = FieldList(path, database, "bookmark", fields3, "new.")

                SQL = "DROP TRIGGER IF EXISTS mpsync_update1; " & _
                      "CREATE TRIGGER mpsync_update1 " & _
                      "AFTER UPDATE OF " & tblflds1 & " ON movie " & _
                      "BEGIN  " & _
                      "DELETE FROM mpsync WHERE tablename = 'movie' AND idMovie = new.idMovie; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds1 & ") " & _
                      "VALUES('movie',datetime('now'),'" & session & "'," & newflds1 & "); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update2; " & _
                      "CREATE TRIGGER mpsync_update2 " & _
                      "AFTER UPDATE OF " & tblflds2 & " ON resume " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'resume' AND idResume = new.idResume; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds2 & ") " & _
                      "VALUES('resume',datetime('now'),'" & session & "'," & newflds2 & "); " & _
                      "END; " & _
                      "DROP TRIGGER IF EXISTS mpsync_update3; " & _
                      "CREATE TRIGGER mpsync_update3 " & _
                      "AFTER UPDATE OF " & tblflds3 & " ON bookmark " & _
                      "BEGIN " & _
                      "DELETE FROM mpsync WHERE tablename = 'bookmark' AND idResume = new.idResume; " & _
                      "INSERT OR REPLACE INTO mpsync(tablename,mps_lastupdated,mps_session," & tblflds3 & ") " & _
                      "VALUES('bookmark',datetime('now'),'" & session & "'," & newflds3 & "); " & _
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
