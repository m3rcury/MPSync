Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports Microsoft.VisualBasic.FileIO

Imports System.Data.SQLite

Public Class CDB_Sync_settings

    Dim i_direction(2) As Image
    Dim i_method(2), _databases As String
    Dim _clicks, _sync_method As Integer

    Public Shared Function Get_Paths(ByRef database As String, ByRef thumbs As String) As Boolean

        database = Nothing
        thumbs = Nothing

        Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Base, "MediaPortalDirs.xml"))

            database = XMLreader.GetValueAsString("Path", "Database", Config.GetFolder(Config.Dir.Database))
            thumbs = XMLreader.GetValueAsString("Path", "Thumbs", Config.GetFolder(Config.Dir.Thumbs))

        End Using

        Return database <> Nothing And thumbs <> Nothing

    End Function

    Private Sub b_source_db_Click(sender As System.Object, e As System.EventArgs)

        Dim objShell As Object
        Dim objFolder As Object

        On Error GoTo SubExit

        objShell = CreateObject("Shell.Application")
        objFolder = objShell.BrowseForFolder(0, "Please select folder containing source databases", 0)

        If IsError(objFolder.Items.Item.Path) Then
            tb_source_path.Text = CStr(objFolder)
        Else
            tb_source_path.Text = objFolder.Items.Item.Path
        End If

SubExit:
    End Sub

    Private Sub b_target_db_Click(sender As System.Object, e As System.EventArgs)

        Dim objShell As Object
        Dim objFolder As Object

        On Error GoTo SubExit

        objShell = CreateObject("Shell.Application")
        objFolder = objShell.BrowseForFolder(0, "Please select folder containing target databases", 0)

        If IsError(objFolder.Items.Item.Path) Then
            tb_target_path.Text = CStr(objFolder)
        Else
            tb_target_path.Text = objFolder.Items.Item.Path
        End If

SubExit:
    End Sub

    Private Sub b_direction_Click(sender As System.Object, e As System.EventArgs)

        _clicks += 1

        If _clicks > 2 Then _clicks = 0

        If _clicks = 0 Then
            cb_sync_method.Items.RemoveAt(0)
        Else
            cb_sync_method.Items.Insert(0, i_method(0))
        End If

        cb_sync_method.Refresh()

        b_direction.Image = i_direction(_clicks)
        b_direction.Refresh()

    End Sub

    Private Sub Get_Settings()

        '  get settings from XML configuration file

        Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "CDB_Sync.xml"))

            _databases = XMLreader.GetValueAsString("Settings", "databases", Nothing)

            tb_source_path.Text = XMLreader.GetValueAsString("Path", "source", Nothing)
            tb_target_path.Text = XMLreader.GetValueAsString("Path", "target", Nothing)
            _clicks = XMLreader.GetValueAsInt("Path", "direction", 0)
            _sync_method = XMLreader.GetValueAsInt("Path", "method", 0)
            nud_sync.Value = XMLreader.GetValueAsInt("Settings", "sync periodicity", 1)
            cb_sync.Text = XMLreader.GetValueAsString("Settings", "sync periodicity value", "minutes")

        End Using

        b_direction.Image = i_direction(_clicks)
        cb_sync_method.Text = i_method(_sync_method)

        rb_all.Checked = (_databases = Nothing)
        rb_specific.Checked = Not rb_all.Checked

    End Sub

    Private Sub b_save_Click(sender As System.Object, e As System.EventArgs) Handles b_save.Click

        _databases = Nothing

        If rb_specific.Checked Then
            For x As Integer = 0 To (clb_databases.CheckedItems.Count - 1)
                _databases += clb_databases.GetItemText(clb_databases.CheckedItems(x)) & "|"
            Next
        End If

        Using XMLwriter As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "CDB_Sync.xml"))

            XMLwriter.SetValue("Path", "source", tb_source_path.Text)
            XMLwriter.SetValue("Path", "target", tb_target_path.Text)
            XMLwriter.SetValue("Path", "direction", _clicks.ToString)
            XMLwriter.SetValue("Path", "method", Array.IndexOf(i_method, cb_sync_method.Text))

            XMLwriter.SetValue("Settings", "sync periodicity", nud_sync.Value)
            XMLwriter.SetValue("Settings", "sync periodicity value", cb_sync.Text)
            XMLwriter.SetValue("Settings", "databases", _databases)

        End Using

        MediaPortal.Profile.Settings.SaveCache()

        MsgBox("Configuration Saved", MsgBoxStyle.Information, "Central DB Synchronise")

    End Sub

    Private Sub b_test_Click(sender As System.Object, e As System.EventArgs) Handles b_test.Click
        Dim cdb As New CDB_Sync_process
        cdb.CDB_SyncProcess()
    End Sub

    Private Sub tb_source_path_TextChanged(sender As System.Object, e As System.EventArgs) Handles tb_source_path.TextChanged

        If FileSystem.DirectoryExists(tb_source_path.Text) Then

            Dim all_checked As Boolean = (_databases = Nothing)

            clb_databases.Items.Clear()

            For Each database As String In IO.Directory.GetFiles(tb_source_path.Text, "*.db3")
                If all_checked Then
                    clb_databases.Items.Add(IO.Path.GetFileName(database), True)
                Else
                    clb_databases.Items.Add(IO.Path.GetFileName(database), _databases.Contains(IO.Path.GetFileName(database)))
                End If
            Next

        End If

    End Sub

    Private Sub rb_all_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_all.CheckedChanged
        clb_databases.Enabled = Not rb_all.Checked
    End Sub

    Private Sub rb_specific_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_specific.CheckedChanged
        clb_databases.Enabled = rb_specific.Checked
    End Sub

    Private Sub CDB_Sync_settings_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        ' check if debug enabled
        Dim args() As String

        args = Environment.GetCommandLineArgs()
        If args(1) = "Debug" Then b_test.Visible = True

        ' initialize version
        Me.Text = Me.Text & "0.0.0.3"

        ' initialize direction images
        i_direction(0) = My.Resources.sync_both
        i_direction(1) = My.Resources.sync_master2slave
        i_direction(2) = My.Resources.sync_slave2master

        ' initialize methods
        i_method = {"Propagate both additions and deletions", "Propagate additions only", "Propagate deletions only"}

        For x = 0 To 2
            cb_sync_method.Items.Add(i_method(x))
        Next

        ' extract System.Data.SQLite.dll from resources to application library
        Dim dll As String = IO.Directory.GetCurrentDirectory() & "\System.Data.SQLite.dll"
        If Not IO.File.Exists(dll) Then My.Computer.FileSystem.WriteAllBytes(dll, My.Resources.System_Data_SQLite, False)

        Get_Settings()

    End Sub

End Class
