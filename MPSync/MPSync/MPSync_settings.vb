Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports Microsoft.VisualBasic.FileIO

Imports System.Data.SQLite

Public Class MPSync_settings

    Dim i_direction(2) As Image
    Dim i_method(2), _databases, _thumbs, _curversion, _version As String
    Dim _clicks_db, _clicks_thumbs, _db_sync_method, _thumbs_sync_method As Integer
    Public Shared syncnow As Boolean = False

    Private Sub Get_Settings()

        If FileIO.FileSystem.FileExists(Config.GetFile(Config.Dir.Config, "CDB_Sync.xml")) Then
            FileIO.FileSystem.DeleteFile(Config.GetFile(Config.Dir.Config, "CDB_Sync.xml"))
        End If

        If FileIO.FileSystem.FileExists(Config.GetFile(Config.Dir.Config, "MPSync.xml")) Then

            '  get settings from XML configuration file

            Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))

                _version = XMLreader.GetValueAsString("Plugin", "version", "0")
                cb_databases.Checked = XMLreader.GetValueAsString("Plugin", "databases", True)
                cb_thumbs.Checked = XMLreader.GetValueAsString("Plugin", "thumbs", True)

                _databases = XMLreader.GetValueAsString("DB Settings", "databases", Nothing)
                _thumbs = XMLreader.GetValueAsString("Thumbs Settings", "thumbs", Nothing)

                tb_db_client_path.Text = XMLreader.GetValueAsString("DB Path", "client", Nothing)
                tb_db_server_path.Text = XMLreader.GetValueAsString("DB Path", "server", Nothing)
                _clicks_db = XMLreader.GetValueAsInt("DB Path", "direction", 0)
                _db_sync_method = XMLreader.GetValueAsInt("DB Path", "method", 0)
                nud_db_sync.Value = XMLreader.GetValueAsInt("DB Settings", "sync periodicity", 15)
                cb_db_sync.Text = XMLreader.GetValueAsString("DB Settings", "sync periodicity value", "minutes")
                cb_db_pause.Checked = XMLreader.GetValueAsString("DB Settings", "pause while playing", False)

                tb_thumbs_client_path.Text = XMLreader.GetValueAsString("Thumbs Path", "client", Nothing)
                tb_thumbs_server_path.Text = XMLreader.GetValueAsString("Thumbs Path", "server", Nothing)
                _clicks_thumbs = XMLreader.GetValueAsInt("Thumbs Path", "direction", 0)
                _thumbs_sync_method = XMLreader.GetValueAsInt("Thumbs Path", "method", 0)
                nud_thumbs_sync.Value = XMLreader.GetValueAsInt("Thumbs Settings", "sync periodicity", 15)
                cb_thumbs_sync.Text = XMLreader.GetValueAsString("Thumbs Settings", "sync periodicity value", "minutes")
                cb_thumbs_pause.Checked = XMLreader.GetValueAsString("Thumbs Settings", "pause while playing", False)

            End Using

        End If

        b_db_direction.Image = i_direction(_clicks_db)
        cb_db_sync_method.Text = i_method(_db_sync_method)
        b_thumbs_direction.Image = i_direction(_clicks_thumbs)
        cb_thumbs_sync_method.Text = i_method(_thumbs_sync_method)

        rb_all_db.Checked = (_databases = Nothing)
        rb_specific_db.Checked = Not rb_all_db.Checked

        rb_all_thumbs.Checked = (_thumbs = Nothing)
        rb_specific_thumbs.Checked = Not rb_all_thumbs.Checked

    End Sub

    Private Sub cb_databases_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cb_databases.CheckedChanged
        If cb_databases.Checked Then tc_main.TabPages.Insert(1, tp_database) Else tc_main.TabPages.Remove(tp_database)
    End Sub

    Private Sub cb_thumbs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cb_thumbs.CheckedChanged
        If cb_thumbs.Checked Then tc_main.TabPages.Add(tp_thumbs) Else tc_main.TabPages.Remove(tp_thumbs)
    End Sub

    Private Sub b_browse_Click(sender As System.Object, e As System.EventArgs) Handles b_db_client.Click, b_thumbs_client.Click, b_db_server.Click, b_thumbs_server.Click

        Dim objShell As Object
        Dim objFolder As Object
        Dim msg As String = Nothing

        On Error GoTo SubExit

        If DirectCast(sender, System.Windows.Forms.Button).Name = "b_db_client" Then
            msg = "Please select folder containing source databases"
        ElseIf DirectCast(sender, System.Windows.Forms.Button).Name = "b_thumbs_client" Then
            msg = "Please select folder containing source thumbs"
        ElseIf DirectCast(sender, System.Windows.Forms.Button).Name = "b_db_server" Then
            msg = "Please select folder containing target databases"
        ElseIf DirectCast(sender, System.Windows.Forms.Button).Name = "b_thumbs_server" Then
            msg = "Please select folder containing target thumbs"
        End If

        objShell = CreateObject("Shell.Application")
        objFolder = objShell.BrowseForFolder(0, msg, 0)

        If DirectCast(sender, System.Windows.Forms.Button).Name = "b_db_client" Then
            If IsError(objFolder.Items.Item.Path) Then
                tb_db_client_path.Text = CStr(objFolder)
            Else
                tb_db_client_path.Text = objFolder.Items.Item.Path
            End If
        ElseIf DirectCast(sender, System.Windows.Forms.Button).Name = "b_thumbs_client" Then
            If IsError(objFolder.Items.Item.Path) Then
                tb_thumbs_client_path.Text = CStr(objFolder)
            Else
                tb_thumbs_client_path.Text = objFolder.Items.Item.Path
            End If
        ElseIf DirectCast(sender, System.Windows.Forms.Button).Name = "b_db_server" Then
            If IsError(objFolder.Items.Item.Path) Then
                tb_db_server_path.Text = CStr(objFolder)
            Else
                tb_db_server_path.Text = objFolder.Items.Item.Path
            End If
        ElseIf DirectCast(sender, System.Windows.Forms.Button).Name = "b_thumbs_server" Then
            If IsError(objFolder.Items.Item.Path) Then
                tb_thumbs_server_path.Text = CStr(objFolder)
            Else
                tb_thumbs_server_path.Text = objFolder.Items.Item.Path
            End If
        End If

