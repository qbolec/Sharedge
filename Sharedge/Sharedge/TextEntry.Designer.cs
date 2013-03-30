namespace Sharedge {
  partial class TextEntry {
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
      this.Textarea = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // Textarea
      // 
      this.Textarea.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.Textarea.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
      this.Textarea.Location = new System.Drawing.Point(12, 13);
      this.Textarea.Multiline = true;
      this.Textarea.Name = "Textarea";
      this.Textarea.Size = new System.Drawing.Size(176, 124);
      this.Textarea.TabIndex = 11;
      this.Textarea.Text = "Poszła ola do przedszkola a tam jej łeb urwało.\r\nCo gorsza na tym nie koniec.\r\n\r\n" +
    "if(x <0 ){\r\n  x = 7;\r\n}";
      this.Textarea.WordWrap = false;
      this.Textarea.MouseEnter += new System.EventHandler(this.Textarea_MouseEnter);
      this.Textarea.MouseLeave += new System.EventHandler(this.Textarea_MouseLeave);
      // 
      // TextEntry
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Black;
      this.Controls.Add(this.Textarea);
      this.Name = "TextEntry";
      this.Size = new System.Drawing.Size(200, 152);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox Textarea;
  }
}
