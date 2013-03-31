Imports MediaPortal.GUI.Library

Imports System.ComponentModel

Public Class MPSync_process_Thumbs

    Dim s_path, t_path As String

    Public Shared Sub bw_thumbs_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim cdb As New MPSync_process_Thumbs

        Do

            ' direction is client to server or both
            If MPSync_process._thumbs_direction <> 2 Then
                cdb.Process_Thumbs_folder(MPSync_process._thumbs_client, MPSync_process._thumbs_server)
            End If

            ' direction is server to client or both
            If MPSync_process._thumbs_direction <> 1 Then
                cdb.Process_Thumbs_folder(MPSync_process._thumbs_server, MPSync_process._thumbs_client)
            End If

            If Not MPSync_settings.syncnow Then
                MPSync_process.wait(MPSync_process._thumbs_sync)
            Else
                MPSync_process.thumbs_complete = True
                Exit Do
            End If

        Loop

    End Sub

    Private Sub Process_Thumbs_folder(ByVal source As String, ByVal target As String)

        If Not IO.Directory.Exists(source) Then
            Log.Error("MPSync: folder " & source & " does not exist")
            Exit Sub
        End If

        If Not IO.Directory.Exists(target) Then IO.Directory.CreateDirectory(target)

        On Error Resume Next

        Dim diff As IEnumerable(Of String)
        Dim s_thumbs() As String = Nothing
        Dim t_thumbs() As String = Nothing
        Dim folder As String

        Dim x As Integer = 0

        Do While Right(source, x) = Right(target, x)
            x += 1
        Loop

        x -= 1
        s_path = Left(source, Len(source) - x)
        t_path = Left(target, Len(target) - x)

        x = -1

        If MPSync_process.p_Debug Then Log.Debug("MPSync: Scanning folder " & source & " for thumbs")

        For Each file As String In IO.Directory.GetFiles(source, "*.*", IO.SearchOption.AllDirectories)
            If InStr(Len(source) + 1, file, "\") > 0 Then
                folder = Mid(file, Len(source) + 1, InStr(Len(source) + 1, file, "\") - Len(source) - 1)
                If (MPSync_process._thumbs.Contains(folder) Or MPSync_process._thumbs.Contains("ALL")) And IO.Path.GetFileName(file) <> "Thumbs.db" Then
                    x += 1
                    ReDim Preserve s_thumbs(x)
                    s_thumbs(x) = Right(file, Len(file) - Len(s_path))
                End If
            End If
        Next

        If x = -1 Then
            ReDim s_thumbs(0)
            s_thumbs(0) = ""
        End If

        x = -1

        If MPSync_process.p_Debug Then Log.Debug("MPSync: Scanning folder " & target & " for thumbs")

        For Each file As String In IO.Directory.getFiles(target, "*.*", IO.SearchOption.AllDirectories)
            If InStr(Len(target) + 1, file, "\") > 0 Then
                folder = Mid(file, Len(target) + 1, InStr(Len(target) + 1, file, "\") - Len(target) - 1)
                If (MPSync_process._thumbs.Contains(folder) Or MPSync_process._thumbs.Contains("ALL")) And IO.Path.GetFileName(file) <> "Thumbs.db" Then
                    x += 1
                    ReDim Preserve t_thumbs(x)
                    t_thumbs(x) = Right(file, Len(file) - Len(t_path))
                End If
            End If
        Next

        If x = -1 Then
            ReDim t_thumbs(0)
            t_thumbs(0) = ""
        End If

        Dim _bw_active_thumbs_jobs, _bw_sync_thumbs_jobs As Integer
        Dim bw_sync_thumbs() As BackgroundWorker

        ' propagate deletions or both
        If MPSync_process._thumbs_sync_method <> 1 And t_thumbs(0) <> "" Then
            diff = t_thumbs.Except(s_thumbs)

            If MPSync_process.p_Debug Then Log.Debug("MPSync: found " & (UBound(diff.ToArray) + 1).ToString & " differences for deletion")

            If UBound(diff.ToArray) >= 0 Then
                ReDim Preserve bw_sync_thumbs(_bw_sync_thumbs_jobs)
                bw_sync_thumbs(_bw_sync_thumbs_jobs) = New BackgroundWorker
                bw_sync_thumbs(_bw_sync_thumbs_jobs).WorkerSupportsCancellation = True
                AddHandler bw_sync_thumbs(_bw_sync_thumbs_jobs).DoWork, AddressOf bw_delete_worker
                AddHandler bw_sync_thumbs(_bw_sync_thumbs_jobs).RunWorkerCompleted, AddressOf bw_worker_completed

                If Not bw_sync_thumbs(_bw_sync_thumbs_jobs).IsBusy Then bw_sync_thumbs(_bw_sync_thumbs_jobs).RunWorkerAsync(diff.ToArray)

                _bw_sync_thumbs_jobs += 1
            End If
        End If

        ' propagate additions or both
        If MPSync_process._thumbs_sync_method <> 2 And s_thumbs(0) <> "" Then
            diff = s_thumbs.Except(t_thumbs)

            If MPSync_process.p_Debug Then Log.Debug("MPSync: found " & (UBound(diff.ToArray) + 1).ToString & " differences for addition")

            If UBound(diff.ToArray) >= 0 Then
                ReDim Preserve bw_sync_thumbs(_bw_sync_thumbs_jobs)
                bw_sync_thumbs(_bw_sync_thumbs_jobs) = New BackgroundWorker
                bw_sync_thumbs(_bw_sync_thumbs_jobs).WorkerSupportsCancellation = True
                AddHandler bw_sync_thumbs(_bw_sync_thumbs_jobs).DoWork, AddressOf bw_copy_worker
                AddHandler bw_sync_thumbs(_bw_sync_thumbs_jobs).RunWorkerCompleted, AddressOf bw_worker_completed

                If Not bw_sync_thumbs(_bw_sync_thumbs_jobs).IsBusy Then bw_sync_thumbs(_bw_sync_thumbs_jobs).RunWorkerAsync(diff.ToArray)

                _bw_sync_thumbs_jobs += 1
            End If
        End If

        If _bw_sync_thumbs_jobs > 0 Then

            Dim busy As Boolean = True

            Do While busy

                For x = 0 To _bw_sync_thumbs_jobs - 1
                    If bw_sync_thumbs(x).IsBusy Then
                        busy = True
                        Exit For
                    Else
                        busy = False
                    End If
                Next

                MPSync_process.wait(30, False)

            Loop

        End If

    End Sub

    Private Sub bw_copy_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm() As String = e.Argument
        Dim x As Integer

        Dim directory As String

        For x = 0 To UBound(parm)
            MPSync_process.CheckPlayerplaying("thumbs", 30)
            directory = IO.Path.GetDirectoryName(t_path & parm(x))

            If Not IO.Directory.Exists(directory) Then
                IO.Directory.CreateDirectory(directory)
                If MPSync_process.p_Debug Then Log.Debug("MPSync: directory missing, creating " & directory)
            End If

            IO.File.Copy(s_path & parm(x), t_path & parm(x), True)
        Next

        e.Result = "MPSync: " & x.ToString & " images added."

    End Sub

    Private Sub bw_delete_worker(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs)

        Dim parm() As String = e.Argument
        Dim x As Integer

        For x = 0 To UBound(parm)
            MPSync_process.CheckPlayerplaying("thumbs", 30)
            Try
                IO.File.Delete(t_path & parm(x))
            Catch ex As Exception
                If MPSync_process.p_Debug Then Log.Error("MPSync: Error deleting " & t_path & parm(x))
            End Try
        Next

        e.Result = "MPSync: " & x.ToString & " images removed."

    End Sub

    Private Sub bw_worker_completed(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        If e.Result <> Nothing Then
            Log.Info(e.Result)
        End If
    End Sub

End Class
