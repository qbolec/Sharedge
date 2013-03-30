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
  public partial class FileEntry : UserControl {
    public FileEntry() {
      InitializeComponent();
    }
    public void SetFileName(string directory,string fileName){
      FilenameLabel.Text = fileName;
      FileIconPicture.Image = Icon.ExtractAssociatedIcon(System.IO.Path.Combine(directory,fileName)).ToBitmap();
    }
    private Point DragStart;
    private void FilenameLabel_MouseDown(object sender, MouseEventArgs e) {
      DragStart = new Point(e.X, e.Y);
    }

    private void FilenameLabel_MouseMove(object sender, MouseEventArgs e) {
      if (e.Button != System.Windows.Forms.MouseButtons.None) {
        if (5< Math.Abs(e.X - DragStart.X) + Math.Abs(e.Y - DragStart.Y)) {
          OnDrag(FilenameLabel.Text);          
        }
      }
    }
    public delegate void OnDragEventHandler(string filename);
    public OnDragEventHandler OnDrag;
  }
}
