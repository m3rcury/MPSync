Imports MediaPortal.Configuration
Imports MediaPortal.GUI.Library

Imports System.Data.SQLite

Public Class MPSync_settings

    Dim i_direction(2) As Image
    Dim i_method(2), _databases, _thumbs, _watched_dbs, _curversion, _version As String
    Dim _clicks_db, _clicks_thumbs, _db_sync_method, _thumbs_sync_method As Integer

    Public Shared syncnow As Boolean = False
    Public Delegate Sub lb_statusItemsAddInvoker(ByVal text As String)

    Public Structure Watched
        Dim database As String
        Dim tables As Array
    End Structure

    Public i_watched(2) As Watched

    Public WriteOnly Property SetWatched
        Set(value)
            i_watched(0).database = "movingpictures.db3"
            i_watched(0).tables = {"user_movie_settings"}
            i_watched(1).database = "MusicDatabaseV12.db3"
            i_watched(1).tables = {"tracks"}
            i_watched(2).database = "TVSeriesDatabase4.db3"
            i_watched(2).tables = {"local_episodes", "online_episodes", "online_series", "season"}
        End Set
    End Property

    Public ReadOnly Property getDatabase As Array
        Get
            If i_watched(0).database = Nothing Then SetWatched = Nothing
            Dim database(UBound(i_watched)) As String
            For x As Integer = 0 To UBound(i_watched)
                database(x) = i_watched(x).database
            Next
            Return database
        End Get
    End Property

    Public ReadOnly Property getTables(ByVal database As String) As Array
        Get
            If i_watched(0).database = Nothing Then SetWatched = Nothing
            Dim y As Integer = Array.IndexOf(getDatabase, database)
            If y <> -1 Then
                Dim tables(UBound(i_watched(y).tables)) As String
                For x As Integer = 0 To UBound(i_watched(y).tables)
                    tables(x) = i_watched(y).tables(x)
                Next
                Return tables
            Else
                Return Nothing
            End If
        End Get
    End Property

    Private Sub populate_checkedlistbox(ByRef clb As CheckedListBox, ByVal path As String, Optional ByVal searchpattern As String = Nothing)

        If IO.Directory.Exists(path) Then

            clb.Items.Clear()

            If searchpattern <> Nothing Then

                Dim all_checked As Boolean = (_databases = Nothing)

                For Each database As String In IO.Directory.getFiles(path, searchpattern)
                    If all_checked Then
                        clb.Items.Add(IO.Path.getFileName(database), True)
                    Else
                        clb.Items.Add(IO.Path.getFileName(database), _databases.Contains(IO.Path.getFileName(database)))
                    End If
                Next

            Else

                Dim all_checked As Boolean = (_thumbs = Nothing)

                For Each folder As String In IO.Directory.getDirectories(path)
                    If all_checked Then
                        clb.Items.Add(IO.Path.getFileName(folder), True)
                    Else
                        clb.Items.Add(IO.Path.getFileName(folder), _thumbs.Contains(IO.Path.getFileName(folder)))
                    End If
                Next

            End If

        End If

    End Sub

    Private Sub populate_watchedchecklistbox(ByRef clb As CheckedListBox)

        clb.Items.Clear()

        Dim all_checked As Boolean = (_watched_dbs = Nothing)

        For x As Integer = 0 To (clb_databases.CheckedItems.Count - 1)
            Dim y As Integer = Array.IndexOf(getDatabase, clb_databases.getItemText(clb_databases.CheckedItems(x)))
            If y >= 0 Then
                If all_checked Then
                    clb.Items.Add(i_watched(y).database, True)
                Else
                    clb.Items.Add(i_watched(y).database, _watched_dbs.Contains(i_watched(y).database))
                End If
            End If
        Next

    End Sub

    Private Sub getSettings()

        If IO.File.Exists(Config.getFile(Config.Dir.Config, "CDB_Sync.xml")) Then IO.File.Delete(Config.getFile(Config.Dir.Config, "CDB_Sync.xml"))

        If IO.File.Exists(Config.getFile(Config.Dir.Config, "MPSync.xml")) Then

            '  get settings from XML configuration file

            Using XMLreader As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.getFile(Config.Dir.Config, "MPSync.xml"))

                _version = XMLreader.getValueAsString("Plugin", "version", "0")
                cb_databases.Checked = XMLreader.getValueAsString("Plugin", "databases", True)
                cb_thumbs.Checked = XMLreader.getValueAsString("Plugin", "thumbs", True)
                cb_debug.Checked = XMLreader.getValueAsString("Plugin", "debug", False)

                _databases = XMLreader.getValueAsString("DB Settings", "databases", Nothing)
                _watched_dbs = XMLreader.getValueAsString("DB Settings", "watched databases", Nothing)
                _thumbs = XMLreader.getValueAsString("Thumbs Settings", "thumbs", Nothing)

                _clicks_db = XMLreader.getValueAsInt("DB Path", "direction", 1)
                tb_db_client_path.Text = XMLreader.getValueAsString("DB Path", "client", Nothing)
                tb_db_server_path.Text = XMLreader.getValueAsString("DB Path", "server", Nothing)
                _db_sync_method = XMLreader.getValueAsInt("DB Path", "method", 0)
                nud_db_sync.Value = XMLreader.getValueAsInt("DB Settings", "sync periodicity", 15)
                cb_db_sync.Text = XMLreader.getValueAsString("DB Settings", "sync periodicity value", "minutes")
                cb_db_pause.Checked = XMLreader.getValueAsString("DB Settings", "pause while playing", False)
                cb_watched.Checked = XMLreader.getValueAsString("DB Settings", "watched", False)

                _clicks_thumbs = XMLreader.getValueAsInt("Thumbs Path", "direction", 1)
                tb_thumbs_client_path.Text = XMLreader.getValueAsString("Thumbs Path", "client", Nothing)
                tb_thumbs_server_path.Text = XMLreader.getValueAsString("Thumbs Path", "server", Nothing)
                _thumbs_sync_method = XMLreader.getValueAsInt("Thumbs Path", "method", 0)
                nud_thumbs_sync.Value = XMLreader.getValueAsInt("Thumbs Settings", "sync periodicity", 15)
                cb_thumbs_sync.Text = XMLreader.getValueAsString("Thumbs Settings", "sync periodicity value", "minutes")
                cb_thumbs_pause.Checked = XMLreader.getValueAsString("Thumbs Settings", "pause while playing", False)

            End Using

        End If

        b_db_direction.Image = i_direction(_clicks_db)
        cb_db_sync_method.Text = i_method(_db_sync_method)
        b_thumbs_direction.Image = i_direction(_clicks_thumbs)
        cb_thumbs_sync_method.Text = i_method(_thumbs_sync_method)

        rb_all_db.Checked = (_databases = Nothing)
        rb_specific_db.Checked = Not rb_all_db.Checked
        rb_w_all.Checked = (_watched_dbs = Nothing)
        rb_w_specific.Checked = Not rb_w_all.Checked

        rb_all_thumbs.Checked = (_thumbs = Nothing)
        rb_specific_thumbs.Checked = Not rb_all_thumbs.Checked

    End Sub

    Private Sub setSettings()

        If IO.File.Exists(Config.GetFile(Config.Dir.Config, "MPSync.xml")) Then IO.File.Delete(Config.GetFile(Config.Dir.Config, "MPSync.xml"))

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

        _watched_dbs = Nothing

        If rb_w_specific.Checked Then
            For x As Integer = 0 To (clb_watched.CheckedItems.Count - 1)
                _watched_dbs += clb_watched.GetItemText(clb_watched.CheckedItems(x)) & "|"
            Next
        End If

        Using XMLwriter As MediaPortal.Profile.Settings = New MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MPSync.xml"))

            XMLwriter.SetValue("Plugin", "version", _curversion)
            XMLwriter.SetValue("Plugin", "databases", cb_databases.Checked)
            XMLwriter.SetValue("Plugin", "thumbs", cb_thumbs.Checked)
            XMLwriter.SetValue("Plugin", "debug", cb_debug.Checked)

            XMLwriter.SetValue("DB Path", "client", tb_db_client_path.Text)
            XMLwriter.SetValue("DB Path", "server", tb_db_server_path.Text)
            XMLwriter.SetValue("DB Path", "direction", _clicks_db.ToString)
            XMLwriter.SetValue("DB Path", "method", Array.IndexOf(i_method, cb_db_sync_method.Text))

            XMLwriter.SetValue("DB Settings", "sync periodicity", nud_db_sync.Value)
            XMLwriter.SetValue("DB Settings", "sync periodicity value", cb_db_sync.Text)
            XMLwriter.SetValue("DB Settings", "pause while playing", cb_db_pause.Checked)
            XMLwriter.SetValue("DB Settings", "databases", _databases)
            XMLwriter.SetValue("DB Settings", "watched", cb_watched.Checked)
            XMLwriter.SetValue("DB Settings", "watched databases", _watched_dbs)

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

    End Sub

    Private Sub cb_databases_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cb_databases.CheckedChanged
        If cb_databases.Checked Then tc_main.TabPages.Insert(1, tp_database) Else tc_main.TabPages.Remove(tp_database)
    End Sub

    Private Sub cb_thumbs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cb_thumbs.CheckedChanged
        If cb_thumbs.Checked Then tc_main.TabPages.Add(tp_thumbs) Else tc_main.TabPages.Remove(tp_thumbs)
    End Sub

    Private Sub cb_watched_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles cb_watched.CheckedChanged

        If cb_watched.Checked Then
            tc_database.TabPages.Add(tp_watched)
            populate_watchedchecklistbox(clb_watched)
        Else
            tc_database.TabPages.Remove(tp_watched)
        End If

    End Sub

    Private Sub b_browse_Click(sender As System.Object, e As System.EventArgs) Handles b_db_client.Click, b_thumbs_client.Click, b_db_server.Click, b_thumbs_server.Click

        Dim objShell As Object
        Dim objFolder As Object
        Dim msg As String = Nothing

        Try

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

        Catch ex As Exception
        End Try

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

            If _clicks_db <> 2 Then
                populate_checkedlistbox(clb_databases, tb_db_client_path.Text, "*.db3")
            ElseIf _clicks_db = 2 Then
                populate_checkedlistbox(clb_databases, tb_db_server_path.Text, "*.db3")
            End If

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

            If _clicks_thumbs <> 2 Then
                populate_checkedlistbox(clb_thumbs, tb_thumbs_client_path.Text)
            ElseIf _clicks_thumbs = 2 Then
                populate_checkedlistbox(clb_thumbs, tb_thumbs_server_path.Text)
            End If

        End If

    End Sub

    Private Sub b_save_Click(sender As System.Object, e As System.EventArgs) Handles b_save.Click

        setSettings()

        MsgBox("Configuration Saved", MsgBoxStyle.Information, "Central DB Synchronise")

    End Sub

    Private Sub tb_path_TextChanged(sender As System.Object, e As System.EventArgs) Handles tb_db_client_path.TextChanged, tb_db_server_path.TextChanged, tb_thumbs_client_path.TextChanged, tb_thumbs_server_path.TextChanged

        If DirectCast(sender, System.Windows.Forms.TextBox).Name = "tb_db_client_path" And _clicks_db <> 2 Then
            populate_checkedlistbox(clb_databases, tb_db_client_path.Text, "*.db3")
        ElseIf DirectCast(sender, System.Windows.Forms.TextBox).Name = "tb_db_server_path" And _clicks_db = 2 Then
            populate_checkedlistbox(clb_databases, tb_db_server_path.Text, "*.db3")
        ElseIf DirectCast(sender, System.Windows.Forms.TextBox).Name = "tb_thumbs_client_path" And _clicks_thumbs <> 2 Then
            populate_checkedlistbox(clb_thumbs, tb_thumbs_client_path.Text)
        ElseIf DirectCast(sender, System.Windows.Forms.TextBox).Name = "tb_thumbs_server_path" And _clicks_thumbs = 2 Then
            populate_checkedlistbox(clb_thumbs, tb_thumbs_server_path.Text)
        End If

    End Sub

    Private Sub rb_all_db_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_all_db.CheckedChanged
        clb_databases.Enabled = Not rb_all_db.Checked
        populate_watchedchecklistbox(clb_watched)
    End Sub

    Private Sub rb_specific_db_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_specific_db.CheckedChanged
        clb_databases.Enabled = rb_specific_db.Checked
        populate_watchedchecklistbox(clb_watched)
    End Sub

    Private Sub rb_all_thumbs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_all_thumbs.CheckedChanged
        clb_thumbs.Enabled = Not rb_all_thumbs.Checked
    End Sub

    Private Sub rb_specific_thumbs_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_specific_thumbs.CheckedChanged
        clb_thumbs.Enabled = rb_specific_thumbs.Checked
    End Sub

    Private Sub clb_databases_SelectedValueChanged(sender As Object, e As System.EventArgs) Handles clb_databases.SelectedValueChanged
        populate_watchedchecklistbox(clb_watched)
    End Sub

    Private Sub rb_w_all_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_w_all.CheckedChanged
        clb_watched.Enabled = Not rb_w_all.Checked
    End Sub

    Private Sub rb_w_specific_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles rb_w_specific.CheckedChanged
        clb_watched.Enabled = rb_w_specific.Checked
    End Sub

    Private Sub b_sync_now_Click(sender As System.Object, e As System.EventArgs) Handles b_sync_now.Click

        setSettings()

        syncnow = True

        'tc_main.TabPages.Add(tp_syncnow)
        'tc_main.SelectedTab = tp_syncnow
        'lb_status.Items.Add("Synchronization started")

        Dim cdb As New MPSync_process
        cdb.MPSyncProcess()

    End Sub

    Public Sub lb_statusItemsAdd(ByVal text As String)
        If lb_status.InvokeRequired = True Then
            lb_status.Invoke(New lb_statusItemsAddInvoker(AddressOf lb_statusItemsAdd), text)
        Else
            lb_status.Items.Add(text)
        End If
    End Sub

    Private Sub MPSync_settings_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        ' initialize version
        _curversion = "0.0.0.10"
        Me.Text = Me.Text & _curversion

        ' initialize direction images
        i_direction(0) = My.Resources.sync_both
        i_direction(1) = My.Resources.sync_master2slave
        i_direction(2) = My.Resources.sync_slave2master

        ' temporarily remove tabs to add according to settings/activity
        tc_main.TabPages.Remove(tp_database)
        tc_main.TabPages.Remove(tp_thumbs)
        tc_main.TabPages.Remove(tp_syncnow)
        tc_database.TabPages.Remove(tp_watched)

        ' initialize methods
        i_method = {"Propagate both additions and deletions", "Propagate additions only", "Propagate deletions only"}

        For x = 0 To 2
            cb_db_sync_method.Items.Add(i_method(x))
            cb_thumbs_sync_method.Items.Add(i_method(x))
        Next

        getSettings()

    End Sub

End Class