SubExit:
    End Sub

    Private Sub b_direction_Click(sender As System.Object, e As System.EventArgs) Handles b_db_direction.Click, b_thumbs_direction.Click

        If DirectCast(sender, System.Windows.Forms.Button).Name = "b_db_direction" Then
            _clicks_db += 1

            If _clicks_db > 2 Then _clicks_db = 0

            If _clicks_db = 0 Then
                cb_db_sync_method.Items.RemoveAt(0)
            Else
                cb_db_sync_method.Items.Insert(0, i_method(0))
            End If

            cb_db_sync_method.Refresh()

            b_db_direction.Image = i_direction(_clicks_db)
            b_db_direction.Refresh()
        ElseIf DirectCast(sender, System.Windows.Forms.Button).Name = "b_thumbs_direction" Then
            _clicks_thumbs += 1

            If _clicks_thumbs > 2 Then _clicks_thumbs = 0

            If _clicks_thumbs = 0 Then
                cb_thumbs_sync_method.Items.RemoveAt(0)
            Else
                cb_thumbs_sync_method.Items.Insert(0, i_method(0))
            End If

            cb_thumbs_sync_method.Refresh()

            b_thumbs_direction.Image = i_direction(_clicks_thumbs)
            b_thumbs_direction.Refresh()
        End If

    End Sub

    Private Sub b_save_Click(sender As System.Object, e As System.EventArgs) Handles b_save.Click

        If FileIO.FileSystem.FileExists(Config.GetFile(Config.Dir.Config, "MPSync.xml")) Then FileIO.FileSystem.DeleteFile(Config.GetFile(Config.Dir.Config, "MPSync.xml"))

        _databases = Nothing

        If rb_specific_db.Checked Then
            For x As Integer = 0 To (clb_databases.CheckedItems.Count - 1)
                _databases += clb_databases.GetItemText(clb_databases.CheckedItems(x)) & "|"
            Next
        End If

        _thumbs = Nothing

        If rb_specific_thumbs.Checked Then
            For x As Integer = 0 To (clb_thumbs.CheckedItems.Count - 1)
                _thumbs += clb_thumbs.GetItemText(clb_thumbs.CheckedItems(x)) & "|"
            Next
        End If

        Using XMLwriter As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))

            XMLwriter.SetValue("Plugin", "version", _curversion)
            XMLwriter.SetValue("Plugin", "databases", cb_databases.Checked)
            XMLwriter.SetValue("Plugin", "thumbs", cb_thumbs.Checked)

            XMLwriter.SetValue("DB Path", "client", tb_db_client_path.Text)
            XMLwriter.SetValue("DB Path", "server", tb_db_server_path.Text)
            XMLwriter.SetValue("DB Path", "direction", _clicks_db.ToString)
            XMLwriter.SetValue("DB Path", "method", Array.IndexOf(i_method, cb_db_sync_method.Text))

            XMLwriter.SetValue("DB Settings", "sync periodicity", nud_db_sync.Value)
            XMLwriter.SetValue("DB Settings", "sync periodicity value", cb_db_sync.Text)
            XMLwriter.SetValue("DB Settings", "pause while playing", cb_db_pause.Checked)
            XMLwriter.SetValue("DB Settings", "databases", _databases)

            XMLwriter.SetValue("Thumbs Path", "client", tb_thumbs_client_path.Text)
            XMLwriter.SetValue("Thumbs Path", "server", tb_thumbs_server_path.Text)
            XMLwriter.SetValue("Thumbs Path", "direction", _clicks_thumbs.ToString)
            XMLwriter.SetValue("Thumbs Path", "method", Array.IndexOf(i_method, cb_thumbs_sync_method.Text))

            XMLwriter.SetValue("Thumbs Settings", "sync periodicity", nud_thumbs_sync.Value)
            XMLwriter.SetValue("Thumbs Settings", "sync periodicity value", cb_thumbs_sync.Text)
            XMLwriter.SetValue("Thumbs Settings", "pause while playing", cb_thumbs_pause.Checked)
            XMLwriter.SetValue("Thumbs Settings", "thumbs", _thumbs)

        End Using

        MediaPortal.Profile.Settings.SaveCache()

        MsgBox("Configuration Saved", MsgBoxStyle.Information, "Central DB Synchronise")

    End Sub

    Private Sub tb_client_path_TextChanged(sender As System.Object, e As System.EventArgs) Handles tb_db_client_path.TextChanged

        If FileSystem.DirectoryExists(tb_db_client_path.Text) Then

            Dim all_checked As Boolean = (_databases = Nothing)

            clb_databases.Items.Clear()

            For Each database As String In IO.Directory.GetFiles(tb_db_client_path.Text, "*.db3")
                If all_checked Then
                    clb_databases.Items.Add(IO.Path.GetFileName(database), True)
                Else
                    clb_databases.Items.Add(IO.Path.GetFileName(database), _databases.Contains(IO.Path.GetFileName(database)))
                End If
            Next

        End If

    End Sub

    Private Sub tb_thumbs_client_path_TextChanged(sender As System.Object, e As System.EventArgs) Handles tb_thumbs_client_path.TextChanged

        If FileSystem.DirectoryExists(tb_thumbs_client_path.Text) Then

            Dim all_checked As Boolean = (_thumbs = Nothing)

            clb_thumbs.Items.Clear()

            For Each folder As String In IO.Directory.GetDirectories(tb_thumbs_client_path.Text)
                If all_checked Then
                    clb_thumbs.Items.Add(IO.Path.GetFileName(folder), True)
                Else
                    clb_thumbs.Items.Add(IO.Path.GetFileName(folder), _thumbs.Contains(IO.Path.GetFileName(folder)))
                End If
            Next

        End If

    End Sub

    Private Sub rb_all_db_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_all_db.CheckedChanged
        clb_databases.Enabled = Not rb_all_db.Checked
    End Sub

    Private Sub rb_specific_db_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_specific_db.CheckedChanged
        clb_databases.Enabled = rb_specific_db.Checked
    End Sub

    Private Sub rb_all_thumbs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_all_thumbs.CheckedChanged
        clb_thumbs.Enabled = Not rb_all_thumbs.Checked
    End Sub

    Private Sub rb_specific_thumbs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_specific_thumbs.CheckedChanged
        clb_thumbs.Enabled = rb_specific_thumbs.Checked
    End Sub

    Private Sub b_sync_now_Click(sender As System.Object, e As System.EventArgs) Handles b_sync_now.Click
        syncnow = True
        Dim cdb As New MPSync_process
        cdb.MPSyncProcess()
        syncnow = False
    End Sub

    Private Sub MPSync_settings_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        ' initialize version
        _curversion = "0.0.0.5"
        Me.Text = Me.Text & _curversion

        ' initialize direction images
        i_direction(0) = My.Resources.sync_both
        i_direction(1) = My.Resources.sync_master2slave
        i_direction(2) = My.Resources.sync_slave2master

        ' temporarily remove tabs to add according to settings
        tc_main.TabPages.Remove(tp_database)
        tc_main.TabPages.Remove(tp_thumbs)

        ' initialize methods
        i_method = {"Propagate both additions and deletions", "Propagate additions only", "Propagate deletions only"}

        For x = 0 To 2
            cb_db_sync_method.Items.Add(i_method(x))
            cb_thumbs_sync_method.Items.Add(i_method(x))
        Next

        ' extract System.Data.SQLite.dll from resources to application library
        Dim dll As String = IO.Directory.GetCurrentDirectory() & "\System.Data.SQLite.dll"
        If Not IO.File.Exists(dll) Then My.Computer.FileSystem.WriteAllBytes(dll, My.Resources.System_Data_SQLite, False)

        Get_Settings()

    End Sub

End Class
