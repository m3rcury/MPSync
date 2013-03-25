Imports MediaPortal.GUI.Library

Imports Microsoft.VisualBasic.FileIO
Imports System.ComponentModel

Imports System.Data.SQLite

Public Class MPSync_process_DB

    Dim _bw_active_db_jobs, bw_sync_db_jobs As Integer
    Dim bw_sync_db() As BackgroundWorker

    Public Shared Sub bw_db_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim cdb As New MPSync_process_DB

        Do

            cdb.bw_sync_db_jobs = 0
            Array.Resize(cdb.bw_sync_db, 0)

            If MPSync_process._db_direction = 1 Or MPSync_process._db_direction = 0 Then
                cdb.Process_DB_folder(MPSync_process._db_client, MPSync_process._db_server)
            End If

            If MPSync_process._db_direction = 2 Or MPSync_process._db_direction = 0 Then
                cdb.Process_DB_folder(MPSync_process._db_server, MPSync_process._db_client)
            End If

            If Not MPSync_settings.syncnow Then MPSync_process.wait(MPSync_process._db_sync) Else Exit Do

        Loop

    End Sub

    Private Sub Process_DB_folder(ByVal source As String, ByVal target As String)

        If FileSystem.DirectoryExists(source) And FileSystem.DirectoryExists(target) Then

            On Error Resume Next

            For Each database As String In IO.Directory.GetFiles(source, "*.db3")

                If MPSync_process._databases.Contains(IO.Path.GetFileName(database)) Or MPSync_process._databases.Contains("ALL") Then
                    Process_tables(source, target, IO.Path.GetFileName(database))
                End If

            Next

            Do While _bw_active_db_jobs > 0
                MPSync_process.wait(30, False)
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

            parm += source & Chr(254) & target & Chr(254) & database & Chr(254) & SQLreader(0) & "¬"

        End While

        SQLconnect.Close()

        If parm <> "" Then
            ReDim Preserve bw_sync_db(bw_sync_db_jobs)
            bw_sync_db(bw_sync_db_jobs) = New BackgroundWorker
            bw_sync_db(bw_sync_db_jobs).WorkerSupportsCancellation = True
            AddHandler bw_sync_db(bw_sync_db_jobs).DoWork, AddressOf bw_sync_db_worker

            If Not bw_sync_db(bw_sync_db_jobs).IsBusy Then bw_sync_db(bw_sync_db_jobs).RunWorkerAsync(parm)

            bw_sync_db_jobs += 1
            _bw_active_db_jobs += 1
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
                            data(1, x) &= SQLreader(y + 1).ToString & Chr(240) & Chr(240)
                        Case Else
                            data(1, x) &= SQLreader(y + 1) & Chr(240) & Chr(240)
                    End Select
                Else
                    data(1, x) &= "NULL" & Chr(240) & Chr(240)
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

    Private Sub bw_sync_db_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm() As String = Split(e.Argument, "¬")
        Dim args(3) As String
        Dim x, bw_db_thread_jobs As Integer
        Dim bw_db_thread() As BackgroundWorker

        For x = 0 To parm.Count - 1

            If parm(x) <> "" Then

                If x = 0 Then
                    args = Split(parm(x), Chr(254))
                    Log.Info("MPSync: synchronization of " & args(2) & " database started.")
                End If

                ReDim Preserve bw_db_thread(bw_db_thread_jobs)
                bw_db_thread(bw_db_thread_jobs) = New BackgroundWorker
                bw_db_thread(bw_db_thread_jobs).WorkerSupportsCancellation = True
                AddHandler bw_db_thread(bw_db_thread_jobs).DoWork, AddressOf bw_db_thread_worker
                AddHandler bw_db_thread(bw_db_thread_jobs).RunWorkerCompleted, AddressOf bw_worker_completed

                If Not bw_db_thread(bw_db_thread_jobs).IsBusy Then bw_db_thread(bw_db_thread_jobs).RunWorkerAsync(parm(x))

                bw_db_thread_jobs += 1

            End If

        Next

        Dim busy As Boolean = True

        Do While busy

            For x = 0 To bw_db_thread_jobs - 1
                If bw_db_thread(x).IsBusy Then
                    busy = True
                    Exit For
                Else
                    busy = False
                End If
            Next

            MPSync_process.wait(30, False)

        Loop

        Log.Info("MPSync: synchronization of " & args(2) & " database completed.")

        _bw_active_db_jobs -= 1

    End Sub

    Private Sub bw_db_thread_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm As String = e.Argument
        Dim args(3) As String
        Dim columns(,), s_data(,), t_data(,) As String

        args = Split(parm, Chr(254))

        s_data = LoadTable(args(0), args(2), args(3), columns)
        t_data = LoadTable(args(1), args(2), args(3))

        Synchronize_DB(args(1), args(2), args(3), columns, s_data, t_data, MPSync_process._db_sync_method)

        e.Result = "MPSync: synchronization of table " & args(3) & " in database " & args(2) & " complete."

    End Sub

    Private Sub bw_worker_completed(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        If e.Result <> Nothing Then
            Log.Info(e.Result)
        End If
    End Sub

    Private Sub Synchronize_DB(ByVal path As String, ByVal database As String, ByVal table As String, ByVal columns(,) As String, ByRef s_data(,) As String, ByRef t_data(,) As String, ByVal method As Integer)

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim fields, values, a_values(), s_temp(), t_temp() As String
        Dim diff As IEnumerable(Of String)
        Dim x, y As Integer

        Log.Info("MPSync: synchronization of table " & table & " in database " & database & " in progress...")

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        fields = Nothing

        For x = 0 To UBound(columns, 2)
            fields &= columns(0, x) & ","
        Next

        fields = Left(fields, Len(fields) - 1)

        SQLcommand.CommandText = "PRAGMA temp_store=2;PRAGMA journal_mode=memory;PRAGMA synchronous=0;"
        SQLcommand.ExecuteNonQuery()

        ReDim s_temp(s_data.GetLength(1) - 1)
        ReDim t_temp(t_data.GetLength(1) - 1)
        Array.Copy(s_data.OfType(Of String)().ToArray(), s_data.GetLength(1), s_temp, 0, s_data.GetLength(1))
        Array.Copy(t_data.OfType(Of String)().ToArray(), t_data.GetLength(1), t_temp, 0, t_data.GetLength(1))

        If method <> 1 Then

            'Log.Info("MPSync: deleting extra entries on target for table " & table & " in database " & database)

            diff = t_temp.Except(s_temp)

            For y = 0 To UBound(diff.ToArray)
                MPSync_process.CheckPlayerplaying("db", 300)
                SQLcommand.CommandText = "DELETE FROM " & table & " WHERE rowid = " & t_data(0, Array.IndexOf(t_temp, diff.ToArray(y)))
                SQLcommand.ExecuteNonQuery()
            Next

            Log.Info("MPSync: " & y.ToString & " record deleted from " & table)

        End If

        If method <> 2 Then

            'Log.Info("MPSync: adding missing entries on target for table " & table & " in database " & database)

            diff = s_temp.Except(t_temp)

            For y = 0 To UBound(diff.ToArray)

                MPSync_process.CheckPlayerplaying("db", 300)

                values = Nothing
                a_values = Split(diff.ToArray(y), Chr(240) & Chr(240))

                For x = 0 To UBound(columns, 2)

                    If a_values(x) = "NULL" Then
                        values &= "NULL,"
                    Else
                        Select Case columns(1, x)

                            Case "INTEGER", "REAL"
                                values &= a_values(x) & ","
                            Case "BOOL"
                                If a_values(x) = "True" Then values &= """1""," Else values &= """0"","
                            Case Else
                                values &= """" & a_values(x).Replace("""", """""") & ""","

                        End Select
                    End If

                Next

                values = Left(values, Len(values) - 1)

                SQLcommand.CommandText = "INSERT INTO " & table & " (" & fields & ") VALUES(" & values & ")"
                SQLcommand.ExecuteNonQuery()

            Next

            Log.Info("MPSync: " & y.ToString & " records added in " & table)

        End If

        SQLconnect.Close()

    End Sub

End Class
