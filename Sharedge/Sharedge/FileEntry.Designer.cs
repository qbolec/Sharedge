namespace Sharedge {
  partial class FileEntry {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.FileIconPicture = new System.Windows.Forms.PictureBox();
      this.FilenameLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.FileIconPicture)).BeginInit();
      this.SuspendLayout();
      // 
      // FileIconPicture
      // 
      this.FileIconPicture.Location = new System.Drawing.Point(3, 3);
      this.FileIconPicture.Name = "FileIconPicture";
      this.FileIconPicture.Size = new System.Drawing.Size(32, 32);
      this.FileIconPicture.TabIndex = 11;
      this.FileIconPicture.TabStop = false;
      // 
      // FilenameLabel
      // 
      this.FilenameLabel.AutoEllipsis = true;
      this.FilenameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
      this.FilenameLabel.ForeColor = System.Drawing.Color.White;
      this.FilenameLabel.Location = new System.Drawing.Point(33, 8);
      this.FilenameLabel.Name = "FilenameLabel";
      this.FilenameLabel.Size = new System.Drawing.Size(165, 24);
      this.FilenameLabel.TabIndex = 10;
      this.FilenameLabel.Text = "Przedwiośnie i takie tam.mp3";
      this.FilenameLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FilenameLabel_MouseDown);
      this.FilenameLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FilenameLabel_MouseMove);
      // 
      // FileEntry
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add(this.FileIconPicture);
      this.Controls.Add(this.FilenameLabel);
      this.Name = "FileEntry";
      this.Size = new System.Drawing.Size(200, 40);
      ((System.ComponentModel.ISupportInitialize)(this.FileIconPicture)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox FileIconPicture;
    private System.Windows.Forms.Label FilenameLabel;
  }
}
