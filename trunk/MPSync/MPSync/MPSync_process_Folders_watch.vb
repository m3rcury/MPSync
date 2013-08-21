﻿Imports System.IO
Imports System.Threading

Class MPSync_process_Folders_watch

    Dim watchfolder As FileSystemWatcher
    Dim m_path As String = Nothing
    Dim s_path As String = Nothing
    Dim t_path As String = Nothing
    Dim folder_type As String = Nothing
    Dim selected_folder() As String = Nothing

    Public Sub watch_folder(ByVal path As String, ByVal foldertype As String, ByVal spath As String, ByVal tpath As String, ByVal selectedfolder() As String)

        m_path = path
        s_path = spath
        t_path = tpath
        folder_type = foldertype
        selected_folder = selectedfolder

        watchfolder = New System.IO.FileSystemWatcher()

        MPSync_process.logStats("MPSync: " & foldertype & " watch starting for " & path, "LOG")

        'this is the path we want to monitor
        watchfolder.Path = path
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

        Try
            Dim folder As String
            Dim file As String = Right(e.FullPath, Len(e.FullPath) - Len(s_path)) & "|WATCH"
            Dim l As Integer = Len(m_path) + 1

            If Right(m_path, 1) <> "\" Then l += 1

            folder = Mid(e.FullPath, l, InStr(l, e.FullPath, "\") - l)

            If selected_folder.Contains(folder) Or selected_folder.Contains("ALL") Then

                Dim parm(0) As String
                parm(0) = file

                If e.ChangeType = WatcherChangeTypes.Changed Or e.ChangeType = WatcherChangeTypes.Created Then
                    MPSync_process_Folders.Copy_objects(s_path, t_path, parm)
                ElseIf e.ChangeType = WatcherChangeTypes.Deleted Then
                    MPSync_process_Folders.Delete_objects(t_path, parm)
                End If

            End If
        Catch ex As Exception
            MPSync_process.logStats("MPSync: fileChange failed with exception: " & ex.Message, "ERROR")
        End Try

    End Sub

    Private Sub fileRename(ByVal source As Object, ByVal e As System.IO.RenamedEventArgs)

        Try
            Dim folder As String
            Dim l As Integer = Len(m_path) + 1

            If Right(m_path, 1) <> "\" Then l += 1

            folder = Mid(e.FullPath, l, InStr(l, e.FullPath, "\") - l)

            If selected_folder.Contains(folder) Or selected_folder.Contains("ALL") Then
                MPSync_process.logStats("MPSync: " & folder_type & " - " & e.OldFullPath & " renamed to " & e.FullPath, "LOG")
                FileSystem.Rename(e.OldFullPath, e.FullPath)
            End If
        Catch ex As Exception
            MPSync_process.logStats("MPSync: rename failed with exception: " & ex.Message, "ERROR")
        End Try

    End Sub

End Class
