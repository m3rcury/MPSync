Imports MediaPortal.GUI.Library

Imports System.IO
Imports System.ComponentModel
Imports System.Security.Cryptography
Imports System.Threading

Public Class MPSync_process_Thumbs

    Dim debug As Boolean
    Dim checkplayer As Integer = 30
    Dim s_path, t_path As String
    Dim watchfolder As FileSystemWatcher

    Public Shared Sub bw_thumbs_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim mps_thumbs As New MPSync_process_Thumbs

        MPSync_process.logStats("MPSync: THUMBS synchronization cycle starting.", "LOG")

        mps_thumbs.debug = MPSync_process.p_Debug

        ' direction is client to server
        If MPSync_process._thumbs_direction <> 2 Then
            mps_thumbs.Process_Thumbs_folder(MPSync_process._thumbs_client, MPSync_process._thumbs_server)
        End If

        ' direction is server to client
        If MPSync_process._thumbs_direction <> 1 Then
            mps_thumbs.Process_Thumbs_folder(MPSync_process._thumbs_server, MPSync_process._thumbs_client)
        End If

        If Not MPSync_settings.syncnow Then

            MPSync_process.logStats("MPSync: THUMBS synchronization cycle complete.", "LOG")
            MPSync_process.logStats("MPSync: THUMBS folder watch starting.", "LOG")

            ' direction is client to server
            If MPSync_process._thumbs_direction <> 2 Then
                mps_thumbs.watch_Thumbs_folder(MPSync_process._thumbs_client, MPSync_process._thumbs_server)
            End If

            ' direction is server to client
            If MPSync_process._thumbs_direction <> 1 Then
                mps_thumbs.watch_Thumbs_folder(MPSync_process._thumbs_server, MPSync_process._thumbs_client)
            End If

        Else
            MPSync_settings.thumbs_complete = True
            MPSync_process.logStats("MPSync: THUMBS synchronization complete.", "INFO")
        End If

    End Sub

    Private Sub watch_Thumbs_folder(ByVal source As String, ByVal target As String)

        watchfolder = New System.IO.FileSystemWatcher()

        'this is the path we want to monitor
        watchfolder.Path = source
        watchfolder.IncludeSubdirectories = True

        'Add a list of Filter we want to specify
        watchfolder.NotifyFilter = NotifyFilters.DirectoryName
        watchfolder.NotifyFilter = watchfolder.NotifyFilter Or NotifyFilters.FileName
        watchfolder.NotifyFilter = watchfolder.NotifyFilter Or NotifyFilters.Attributes

        ' add the handler to each event
        AddHandler watchfolder.Changed, AddressOf fileChange
        AddHandler watchfolder.Created, AddressOf fileChange
        AddHandler watchfolder.Deleted, AddressOf fileChange
        AddHandler watchfolder.Renamed, AddressOf fileRename

        'Set this property to true to start watching
        watchfolder.EnableRaisingEvents = True

        Dim autoEvent As New AutoResetEvent(False)
        autoEvent.WaitOne()

    End Sub

    Private Sub fileChange(ByVal source As Object, ByVal e As System.IO.FileSystemEventArgs)

        Dim file As String = Right(e.FullPath, Len(e.FullPath) - Len(s_path)) & "|"
        Dim folder As String
        Dim l As Integer = Len(source.Path) + 1

        folder = Mid(e.FullPath, l, InStr(l, e.FullPath, "\") - l)

        If MPSync_process._thumbs.Contains(folder) Or MPSync_process._thumbs.Contains("ALL") Then

            Dim parm(0) As String

            parm(0) = file

            If e.ChangeType = WatcherChangeTypes.Changed Or e.ChangeType = WatcherChangeTypes.Created Then
                Copy_Images(parm)
            End If

            If e.ChangeType = WatcherChangeTypes.Deleted Then
                Delete_Images(parm)
            End If

        End If

    End Sub

    Public Sub fileRename(ByVal source As Object, ByVal e As System.IO.RenamedEventArgs)

        Dim file As String = Right(e.FullPath, Len(e.FullPath) - Len(s_path)) & "|"
        Dim folder As String
        Dim l As Integer = Len(source) + 1

        folder = Mid(e.FullPath, l, InStr(l, e.FullPath, "\") - l)

        If MPSync_process._thumbs.Contains(folder) Or MPSync_process._thumbs.Contains("ALL") Then

            Dim parm(0) As String

            parm(0) = file

            If e.ChangeType = WatcherChangeTypes.Changed Or e.ChangeType = WatcherChangeTypes.Created Then
                Copy_Images(parm)
            End If

        End If

    End Sub

    Private Function getThumbsDetails(ByVal path As String, ByVal c_path As String) As Array

        Dim thumbs() As String = Nothing
        Dim folder As String = Nothing
        Dim l1 As Integer = Len(path) + 1
        Dim l2 As Integer = Len(c_path)
        Dim x As Integer = -1

        Dim dir As New DirectoryInfo(path)

        Dim list = dir.GetFiles("*.*", SearchOption.AllDirectories)

        For Each file As IO.FileInfo In list

            If file.Name <> "Thumbs.db" Then

                folder = Mid(file.FullName, l1, InStr(l1, file.FullName, "\") - l1)

                If MPSync_process._thumbs.Contains(folder) Or MPSync_process._thumbs.Contains("ALL") Then
                    x += 1
                    ReDim Preserve thumbs(x)
                    thumbs(x) = Right(file.FullName, Len(file.FullName) - l2) & "|" & file.LastWriteTimeUtc
                End If
            End If

        Next

        Array.Sort(thumbs)

        If x = -1 Then x = 0

        If debug Then MPSync_process.logStats("MPSync: " & x.ToString & " images found in folder " & path, "DEBUG")

        Return thumbs

    End Function

    Private Sub Process_Thumbs_folder(ByVal source As String, ByVal target As String)

        If Not Directory.Exists(source) Then
            MPSync_process.logStats("MPSync: folder " & source & " does not exist", "ERROR")
            Exit Sub
        End If

        If Not Directory.Exists(target) Then Directory.CreateDirectory(target)

        On Error Resume Next

        Dim diff As IEnumerable(Of String)
        Dim s_thumbs() As String = Nothing
        Dim t_thumbs() As String = Nothing
        Dim x As Integer = 0

        Do While Right(source, x) = Right(target, x)
            x += 1
        Loop

        x -= 1
        s_path = Left(source, Len(source) - x)
        t_path = Left(target, Len(target) - x)

        MPSync_process.logStats("MPSync: Scanning folder " & source & " for thumbs", "LOG")

        s_thumbs = getThumbsDetails(source, s_path)

        MPSync_process.logStats("MPSync: Scanning folder " & target & " for thumbs", "LOG")

        t_thumbs = getThumbsDetails(target, t_path)

        ' propagate deletions or both
        If MPSync_process._thumbs_sync_method <> 1 And t_thumbs(0) <> "" Then
            diff = t_thumbs.Except(s_thumbs)

            If debug Then MPSync_process.logStats("MPSync: found " & (UBound(diff.ToArray) + 1).ToString & " differences for deletion", "DEBUG")

            If UBound(diff.ToArray) >= 0 Then Delete_Images(diff.ToArray)

        End If

        ' propagate additions or both
        If MPSync_process._thumbs_sync_method <> 2 And s_thumbs(0) <> "" Then
            diff = s_thumbs.Except(t_thumbs)

            If debug Then MPSync_process.logStats("MPSync: found " & (UBound(diff.ToArray) + 1).ToString & " differences for addition/replacement", "DEBUG")

            If UBound(diff.ToArray) >= 0 Then Copy_Images(diff.ToArray)

        End If

    End Sub

    Private Sub Copy_Images(ByVal parm As Array)

        Dim file As Array
        Dim x As Integer

        Dim directory As String

        For x = 0 To UBound(parm)
            MPSync_process.CheckPlayerplaying("thumbs", checkplayer)

            file = Split(parm(x), "|")

            directory = IO.Path.GetDirectoryName(t_path & file(0))

            If Not IO.Directory.Exists(directory) Then
                IO.Directory.CreateDirectory(directory)
                If debug Then MPSync_process.logStats("MPSync: directory missing, creating " & directory, "DEBUG")
            End If

            IO.File.Copy(s_path & file(0), t_path & file(0), True)

            If debug Then MPSync_process.logStats("MPSync: copying " & file(0), "DEBUG")
        Next

        MPSync_process.logStats("MPSync: " & x.ToString & " images added/replaced.", "LOG")

    End Sub

    Private Sub Delete_Images(ByVal parm As Array)

        Dim file As Array
        Dim x As Integer

        For x = 0 To UBound(parm)
            MPSync_process.CheckPlayerplaying("thumbs", checkplayer)

            file = Split(parm(x), "|")

            Try
                IO.File.Delete(t_path & file(0))
                If debug Then MPSync_process.logStats("MPSync: deleting " & parm(x), "DEBUG")
            Catch ex As Exception
                MPSync_process.logStats("MPSync: Error deleting " & t_path & parm(x), "ERROR")
            End Try
        Next

        MPSync_process.logStats("MPSync: " & x.ToString & " images removed.", "LOG")

    End Sub

End Class
