namespace Sharedge
{
    partial class SharedgeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.components = new System.ComponentModel.Container();
      this.QuitButton = new System.Windows.Forms.Button();
      this.IncomingDirectoryWatcher = new System.IO.FileSystemWatcher();
      this.ResizingTimer = new System.Windows.Forms.Timer(this.components);
      this.ScreenshotPreview = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.IncomingDirectoryWatcher)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ScreenshotPreview)).BeginInit();
      this.SuspendLayout();
      // 
      // QuitButton
      // 
      this.QuitButton.BackColor = System.Drawing.Color.Transparent;
      this.QuitButton.Cursor = System.Windows.Forms.Cursors.Hand;
      this.QuitButton.FlatAppearance.BorderSize = 0;
      this.QuitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
      this.QuitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
      this.QuitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.QuitButton.Font = new System.Drawing.Font("Wingdings", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
      this.QuitButton.ForeColor = System.Drawing.Color.White;
      this.QuitButton.Location = new System.Drawing.Point(170, 0);
      this.QuitButton.Name = "QuitButton";
      this.QuitButton.Size = new System.Drawing.Size(30, 30);
      this.QuitButton.TabIndex = 0;
      this.QuitButton.Text = "x";
      this.QuitButton.UseVisualStyleBackColor = false;
      this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
      // 
      // IncomingDirectoryWatcher
      // 
      this.IncomingDirectoryWatcher.EnableRaisingEvents = true;
      this.IncomingDirectoryWatcher.SynchronizingObject = this;
      // 
      // ResizingTimer
      // 
      this.ResizingTimer.Enabled = true;
      this.ResizingTimer.Interval = 40;
      this.ResizingTimer.Tick += new System.EventHandler(this.ResizingTimer_Tick);
      // 
      // ScreenshotPreview
      // 
      this.ScreenshotPreview.Location = new System.Drawing.Point(132, 682);
      this.ScreenshotPreview.Name = "ScreenshotPreview";
      this.ScreenshotPreview.Size = new System.Drawing.Size(56, 49);
      this.ScreenshotPreview.TabIndex = 7;
      this.ScreenshotPreview.TabStop = false;
      this.ScreenshotPreview.Visible = false;
      // 
      // SharedgeForm
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.ClientSize = new System.Drawing.Size(200, 758);
      this.Controls.Add(this.ScreenshotPreview);
      this.Controls.Add(this.QuitButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SharedgeForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.SharedgeForm_Load);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.SharedgeForm_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.SharedgeForm_DragEnter);
      this.DragOver += new System.Windows.Forms.DragEventHandler(this.SharedgeForm_DragOver);
      this.DragLeave += new System.EventHandler(this.SharedgeForm_DragLeave);
      this.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.SharedgeForm_QueryContinueDrag);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SharedgeForm_MouseMove);
      ((System.ComponentModel.ISupportInitialize)(this.IncomingDirectoryWatcher)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ScreenshotPreview)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button QuitButton;
        private System.IO.FileSystemWatcher IncomingDirectoryWatcher;
        private System.Windows.Forms.Timer ResizingTimer;
        private System.Windows.Forms.PictureBox ScreenshotPreview;
    }
}

