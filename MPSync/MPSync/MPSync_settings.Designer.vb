<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MPSync_settings
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MPSync_settings))
        Me.b_save = New System.Windows.Forms.Button()
        Me.tb_master_path = New System.Windows.Forms.TextBox()
        Me.tb_slave_path = New System.Windows.Forms.TextBox()
        Me.l_copyright = New System.Windows.Forms.Label()
        Me.b_sync_now = New System.Windows.Forms.Button()
        Me.tc_main = New System.Windows.Forms.TabControl()
        Me.tp_settings = New System.Windows.Forms.TabPage()
        Me.cb_thumbs = New System.Windows.Forms.CheckBox()
        Me.cb_databases = New System.Windows.Forms.CheckBox()
        Me.tp_database = New System.Windows.Forms.TabPage()
        Me.tc_database = New System.Windows.Forms.TabControl()
        Me.tp_db_paths = New System.Windows.Forms.TabPage()
        Me.cb_db_sync_method = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.b_db_server = New System.Windows.Forms.Button()
        Me.tb_db_server_path = New System.Windows.Forms.TextBox()
        Me.tb_db_client_path = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.b_db_client = New System.Windows.Forms.Button()
        Me.tp_db_advancedsettings = New System.Windows.Forms.TabPage()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.rb_specific_db = New System.Windows.Forms.RadioButton()
        Me.rb_all_db = New System.Windows.Forms.RadioButton()
        Me.cb_db_sync = New System.Windows.Forms.ComboBox()
        Me.nud_db_sync = New System.Windows.Forms.NumericUpDown()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.clb_databases = New System.Windows.Forms.CheckedListBox()
        Me.tp_thumbs = New System.Windows.Forms.TabPage()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.cb_thumbs_sync_method = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.b_thumbs_server = New System.Windows.Forms.Button()
        Me.tb_thumbs_server_path = New System.Windows.Forms.TextBox()
        Me.tb_thumbs_client_path = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.b_thumbs_client = New System.Windows.Forms.Button()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.rb_specific_thumbs = New System.Windows.Forms.RadioButton()
        Me.rb_all_thumbs = New System.Windows.Forms.RadioButton()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.clb_thumbs = New System.Windows.Forms.CheckedListBox()
        Me.cb_thumbs_sync = New System.Windows.Forms.ComboBox()
        Me.nud_thumbs_sync = New System.Windows.Forms.NumericUpDown()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.b_db_direction = New System.Windows.Forms.Button()
        Me.b_thumbs_direction = New System.Windows.Forms.Button()
        Me.tc_main.SuspendLayout()
        Me.tp_settings.SuspendLayout()
        Me.tp_database.SuspendLayout()
        Me.tc_database.SuspendLayout()
        Me.tp_db_paths.SuspendLayout()
        Me.tp_db_advancedsettings.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.nud_db_sync, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tp_thumbs.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.nud_thumbs_sync, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'b_save
        '
        Me.b_save.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.b_save.Location = New System.Drawing.Point(262, 235)
        Me.b_save.Name = "b_save"
        Me.b_save.Size = New System.Drawing.Size(75, 23)
        Me.b_save.TabIndex = 58
        Me.b_save.Text = "Save"
        Me.b_save.UseVisualStyleBackColor = True
        '
        'tb_master_path
        '
        Me.tb_master_path.Location = New System.Drawing.Point(12, 45)
        Me.tb_master_path.Name = "tb_master_path"
        Me.tb_master_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_master_path.TabIndex = 51
        '
        'tb_slave_path
        '
        Me.tb_slave_path.Location = New System.Drawing.Point(15, 143)
        Me.tb_slave_path.Name = "tb_slave_path"
        Me.tb_slave_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_slave_path.TabIndex = 54
        '
        'l_copyright
        '
        Me.l_copyright.AutoSize = True
        Me.l_copyright.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.l_copyright.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.l_copyright.Location = New System.Drawing.Point(2, 256)
        Me.l_copyright.Name = "l_copyright"
        Me.l_copyright.Size = New System.Drawing.Size(133, 13)
        Me.l_copyright.TabIndex = 61
        Me.l_copyright.Text = "Copyright © 2013, m3rcury"
        '
        'b_sync_now
        '
        Me.b_sync_now.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.b_sync_now.Location = New System.Drawing.Point(227, 137)
        Me.b_sync_now.Name = "b_sync_now"
        Me.b_sync_now.Size = New System.Drawing.Size(128, 23)
        Me.b_sync_now.TabIndex = 62
        Me.b_sync_now.Text = "Synchronise Now"
        Me.b_sync_now.UseVisualStyleBackColor = True
        '
        'tc_main
        '
        Me.tc_main.Controls.Add(Me.tp_settings)
        Me.tc_main.Controls.Add(Me.tp_database)
        Me.tc_main.Controls.Add(Me.tp_thumbs)
        Me.tc_main.Location = New System.Drawing.Point(5, 3)
        Me.tc_main.Name = "tc_main"
        Me.tc_main.SelectedIndex = 0
        Me.tc_main.Size = New System.Drawing.Size(590, 226)
        Me.tc_main.TabIndex = 64
        '
        'tp_settings
        '
        Me.tp_settings.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.tp_settings.Controls.Add(Me.cb_thumbs)
        Me.tp_settings.Controls.Add(Me.cb_databases)
        Me.tp_settings.Controls.Add(Me.b_sync_now)
        Me.tp_settings.Location = New System.Drawing.Point(4, 22)
        Me.tp_settings.Name = "tp_settings"
        Me.tp_settings.Padding = New System.Windows.Forms.Padding(3)
        Me.tp_settings.Size = New System.Drawing.Size(582, 200)
        Me.tp_settings.TabIndex = 3
        Me.tp_settings.Text = "Settings"
        '
        'cb_thumbs
        '
        Me.cb_thumbs.AutoSize = True
        Me.cb_thumbs.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cb_thumbs.Location = New System.Drawing.Point(191, 66)
        Me.cb_thumbs.Name = "cb_thumbs"
        Me.cb_thumbs.Size = New System.Drawing.Size(190, 20)
        Me.cb_thumbs.TabIndex = 64
        Me.cb_thumbs.Text = "Synchronise Thumbs folder"
        Me.cb_thumbs.UseVisualStyleBackColor = True
        '
        'cb_databases
        '
        Me.cb_databases.AutoSize = True
        Me.cb_databases.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cb_databases.Location = New System.Drawing.Point(191, 40)
        Me.cb_databases.Name = "cb_databases"
        Me.cb_databases.Size = New System.Drawing.Size(201, 20)
        Me.cb_databases.TabIndex = 63
        Me.cb_databases.Text = "Synchronise Database folder"
        Me.cb_databases.UseVisualStyleBackColor = True
        '
        'tp_database
        '
        Me.tp_database.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.tp_database.Controls.Add(Me.tc_database)
        Me.tp_database.Location = New System.Drawing.Point(4, 22)
        Me.tp_database.Name = "tp_database"
        Me.tp_database.Padding = New System.Windows.Forms.Padding(3)
        Me.tp_database.Size = New System.Drawing.Size(582, 200)
        Me.tp_database.TabIndex = 1
        Me.tp_database.Text = "Database"
        '
        'tc_database
        '
        Me.tc_database.Controls.Add(Me.tp_db_paths)
        Me.tc_database.Controls.Add(Me.tp_db_advancedsettings)
        Me.tc_database.Location = New System.Drawing.Point(5, 8)
        Me.tc_database.Name = "tc_database"
        Me.tc_database.SelectedIndex = 0
        Me.tc_database.Size = New System.Drawing.Size(573, 184)
        Me.tc_database.TabIndex = 64
        '
        'tp_db_paths
        '
        Me.tp_db_paths.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.tp_db_paths.Controls.Add(Me.cb_db_sync_method)
        Me.tp_db_paths.Controls.Add(Me.b_db_direction)
        Me.tp_db_paths.Controls.Add(Me.Label2)
        Me.tp_db_paths.Controls.Add(Me.b_db_server)
        Me.tp_db_paths.Controls.Add(Me.tb_db_server_path)
        Me.tp_db_paths.Controls.Add(Me.tb_db_client_path)
        Me.tp_db_paths.Controls.Add(Me.Label1)
        Me.tp_db_paths.Controls.Add(Me.b_db_client)
        Me.tp_db_paths.Location = New System.Drawing.Point(4, 22)
        Me.tp_db_paths.Name = "tp_db_paths"
        Me.tp_db_paths.Padding = New System.Windows.Forms.Padding(3)
        Me.tp_db_paths.Size = New System.Drawing.Size(565, 158)
        Me.tp_db_paths.TabIndex = 0
        Me.tp_db_paths.Text = "Paths"
        '
        'cb_db_sync_method
        '
        Me.cb_db_sync_method.FormattingEnabled = True
        Me.cb_db_sync_method.Location = New System.Drawing.Point(327, 76)
        Me.cb_db_sync_method.Name = "cb_db_sync_method"
        Me.cb_db_sync_method.Size = New System.Drawing.Size(215, 21)
        Me.cb_db_sync_method.TabIndex = 68
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(6, 108)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(94, 13)
        Me.Label2.TabIndex = 66
        Me.Label2.Text = "Server DB path"
        '
        'b_db_server
        '
        Me.b_db_server.Location = New System.Drawing.Point(504, 121)
        Me.b_db_server.Name = "b_db_server"
        Me.b_db_server.Size = New System.Drawing.Size(57, 23)
        Me.b_db_server.TabIndex = 65
        Me.b_db_server.Text = "Browse"
        Me.b_db_server.UseVisualStyleBackColor = True
        '
        'tb_db_server_path
        '
        Me.tb_db_server_path.Location = New System.Drawing.Point(6, 124)
        Me.tb_db_server_path.Name = "tb_db_server_path"
        Me.tb_db_server_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_db_server_path.TabIndex = 64
        '
        'tb_db_client_path
        '
        Me.tb_db_client_path.Location = New System.Drawing.Point(3, 26)
        Me.tb_db_client_path.Name = "tb_db_client_path"
        Me.tb_db_client_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_db_client_path.TabIndex = 61
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(3, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(89, 13)
        Me.Label1.TabIndex = 63
        Me.Label1.Text = "Client DB path"
        '
        'b_db_client
        '
        Me.b_db_client.Location = New System.Drawing.Point(501, 23)
        Me.b_db_client.Name = "b_db_client"
        Me.b_db_client.Size = New System.Drawing.Size(57, 23)
        Me.b_db_client.TabIndex = 62
        Me.b_db_client.Text = "Browse"
        Me.b_db_client.UseVisualStyleBackColor = True
        '
        'tp_db_advancedsettings
        '
        Me.tp_db_advancedsettings.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.tp_db_advancedsettings.Controls.Add(Me.GroupBox1)
        Me.tp_db_advancedsettings.Controls.Add(Me.cb_db_sync)
        Me.tp_db_advancedsettings.Controls.Add(Me.nud_db_sync)
        Me.tp_db_advancedsettings.Controls.Add(Me.Label4)
        Me.tp_db_advancedsettings.Controls.Add(Me.Label3)
        Me.tp_db_advancedsettings.Controls.Add(Me.clb_databases)
        Me.tp_db_advancedsettings.Location = New System.Drawing.Point(4, 22)
        Me.tp_db_advancedsettings.Name = "tp_db_advancedsettings"
        Me.tp_db_advancedsettings.Padding = New System.Windows.Forms.Padding(3)
        Me.tp_db_advancedsettings.Size = New System.Drawing.Size(565, 158)
        Me.tp_db_advancedsettings.TabIndex = 1
        Me.tp_db_advancedsettings.Text = "Advanced Settings"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.rb_specific_db)
        Me.GroupBox1.Controls.Add(Me.rb_all_db)
        Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.Location = New System.Drawing.Point(10, 63)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(273, 76)
        Me.GroupBox1.TabIndex = 14
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = " Synchronise Databases "
        '
        'rb_specific_db
        '
        Me.rb_specific_db.AutoSize = True
        Me.rb_specific_db.Location = New System.Drawing.Point(11, 51)
        Me.rb_specific_db.Name = "rb_specific_db"
        Me.rb_specific_db.Size = New System.Drawing.Size(117, 17)
        Me.rb_specific_db.TabIndex = 1
        Me.rb_specific_db.TabStop = True
        Me.rb_specific_db.Text = "Specific Databases"
        Me.rb_specific_db.UseVisualStyleBackColor = True
        '
        'rb_all_db
        '
        Me.rb_all_db.AutoSize = True
        Me.rb_all_db.Location = New System.Drawing.Point(11, 24)
        Me.rb_all_db.Name = "rb_all_db"
        Me.rb_all_db.Size = New System.Drawing.Size(90, 17)
        Me.rb_all_db.TabIndex = 0
        Me.rb_all_db.TabStop = True
        Me.rb_all_db.Text = "All Databases"
        Me.rb_all_db.UseVisualStyleBackColor = True
        '
        'cb_db_sync
        '
        Me.cb_db_sync.FormattingEnabled = True
        Me.cb_db_sync.Items.AddRange(New Object() {"minutes", "hours"})
        Me.cb_db_sync.Location = New System.Drawing.Point(160, 26)
        Me.cb_db_sync.Name = "cb_db_sync"
        Me.cb_db_sync.Size = New System.Drawing.Size(80, 21)
        Me.cb_db_sync.TabIndex = 13
        Me.cb_db_sync.Text = "minutes"
        '
        'nud_db_sync
        '
        Me.nud_db_sync.Location = New System.Drawing.Point(120, 27)
        Me.nud_db_sync.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        Me.nud_db_sync.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nud_db_sync.Name = "nud_db_sync"
        Me.nud_db_sync.Size = New System.Drawing.Size(36, 20)
        Me.nud_db_sync.TabIndex = 12
        Me.nud_db_sync.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.nud_db_sync.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(7, 29)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(115, 13)
        Me.Label4.TabIndex = 11
        Me.Label4.Text = "Synchronise every "
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(286, 10)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(195, 13)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "Select Databases to Synchronise"
        '
        'clb_databases
        '
        Me.clb_databases.Enabled = False
        Me.clb_databases.FormattingEnabled = True
        Me.clb_databases.Location = New System.Drawing.Point(289, 26)
        Me.clb_databases.Name = "clb_databases"
        Me.clb_databases.Size = New System.Drawing.Size(265, 124)
        Me.clb_databases.TabIndex = 0
        '
        'tp_thumbs
        '
        Me.tp_thumbs.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.tp_thumbs.Controls.Add(Me.TabControl1)
        Me.tp_thumbs.Location = New System.Drawing.Point(4, 22)
        Me.tp_thumbs.Name = "tp_thumbs"
        Me.tp_thumbs.Size = New System.Drawing.Size(582, 200)
        Me.tp_thumbs.TabIndex = 2
        Me.tp_thumbs.Text = "Thumbs"
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Location = New System.Drawing.Point(5, 8)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(573, 184)
        Me.TabControl1.TabIndex = 65
        '
        'TabPage1
        '
        Me.TabPage1.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.TabPage1.Controls.Add(Me.cb_thumbs_sync_method)
        Me.TabPage1.Controls.Add(Me.b_thumbs_direction)
        Me.TabPage1.Controls.Add(Me.Label5)
        Me.TabPage1.Controls.Add(Me.b_thumbs_server)
        Me.TabPage1.Controls.Add(Me.tb_thumbs_server_path)
        Me.TabPage1.Controls.Add(Me.tb_thumbs_client_path)
        Me.TabPage1.Controls.Add(Me.Label6)
        Me.TabPage1.Controls.Add(Me.b_thumbs_client)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(565, 158)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Paths"
        '
        'cb_thumbs_sync_method
        '
        Me.cb_thumbs_sync_method.FormattingEnabled = True
        Me.cb_thumbs_sync_method.Location = New System.Drawing.Point(327, 76)
        Me.cb_thumbs_sync_method.Name = "cb_thumbs_sync_method"
        Me.cb_thumbs_sync_method.Size = New System.Drawing.Size(215, 21)
        Me.cb_thumbs_sync_method.TabIndex = 68
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(6, 108)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(121, 13)
        Me.Label5.TabIndex = 66
        Me.Label5.Text = "Server Thumbs path"
        '
        'b_thumbs_server
        '
        Me.b_thumbs_server.Location = New System.Drawing.Point(504, 121)
        Me.b_thumbs_server.Name = "b_thumbs_server"
        Me.b_thumbs_server.Size = New System.Drawing.Size(57, 23)
        Me.b_thumbs_server.TabIndex = 65
        Me.b_thumbs_server.Text = "Browse"
        Me.b_thumbs_server.UseVisualStyleBackColor = True
        '
        'tb_thumbs_server_path
        '
        Me.tb_thumbs_server_path.Location = New System.Drawing.Point(6, 124)
        Me.tb_thumbs_server_path.Name = "tb_thumbs_server_path"
        Me.tb_thumbs_server_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_thumbs_server_path.TabIndex = 64
        '
        'tb_thumbs_client_path
        '
        Me.tb_thumbs_client_path.Location = New System.Drawing.Point(3, 26)
        Me.tb_thumbs_client_path.Name = "tb_thumbs_client_path"
        Me.tb_thumbs_client_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_thumbs_client_path.TabIndex = 61
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(3, 10)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(116, 13)
        Me.Label6.TabIndex = 63
        Me.Label6.Text = "Client Thumbs path"
        '
        'b_thumbs_client
        '
        Me.b_thumbs_client.Location = New System.Drawing.Point(501, 23)
        Me.b_thumbs_client.Name = "b_thumbs_client"
        Me.b_thumbs_client.Size = New System.Drawing.Size(57, 23)
        Me.b_thumbs_client.TabIndex = 62
        Me.b_thumbs_client.Text = "Browse"
        Me.b_thumbs_client.UseVisualStyleBackColor = True
        '
        'TabPage2
        '
        Me.TabPage2.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.TabPage2.Controls.Add(Me.GroupBox2)
        Me.TabPage2.Controls.Add(Me.Label8)
        Me.TabPage2.Controls.Add(Me.clb_thumbs)
        Me.TabPage2.Controls.Add(Me.cb_thumbs_sync)
        Me.TabPage2.Controls.Add(Me.nud_thumbs_sync)
        Me.TabPage2.Controls.Add(Me.Label7)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(565, 158)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Advanced Settings"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.rb_specific_thumbs)
        Me.GroupBox2.Controls.Add(Me.rb_all_thumbs)
        Me.GroupBox2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox2.Location = New System.Drawing.Point(10, 62)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(273, 76)
        Me.GroupBox2.TabIndex = 17
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = " Synchronise Thumbs "
        '
        'rb_specific_thumbs
        '
        Me.rb_specific_thumbs.AutoSize = True
        Me.rb_specific_thumbs.Location = New System.Drawing.Point(11, 51)
        Me.rb_specific_thumbs.Name = "rb_specific_thumbs"
        Me.rb_specific_thumbs.Size = New System.Drawing.Size(104, 17)
        Me.rb_specific_thumbs.TabIndex = 1
        Me.rb_specific_thumbs.TabStop = True
        Me.rb_specific_thumbs.Text = "Specific Thumbs"
        Me.rb_specific_thumbs.UseVisualStyleBackColor = True
        '
        'rb_all_thumbs
        '
        Me.rb_all_thumbs.AutoSize = True
        Me.rb_all_thumbs.Location = New System.Drawing.Point(11, 24)
        Me.rb_all_thumbs.Name = "rb_all_thumbs"
        Me.rb_all_thumbs.Size = New System.Drawing.Size(77, 17)
        Me.rb_all_thumbs.TabIndex = 0
        Me.rb_all_thumbs.TabStop = True
        Me.rb_all_thumbs.Text = "All Thumbs"
        Me.rb_all_thumbs.UseVisualStyleBackColor = True
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(286, 9)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(179, 13)
        Me.Label8.TabIndex = 16
        Me.Label8.Text = "Select Thumbs to Synchronise"
        '
        'clb_thumbs
        '
        Me.clb_thumbs.Enabled = False
        Me.clb_thumbs.FormattingEnabled = True
        Me.clb_thumbs.Location = New System.Drawing.Point(289, 25)
        Me.clb_thumbs.Name = "clb_thumbs"
        Me.clb_thumbs.Size = New System.Drawing.Size(265, 124)
        Me.clb_thumbs.TabIndex = 15
        '
        'cb_thumbs_sync
        '
        Me.cb_thumbs_sync.FormattingEnabled = True
        Me.cb_thumbs_sync.Items.AddRange(New Object() {"minutes", "hours"})
        Me.cb_thumbs_sync.Location = New System.Drawing.Point(160, 26)
        Me.cb_thumbs_sync.Name = "cb_thumbs_sync"
        Me.cb_thumbs_sync.Size = New System.Drawing.Size(80, 21)
        Me.cb_thumbs_sync.TabIndex = 13
        Me.cb_thumbs_sync.Text = "minutes"
        '
        'nud_thumbs_sync
        '
        Me.nud_thumbs_sync.Location = New System.Drawing.Point(120, 27)
        Me.nud_thumbs_sync.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        Me.nud_thumbs_sync.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nud_thumbs_sync.Name = "nud_thumbs_sync"
        Me.nud_thumbs_sync.Size = New System.Drawing.Size(36, 20)
        Me.nud_thumbs_sync.TabIndex = 12
        Me.nud_thumbs_sync.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.nud_thumbs_sync.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(7, 29)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(115, 13)
        Me.Label7.TabIndex = 11
        Me.Label7.Text = "Synchronise every "
        '
        'b_db_direction
        '
        Me.b_db_direction.Image = Global.MPSync.My.Resources.Resources.sync_both
        Me.b_db_direction.Location = New System.Drawing.Point(252, 53)
        Me.b_db_direction.Name = "b_db_direction"
        Me.b_db_direction.Size = New System.Drawing.Size(64, 64)
        Me.b_db_direction.TabIndex = 67
        Me.b_db_direction.UseVisualStyleBackColor = True
        '
        'b_thumbs_direction
        '
        Me.b_thumbs_direction.Image = Global.MPSync.My.Resources.Resources.sync_both
        Me.b_thumbs_direction.Location = New System.Drawing.Point(252, 53)
        Me.b_thumbs_direction.Name = "b_thumbs_direction"
        Me.b_thumbs_direction.Size = New System.Drawing.Size(64, 64)
        Me.b_thumbs_direction.TabIndex = 67
        Me.b_thumbs_direction.UseVisualStyleBackColor = True
        '
        'MPSync_settings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(599, 271)
        Me.Controls.Add(Me.tc_main)
        Me.Controls.Add(Me.l_copyright)
        Me.Controls.Add(Me.b_save)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "MPSync_settings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "MediaPortal Sync settings v"
        Me.tc_main.ResumeLayout(False)
        Me.tp_settings.ResumeLayout(False)
        Me.tp_settings.PerformLayout()
        Me.tp_database.ResumeLayout(False)
        Me.tc_database.ResumeLayout(False)
        Me.tp_db_paths.ResumeLayout(False)
        Me.tp_db_paths.PerformLayout()
        Me.tp_db_advancedsettings.ResumeLayout(False)
        Me.tp_db_advancedsettings.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.nud_db_sync, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tp_thumbs.ResumeLayout(False)
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        CType(Me.nud_thumbs_sync, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents b_save As System.Windows.Forms.Button
    Friend WithEvents tb_master_path As System.Windows.Forms.TextBox
    Friend WithEvents tb_slave_path As System.Windows.Forms.TextBox
    Friend WithEvents l_copyright As System.Windows.Forms.Label
    Friend WithEvents b_sync_now As System.Windows.Forms.Button
    Friend WithEvents tc_main As System.Windows.Forms.TabControl
    Friend WithEvents tp_database As System.Windows.Forms.TabPage
    Friend WithEvents tc_database As System.Windows.Forms.TabControl
    Friend WithEvents tp_db_paths As System.Windows.Forms.TabPage
    Friend WithEvents cb_db_sync_method As System.Windows.Forms.ComboBox
    Friend WithEvents b_db_direction As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents b_db_server As System.Windows.Forms.Button
    Friend WithEvents tb_db_server_path As System.Windows.Forms.TextBox
    Friend WithEvents tb_db_client_path As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents b_db_client As System.Windows.Forms.Button
    Friend WithEvents tp_db_advancedsettings As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents rb_specific_db As System.Windows.Forms.RadioButton
    Friend WithEvents rb_all_db As System.Windows.Forms.RadioButton
    Friend WithEvents cb_db_sync As System.Windows.Forms.ComboBox
    Friend WithEvents nud_db_sync As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents clb_databases As System.Windows.Forms.CheckedListBox
    Friend WithEvents tp_thumbs As System.Windows.Forms.TabPage
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents cb_thumbs_sync_method As System.Windows.Forms.ComboBox
    Friend WithEvents b_thumbs_direction As System.Windows.Forms.Button
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents b_thumbs_server As System.Windows.Forms.Button
    Friend WithEvents tb_thumbs_server_path As System.Windows.Forms.TextBox
    Friend WithEvents tb_thumbs_client_path As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents b_thumbs_client As System.Windows.Forms.Button
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents cb_thumbs_sync As System.Windows.Forms.ComboBox
    Friend WithEvents nud_thumbs_sync As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents tp_settings As System.Windows.Forms.TabPage
    Friend WithEvents cb_thumbs As System.Windows.Forms.CheckBox
    Friend WithEvents cb_databases As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents rb_specific_thumbs As System.Windows.Forms.RadioButton
    Friend WithEvents rb_all_thumbs As System.Windows.Forms.RadioButton
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents clb_thumbs As System.Windows.Forms.CheckedListBox

End Class
