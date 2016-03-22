Imports System.IO
Imports System.Threading
Imports System.Data.SQLite

Public Class MPSync_process_DB_watch

    Private mps As New MPSync_settings
    Private mps_DB As New MPSync_process_DB

    Dim dlm As String = Chr(7) & "~" & Chr(30)
    Dim watchfolder As FileSystemWatcher
    Dim s_path As String = Nothing
    Dim t_path As String = Nothing
    Dim w_master As Boolean = MPSync_process._db_direction <> 2

    Public Shared Sub bw_DB_watch_folder(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim mps_db_watch As New MPSync_process_DB_watch

        If mps_db_watch.w_master Then
            mps_db_watch.s_path = MPSync_process._db_client
            mps_db_watch.t_path = MPSync_process._db_server
        Else
            mps_db_watch.s_path = MPSync_process._db_server
            mps_db_watch.t_path = MPSync_process._db_client
        End If

        mps_db_watch.mps.SetWatched = Nothing

        mps_db_watch.watchfolder = New System.IO.FileSystemWatcher()

        MPSync_process.logStats("MPSync: [MPSync_process.WorkMethod][bw_DB_watch_folder] watch synchronization starting for " & mps_db_watch.s_path, "LOG")

        'this is the path we want to monitor
        mps_db_watch.watchfolder.Path = mps_db_watch.t_path
        mps_db_watch.watchfolder.IncludeSubdirectories = False

        'Add a list of Filter we want to specify
        mps_db_watch.watchfolder.NotifyFilter = NotifyFilters.LastWrite

        ' add the handler to each event
        AddHandler mps_db_watch.watchfolder.Changed, AddressOf mps_db_watch.DBfileChange

        'Set this property to true to start watching
        mps_db_watch.watchfolder.EnableRaisingEvents = True

        Dim autoEvent As New AutoResetEvent(False)
        autoEvent.WaitOne()

    End Sub

    Private Sub DBfileChange(ByVal source As Object, ByVal e As System.IO.FileSystemEventArgs)

        Try
            Dim x As Integer = Array.IndexOf(mps.getDatabase, FileIO.FileSystem.GetName(e.FullPath))

            If x <> -1 Then

                Dim database As String = FileIO.FileSystem.GetName(e.FullPath)

                If Array.IndexOf(mps.getDatabase, database) <> -1 Then

                    Dim tables As Array = mps.getTables(database)

                    For x = 0 To UBound(tables)

                        If MPSync_process._db_direction <> 2 Then
                            UpdateMaster(s_path, t_path, database, tables(x))
                        Else
                            UpdateSlave(t_path, s_path, database, tables(x))
                        End If

                    Next

                End If

            End If

        Catch ex As Exception
            MPSync_process.logStats("MPSync: Watch synchronization failed on " & e.FullPath & " with exception: " & ex.Message, "ERROR")
        End Try

    End Sub

    Private Function BuildUpdateArray_mpsync(ByVal w_values(,) As String, ByVal s_data As Array, ByVal mps_columns As Array, ByVal columns As Array) As Array

        'MPSync_process.logStats("MPSync: [BuildUpdateArray_mpsync]", "DEBUG")

        Dim x, z As Integer
        Dim w_pk(), s_pk() As String

        If w_values.OfType(Of String)().ToArray().Length = 0 Then Return s_data

        w_pk = mps_DB.getPkValues(w_values, mps_columns, columns)

        If s_data.OfType(Of String)().ToArray().Length > 0 Then

            s_pk = mps_DB.getPkValues(s_data, mps_columns, columns)

            x = Array.IndexOf(mps_columns.OfType(Of String)().ToArray(), "mps_lastupdated")

            For y As Integer = 0 To (s_data.GetLength(1) - 1)

                z = w_pk.Contains(s_pk(y))

                If z <> -1 Then

                    If mps_DB.getLastUpdateDate(s_data(1, y), x) > mps_DB.getLastUpdateDate(w_values(1, z), x) Then
                        w_values(1, z) = s_data(1, y)
                    End If

                End If

            Next

        End If

        Return w_values

    End Function

    Private Sub UpdateSlave(ByVal source As String, ByVal target As String, ByVal database As String, ByVal table As String)

        MPSync_process.logStats("MPSync: [UpdateSlave] synchronization of mpsync for table " & table & " in database " & source & database & " in progress...", "LOG")

        Dim columns As Array = mps_DB.getFields(source, database, "mpsync")

        If mps_DB.Synchronize_DB(source, target, database, "~mpsync~", columns, 1) Then UpdateMaster(source, target, database, table)

        MPSync_process.logStats("MPSync: [UpdateSlave] synchronization of mpsync for table " & table & " in database " & source & database & " complete.", "LOG")

    End Sub

    Private Sub UpdateMaster(ByVal source As String, ByVal target As String, ByVal database As String, ByVal table As String)

        MPSync_process.logStats("MPSync: [UpdateMaster] synchronization of watched for table " & table & " in database " & source & database & " in progress...", "LOG")

        Dim mps_columns As Array = Nothing
        Dim columns, s_data, t_data, w_values As Array

        s_data = mps_DB.LoadTable(source, database, "mpsync", mps_columns, "tablename = '" & table & "'", "mps_lastupdated")
        t_data = mps_DB.LoadTable(target, database, "mpsync", mps_columns, "tablename = '" & table & "'", "mps_lastupdated")

        If s_data Is Nothing And t_data Is Nothing Then
            MPSync_process.logStats("MPSync: [UpdateMaster] synchronization of watched for table " & table & " in database " & source & database & " nothing to update.", "LOG")
            Exit Sub
        End If

        columns = mps_DB.getFields(source, database, table)
        w_values = BuildUpdateArray_mpsync(t_data, s_data, columns, columns)

        If w_master Then
            UpdateRecords_mpsync(source, database, table, w_values, mps_columns, columns)
            Cleanup_mpsync(target, database, t_data)
        Else
            UpdateRecords_mpsync(target, database, table, w_values, mps_columns, columns)
            Cleanup_mpsync(source, database, s_data)
        End If

        MPSync_process.logStats("MPSync: [UpdateMaster] synchronization of watched for table " & table & " in database " & source & database & " complete.", "LOG")

    End Sub

    Private Sub UpdateRecords_mpsync(ByVal path As String, ByVal database As String, ByVal table As String, ByVal w_values As Array, ByVal table_columns As Array, ByVal columns As Array)

        If w_values.OfType(Of String)().ToArray().Length = 0 Then Exit Sub

        Dim i, x, z As Integer
        Dim pkey As String = Nothing
        Dim updcols As Array = mps_DB.getArray(table_columns, 0)
        Dim fields, updvalues, where, update(), a_values() As String
        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand = SQLconnect.CreateCommand

        x = mps_DB.getPK(columns, pkey)

        SQLconnect.ConnectionString = "Data Source=" & MPSync_process.p_Database(path & database)
        SQLconnect.Open()
        SQLcommand.CommandText = "PRAGMA temp_store=2;PRAGMA journal_mode=off;PRAGMA synchronous=off;"
        SQLcommand.ExecuteNonQuery()

        For y As Integer = 0 To UBound(w_values, 2)

            i = 0
            fields = Nothing
            update = Nothing
            where = Nothing
            a_values = Split(w_values(1, y), dlm)

            For x = 0 To UBound(columns, 2)
                z = Array.IndexOf(updcols, columns(0, x))
                If z <> -1 Then
                    If columns(0, x) <> pkey Then
                        ReDim Preserve update(i)
                        update(i) = columns(0, x) & "=" & mps_DB.FormatValue(a_values(z), columns(1, x))
                        fields &= columns(0, x) & ","
                        i += 1
                    Else
                        where = pkey & " = " & mps_DB.FormatValue(a_values(z), columns(1, x))
                    End If
                End If
            Next

            fields = Left(fields, Len(fields) - 1)
            where = Left(where, Len(where) - 1)

            ' get update values from table and compare if anything changed
            Dim curvalues() As String

            curvalues = mps_DB.getCurrentTableValues(path, database, table, columns, updcols, pkey, fields, where)

            If curvalues IsNot Nothing Then

                ' construct update clause
                updvalues = mps_DB.getUpdateValues(update, curvalues)

                If updvalues <> Nothing Then

                    Try
                        MPSync_process.logStats("MPSync: [UpdateRecords_mpsync] UPDATE " & table & " SET " & updvalues & " WHERE " & where, "DEBUG")
                        SQLcommand.CommandText = "UPDATE " & table & " SET " & updvalues & " WHERE " & where
                        SQLcommand.ExecuteNonQuery()
                    Catch ex As Exception
                        MPSync_process.logStats("MPSync: [UpdateRecords_mpsync] SQL statement [" & (SQLcommand.CommandText).Replace("""", "'") & "] on " & path & database & " failed with exception: " & ex.Message, "DEBUG")
                        MPSync_process.logStats("MPSync: [UpdateRecords_mpsync] Error synchronizing table " & table & " in database " & path & database, "ERROR")
                    End Try

                End If
            End If

        Next

        SQLconnect.Close()

    End Sub

    Private Sub Cleanup_mpsync(ByVal path As String, ByVal database As String, ByVal data As Array)

        If data.OfType(Of String)().ToArray().Length = 0 Or data(0, 0) = Nothing Then Exit Sub

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand = SQLconnect.CreateCommand

        SQLconnect.ConnectionString = "Data Source=" & MPSync_process.p_Database(path & database)
        SQLconnect.Open()

        Try
            For y = 0 To UBound(data, 2)
                MPSync_process.logStats("MPSync: [Cleanup_mpsync] DELETE FROM mpsync WHERE rowid = " & data(0, y), "DEBUG")
                SQLcommand.CommandText = "DELETE FROM mpsync WHERE rowid = " & data(0, y)
                SQLcommand.ExecuteNonQuery()
            Next
        Catch ex As Exception
            MPSync_process.logStats("MPSync: [Cleanup_mpsync] SQL statement [" & (SQLcommand.CommandText).Replace("""", "'") & "] failed with exception: " & ex.Message, "DEBUG")
            MPSync_process.logStats("MPSync: [Cleanup_mpsync] Error deleting record from table mpsync in database " & path & database & " with exception: " & ex.Message, "ERROR")
        End Try

        SQLconnect.Close()

    End Sub

End Class
