Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports System.ComponentModel
Imports System.Data.SQLite

Public Class MPSync_process_DB

    Dim debug As Boolean
    Dim lastsync As String
    Dim dlm As String = Chr(7) & "~" & Chr(30)
    Dim checkplayer As Integer = 5
    Dim _bw_active_db_jobs, bw_sync_db_jobs As Integer
    Dim bw_sync_db() As BackgroundWorker

    Private ReadOnly Property p_Session As String
        Get
            Dim session As String
            Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))
                session = XMLreader.GetValueAsString("Plugin", "session ID", Nothing)
            End Using
            Return session
        End Get
    End Property

    Private Property p_lastsync As String
        Get
            Dim lastsync As String
            Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))
                lastsync = XMLreader.GetValueAsString("Plugin", "last sync", "0001-01-01 00:00:00")
            End Using
            Return lastsync
        End Get
        Set(value As String)
            Using XMLwriter As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))
                XMLwriter.SetValue("Plugin", "last sync", value)
            End Using
            MediaPortal.Profile.Settings.SaveCache()
        End Set
    End Property

    Private Function LoadTable(ByVal path As String, ByVal database As String, ByVal table As String, Optional ByRef columns As Array = Nothing, Optional ByVal where As String = Nothing, Optional ByVal order As String = Nothing) As Array

        Dim x, y, z, records As Integer
        Dim fields As String = "*"
        Dim data(,) As String = Nothing

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader = Nothing

        Try

            If columns Is Nothing Then
                columns = getFields(path, database, table)
            Else
                fields = getSelectFields(columns)
            End If

            z = getPK(columns)
            records = RecordCount(path, database, table, where)

            SQLconnect.ConnectionString = "Data Source=" & path & database & ";Read Only=True;"
            SQLconnect.Open()
            SQLcommand = SQLconnect.CreateCommand

            If records > 0 Then

                SQLcommand.CommandText = "SELECT rowid, " & fields & " FROM " & table

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
                                    data(1, x) &= SQLreader(y + 1).ToString.Replace(",", ".") & dlm
                                    If y = z Then data(2, x) = SQLreader(y + 1).ToString
                                Case "TIMESTAMP"
                                    data(1, x) &= Format(SQLreader(y + 1), "yyyy-MM-dd HH:mm:ss") & dlm
                                    If y = z Then data(2, x) = Format(SQLreader(y + 1), "yyyy-MM-dd HH:mm:ss")
                                Case Else
                                    data(1, x) &= SQLreader(y + 1) & dlm
                                    If y = z Then data(2, x) = SQLreader(y + 1)
                            End Select
                        Else
                            data(1, x) &= "NULL" & dlm
                            If y = z Then data(2, x) = "NULL"
                        End If
                    Next

                    x += 1

                End While

            Else
                ReDim data(2, 0)
            End If

        Catch ex As Exception
            If debug Then MPSync_process.logStats("MPSync: Error reading table " & table & " rowid """ & SQLreader(0) & """ in " & database & " with exception: " & ex.Message, "DEBUG")
            MPSync_process.logStats("MPSync: Error reading data from table " & table & " in database " & database, "ERROR")
            data = Nothing

        End Try

        SQLconnect.Close()

        Return data

    End Function

    Private Function FormatValue(ByVal value As Object, ByVal type As String) As String

        Dim fmtvalue As String

        If value.ToString = "NULL" Then
            fmtvalue = "NULL,"
        Else
            Select Case type

                Case "INTEGER", "REAL"
                    fmtvalue = value.ToString.Replace(",", ".") & ","
                Case "BOOL"
                    If value.ToString = "True" Then fmtvalue = "'1'," Else fmtvalue = "'0',"
                Case Else
                    fmtvalue = "'" & value.ToString.Replace("'", "''") & "',"

            End Select
        End If

        Return fmtvalue

    End Function

    Private Function BuildUpdateArray_mpsync(ByVal w_values(,) As String, ByVal s_data As Array, ByVal mps_columns As Array, ByVal columns As Array) As Array

        Dim x, z As Integer
        Dim w_pk(), s_pk() As String

        If w_values.OfType(Of String)().ToArray().Length = 0 Then Return s_data

        w_pk = getPkValues(w_values, mps_columns, columns)

        If s_data.OfType(Of String)().ToArray().Length > 0 Then

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

    Private Function BuildUpdateArray(ByVal s_data As Array, ByVal t_data As Array, ByVal columns As Array) As Array

        Dim x As Integer = getPK(columns)

        If x = -1 Then Return Nothing

        Dim w_values(,) As String = Nothing
        Dim s_temp(), t_temp() As String
        Dim same As IEnumerable(Of String)

        If x <> -1 Then
            s_temp = getArray(s_data, 2)
            t_temp = getArray(t_data, 2)
        Else
            s_temp = getArray(s_data, 1)
            t_temp = getArray(t_data, 1)
        End If

        If s_temp(0) <> Nothing Then

            Dim y, z As Integer

            same = s_temp.Intersect(t_temp)

            ReDim w_values(UBound(s_data, 1), UBound(same.ToArray))

            For x = 0 To UBound(same.ToArray)
                y = Array.IndexOf(s_temp, same.ToArray(x))
                For z = 0 To UBound(s_data, 1)
                    w_values(z, x) = s_data(z, y)
                Next
            Next

        End If

        Return w_values

    End Function

    Private Function getFields(ByVal path As String, ByVal database As String, ByVal table As String) As Array

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim columns(,) As String = Nothing
        Dim x As Integer = 0

        SQLconnect.ConnectionString = "Data Source=" & path & database & ";Read Only=True;"
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

    Private Function getSelectFields(ByVal columns As Array) As String

        Dim fields As String = Nothing

        For x = 0 To UBound(columns, 2)
            fields &= columns(0, x) & ","
        Next

        Return Left(fields, Len(fields) - 1)

    End Function

    Private Function RecordCount(ByVal path As String, ByVal database As String, ByVal table As String, Optional ByVal where As String = Nothing) As Integer

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim x As Integer = 0

        SQLconnect.ConnectionString = "Data Source=" & path & database & ";Read Only=True;"
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

    Private Function getPkValues(ByVal values As Array, ByVal mps_columns As Array, ByVal columns As Array) As Array

        Dim x, y As Integer
        Dim temp2() As String
        Dim PKs() As String = Nothing

        x = getPK(columns)

        If x = -1 Then Return Nothing

        Dim mps_cols As Array = getArray(mps_columns, 0)

        x = Array.IndexOf(mps_cols, columns(0, x))

        Dim temp1 As Array = getArray(values, 1)

        If temp1(0) IsNot Nothing Then
            For y = 0 To UBound(temp1)
                temp2 = Split(temp1(y), dlm)
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

        Dim a_values() As String = Split(values, dlm)

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

            SQLconnect.ConnectionString = "Data Source=" & path & database & ";Read Only=True;"
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

        Dim mps_db As New MPSync_process_DB

        Do

            MPSync_process.logStats("MPSync: DB synchronization cycle starting.", "LOG")

            mps_db.bw_sync_db_jobs = 0
            Array.Resize(mps_db.bw_sync_db, 0)
            mps_db.debug = MPSync_process.p_Debug

            ' direction is client to server or both
            If MPSync_process._db_direction <> 2 Then
                mps_db.Process_DB_folder(MPSync_process._db_client, MPSync_process._db_server)
            End If

            ' direction is server to client or both
            If MPSync_process._db_direction <> 1 Then
                mps_db.Process_DB_folder(MPSync_process._db_server, MPSync_process._db_client)
            End If

            If Not MPSync_settings.syncnow Then
                MPSync_process.logStats("MPSync: DB synchronization cycle complete.", "LOG")
                MPSync_process.wait(MPSync_process._db_sync, , "DB")
            Else
                MPSync_settings.db_complete = True
                MPSync_process.logStats("MPSync: DB synchronization complete.", "INFO")
                Exit Do
            End If

        Loop

    End Sub

    Private Sub Process_DB_folder(ByVal source As String, ByVal target As String)

        If Not IO.Directory.Exists(source) Then
            MPSync_process.logStats("MPSync: folder " & source & " does not exist", "ERROR")
            Exit Sub
        End If

        If debug Then MPSync_process.logStats("MPSync: synchronizing from " & source & " to " & target, "DEBUG")

        On Error Resume Next

        Dim x As Integer
        Dim s_lastwrite, t_lastwrite As Date

        lastsync = p_lastsync

        For Each database As String In IO.Directory.GetFiles(source, "*.db3")

            If IO.Path.GetExtension(database) <> ".db3-journal" Then

                Dim db As String = IO.Path.GetFileName(database)

                If MPSync_process._databases.Contains(db) Or MPSync_process._databases.Contains("ALL") Then

                    If MPSync_process.sync_type = "Triggers" Then
                        ProcessTables(source, target, db)
                    Else

                        x = Array.IndexOf(MPSync_process.dbname, db)

                        s_lastwrite = My.Computer.FileSystem.GetFileInfo(database).LastWriteTimeUtc
                        t_lastwrite = My.Computer.FileSystem.GetFileInfo(target & db).LastWriteTimeUtc

                        If MPSync_process.dbinfo(x).LastWriteTimeUtc < s_lastwrite Or MPSync_process.dbinfo(x).LastWriteTimeUtc <> t_lastwrite Then
                            ProcessTables(source, target, db)
                        Else
                            If debug Then MPSync_process.logStats("MPSync: no changes detected in " & database & ". Skipping synchronization.", "DEBUG")
                        End If

                    End If

                End If

            End If

        Next

        p_lastsync = Now.ToLocalTime.ToString("yyyy-MM-dd HH:mm:ss")

        If Not MPSync_process._objects.Contains("NOTHING") Then ProcessObject(source, target)

        Do While _bw_active_db_jobs > 0
            If MPSync_process.p_Debug Then MPSync_process.logStats("MPSync: [Process_DB_folder] waiting for background threads to finish...", "DEBUG")
            MPSync_process.wait(10, False)
        Loop

    End Sub

    Private Sub ProcessTables(ByVal source As String, ByVal target As String, ByVal database As String)

        CheckTables(source, target, database)

        Dim omit As Array = {"mpsync", "mpsync_trigger", "sqlite_sequence", "sqlite_stat1", "sqlite_stat2"}
        Dim parm As String = ""
        Dim x As Integer = 0

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader

        SQLconnect.ConnectionString = "Data Source=" & source & database & ";Read Only=True;"
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        If MPSync_process.TableExist(source, database, "mpsync_trigger") Then

            If debug Then MPSync_process.logStats("MPSync: checking for records in mpsync_trigger table in database " & source & database, "DEBUG")

            Try

                SQLcommand.CommandText = "SELECT tablename FROM mpsync_trigger WHERE lastupdated > '" & lastsync & "' ORDER BY lastupdated, tablename"
                SQLreader = SQLcommand.ExecuteReader()

                While SQLreader.Read()

                    parm += source & "|" & target & "|" & database & "|" & SQLreader(0) & "¬"
                    x += 1

                End While

                If debug Then MPSync_process.logStats("MPSync: " & x.ToString & " records found table mpsync_trigger in database " & source & database, "DEBUG")

            Catch ex As Exception
                MPSync_process.logStats("MPSync: Error reading mpsync_trigger from " & target & database & " with exception: " & ex.Message, "ERROR")
            End Try

            If MPSync_process.TableExist(target, database, "mpsync") Then

                SQLconnect.Close()

                If debug Then MPSync_process.logStats("MPSync: checking for records in mpsync table in database " & target & database, "DEBUG")

                Try

                    SQLconnect.ConnectionString = "Data Source=" & target & database & ";Read Only=True;"
                    SQLconnect.Open()
                    SQLcommand = SQLconnect.CreateCommand
                    SQLcommand.CommandText = "SELECT tablename FROM mpsync ORDER BY mps_lastupdated, tablename"
                    SQLreader = SQLcommand.ExecuteReader()

                    While SQLreader.Read()

                        If Not parm.Contains(SQLreader(0)) Then
                            parm += source & "|" & target & "|" & database & "|" & SQLreader(0) & "¬"
                            x += 1
                        End If

                    End While

                    If debug Then MPSync_process.logStats("MPSync: " & x.ToString & " records found in table mpsync in database " & target & database, "DEBUG")

                Catch ex As Exception
                    MPSync_process.logStats("MPSync: Error reading mpsync from " & target & database & " with exception: " & ex.Message, "ERROR")
                End Try

            End If

        Else

            If debug Then MPSync_process.logStats("MPSync: selecting tables from database " & source & database, "DEBUG")

            SQLcommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name"
            SQLreader = SQLcommand.ExecuteReader()

            While SQLreader.Read()

                If Array.IndexOf(omit, SQLreader(0)) = -1 Then
                    parm += source & "|" & target & "|" & database & "|" & SQLreader(0) & "¬"
                    x += 1
                End If

            End While

            If debug Then MPSync_process.logStats("MPSync: " & x.ToString & " table selected from database " & source & database, "DEBUG")

        End If

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

    Private Sub ProcessObject(ByVal source As String, ByVal target As String)

        Dim s_lastwrite, t_lastwrite As Date

        For Each objects As String In IO.Directory.GetFiles(source, "*.*")

            Dim obj As String = IO.Path.GetFileName(objects)

            If MPSync_process._objects.Contains(obj) Then

                s_lastwrite = My.Computer.FileSystem.GetFileInfo(objects).LastWriteTimeUtc
                t_lastwrite = My.Computer.FileSystem.GetFileInfo(target & obj).LastWriteTimeUtc

                If s_lastwrite > t_lastwrite Then
                    Try
                        MPSync_process.logStats("MPSync: Copying object " & objects & " to " & target & obj, "LOG")
                        IO.File.Copy(objects, target & obj, True)
                    Catch ex As Exception
                        MPSync_process.logStats("MPSync: Error copying " & objects & " with exception: " & ex.Message, "ERROR")
                    End Try
                Else
                    If debug Then MPSync_process.logStats("MPSync: no changes detected in " & objects & ". Skipping copy.", "DEBUG")
                End If

            End If

        Next

    End Sub

    Private Sub CheckTables(ByVal source As String, ByVal target As String, ByVal database As String)

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand
        Dim SQLreader As SQLiteDataReader
        Dim s_columns, t_columns As Array
        Dim s_temp(), t_temp() As String
        Dim missing(,) As String = Nothing
        Dim diff As IEnumerable(Of String)

        SQLconnect.ConnectionString = "Data Source=" & source & database & ";Read Only=True;"
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "SELECT name, sql FROM sqlite_master WHERE type=""table"""
        SQLreader = SQLcommand.ExecuteReader()

        While SQLreader.Read()

            s_columns = getFields(source, database, SQLreader(0))
            t_columns = getFields(target, database, SQLreader(0))

            If t_columns Is Nothing Then
                CreateTable(target, database, SQLreader(1))
                t_columns = s_columns
            End If

            s_temp = getArray(s_columns, 0)
            t_temp = getArray(t_columns, 0)

            diff = s_temp.Except(t_temp)

            If diff.OfType(Of String)().ToArray().Length > 0 Then

                For x As Integer = 0 To UBound(diff.ToArray)
                    ReDim Preserve missing(UBound(s_columns, 1), x)
                    For y As Integer = 0 To UBound(s_columns, 1)
                        missing(y, x) = s_columns(y, Array.IndexOf(s_temp, diff.ToArray(x)))
                    Next
                Next

                AddTableMissingFields(target, database, SQLreader(0), missing)

            End If

        End While

        SQLconnect.Close()

    End Sub

    Private Sub CreateTable(ByVal path As String, ByVal database As String, ByVal sql As String)

        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = sql
        SQLcommand.ExecuteNonQuery()
        SQLconnect.Close()

    End Sub

    Private Sub AddTableMissingFields(ByVal path As String, ByVal database As String, ByVal table As String, ByVal missing As Array)

        Dim SQL As String
        Dim SQLconnect As New SQLite.SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        MPSync_process.logStats("MPSync: Adding missing columns on target for table " & table & " in database " & database, "LOG")

        For x As Integer = 0 To UBound(missing, 2)

            SQL = "ALTER TABLE " & table & " ADD COLUMN " & missing(0, x) & " " & missing(1, x) & " "

            If missing(2, x) = "1" Then SQL &= "NOT NULL "

            Try
                If debug Then MPSync_process.logStats("MPSync: " & SQL, "DEBUG")
                SQLcommand.CommandText = SQL
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                If debug Then MPSync_process.logStats("MPSync: " & SQL & " error with exception: " & ex.Message, "DEBUG")
                MPSync_process.logStats("MPSync: Error adding field " & missing(0, x) & " to table " & table & " in " & path & database, "ERROR")
            End Try

        Next

        SQLconnect.Close()

    End Sub

    Private Sub bw_sync_db_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm() As String = Split(e.Argument, "¬")
        Dim args(3) As String
        Dim x, bw_db_thread_jobs As Integer
        Dim bw_db_thread() As BackgroundWorker = Nothing

        For x = 0 To parm.Count - 1

            If parm(x) <> "" Then

                If x = 0 Then
                    args = Split(parm(x), "|")
                    MPSync_process.logStats("MPSync: synchronization of " & args(2) & " database started.", "INFO")
                End If

                ReDim Preserve bw_db_thread(bw_db_thread_jobs)
                bw_db_thread(bw_db_thread_jobs) = New BackgroundWorker
                bw_db_thread(bw_db_thread_jobs).WorkerSupportsCancellation = True
                AddHandler bw_db_thread(bw_db_thread_jobs).DoWork, AddressOf bw_db_thread_worker
                AddHandler bw_db_thread(bw_db_thread_jobs).RunWorkerCompleted, AddressOf bw_worker_completed

                If MPSync_settings.syncnow Then bw_db_thread(bw_db_thread_jobs).WorkerReportsProgress = True

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

            If MPSync_process.p_Debug Then MPSync_process.logStats("MPSync: [bw_sync_db_worker] waiting for background threads to finish...", "DEBUG")

            For x = 0 To bw_db_thread_jobs - 1
                If bw_db_thread(x).IsBusy Then
                    busy = True
                    Exit For
                Else
                    busy = False
                End If
            Next

            MPSync_process.wait(10, False)

        Loop

        If MPSync_process.sync_type = "Timestamp" Then

            Dim s_lastwrite As Date = My.Computer.FileSystem.GetFileInfo(args(0) & args(2)).LastWriteTimeUtc
            Dim t_lastwrite As Date = My.Computer.FileSystem.GetFileInfo(args(1) & args(2)).LastWriteTimeUtc

            x = Array.IndexOf(MPSync_process.dbname, args(2))

            If s_lastwrite > t_lastwrite Then
                MPSync_process.dbinfo(x).LastWriteTimeUtc = s_lastwrite
            ElseIf s_lastwrite < t_lastwrite Then
                MPSync_process.dbinfo(x).LastWriteTimeUtc = t_lastwrite
            End If

        End If

        MPSync_process.logStats("MPSync: synchronization of " & args(2) & " database completed.", "INFO")

        _bw_active_db_jobs -= 1

    End Sub

    Private Sub bw_db_thread_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm As String = e.Argument
        Dim args(3) As String
        Dim columns(,) As String = Nothing
        Dim s_data(,), t_data(,) As String

        args = Split(parm, "|")

        If MPSync_process.check_watched Then
            ' check if master client
            If MPSync_process._db_direction <> 2 Then
                UpdateMaster(args(0), args(1), args(2), args(3), True)
            Else
                UpdateSlave(args(0), args(1), args(2), args(3))
            End If
        End If

        s_data = LoadTable(args(0), args(2), args(3), columns)
        t_data = LoadTable(args(1), args(2), args(3), columns)

        Synchronize_DB(args(1), args(2), args(3), columns, s_data, t_data, MPSync_process._db_sync_method)

        e.Result = "MPSync: synchronization of table " & args(3) & " in database " & args(2) & " complete."

    End Sub

    Private Sub bw_worker_completed(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        On Error Resume Next
        If e.Result <> Nothing Then
            MPSync_process.logStats(e.Result, "LOG")
        End If
    End Sub

    Private Function Synchronize_DB(ByVal path As String, ByVal database As String, ByVal table As String, ByVal columns As Array, ByRef s_data As Array, ByRef t_data As Array, ByVal method As Integer) As Boolean

        MPSync_process.logStats("MPSync: synchronization of table " & table & " in database " & path & database & " in progress...", "LOG")

        DeleteRecords(path, database, table, s_data, t_data, columns, method)
        Return InsertRecords(path, database, table, s_data, t_data, columns, method)

    End Function

    Private Sub UpdateSlave(ByVal source As String, ByVal target As String, ByVal database As String, ByVal table As String)

        Dim mps As New MPSync_settings

        Dim x As Integer = Array.IndexOf(mps.getDatabase, database)

        If x <> -1 Then
            If Array.IndexOf(mps.getTables(database), table) <> -1 Then

                MPSync_process.logStats("MPSync: synchronization of mpsync for table " & table & " in database " & source & database & " in progress...", "LOG")

                Dim columns As Array = Nothing
                Dim s_data, t_data As Array

                s_data = LoadTable(target, database, "mpsync", columns, "tablename = '" & table & "'", "mps_lastupdated")
                t_data = LoadTable(source, database, "mpsync", columns, "tablename = '" & table & "' AND mps_session = '" & p_Session & "'", "mps_lastupdated")

                If Synchronize_DB(source, database, "mpsync", columns, s_data, t_data, 1) Then
                    UpdateMaster(source, target, database, table)
                End If

                MPSync_process.logStats("MPSync: synchronization of mpsync for table " & table & " in database " & source & database & " complete.", "LOG")

            End If
        End If

    End Sub

    Private Sub UpdateMaster(ByVal source As String, ByVal target As String, ByVal database As String, ByVal table As String, Optional ByVal master As Boolean = False)

        Dim mps As New MPSync_settings

        Dim x As Integer = Array.IndexOf(mps.getDatabase, database)

        If x <> -1 Then
            If Array.IndexOf(mps.getTables(database), table) <> -1 Then

                MPSync_process.logStats("MPSync: synchronization of watched for table " & table & " in database " & source & database & " in progress...", "LOG")

                Dim mps_columns As Array = Nothing
                Dim columns, s_data, t_data, w_values As Array

                s_data = LoadTable(source, database, "mpsync", mps_columns, "tablename = '" & table & "'", "mps_lastupdated")
                t_data = LoadTable(target, database, "mpsync", mps_columns, "tablename = '" & table & "'", "mps_lastupdated")

                If s_data Is Nothing And t_data Is Nothing Then
                    MPSync_process.logStats("MPSync: synchronization of watched for table " & table & " in database " & source & database & " nothing to update.", "LOG")
                    Exit Sub
                End If

                columns = getFields(source, database, table)
                w_values = BuildUpdateArray_mpsync(t_data, s_data, mps_columns, columns)

                UpdateRecords(source, database, table, w_values, mps_columns, columns)

                If master Then Cleanup_mpsync(source, database, s_data)

                Cleanup_mpsync(target, database, t_data)

                MPSync_process.logStats("MPSync: synchronization of watched for table " & table & " in database " & source & database & " complete.", "LOG")

            End If
        End If

    End Sub

    Private Sub UpdateRecords(ByVal path As String, ByVal database As String, ByVal table As String, ByVal w_values As Array, ByVal table_columns As Array, ByVal columns As Array)

        If w_values.OfType(Of String)().ToArray().Length = 0 Then Exit Sub

        Dim i, x, z As Integer
        Dim pkey As String = Nothing
        Dim updcols As Array = getArray(table_columns, 0)
        Dim fields, updvalues, where, update(), a_values() As String
        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        x = getPK(columns, pkey)

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA temp_store=2;PRAGMA journal_mode=memory;"
        SQLcommand.ExecuteNonQuery()

        For y As Integer = 0 To UBound(w_values, 2)

            MPSync_process.CheckPlayerplaying("db", checkplayer)

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
                        If debug Then MPSync_process.logStats("MPSync: UPDATE " & table & " SET " & updvalues & " WHERE " & where, "DEBUG")
                        SQLcommand.CommandText = "UPDATE " & table & " SET " & updvalues & " WHERE " & where
                        SQLcommand.ExecuteNonQuery()
                    Catch ex As Exception
                        If debug Then MPSync_process.logStats("MPSync: Error SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ in " & path & database & " with exception: " & ex.Message, "DEBUG")
                        MPSync_process.logStats("MPSync: Error synchronizing table " & table & " in database " & path & database, "ERROR")
                    End Try

                End If
            End If
        Next

        SQLconnect.Close()

    End Sub

    Private Function InsertRecords(ByVal path As String, ByVal database As String, ByVal table As String, ByVal s_data As Array, ByVal t_data As Array, ByVal columns As Array, ByVal method As Integer) As Boolean

        ' propagate additions
        If method = 2 Then Return True

        If s_data.OfType(Of String)().ToArray().Length = 0 Then Return True

        If debug Then MPSync_process.logStats("MPSync: adding missing entries on target for table " & table & " in database " & path & database, "DEBUG")

        Dim s_temp(), t_temp() As String

        s_temp = getArray(s_data, 1)
        t_temp = getArray(t_data, 1)

        Dim fields, values, a_values() As String
        Dim diff As IEnumerable(Of String)
        Dim y As Integer
        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        fields = getSelectFields(columns)

        diff = s_temp.Except(t_temp)

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA temp_store=2;PRAGMA journal_mode=memory;"
        SQLcommand.ExecuteNonQuery()

        For y = 0 To UBound(diff.ToArray)

            MPSync_process.CheckPlayerplaying("db", checkplayer)

            values = Nothing
            a_values = Split(diff.ToArray(y), dlm)

            For x = 0 To UBound(columns, 2)
                values &= FormatValue(a_values(x), columns(1, x))
            Next

            values = Left(values, Len(values) - 1)

            Try
                If debug Then MPSync_process.logStats("MPSync: INSERT OR REPLACE INTO " & table & " (" & fields & ") VALUES(" & values & ")", "DEBUG")
                SQLcommand.CommandText = "INSERT OR REPLACE INTO " & table & " (" & fields & ") VALUES(" & values & ")"
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                If debug Then MPSync_process.logStats("MPSync: SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ in " & path & database & " error with exception: " & ex.Message, "DEBUG")
                MPSync_process.logStats("MPSync: Error adding record to table " & table & " in database " & path & database & " with exception: " & ex.Message, "ERROR")
                SQLconnect.Close()
                Return False
            End Try

        Next

        SQLconnect.Close()

        If debug Then MPSync_process.logStats("MPSync: " & y.ToString & " records added in " & table & " in database " & path & database, "DEBUG")

        Return True

    End Function

    Private Sub DeleteRecords(ByVal path As String, ByVal database As String, ByVal table As String, ByVal s_data As Array, ByVal t_data As Array, ByVal columns As Array, ByVal method As Integer)

        ' propagate deletions
        If method = 1 Or table = "mpsync" Then Exit Sub

        If t_data.OfType(Of String)().ToArray().Length = 0 Then Exit Sub

        If debug Then MPSync_process.logStats("MPSync: deleting extra entries on target for table " & table & " in database " & path & database, "DEBUG")

        Dim s_temp(), t_temp() As String

        If getPK(columns) <> -1 Then
            s_temp = getArray(s_data, 2)
            t_temp = getArray(t_data, 2)
        Else
            s_temp = getArray(s_data, 1)
            t_temp = getArray(t_data, 1)
        End If

        Dim diff As IEnumerable(Of String)
        Dim y As Integer
        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        diff = t_temp.Except(s_temp)

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand
        SQLcommand.CommandText = "PRAGMA temp_store=2;PRAGMA journal_mode=memory;"
        SQLcommand.ExecuteNonQuery()

        For y = 0 To UBound(diff.ToArray)
            MPSync_process.CheckPlayerplaying("db", checkplayer)
            Try
                If debug Then MPSync_process.logStats("MPSync: DELETE FROM " & table & " WHERE rowid = " & t_data(0, Array.IndexOf(t_temp, diff.ToArray(y))), "DEBUG")
                SQLcommand.CommandText = "DELETE FROM " & table & " WHERE rowid = " & t_data(0, Array.IndexOf(t_temp, diff.ToArray(y)))
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                If debug Then MPSync_process.logStats("MPSync: SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ in " & path & database & " error with exception: " & ex.Message, "DEBUG")
                MPSync_process.logStats("MPSync: Error deleting record from table " & table & " in database " & path & database & " with exception: " & ex.Message, "ERROR")
            End Try
        Next

        SQLconnect.Close()

        If debug Then MPSync_process.logStats("MPSync: " & y.ToString & " record deleted from " & table & " in database " & path & database, "DEBUG")

    End Sub

    Private Sub Cleanup_mpsync(ByVal path As String, ByVal database As String, ByVal data As Array)

        If data.OfType(Of String)().ToArray().Length = 0 Or data(0, 0) = Nothing Then Exit Sub

        Dim SQLconnect As New SQLiteConnection()
        Dim SQLcommand As SQLiteCommand

        SQLconnect.ConnectionString = "Data Source=" & path & database
        SQLconnect.Open()
        SQLcommand = SQLconnect.CreateCommand

        For y = 0 To UBound(data, 2)

            Try
                If debug Then MPSync_process.logStats("MPSync: DELETE FROM mpsync WHERE rowid = " & data(0, y), "DEBUG")
                SQLcommand.CommandText = "DELETE FROM mpsync WHERE rowid = " & data(0, y)
                SQLcommand.ExecuteNonQuery()
            Catch ex As Exception
                If debug Then MPSync_process.logStats("MPSync: Error imports SQL """ & (SQLcommand.CommandText).Replace("""", "'") & """ with exception: " & ex.Message, "DEBUG")
                MPSync_process.logStats("MPSync: Error deleting record from table mpsync in database " & path & database & " with exception: " & ex.Message, "ERROR")
            End Try

        Next

        SQLconnect.Close()

    End Sub

End Class
