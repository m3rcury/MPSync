Imports MediaPortal.GUI.Library

Imports System.IO
Imports System.ComponentModel
Imports System.Threading

Public Class MPSync_process_Folders

    Private Shared checkplayer As Integer = 30

    Dim s_paths() As String = Nothing
    Dim t_paths() As String = Nothing
    Dim foldertypes() As String = Nothing

    Public Shared Sub bw_folders_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim mps_folders As New MPSync_process_Folders

        MPSync_process.logStats("MPSync: [MPSync_process.WorkMethod][bw_folders_worker] Folders synchronization started.", "INFO")

        Dim x As Integer = -1
        Dim process_thread() As Thread = Nothing
        Dim item As Array
        Dim list As Array = MPSync_process.p_object_list

        For Each obj As String In list

            item = Split(obj, "¬")

            If item(1) = "True" Then
                x += 1

                ReDim Preserve mps_folders.foldertypes(x), mps_folders.s_paths(x), mps_folders.t_paths(x)
                mps_folders.foldertypes(x) = UCase(item(0))

                ReDim Preserve process_thread(x)
                process_thread(x) = New Thread(AddressOf mps_folders.folders_processing)
                process_thread(x).Start(item(0))
            End If

        Next

        If x <> -1 Then

            Dim active As Boolean = True

            Do Until active = False

                active = False

                For y As Integer = 0 To x

                    If process_thread(y).IsAlive Then
                        active = True
                        Exit For
                    End If

                Next

                MPSync_process.wait(10, False)

            Loop

        End If

        If MPSync_settings.syncnow Then MPSync_settings.folders_complete = True

        MPSync_process.logStats("MPSync: [MPSync_process.WorkMethod][bw_folders_worker] Folders synchronization complete.", "INFO")

    End Sub

    Private Sub folders_processing(ByVal foldertype As String)

        Dim folders_client As String = Nothing
        Dim folders_server As String = Nothing
        Dim folders() As String = Nothing
        Dim folders_direction As Integer = Nothing
        Dim folders_sync_method As Integer = Nothing

        MPSync_process.getObjectSettings(foldertype, folders_client, folders_server, folders_direction, folders_sync_method, folders)

        Process(UCase(foldertype), folders_client, folders_server, folders_direction, folders_sync_method, folders)

    End Sub

    Private Sub Process(ByVal foldertype As String, ByVal clientpath As String, ByVal serverpath As String, ByVal direction As String, ByVal folders_sync_method As Integer, ByVal selectedfolder() As String)

        MPSync_process.logStats("MPSync: [Process] " & foldertype & " synchronization cycle starting.", "LOG")

        ' direction is client to server
        If direction <> 2 Then
            Process_folder(foldertype, clientpath, serverpath, folders_sync_method, selectedfolder)
        End If

        ' direction is server to client
        If direction <> 1 Then
            Process_folder(foldertype, serverpath, clientpath, folders_sync_method, selectedfolder)
        End If

        MPSync_process.logStats("MPSync: [Process] " & foldertype & " synchronization cycle complete.", "LOG")

        If Not MPSync_settings.syncnow Then

            Dim x As Integer = Array.IndexOf(foldertypes, foldertype)
            Dim mps_w As New MPSync_process_Folders_watch

            ' direction is client to server
            If direction <> 2 Then
                mps_w.watch_folder(clientpath, foldertype, s_paths(x), t_paths(x), selectedfolder)
            End If

            ' direction is server to client
            If direction <> 1 Then
                mps_w.watch_folder(serverpath, foldertype, s_paths(x), t_paths(x), selectedfolder)
            End If

        End If

    End Sub

    Private Function getobjectsDetails(ByVal path As String, ByVal c_path As String, ByVal selectedfolders() As String) As Array

        Dim objects() As String = Nothing
        Dim folder As String = Nothing
        Dim l1 As Integer = Len(path) + 1
        Dim l2 As Integer = Len(c_path)
        Dim x As Integer = -1

        MPSync_process.logStats("MPSync: [getobjectsDetails] Scanning folder " & path & " for objects", "LOG")

        Try

            Dim dir As New DirectoryInfo(path)
            Dim list = dir.GetFiles("*.*", SearchOption.AllDirectories)

            For Each file As IO.FileInfo In list

                Try
                    folder = Mid(file.FullName, l1, InStr(l1, file.FullName, "\") - l1)

                    If selectedfolders.Contains(folder) Or selectedfolders.Contains("ALL") Then
                        x += 1
                        ReDim Preserve objects(x)
                        objects(x) = Right(file.FullName, Len(file.FullName) - l2) & "|" & file.LastWriteTimeUtc
                    End If
                Catch ex As Exception
                End Try

            Next

        Catch ex As Exception
            MPSync_process.logStats("MPSync: [getobjectsDetails] failed to read objects from folder " & path & " with exception: " & ex.Message, "ERROR")
        End Try

        If x = -1 Then
            x = 0
            ReDim objects(x)
            objects(x) = ""
        Else
            Array.Sort(objects)
        End If

        MPSync_process.logStats("MPSync: [getobjectsDetails] " & x.ToString & " objects found in folder " & path, "LOG")

        Return objects

    End Function

    Private Sub Process_folder(ByVal foldertype As String, ByVal source As String, ByVal target As String, ByVal folders_sync_method As Integer, ByVal selectedfolder() As String)

        If Not Directory.Exists(source) Then
            MPSync_process.logStats("MPSync: [Process_folder] folder " & source & " does not exist", "ERROR")
            Exit Sub
        End If

        If Not Directory.Exists(target) Then Directory.CreateDirectory(target)

        On Error Resume Next

        Dim diff As IEnumerable(Of String)
        Dim s_folders() As String = Nothing
        Dim t_folders() As String = Nothing
        Dim s_path, t_path As String
        Dim x As Integer = 0

        Do While UCase(Right(source, x)) = UCase(Right(target, x))
            x += 1
        Loop

        x -= 1
        s_path = Left(source, Len(source) - x)
        t_path = Left(target, Len(target) - x)

        x = Array.IndexOf(foldertypes, foldertype)

        s_paths(x) = s_path
        t_paths(x) = t_path

        s_folders = getobjectsDetails(source, s_path, selectedfolder)
        t_folders = getobjectsDetails(target, t_path, selectedfolder)

        ' propagate deletions or both
        If folders_sync_method <> 1 And t_folders(0) <> "" Then
            diff = t_folders.Except(s_folders, StringComparer.InvariantCultureIgnoreCase)

            MPSync_process.logStats("MPSync: [Process_folder] found " & (UBound(diff.ToArray) + 1).ToString & " differences for deletion between " & source & " and " & target, "DEBUG")

            If UBound(diff.ToArray) >= 0 Then
                If (diff.Count / t_folders.Count) <= 0.25 Then
                    Delete_objects(t_path, diff.ToArray)
                Else
                    MPSync_process.logStats("MPSync: [Process_folder] differences for deletion exceed 25% treshold.  Deletion not allowed for " & source & " and " & target, "INFO")
                End If
            End If

        End If

        ' propagate additions or both
        If folders_sync_method <> 2 And s_folders(0) <> "" Then
            diff = s_folders.Except(t_folders, StringComparer.InvariantCultureIgnoreCase)

            MPSync_process.logStats("MPSync: [Process_folder] found " & (UBound(diff.ToArray) + 1).ToString & " differences for addition/replacement between " & source & " and " & target, "DEBUG")

            If UBound(diff.ToArray) >= 0 Then Copy_objects(s_path, t_path, diff.ToArray)

        End If

        Array.Clear(s_folders, 0, UBound(s_folders))
        Array.Clear(t_folders, 0, UBound(t_folders))

    End Sub

    Public Shared Sub Copy_objects(ByVal s_path As String, ByVal t_path As String, ByVal parm As Array)

        Dim file As Array = Nothing
        Dim x As Integer
        Dim times As Integer
        Dim lock As New ReaderWriterLockSlim

        Dim directory As String

        For x = 0 To UBound(parm)

            Do While MPSync_process.CheckPlayerplaying("folders")
                MPSync_process.wait(checkplayer, False)
            Loop

            times = 5
            file = Split(parm(x), "|")

            directory = IO.Path.GetDirectoryName(t_path & file(0))

            If Not IO.Directory.Exists(directory) Then
                IO.Directory.CreateDirectory(directory)
                MPSync_process.logStats("MPSync: [Copy_objects] directory missing, creating " & directory, "LOG")
            End If

            Do While times > 0 And Not lock.TryEnterReadLock(250)
                times -= 1
            Loop

            If times > 0 Then
                Try
                    IO.File.Copy(s_path & file(0), t_path & file(0), True)
                    MPSync_process.logStats("MPSync: [Copy_objects] " & t_path & file(0) & " copied.", "DEBUG")
                Catch ex As Exception
                    MPSync_process.logStats("MPSync: [Copy_objects] copy failed with exception: " & ex.Message, "ERROR")
                End Try
            Else
                MPSync_process.logStats("MPSync: [Copy_objects] could not get read lock on file " & s_path & file(0), "ERROR")
            End If

            lock.ExitReadLock()

        Next

        If file(1) <> "WATCH" Then MPSync_process.logStats("MPSync: [Copy_objects] " & x.ToString & " objects added/replaced.", "LOG")

    End Sub

    Public Shared Sub Delete_objects(ByVal t_path As String, ByVal parm As Array)

        Dim file As Array = Nothing
        Dim x As Integer
        Dim times As Integer
        Dim lock As New ReaderWriterLockSlim

        For x = 0 To UBound(parm)

            Do While MPSync_process.CheckPlayerplaying("folders")
                MPSync_process.wait(checkplayer, False)
            Loop

            times = 5
            file = Split(parm(x), "|")

            Do While times > 0 And Not lock.TryEnterReadLock(250)
                times -= 1
            Loop

            If times > 0 Then
                Try
                    IO.File.Delete(t_path & file(0))
                    MPSync_process.logStats("MPSync: [Delete_objects] " & t_path & file(0) & " deleted.", "DEBUG")
                Catch ex As Exception
                    MPSync_process.logStats("MPSync: [Delete_objects] delete failed with exception: " & ex.Message, "ERROR")
                End Try
            Else
                MPSync_process.logStats("MPSync: [Delete_objects] could not get read lock on file " & t_path & file(0), "ERROR")
            End If

            lock.ExitReadLock()

        Next

        If file(1) <> "WATCH" Then MPSync_process.logStats("MPSync: [Delete_objects] " & x.ToString & " objects removed.", "LOG")

    End Sub

End Class
