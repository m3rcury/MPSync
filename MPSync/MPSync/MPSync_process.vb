Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports Microsoft.VisualBasic.FileIO

Imports System.ComponentModel
Imports System.Threading

Public Class MPSync_process

    Public Shared _db_client, _db_server, _thumbs_client, _thumbs_server, _databases(), _thumbs() As String
    Public Shared _db_sync, _thumbs_sync, _db_direction, _db_sync_method, _thumbs_direction, _thumbs_sync_method As Integer
    Public Shared _db_pause, _thumbs_pause As Boolean
    Dim checked_databases, checked_thumbs As Boolean
    Dim complete As Boolean = False

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

        Dim file As String = Config.GetFile(Config.Dir.Config, "MPSync.xml")

        If Not FileSystem.FileExists(file) Then Return

        Log.Info("MPSync: process plugin initialisation.")

        ' get configuratin from XML file

        Dim db_sync_value, thumbs_sync_value, databases, thumbs, version As String

        Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(file)

            version = XMLreader.GetValueAsString("Plugin", "version", "0")
            checked_databases = XMLreader.GetValueAsString("Plugin", "databases", True)
            checked_thumbs = XMLreader.GetValueAsString("Plugin", "thumbs", True)

            _db_client = XMLreader.GetValueAsString("DB Path", "client", Nothing)
            _db_server = XMLreader.GetValueAsString("DB Path", "server", Nothing)
            _db_direction = XMLreader.GetValueAsInt("DB Path", "direction", 0)
            _db_sync_method = XMLreader.GetValueAsInt("DB Path", "method", 0)

            _db_sync = XMLreader.GetValueAsInt("DB Settings", "sync periodicity", 15)
            db_sync_value = XMLreader.GetValueAsString("DB Settings", "sync periodicity value", "minutes")
            _db_pause = XMLreader.GetValueAsString("DB Settings", "pause while playing", False)
            databases = XMLreader.GetValueAsString("DB Settings", "databases", Nothing)

            _thumbs_client = XMLreader.GetValueAsString("Thumbs Path", "client", Nothing)
            _thumbs_server = XMLreader.GetValueAsString("Thumbs Path", "server", Nothing)
            _thumbs_direction = XMLreader.GetValueAsInt("Thumbs Path", "direction", 0)
            _thumbs_sync_method = XMLreader.GetValueAsInt("Thumbs Path", "method", 0)

            _thumbs_sync = XMLreader.GetValueAsInt("Thumbs Settings", "sync periodicity", 15)
            thumbs_sync_value = XMLreader.GetValueAsString("Thumbs Settings", "sync periodicity value", "minutes")
            _thumbs_pause = XMLreader.GetValueAsString("Thumbs Settings", "pause while playing", False)
            thumbs = XMLreader.GetValueAsString("Thumbs Settings", "thumbs", Nothing)

        End Using

        If _db_client <> Nothing And _db_server <> Nothing And checked_databases Then

            ' get list of databases to synchronise
            If databases <> Nothing Then
                _databases = Split(databases, "|")
            Else
                ReDim _databases(0)
                _databases(0) = "ALL"
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

            Dim autoEvent As New AutoResetEvent(False)

            ThreadPool.QueueUserWorkItem(AddressOf WorkMethod, autoEvent)

            autoEvent.WaitOne()

        Else
            WorkMethod(Nothing)
        End If

        Return

    End Sub

    Private Sub WorkMethod(stateInfo As Object)

        Dim db As Boolean = True
        Dim thumbs As Boolean = True

        If _db_client <> Nothing And _db_server <> Nothing And checked_databases Then
            Dim bw_db As New BackgroundWorker
            bw_db.WorkerSupportsCancellation = True
            AddHandler bw_db.DoWork, AddressOf MPSync_process_DB.bw_db_worker
            AddHandler bw_db.RunWorkerCompleted, AddressOf bw_worker_completed
            bw_db.RunWorkerAsync()
        Else
            db = False
        End If

        If _thumbs_client <> Nothing And _thumbs_server <> Nothing And checked_thumbs Then
            Dim bw_thumbs As New BackgroundWorker
            bw_thumbs.WorkerSupportsCancellation = True
            AddHandler bw_thumbs.DoWork, AddressOf MPSync_process_Thumbs.bw_thumbs_worker
            bw_thumbs.RunWorkerAsync()
        Else
            thumbs = False
        End If

        If MPSync_settings.syncnow Then
            Do While Not complete
                wait(30, False)
            Loop
        End If

        If db = False And thumbs = False Then CType(stateInfo, AutoResetEvent).Set()

    End Sub

    Private Sub bw_worker_completed(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        If Not e.Cancelled Then complete = True
    End Sub

End Class
