Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports Microsoft.VisualBasic.FileIO

Imports System.Data.SQLite

Public Class CDB_Sync_settings

    Dim i_direction(2) As Image
    Dim i_method(2) As String
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

    Private Sub b_source_db_Click(sender As System.Object, e As System.EventArgs) Handles b_source_db.Click

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

    Private Sub b_target_db_Click(sender As System.Object, e As System.EventArgs) Handles b_target_db.Click

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

    Private Sub b_direction_Click(sender As System.Object, e As System.EventArgs) Handles b_direction.Click

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

            tb_source_path.Text = XMLreader.GetValueAsString("Path", "source", Nothing)
            tb_target_path.Text = XMLreader.GetValueAsString("Path", "target", Nothing)
            _clicks = XMLreader.GetValueAsInt("Path", "direction", 0)
            _sync_method = XMLreader.GetValueAsInt("Path", "method", 0)

        End Using

        b_direction.Image = i_direction(_clicks)
        cb_sync_method.Text = i_method(_sync_method)

    End Sub

    Private Sub b_save_Click(sender As System.Object, e As System.EventArgs) Handles b_save.Click

        Using XMLwriter As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "CDB_Sync.xml"))

            XMLwriter.SetValue("Path", "source", tb_source_path.Text)
            XMLwriter.SetValue("Path", "target", tb_target_path.Text)
            XMLwriter.SetValue("Path", "direction", _clicks.ToString)
            XMLwriter.SetValue("Path", "method", Array.IndexOf(i_method, cb_sync_method.Text))

        End Using

        MediaPortal.Profile.Settings.SaveCache()

        MsgBox("Configuration Saved", MsgBoxStyle.Information, "Central DB Synchronise")

    End Sub

    Private Sub CDB_Sync_settings_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        ' initialize version
        Me.Text = Me.Text & "0.0.0.1"

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
