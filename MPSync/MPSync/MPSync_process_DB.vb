Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports System.ComponentModel
Imports System.Data.SQLite

Public Class MPSync_process_DB

    Dim debug As Boolean
    Dim _bw_active_db_jobs, bw_sync_db_jobs As Integer
    Dim bw_sync_db() As BackgroundWorker

    Private Function LoadTable(ByVal path As String, ByVal database As String, ByVal table As String, Optional ByRef columns As Array = Nothing, Optional ByVal where As String = Nothing, Optional ByVal order As String = Nothing) As Array

        Dim x, y, z, records As Integer
        Dim data(,) As String = Nothing

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader = Nothing

        Try

            columns = getFields(path, database, table)
            records = RecordCount(path, database, table, where)
            z = getPK(columns)

            If records > 0 Then

                SQLconnect.ConnectionString = "Data Source=" + path + database
                SQLconnect.Open()
                SQLcommand = SQLconnect.CreateCommand

                SQLcommand.CommandText = "SELECT rowid, * FROM " & table

                If where <> Nothing Then
                    SQLcommand.CommandText &= " WHERE " & where
                End If

                If order <> Nothing Then
                    SQLcommand.CommandText &= " ORDER BY " & order
                End If

                SQLreader = SQLcommand.ExecuteReader()

                ReDim Preserve data(2, records - 1)

                While SQLreader.Read()

                    data(0, x) = SQLreader(0)

                    For y = 0 To UBound(columns, 2)
                        If Not IsDBNull(SQLreader(y + 1)) Then
                            Select Case columns(1, y)
                                Case "INTEGER", "REAL", "BLOB"
                                    data(1, x) &= SQLreader(y + 1).ToString & Chr(240) & Chr(240)
                                    If y = z Then data(2, x) = SQLreader(y + 1).ToString
                                Case "TIMESTAMP"
                                    data(1, x) &= Format(SQLreader(y + 1), "yyyy-MM-dd HH:mm:ss") & Chr(240) & Chr(240)
                                    If y = z Then data(2, x) = Format(SQLreader(y + 1), "yyyy-MM-dd HH:mm:ss")
                                Case Else
                                    data(1, x) &= SQLreader(y + 1) & Chr(240) & Chr(240)
                                    If y = z Then data(2, x) = SQLreader(y + 1)
                            End Select
                        Else
                            data(1, x) &= "NULL" & Chr(240) & Chr(240)
                            If y = z Then data(2, x) = "NULL"
                        End If
                    Next

                    data(1, x) = data(1, x).Replace("'", "''")
                    x += 1

                End While

                SQLconnect.Close()

            Else
                ReDim data(2, 0)
            End If

        Catch ex As Exception
            If MPSync_process.p_Debug Then Log.Debug("MPSync: Error reading table " & table & " rowid """ & SQLreader(0) & """ in " & database & " with exception: " & ex.Message)
            Log.Error("MPSync: Error reading data from table " & table & " in database " & database)

            data = Nothing

        End Try

        Return data

    End Function

    Private Function getFields(ByVal path As String, ByVal database As String, ByVal table As String) As Array

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
            columns(0, x) = LCase(SQLreader(1))
            columns(1, x) = UCase(SQLreader(2))
            If Not IsDBNull(SQLreader(4)) Then columns(2, x) = SQLreader(4).ToString.Replace("'", "")
            columns(3, x) = SQLreader(5)
            x += 1
        End While

        SQLconnect.Close()

        Return columns

    End Function

    Private Function RecordCount(ByVal path As String, ByVal database As String, ByVal table As String, Optional ByVal where As String = Nothing) As Integer

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim x As Integer = 0

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        If where = Nothing Then
            SQLcommand.CommandText = "SELECT COUNT(*) FROM " & table
        Else
            SQLcommand.CommandText = "SELECT COUNT(*) FROM " & table & " WHERE " & where
        End If

        SQLreader = SQLcommand.ExecuteReader()
        SQLreader.Read()
        x = Int(SQLreader(0))
        SQLconnect.Close()

        Return x

    End Function

    Private Function FormatValue(ByVal value As String, ByVal type As String) As String

        Dim fmtvalue As String

        If value = "NULL" Then
            fmtvalue = "NULL,"
        Else
            Select Case type

                Case "INTEGER", "REAL"
                    fmtvalue = value & ","
                Case "BOOL"
                    If value = "True" Then fmtvalue = "'1'," Else fmtvalue = "'0',"
                Case Else
                    fmtvalue = "'" & value & "',"

            End Select
        End If

        Return fmtvalue

    End Function

    Private Function BuildUpdateArray(ByVal w_values(,) As String, ByVal s_data As Array, ByVal mps_columns As Array, ByVal columns As Array) As Array

        Dim x, z As Integer
        Dim w_pk(), s_pk() As String

        If w_values.OfType(Of String)().ToArray().Length = 0 Then Return s_data

        w_pk = getPkValues(w_values, mps_columns, columns)

        If s_data.Length > 0 Then

            s_pk = getPkValues(s_data, mps_columns, columns)

            x = Array.IndexOf(mps_columns.OfType(Of String)().ToArray(), "mps_lastupdated")

            For y As Integer = 0 To (s_data.GetLength(1) - 1)

                z = w_pk.Contains(s_pk(y))

                If z <> -1 Then

                    If getLastUpdateDate(s_data(1, y), x) > getLastUpdateDate(w_values(1, z), x) Then
                        w_values(1, z) = s_data(1, y)
                    End If

                End If

            Next

        End If

        Return w_values

    End Function

    Private Function getPkValues(ByVal values As Array, ByVal mps_columns As Array, ByVal columns As Array) As Array

        Dim x, y As Integer
        Dim temp2() As String
        Dim PKs() As String = Nothing

        x = getPK(columns)

        Dim mps_cols As Array = getArray(mps_columns, 0)

        x = Array.IndexOf(mps_cols, columns(0, x))

        Dim temp1 As Array = getArray(values, 1)

        If temp1(0) IsNot Nothing Then
            For y = 0 To UBound(temp1)
                temp2 = Split(temp1(y), Chr(240) & Chr(240))
                ReDim Preserve PKs(y)
                PKs(y) = temp2(x)
            Next
        Else
            Return Nothing
        End If

        Return PKs

    End Function

    Private Function getPK(ByVal columns As Array, Optional ByRef pkey As String = Nothing) As Integer

        Dim x As Integer
        Dim pk As Array = getArray(columns, 3)

        x = Array.IndexOf(pk, "1")

        If x <> -1 Then
            pkey = columns(0, x)
            Return x
        Else
            pkey = Nothing
            Return -1
        End If

    End Function

    Private Function getLastUpdateDate(ByVal values As String, ByVal index As Integer) As String

        Dim a_values() As String = Split(values, Chr(240) & Chr(240))

        Return a_values(index)

    End Function

    Private Function getArray(ByVal array As Array, ByVal dimension As Integer) As Array

        If array Is Nothing Then Return Nothing

        Dim newarray(0) As String

        Try
            For x As Integer = 0 To UBound(array, 2)
                ReDim Preserve newarray(x)
                newarray(x) = array(dimension, x)
            Next
        Catch ex As Exception
            Return Nothing
        End Try

        Return newarray

    End Function

    Private Function getCurrentTableValues(ByVal path As String, ByVal database As String, ByVal table As String, ByVal columns As Array, ByVal mps_cols As Array, ByVal pkey As String, ByVal fields As String, ByVal where As String) As Array

        Dim records As Integer = RecordCount(path, database, table, where)

        If records > 0 Then

            Dim SQLconnect As New SQLiteConnection()
            Dim SQLcommand As SQLiteCommand
            Dim SQLreader As SQLiteDataReader

            SQLconnect.ConnectionString = "Data Source=" & path & database
            SQLcommand = SQLconnect.CreateCommand
            SQLconnect.Open()
            SQLcommand.CommandText = "SELECT " & fields & " FROM " & table & " WHERE " & where
            SQLreader = SQLcommand.ExecuteReader()
            SQLreader.Read()

            Dim i, x, z As Integer
            Dim curvalues() As String

            curvalues = Nothing

            For x = 0 To UBound(columns, 2)
                z = Array.IndexOf(mps_cols, columns(0, x))
                If z <> -1 Then
                    If columns(0, x) <> pkey Then
                        ReDim Preserve curvalues(i)
                        If Not IsDBNull(SQLreader(i)) Then
                            curvalues(i) = columns(0, x) & "=" & FormatValue(SQLreader(i), columns(1, x))
                        Else
                            curvalues(i) = columns(0, x) & "=" & FormatValue("NULL", columns(1, x))
                        End If
                        i += 1
                    End If
                End If
            Next

            SQLconnect.Close()

            Return curvalues

        Else
            Return Nothing
        End If

    End Function

    Private Function getUpdateValues(ByVal newvalues As Array, ByVal curvalues As Array) As String

        Dim updvalues As String = Nothing

        For x As Integer = 0 To UBound(newvalues)
            If newvalues(x) <> curvalues(x) Then
                updvalues &= newvalues(x)
            End If
        Next

        If updvalues <> Nothing Then updvalues = Left(updvalues, Len(updvalues) - 1)

        Return updvalues

    End Function

    Public Shared Sub bw_db_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim cdb As New MPSync_process_DB

        Do

            cdb.bw_sync_db_jobs = 0
            Array.Resize(cdb.bw_sync_db, 0)

            ' direction is client to server or both
            If MPSync_process._db_direction <> 2 Then
                cdb.Process_DB_folder(MPSync_process._db_client, MPSync_process._db_server)
            End If

            ' direction is server to client or both
            If MPSync_process._db_direction <> 1 Then
                cdb.Process_DB_folder(MPSync_process._db_server, MPSync_process._db_client)
            End If

            If Not MPSync_settings.syncnow Then
                MPSync_process.wait(MPSync_process._db_sync)
            Else
                MPSync_process.db_complete = True
                Exit Do
            End If

        Loop

    End Sub

    Private Sub Process_DB_folder(ByVal source As String, ByVal target As String)

        If Not IO.Directory.Exists(source) Then
            Log.Error("MPSync: folder " & source & " does not exist")
            Exit Sub
        End If

        If MPSync_process.p_Debug Then Log.Debug("MPSync: synchronizing from " & source & " to " & target)

        If Not IO.Directory.Exists(target) Then IO.Directory.CreateDirectory(target)

        On Error Resume Next

        For Each database As String In IO.Directory.GetFiles(source, "*.db3")

            Dim db As String = IO.Path.GetFileName(database)

            If MPSync_process._databases.Contains(db) Or MPSync_process._databases.Contains("ALL") Then

                If IO.File.Exists(target & db) Then
                    Process_tables(source, target, db)
                Else
                    IO.File.Copy(database, target & db, True)
                End If

            End If

        Next

        Do While _bw_active_db_jobs > 0
            MPSync_process.wait(30, False)
        Loop

    End Sub

    Private Sub Process_tables(ByVal source As String, ByVal target As String, ByVal database As String)

        Dim parm As String = Nothing

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader

        SQLconnect.ConnectionString = "Data Source=" + source + database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type=""table"""
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()

            If SQLreader(0) <> "mpsync" Then parm += source & Chr(254) & target & Chr(254) & database & Chr(254) & SQLreader(0) & "¬"

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

                If MPSync_settings.syncnow Then
                    bw_db_thread(bw_db_thread_jobs).WorkerReportsProgress = True
                    AddHandler bw_db_thread(bw_db_thread_jobs).ProgressChanged, AddressOf bw_worker_progresschanged
                End If

                If Not bw_db_thread(bw_db_thread_jobs).IsBusy Then
                    bw_db_thread(bw_db_thread_jobs).RunWorkerAsync(parm(x))
                    If MPSync_settings.syncnow Then
                        bw_db_thread(bw_db_thread_jobs).ReportProgress(0, "synchronization of table " & args(3) & " in database " & args(2) & " in progress.")
                    End If
                End If

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
        Dim columns(,) As String = Nothing
        Dim s_data(,), t_data(,) As String

        args = Split(parm, Chr(254))

        s_data = LoadTable(args(0), args(2), args(3), columns)
        t_data = LoadTable(args(1), args(2), args(3))

        ' check if master client
        If MPSync_process._db_direction <> 2 Then
            Update_Status(args(0), args(1), args(2), args(3))
        Else
            Update_mpsync(args(0), args(1), args(2), args(3))
        End If

        Synchronize_DB(args(1), args(2), args(3), columns, s_data, t_data, MPSync_process._db_sync_method)

        e.Result = "MPSync: synchronization of table " & args(3) & " in database " & args(2) & " complete."

    End Sub

    Private Sub bw_worker_progresschanged(ByVal sender As System.Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs)
        'mps.lb_statusItemsAdd(e.UserState.ToString)
    End Sub

    Private Sub bw_worker_completed(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        On Error Resume Next
        If e.Result <> Nothing Then
            Log.Info(e.Result)
        End If
    End Sub

    Private Sub Synchronize_DB(ByVal path As String, ByVal database As String, ByVal table As String, ByVal columns As Array, ByRef s_data As Array, ByRef t_data As Array, ByVal method As Integer)

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim fields, values, a_values(), s_temp(), t_temp() As String
        Dim diff As IEnumerable(Of String)
        Dim x, y, pk As Integer

        Log.Info("MPSync: synchronization of table " & table & " in database " & path & database & " in progress...")

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA temp_store=2;PRAGMA journal_mode=memory;PRAGMA synchronous=0;"
        SQLcommand.ExecuteNonQuery()

        fields = Nothing

        For x = 0 To UBound(columns, 2)
            fields &= columns(0, x) & ","
        Next

        fields = Left(fields, Len(fields) - 1)

        pk = getPK(columns)

        If pk <> -1 Then
            s_temp = getArray(s_data, 2)
            t_temp = getArray(t_data, 2)
        Else
            s_temp = getArray(s_data, 1)
            t_temp = getArray(t_data, 1)
        End If

        ' propagate deletions
        If method <> 1 And table <> "mpsync" Then

            If MPSync_process.p_Debug Then Log.Debug("MPSync: deleting extra entries on target for table " & table & " in database " & path & database)

            diff = t_temp.Except(s_temp)

            For y = 0 To UBound(diff.ToArray)
                MPSync_process.CheckPlayerplaying("db", 30)
                Try
                    If MPSync_process.p_Debug Then Log.Debug("MPSync: DELETE FROM " & table & " WHERE rowid = " & t_data(0, Array.IndexOf(t_temp, diff.ToArray(y))))
                    SQLcommand.CommandText = "DELETE FROM " & table & " WHERE rowid = " & t_data(0, Array.IndexOf(t_temp, diff.ToArray(y)))
                    SQLcommand.ExecuteNonQuery()
                Catch ex As Exception
                    If MPSync_process.p_Debug Then Log.Debug("MPSync: Error using SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ in " & path & database & " with exception: " & ex.Message)
                    Log.Error("MPSync: Error deleting record from table " & table & " in database " & path & database)
                End Try
            Next

            If MPSync_process.p_Debug Then Log.Debug("MPSync: " & y.ToString & " record deleted from " & table & " in database " & path & database)

        End If

        ' propagate additions
        If method <> 2 Then

            If MPSync_process.p_Debug Then Log.Debug("MPSync: adding missing entries on target for table " & table & " in database " & path & database)

            If pk <> -1 Then
                s_temp = getArray(s_data, 1)
                t_temp = getArray(t_data, 1)
            End If

            If s_temp(0) <> Nothing Then

                diff = s_temp.Except(t_temp)

                For y = 0 To UBound(diff.ToArray)

                    MPSync_process.CheckPlayerplaying("db", 30)

                    values = Nothing
                    a_values = Split(diff.ToArray(y), Chr(240) & Chr(240))

                    For x = 0 To UBound(columns, 2)
                        values &= FormatValue(a_values(x), columns(1, x))
                    Next

                    values = Left(values, Len(values) - 1)

                    Try
                        If MPSync_process.p_Debug Then Log.Debug("MPSync: INSERT OR REPLACE INTO " & table & " (" & fields & ") VALUES(" & values & ")")
                        SQLcommand.CommandText = "INSERT OR REPLACE INTO " & table & " (" & fields & ") VALUES(" & values & ")"
                        SQLcommand.ExecuteNonQuery()
                    Catch ex As Exception
                        If MPSync_process.p_Debug Then Log.Debug("MPSync: Error using SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ in " & path & database & " with exception: " & ex.Message)
                        Log.Error("MPSync: Error adding record to table " & table & " in database " & path & database)
                    End Try

                Next

            End If

            If MPSync_process.p_Debug Then Log.Debug("MPSync: " & y.ToString & " records added in " & table & " in database " & path & database)

        End If

        SQLconnect.Close()

    End Sub

    Private Sub Update_Status(ByVal source As String, ByVal target As String, ByVal database As String, ByVal table As String)

        Dim mps As New MPSync_settings

        Dim x As Integer = Array.IndexOf(mps.getDatabase, database)

        If x <> -1 Then
            If Array.IndexOf(mps.getTables(database), table) <> -1 Then

                Log.Info("MPSync: synchronization of watched for table " & table & " in database " & source & database & " in progress...")

                Dim mps_columns As Array = Nothing
                Dim columns, s_data, t_data, w_values As Array

                s_data = LoadTable(source, database, "mpsync", mps_columns, "tablename = '" & table & "'", "mps_lastupdated")
                t_data = LoadTable(target, database, "mpsync", , "tablename = '" & table & "'", "mps_lastupdated")

                If s_data Is Nothing And t_data Is Nothing Then
                    Log.Info("MPSync: synchronization of watched for table " & table & " in database " & source & database & " nothing to update.")
                    Exit Sub
                End If

                columns = getFields(source, database, table)

                w_values = BuildUpdateArray(t_data, s_data, mps_columns, columns)

                Dim mps_cols As Array = getArray(mps_columns, 0)

                UpdateTable(source, database, table, w_values, columns, mps_cols)

                ' cleanup mpsync on local and server since client is master
                Cleanup_mpsync(source, database, s_data)
                Cleanup_mpsync(target, database, t_data)

                Log.Info("MPSync: synchronization of watched for table " & table & " in database " & source & database & " complete.")

            End If
        End If

    End Sub

    Private Sub UpdateTable(ByVal path As String, ByVal database As String, ByVal table As String, ByVal w_values As Array, ByVal columns As Array, ByVal updcols As Array)

        Dim i, x, z As Integer
        Dim pkey As String = Nothing
        Dim fields, updvalues, where, update(), a_values() As String
        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        If w_values.OfType(Of String)().ToArray().Length = 0 Then Exit Sub

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA temp_store=2;PRAGMA journal_mode=memory;PRAGMA synchronous=0;"
        SQLcommand.ExecuteNonQuery()

        x = getPK(columns, pkey)

        For y As Integer = 0 To UBound(w_values, 2)

            MPSync_process.CheckPlayerplaying("db", 30)

            i = 0
            fields = Nothing
            update = Nothing
            where = Nothing
            a_values = Split(w_values(1, y), Chr(240) & Chr(240))

            For x = 0 To UBound(columns, 2)
                z = Array.IndexOf(updcols, columns(0, x))
                If z <> -1 Then
                    If columns(0, x) <> pkey Then
                        ReDim Preserve update(i)
                        update(i) = columns(0, x) & "=" & FormatValue(a_values(z), columns(1, x))
                        fields &= columns(0, x) & ","
                        i += 1
                    Else
                        where = pkey & " = " & FormatValue(a_values(z), columns(1, x))
                    End If
                End If
            Next

            fields = Left(fields, Len(fields) - 1)
            where = Left(where, Len(where) - 1)

            ' get update values from table and compare if anything changed
            Dim curvalues() As String

            curvalues = getCurrentTableValues(path, database, table, columns, updcols, pkey, fields, where)

            If curvalues IsNot Nothing Then

                ' construct update clause
                updvalues = getUpdateValues(update, curvalues)

                If updvalues <> Nothing Then

                    Try
                        If MPSync_process.p_Debug Then Log.Debug("MPSync: UPDATE " & table & " SET " & updvalues & " WHERE " & where)
                        SQLcommand.CommandText = "UPDATE " & table & " SET " & updvalues & " WHERE " & where
                        SQLcommand.ExecuteNonQuery()
                    Catch ex As Exception
                        If MPSync_process.p_Debug Then Log.Debug("MPSync: Error using SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ in " & path & database & " with exception: " & ex.Message)
                        Log.Error("MPSync: Error synchronizing table " & table & " in database " & path & database)
                    End Try

                End If
            End If
        Next

        SQLconnect.Close()

    End Sub

    Private Sub Update_mpsync(ByVal source As String, ByVal target As String, ByVal database As String, ByVal table As String)

        Dim mps As New MPSync_settings

        Dim x As Integer = Array.IndexOf(mps.getDatabase, database)

        If x <> -1 Then
            If Array.IndexOf(mps.getTables(database), table) <> -1 Then

                Log.Info("MPSync: synchronization of mpsync for table " & table & " in database " & source & database & " in progress...")

                Dim columns As Array = Nothing
                Dim s_data, t_data As Array

                s_data = LoadTable(target, database, "mpsync", columns, "tablename = '" & table & "'", "mps_lastupdated")
                t_data = LoadTable(source, database, "mpsync", , "tablename = '" & table & "'", "mps_lastupdated")

                Synchronize_DB(source, database, "mpsync", columns, s_data, t_data, 1)

                ' cleanup mpsync on client
                Cleanup_mpsync(target, database, s_data)

                Log.Info("MPSync: synchronization of mpsync for table " & table & " in database " & source & database & " complete.")

            End If
        End If

    End Sub

    Private Sub Cleanup_mpsync(ByVal path As String, ByVal database As String, ByVal data As Array)

        If data.OfType(Of String)().ToArray().Length > 0 Then

            Dim SQLconnect As New SQLiteConnection()
            Dim SQLcommand As SQLiteCommand

            SQLconnect.ConnectionString = "Data Source=" & path & database
            SQLconnect.Open()
            SQLcommand = SQLconnect.CreateCommand

            For y = 0 To UBound(data, 2)

                Try
                    If MPSync_process.p_Debug Then Log.Debug("MPSync: DELETE FROM mpsync WHERE rowid = " & data(0, y))
                    SQLcommand.CommandText = "DELETE FROM mpsync WHERE rowid = " & data(0, y)
                    SQLcommand.ExecuteNonQuery()
                Catch ex As Exception
                    If MPSync_process.p_Debug Then Log.Debug("MPSync: Error using SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ with exception: " & ex.Data.ToString)
                    Log.Error("MPSync: Error deleting record from table mpsync in database " & path & database)
                End Try

            Next

            SQLconnect.Close()

        End If

    End Sub

End Class
