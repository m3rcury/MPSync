<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CDB_Sync_settings
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CDB_Sync_settings))
        Me.b_save = New System.Windows.Forms.Button()
        Me.tb_master_path = New System.Windows.Forms.TextBox()
        Me.tb_slave_path = New System.Windows.Forms.TextBox()
        Me.l_copyright = New System.Windows.Forms.Label()
        Me.b_test = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.tp_paths = New System.Windows.Forms.TabPage()
        Me.cb_sync_method = New System.Windows.Forms.ComboBox()
        Me.b_direction = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.b_target_db = New System.Windows.Forms.Button()
        Me.tb_target_path = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.b_source_db = New System.Windows.Forms.Button()
        Me.tb_source_path = New System.Windows.Forms.TextBox()
        Me.tp_advancedsettings = New System.Windows.Forms.TabPage()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.rb_specific = New System.Windows.Forms.RadioButton()
        Me.rb_all = New System.Windows.Forms.RadioButton()
        Me.cb_sync = New System.Windows.Forms.ComboBox()
        Me.nud_sync = New System.Windows.Forms.NumericUpDown()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.clb_databases = New System.Windows.Forms.CheckedListBox()
        Me.TabControl1.SuspendLayout()
        Me.tp_paths.SuspendLayout()
        Me.tp_advancedsettings.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.nud_sync, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'b_save
        '
        Me.b_save.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.b_save.Location = New System.Drawing.Point(256, 196)
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
        Me.l_copyright.Location = New System.Drawing.Point(2, 217)
        Me.l_copyright.Name = "l_copyright"
        Me.l_copyright.Size = New System.Drawing.Size(133, 13)
        Me.l_copyright.TabIndex = 61
        Me.l_copyright.Text = "Copyright © 2013, m3rcury"
        '
        'b_test
        '
        Me.b_test.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.b_test.Location = New System.Drawing.Point(513, 196)
        Me.b_test.Name = "b_test"
        Me.b_test.Size = New System.Drawing.Size(57, 23)
        Me.b_test.TabIndex = 62
        Me.b_test.Text = "Test"
        Me.b_test.UseVisualStyleBackColor = True
        Me.b_test.Visible = False
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.tp_paths)
        Me.TabControl1.Controls.Add(Me.tp_advancedsettings)
        Me.TabControl1.Location = New System.Drawing.Point(7, 6)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(573, 184)
        Me.TabControl1.TabIndex = 63
        '
        'tp_paths
        '
        Me.tp_paths.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.tp_paths.Controls.Add(Me.cb_sync_method)
        Me.tp_paths.Controls.Add(Me.b_direction)
        Me.tp_paths.Controls.Add(Me.Label2)
        Me.tp_paths.Controls.Add(Me.b_target_db)
        Me.tp_paths.Controls.Add(Me.tb_target_path)
        Me.tp_paths.Controls.Add(Me.Label1)
        Me.tp_paths.Controls.Add(Me.b_source_db)
        Me.tp_paths.Controls.Add(Me.tb_source_path)
        Me.tp_paths.Location = New System.Drawing.Point(4, 22)
        Me.tp_paths.Name = "tp_paths"
        Me.tp_paths.Padding = New System.Windows.Forms.Padding(3)
        Me.tp_paths.Size = New System.Drawing.Size(565, 158)
        Me.tp_paths.TabIndex = 0
        Me.tp_paths.Text = "Paths"
        '
        'cb_sync_method
        '
        Me.cb_sync_method.FormattingEnabled = True
        Me.cb_sync_method.Location = New System.Drawing.Point(327, 76)
        Me.cb_sync_method.Name = "cb_sync_method"
        Me.cb_sync_method.Size = New System.Drawing.Size(215, 21)
        Me.cb_sync_method.TabIndex = 68
        '
        'b_direction
        '
        Me.b_direction.Image = Global.CDB_Sync.My.Resources.Resources.sync_both
        Me.b_direction.Location = New System.Drawing.Point(252, 53)
        Me.b_direction.Name = "b_direction"
        Me.b_direction.Size = New System.Drawing.Size(64, 64)
        Me.b_direction.TabIndex = 67
        Me.b_direction.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(6, 108)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(94, 13)
        Me.Label2.TabIndex = 66
        Me.Label2.Text = "Target DB path"
        '
        'b_target_db
        '
        Me.b_target_db.Location = New System.Drawing.Point(504, 121)
        Me.b_target_db.Name = "b_target_db"
        Me.b_target_db.Size = New System.Drawing.Size(57, 23)
        Me.b_target_db.TabIndex = 65
        Me.b_target_db.Text = "Browse"
        Me.b_target_db.UseVisualStyleBackColor = True
        '
        'tb_target_path
        '
        Me.tb_target_path.Location = New System.Drawing.Point(6, 124)
        Me.tb_target_path.Name = "tb_target_path"
        Me.tb_target_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_target_path.TabIndex = 64
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(3, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(97, 13)
        Me.Label1.TabIndex = 63
        Me.Label1.Text = "Source DB path"
        '
        'b_source_db
        '
        Me.b_source_db.Location = New System.Drawing.Point(501, 23)
        Me.b_source_db.Name = "b_source_db"
        Me.b_source_db.Size = New System.Drawing.Size(57, 23)
        Me.b_source_db.TabIndex = 62
        Me.b_source_db.Text = "Browse"
        Me.b_source_db.UseVisualStyleBackColor = True
        '
        'tb_source_path
        '
        Me.tb_source_path.Location = New System.Drawing.Point(3, 26)
        Me.tb_source_path.Name = "tb_source_path"
        Me.tb_source_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_source_path.TabIndex = 61
        '
        'tp_advancedsettings
        '
        Me.tp_advancedsettings.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.tp_advancedsettings.Controls.Add(Me.GroupBox1)
        Me.tp_advancedsettings.Controls.Add(Me.cb_sync)
        Me.tp_advancedsettings.Controls.Add(Me.nud_sync)
        Me.tp_advancedsettings.Controls.Add(Me.Label4)
        Me.tp_advancedsettings.Controls.Add(Me.Label3)
        Me.tp_advancedsettings.Controls.Add(Me.clb_databases)
        Me.tp_advancedsettings.Location = New System.Drawing.Point(4, 22)
        Me.tp_advancedsettings.Name = "tp_advancedsettings"
        Me.tp_advancedsettings.Padding = New System.Windows.Forms.Padding(3)
        Me.tp_advancedsettings.Size = New System.Drawing.Size(565, 158)
        Me.tp_advancedsettings.TabIndex = 1
        Me.tp_advancedsettings.Text = "Advanced Settings"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.rb_specific)
        Me.GroupBox1.Controls.Add(Me.rb_all)
        Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GroupBox1.Location = New System.Drawing.Point(10, 63)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(273, 76)
        Me.GroupBox1.TabIndex = 14
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = " Synchronise Databases "
        '
        'rb_specific
        '
        Me.rb_specific.AutoSize = True
        Me.rb_specific.Location = New System.Drawing.Point(11, 51)
        Me.rb_specific.Name = "rb_specific"
        Me.rb_specific.Size = New System.Drawing.Size(117, 17)
        Me.rb_specific.TabIndex = 1
        Me.rb_specific.TabStop = True
        Me.rb_specific.Text = "Specific Databases"
        Me.rb_specific.UseVisualStyleBackColor = True
        '
        'rb_all
        '
        Me.rb_all.AutoSize = True
        Me.rb_all.Location = New System.Drawing.Point(11, 24)
        Me.rb_all.Name = "rb_all"
        Me.rb_all.Size = New System.Drawing.Size(90, 17)
        Me.rb_all.TabIndex = 0
        Me.rb_all.TabStop = True
        Me.rb_all.Text = "All Databases"
        Me.rb_all.UseVisualStyleBackColor = True
        '
        'cb_sync
        '
        Me.cb_sync.FormattingEnabled = True
        Me.cb_sync.Items.AddRange(New Object() {"minutes", "hours"})
        Me.cb_sync.Location = New System.Drawing.Point(160, 26)
        Me.cb_sync.Name = "cb_sync"
        Me.cb_sync.Size = New System.Drawing.Size(80, 21)
        Me.cb_sync.TabIndex = 13
        Me.cb_sync.Text = "minutes"
        '
        'nud_sync
        '
        Me.nud_sync.Location = New System.Drawing.Point(120, 27)
        Me.nud_sync.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        Me.nud_sync.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nud_sync.Name = "nud_sync"
        Me.nud_sync.Size = New System.Drawing.Size(36, 20)
        Me.nud_sync.TabIndex = 12
        Me.nud_sync.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.nud_sync.Value = New Decimal(New Integer() {1, 0, 0, 0})
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
        'CDB_Sync_settings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(587, 233)
        Me.Controls.Add(Me.b_test)
        Me.Controls.Add(Me.l_copyright)
        Me.Controls.Add(Me.b_save)
        Me.Controls.Add(Me.TabControl1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "CDB_Sync_settings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Centralized DB Sync settings v"
        Me.TabControl1.ResumeLayout(False)
        Me.tp_paths.ResumeLayout(False)
        Me.tp_paths.PerformLayout()
        Me.tp_advancedsettings.ResumeLayout(False)
        Me.tp_advancedsettings.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.nud_sync, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents b_save As System.Windows.Forms.Button
    Friend WithEvents tb_master_path As System.Windows.Forms.TextBox
    Friend WithEvents tb_slave_path As System.Windows.Forms.TextBox
    Friend WithEvents l_copyright As System.Windows.Forms.Label
    Friend WithEvents b_test As System.Windows.Forms.Button
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents tp_paths As System.Windows.Forms.TabPage
    Friend WithEvents cb_sync_method As System.Windows.Forms.ComboBox
    Friend WithEvents b_direction As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents b_target_db As System.Windows.Forms.Button
    Friend WithEvents tb_target_path As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents b_source_db As System.Windows.Forms.Button
    Friend WithEvents tb_source_path As System.Windows.Forms.TextBox
    Friend WithEvents tp_advancedsettings As System.Windows.Forms.TabPage
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents clb_databases As System.Windows.Forms.CheckedListBox
    Friend WithEvents cb_sync As System.Windows.Forms.ComboBox
    Friend WithEvents nud_sync As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents rb_specific As System.Windows.Forms.RadioButton
    Friend WithEvents rb_all As System.Windows.Forms.RadioButton

End Class
