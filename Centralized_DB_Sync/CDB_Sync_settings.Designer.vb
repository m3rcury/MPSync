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
        Me.b_source_db = New System.Windows.Forms.Button()
        Me.tb_source_path = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.b_target_db = New System.Windows.Forms.Button()
        Me.tb_target_path = New System.Windows.Forms.TextBox()
        Me.b_save = New System.Windows.Forms.Button()
        Me.tb_master_path = New System.Windows.Forms.TextBox()
        Me.tb_slave_path = New System.Windows.Forms.TextBox()
        Me.cb_sync_method = New System.Windows.Forms.ComboBox()
        Me.b_direction = New System.Windows.Forms.Button()
        Me.l_copyright = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'b_source_db
        '
        Me.b_source_db.Location = New System.Drawing.Point(510, 42)
        Me.b_source_db.Name = "b_source_db"
        Me.b_source_db.Size = New System.Drawing.Size(57, 23)
        Me.b_source_db.TabIndex = 52
        Me.b_source_db.Text = "Browse"
        Me.b_source_db.UseVisualStyleBackColor = True
        '
        'tb_source_path
        '
        Me.tb_source_path.Location = New System.Drawing.Point(12, 45)
        Me.tb_source_path.Name = "tb_source_path"
        Me.tb_source_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_source_path.TabIndex = 51
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 29)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(97, 13)
        Me.Label1.TabIndex = 53
        Me.Label1.Text = "Source DB path"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(15, 127)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(94, 13)
        Me.Label2.TabIndex = 56
        Me.Label2.Text = "Target DB path"
        '
        'b_target_db
        '
        Me.b_target_db.Location = New System.Drawing.Point(513, 140)
        Me.b_target_db.Name = "b_target_db"
        Me.b_target_db.Size = New System.Drawing.Size(57, 23)
        Me.b_target_db.TabIndex = 55
        Me.b_target_db.Text = "Browse"
        Me.b_target_db.UseVisualStyleBackColor = True
        '
        'tb_target_path
        '
        Me.tb_target_path.Location = New System.Drawing.Point(15, 143)
        Me.tb_target_path.Name = "tb_target_path"
        Me.tb_target_path.Size = New System.Drawing.Size(492, 20)
        Me.tb_target_path.TabIndex = 54
        '
        'b_save
        '
        Me.b_save.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.b_save.Location = New System.Drawing.Point(256, 180)
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
        'cb_sync_method
        '
        Me.cb_sync_method.FormattingEnabled = True
        Me.cb_sync_method.Location = New System.Drawing.Point(336, 95)
        Me.cb_sync_method.Name = "cb_sync_method"
        Me.cb_sync_method.Size = New System.Drawing.Size(215, 21)
        Me.cb_sync_method.TabIndex = 60
        '
        'b_direction
        '
        Me.b_direction.Image = Global.CDB_Sync.My.Resources.Resources.sync_both
        Me.b_direction.Location = New System.Drawing.Point(261, 72)
        Me.b_direction.Name = "b_direction"
        Me.b_direction.Size = New System.Drawing.Size(64, 64)
        Me.b_direction.TabIndex = 57
        Me.b_direction.UseVisualStyleBackColor = True
        '
        'l_copyright
        '
        Me.l_copyright.AutoSize = True
        Me.l_copyright.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.l_copyright.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.l_copyright.Location = New System.Drawing.Point(2, 201)
        Me.l_copyright.Name = "l_copyright"
        Me.l_copyright.Size = New System.Drawing.Size(133, 13)
        Me.l_copyright.TabIndex = 61
        Me.l_copyright.Text = "Copyright © 2013, m3rcury"
        '
        'CDB_Sync_settings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(587, 217)
        Me.Controls.Add(Me.l_copyright)
        Me.Controls.Add(Me.cb_sync_method)
        Me.Controls.Add(Me.b_save)
        Me.Controls.Add(Me.b_direction)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.b_target_db)
        Me.Controls.Add(Me.tb_target_path)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.b_source_db)
        Me.Controls.Add(Me.tb_source_path)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "CDB_Sync_settings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Centralized DB Sync settings v"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents b_source_db As System.Windows.Forms.Button
    Friend WithEvents tb_source_path As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents b_target_db As System.Windows.Forms.Button
    Friend WithEvents tb_target_path As System.Windows.Forms.TextBox
    Friend WithEvents b_direction As System.Windows.Forms.Button
    Friend WithEvents b_save As System.Windows.Forms.Button
    Friend WithEvents tb_master_path As System.Windows.Forms.TextBox
    Friend WithEvents tb_slave_path As System.Windows.Forms.TextBox
    Friend WithEvents cb_sync_method As System.Windows.Forms.ComboBox
    Friend WithEvents l_copyright As System.Windows.Forms.Label

End Class
