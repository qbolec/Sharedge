using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sharedge {
  public partial class TextEntry : UserControl {
    public TextEntry() {
      InitializeComponent();
    }
    private int lines;
    public void SetText(string text) {
      Textarea.Text = text;
      var liner = new System.Text.RegularExpressions.Regex("\n");
      lines = liner.Split(text).Length;
      Textarea.Height = 20 * Math.Min(lines+1,7) + 4;
      Height = Textarea.Height + 28;
    }

    private void Textarea_MouseEnter(object sender, EventArgs e) {
      if (7 <= lines) {
        this.Textarea.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      } else {
        this.Textarea.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
      }
    }

    private void Textarea_MouseLeave(object sender, EventArgs e) {
      this.Textarea.ScrollBars = System.Windows.Forms.ScrollBars.None;
    }

  }
}
