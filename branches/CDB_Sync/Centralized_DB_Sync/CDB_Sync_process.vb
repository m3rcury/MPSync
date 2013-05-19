Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports Microsoft.VisualBasic.FileIO

Imports System.Data.SQLite
Imports System.ComponentModel

Public Class CDB_Sync_process

    Dim _source, _target, _databases() As String
    Dim _direction, _sync_method, _bw_active_jobs, _bw_sync_jobs As Integer
    Dim bw_sync() As BackgroundWorker

    Public Function CPU_Usage_Percent() As String

        Dim cpu As PerformanceCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")

        Dim dummy As Long

        dummy = cpu.NextValue()
        System.Threading.Thread.Sleep(1000)

        Return cpu.NextValue().ToString("#0") & "%"

    End Function

    Private Sub wait(ByVal seconds As Long, Optional ByVal verbose As Boolean = True)

        If verbose Then Log.Info("CDB_Sync: process plugin sleeping " & seconds.ToString & " seconds.....")

        System.Threading.Thread.Sleep(seconds * 1000)

    End Sub

    Public Sub CDB_SyncProcess()

        Dim file As String = Config.GetFile(Config.Dir.Config, "CDB_Sync.xml")

        If Not FileSystem.FileExists(file) Then Return

        Log.Info("CDB_Sync: process plugin initialisation.")

        ' get configuratin from XML file

        Dim sync As Integer
        Dim sync_value, databases As String

        Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(file)

            _source = XMLreader.GetValueAsString("Path", "source", Nothing)
            _target = XMLreader.GetValueAsString("Path", "target", Nothing)
            _direction = XMLreader.GetValueAsInt("Path", "direction", 0)
            _sync_method = XMLreader.GetValueAsInt("Path", "method", 0)

            sync = XMLreader.GetValueAsInt("Settings", "sync periodicity", 1)
            sync_value = XMLreader.GetValueAsString("Settings", "sync periodicity value", "minutes")
            databases = XMLreader.GetValueAsString("Settings", "databases", Nothing)

        End Using

        If _source = Nothing Or _target = Nothing Then
            Log.Error("CDB_Sync: process plugin not configured!!.")
            Return
        End If

        ' get list of databases to synchronise

        If databases <> Nothing Then
            _databases = Split(databases, "|")
        Else
            ReDim _databases(0)
            _databases(0) = "ALL"
        End If

        ' calculate actual sync periodicity in seconds

        If sync_value = "minutes" Then
            sync = sync * 60
        ElseIf sync_value = "hours" Then
            sync = sync * 3600
        End If

        ' check that both paths end with a "\"
        If InStrRev(_source, "\") <> Len(_source) Then _source = Trim(_source) & "\"
        If InStrRev(_target, "\") <> Len(_target) Then _target = Trim(_target) & "\"

        Do

            _bw_sync_jobs = 0
            Array.Resize(bw_sync, 0)

            If _direction = 1 Or _direction = 0 Then
                Process_folder(_source, _target)
            End If

            If _direction = 2 Or _direction = 0 Then
                Process_folder(_target, _source)
            End If

            wait(sync)

        Loop

        Return

    End Sub

    Private Sub Process_folder(ByVal source As String, ByVal target As String)

        If FileSystem.DirectoryExists(source) Then

            On Error Resume Next

            For Each database As String In IO.Directory.GetFiles(source, "*.db3")

                If _databases.Contains(IO.Path.GetFileName(database)) Or _databases.Contains("ALL") Then
                    Process_tables(source, target, IO.Path.GetFileName(database))
                End If

            Next

            Do While _bw_active_jobs > 0
                wait(30, False)
            Loop

        End If

    End Sub

    Private Sub Process_tables(ByVal source As String, ByVal target As String, ByVal database As String)

        Dim parm As String = Nothing

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader

        If Not FileSystem.FileExists(source + database) Or Not FileSystem.FileExists(target + database) Then
            Exit Sub
        End If

        SQLconnect.ConnectionString = "Data Source=" + source + database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type=""table"""
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()

            parm += source & "|" & target & "|" & database & "|" & SQLreader(0) & "¬"

        End While

        SQLconnect.Close()

        If parm <> "" Then
            ReDim Preserve bw_sync(_bw_sync_jobs)
            bw_sync(_bw_sync_jobs) = New BackgroundWorker
            bw_sync(_bw_sync_jobs).WorkerSupportsCancellation = True
            AddHandler bw_sync(_bw_sync_jobs).DoWork, AddressOf bw_sync_worker

            If Not bw_sync(_bw_sync_jobs).IsBusy Then bw_sync(_bw_sync_jobs).RunWorkerAsync(parm)

            _bw_sync_jobs += 1
            _bw_active_jobs += 1
        End If

    End Sub

    Private Function LoadTable(ByVal path As String, ByVal database As String, ByVal table As String, Optional ByRef columns(,) As String = Nothing) As Array

        Dim x, y, records As Integer
        Dim data(,) As String = Nothing

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader

        columns = GetFields(path, database, table)
        records = RecordCount(path, database, table)

        SQLconnect.ConnectionString = "Data Source=" + path + database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT rowid, * FROM " & table
        SQLreader = SQLcommand.ExecuteReader()

        ReDim Preserve data(1, records - 1)

        While SQLreader.Read()

            data(0, x) = SQLreader(0)

            For y = 0 To UBound(columns, 2)
                If Not IsDBNull(SQLreader(y + 1)) Then
                    Select Case UCase(columns(1, y))
                        Case "INTEGER", "REAL", "BLOB"
                            data(1, x) &= SQLreader(y + 1).ToString & Chr(255)
                        Case Else
                            data(1, x) &= SQLreader(y + 1) & Chr(255)
                    End Select
                Else
                    data(1, x) &= columns(2, y) & Chr(255)
                End If
            Next

            x += 1

        End While

        SQLconnect.Close()

        Return data

    End Function

    Private Function GetFields(ByVal path As String, ByVal database As String, ByVal table As String) As Array

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim columns(,) As String = Nothing
        Dim x As Integer = 0

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA table_info (" & table & ")"
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()
            ReDim Preserve columns(3, x)
            columns(0, x) = SQLreader(1)
            columns(1, x) = SQLreader(2)
            If Not IsDBNull(SQLreader(4)) Then columns(2, x) = SQLreader(4).ToString.Replace("'", "")
            columns(3, x) = SQLreader(5)
            x += 1
        End While

        SQLconnect.Close()

        Return columns

    End Function

    Private Function RecordCount(ByVal path As String, ByVal database As String, ByVal table As String) As Integer

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim x As Integer = 0

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT COUNT(*) FROM " & table
        SQLreader = SQLcommand.ExecuteReader()
        SQLreader.Read()
        x = Int(SQLreader(0))
        SQLconnect.Close()

        Return x

    End Function

    Private Sub bw_sync_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm() As String = Split(e.Argument, "¬")
        Dim args(3) As String
        Dim x, bw_thread_jobs As Integer
        Dim bw_thread() As BackgroundWorker

        For x = 0 To parm.Count - 1

            If parm(x) <> "" Then

                If x = 0 Then
                    args = Split(parm(x), "|")
                    Log.Info("CDB_Sync: synchronization of " & args(2) & " database started.")
                End If

                ReDim Preserve bw_thread(bw_thread_jobs)
                bw_thread(bw_thread_jobs) = New BackgroundWorker
                bw_thread(bw_thread_jobs).WorkerSupportsCancellation = True
                AddHandler bw_thread(bw_thread_jobs).DoWork, AddressOf bw_thread_worker
                AddHandler bw_thread(bw_thread_jobs).RunWorkerCompleted, AddressOf bw_worker_completed

                If Not bw_thread(bw_thread_jobs).IsBusy Then bw_thread(bw_thread_jobs).RunWorkerAsync(parm(x))

                bw_thread_jobs += 1

            End If

        Next

        Dim busy As Boolean = True

        Do While busy

            For x = 0 To bw_thread_jobs - 1
                If bw_thread(x).IsBusy Then
                    busy = True
                    Exit For
                Else
                    busy = False
                End If
            Next

            wait(30, False)

        Loop

        Log.Info("CDB_Sync: synchronization of " & args(2) & " database completed.")

        _bw_active_jobs -= 1

    End Sub

    Private Sub bw_thread_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm As String = e.Argument
        Dim args(3) As String
        Dim columns(,), s_data(,), t_data(,) As String

        args = Split(parm, "|")

        s_data = LoadTable(args(0), args(2), args(3), columns)
        t_data = LoadTable(args(1), args(2), args(3))

        Synchronize(args(1), args(2), args(3), columns, s_data, t_data, _sync_method)

        e.Result = "CDB_Sync: synchronization of table " & args(3) & " in database " & args(2) & " complete."

    End Sub

    Private Sub bw_worker_completed(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        If e.Result <> Nothing Then
            Log.Info(e.Result)
        End If
    End Sub

    Private Sub Synchronize(ByVal path As String, ByVal database As String, ByVal table As String, ByVal columns(,) As String, ByRef s_data(,) As String, ByRef t_data(,) As String, ByVal method As Integer)

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim fields, values, a_values(), temp() As String
        Dim x, y As Integer

        On Error Resume Next

        Log.Info("CDB_Sync: synchronization of table " & table & " in database " & database & " in progress...")

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        fields = Nothing

        For x = 0 To UBound(columns, 2)
            fields &= columns(0, x) & ","
        Next

        fields = Left(fields, Len(fields) - 1)

        If method = 2 Then

            'Log.Info("CDB_Sync: deleting extra entries on target for table " & table & " in database " & database)

            ReDim temp(s_data.GetLength(1) - 1)
            Array.Copy(s_data.OfType(Of String)().ToArray(), s_data.GetLength(1), temp, 0, s_data.GetLength(1))
            Array.Sort(temp)

            For y = 0 To UBound(t_data, 2)
                If temp.Contains(t_data(1, y)) = False Then
                    SQLcommand.CommandText = "DELETE FROM " & table & " WHERE rowid = " & t_data(0, y)
                    SQLcommand.ExecuteNonQuery()
                    'Log.Info("CDB_Sync: record deleted from " & table)
                End If
            Next

            Array.Clear(temp, 0, temp.Length)

        End If

        If method = 0 Or method = 1 Then

            'Log.Info("CDB_Sync: adding missing entries on target for table " & table & " in database " & database)

            ReDim temp(t_data.GetLength(1) - 1)
            Array.Copy(t_data.OfType(Of String)().ToArray(), t_data.GetLength(1), temp, 0, t_data.GetLength(1))
            Array.Sort(temp)

            For y = 0 To UBound(s_data, 2)
                If temp.Contains(s_data(1, y)) = False Then

                    values = Nothing
                    a_values = Split(s_data(1, y), Chr(255))

                    For x = 0 To UBound(columns, 2)

                        Select Case columns(1, x)

                            Case "INTEGER", "REAL"
                                values &= a_values(x) & ","
                            Case Else
                                values &= """" & a_values(x) & ""","
                        End Select

                    Next

                    values = Left(values, Len(values) - 1)

                    SQLcommand.CommandText = "INSERT INTO " & table & " (" & fields & ") VALUES(" & values & ")"
                    SQLcommand.ExecuteNonQuery()
                    'Log.Info("CDB_Sync: record added in " & table)

                End If
            Next

            Array.Clear(temp, 0, temp.Length)

        End If

        SQLconnect.Close()

    End Sub

End Class
