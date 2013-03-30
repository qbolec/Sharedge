using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Runtime.InteropServices;
namespace Sharedge {

  

  public partial class SharedgeForm : Form {
    public struct RECT {
      public int Left;       // Specifies the x-coordinate of the upper-left corner of the rectangle.
      public int Top;        // Specifies the y-coordinate of the upper-left corner of the rectangle.
      public int Right;      // Specifies the x-coordinate of the lower-right corner of the rectangle.
      public int Bottom;     // Specifies the y-coordinate of the lower-right corner of the rectangle.

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWINFO {
      public uint cbSize;
      public RECT rcWindow;
      public RECT rcClient;
      public uint dwStyle;
      public uint dwExStyle;
      public uint dwWindowStatus;
      public uint cxWindowBorders;
      public uint cyWindowBorders;
      public ushort atomWindowType;
      public ushort wCreatorVersion;

      public WINDOWINFO(Boolean? filler)
        : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
      {
        cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
      }

    }

    class Win32 {
      /// <summary>
      /// Gets the foreground window.
      /// </summary>
      /// <returns></returns>
      [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
      public static extern IntPtr GetForegroundWindow();

      /// <summary>
      /// Gets the window info.
      /// </summary>
      /// <param name="hwnd">The HWND.</param>
      /// <param name="pwi">The pwi.</param>
      /// <returns></returns>
      [return: MarshalAs(UnmanagedType.Bool)]
      [DllImport("user32.dll", SetLastError = true)]
      public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

    }
    abstract class AbstractLayoutStrategy {
      protected SharedgeForm Form;
      public AbstractLayoutStrategy(SharedgeForm form) {
        this.Form = form;
      }
      abstract public void Snap();
      abstract public bool IsOutOfBounds(Point p);
      abstract public int MinWidthForFakeCursor();
      abstract public int MinWidthForScreenshot();
      abstract public Point GetJointPosition();
      abstract public int EntryLeft();
      public void ReflowEntries(){
        int lastY = 30;
        int left = this.EntryLeft();
        foreach (Control entry in Form.Entries) {
          entry.Top = lastY;
          lastY = lastY + entry.Height;
          entry.Left = left;
        }
      }
      virtual protected void AdaptToNewWidth(int goalWidth) {
        
      }
      abstract public bool IsScreenshotVisible();
      public void SetWidth(int goalWidth) {
        int oldFakeCursorLeft = Form.FakeCursor.Left + Form.Left;
        int oldScreenshotLeft = Form.ScreenshotPreview.Left + Form.Left;
        AdaptToNewWidth(goalWidth);
        Form.Width = goalWidth;
        Form.FakeCursor.Left = oldFakeCursorLeft - Form.Left;
        Form.ScreenshotPreview.Left = oldScreenshotLeft - Form.Left;
        ReflowEntries();
      }
    }
    class LeftSideLayoutStrategy : AbstractLayoutStrategy {
      public LeftSideLayoutStrategy(SharedgeForm form)
        : base(form) {        
      }
      override public int EntryLeft() {
        return 0;        
      }
      override public void Snap() {
        Form.Width = 200;
        Form.Height = Screen.PrimaryScreen.Bounds.Height;
        Form.Left = Screen.PrimaryScreen.Bounds.X;
        Form.Top = Screen.PrimaryScreen.Bounds.Y;
        Form.Text = "Left Sharedge";
      }
      override public bool IsOutOfBounds(Point p){
        return p.X <= Form.Left;
      }
      override public bool IsScreenshotVisible() {
        return 0 < Form.ScreenshotPreview.Right;
      }
      override public int MinWidthForScreenshot() {
        return Form.ScreenshotPreview.Right;
      }
      override public int MinWidthForFakeCursor() {
        return Form.FakeCursor.Right;
      }
      public override Point GetJointPosition() {
        return new Point(Form.Left, Form.Top + Form.Height / 2);        
      }
    }
    class RightSideLayoutStrategy : AbstractLayoutStrategy {
      public RightSideLayoutStrategy(SharedgeForm form)
        : base(form) {        
      }
      override protected void AdaptToNewWidth(int goalWidth) {
        Form.Left += Form.Width - goalWidth;
      }
      
      override public int EntryLeft() {
        return Form.Width - 200;        
      }
      
      override public void Snap() {
        Form.Width = 200;
        Form.Height = Screen.PrimaryScreen.Bounds.Height;
        Form.Left = Screen.PrimaryScreen.Bounds.Right - Form.Width;
        Form.Top = Screen.PrimaryScreen.Bounds.Y;
        Form.Text = "Right Sharedge";
      }
      override public bool IsOutOfBounds(Point p){
        return Form.Right-1 <=p.X;
      }
      override public bool IsScreenshotVisible() {
        return Form.ScreenshotPreview.Left < Form.Width;
      }
      
      override public int MinWidthForFakeCursor() {
        return Form.Width - Form.FakeCursor.Left;          
      }
      override public int MinWidthForScreenshot() {
        return Form.Width - Form.ScreenshotPreview.Left;
      }
      public override Point GetJointPosition() {
        return new Point(Form.Right, Form.Top + Form.Height / 2);
      }
    }
    private State TheState;
    private Point interceptedAt;
    private Point virtualPosition;
    private Point lastSeen;
    private bool intercepted = false;
    private bool DragingOver = false;
    private AbstractLayoutStrategy LayoutStrategy;
    private TransparentControl FakeCursor;
    private List<Control> Entries = new List<Control>();
    private DateTime OpenDeadline = DateTime.Now;
    public enum CURSOR_KIND {
      ARROW,
      DRAG_COPY,
    };

    public SharedgeForm(State state) {
      this.TheState = state;
      InitializeComponent();

      FakeCursor = new TransparentControl();
      this.Controls.Add(FakeCursor);
      FakeCursor.BringToFront();
      FakeCursor.Visible = false;

      switch(TheState.Side){
        case "right":
          LayoutStrategy = new RightSideLayoutStrategy(this);
          break;
        default:
          LayoutStrategy = new LeftSideLayoutStrategy(this);
          break;
      }
    }
    private void Open() {
      OpenDeadline = DateTime.Now.Add(new TimeSpan(0, 0, 3));
    }
    private void PushEntry(Control entry) {
      Controls.Add(entry);
      Entries.Add(entry);
      Open();
      LayoutStrategy.ReflowEntries();
    }
    private void AddFileEntry(string filename) {
      var entry = new FileEntry();
      entry.SetFileName(TheState.TransferCenter.IncomingDirectory, filename);
      entry.OnDrag += OnDragFile;
      PushEntry(entry);
    }
    void OnDragFile(string filename) {
      var dataObject = new DataObject();
      var path = Path.Combine(TheState.TransferCenter.IncomingDirectory, filename);
      var filePaths = new System.Collections.Specialized.StringCollection();
      filePaths.Add(path);
      dataObject.SetFileDropList(filePaths);
      this.DoDragDrop(dataObject, DragDropEffects.Move | DragDropEffects.Copy);
    }

    private void AddTextEntry(string text) {
      var entry = new TextEntry();
      entry.SetText(text);
      PushEntry(entry);
    }
    
    private void SharedgeForm_Load(object sender, EventArgs e) {
      this.LayoutStrategy.Snap();
      initIncomingFolder();
      this.Capture = true;
      this.TopMost = true;
      TheState.TransferCenter.MouseMove += SetFakeCursorRelativePos;
      foreach (Control Control in Controls) {
        Control.MouseMove += SharedgeForm_MouseMove;
        //Control.DragOver += SharedgeForm_DragOver;        
      }
      
      
    }
    private void initIncomingFolder() {
     
      TheState.TransferCenter.NewText += OnNewText;
      TheState.TransferCenter.BitmapMove += OnBitmapMove;
      TheState.TransferCenter.BitmapReceived += OnBitmapReceived;
      TheState.TransferCenter.NewFile += OnNewFile;

      TheState.TransferCenter.Start();
    }
    private void OnBitmapMove(Rectangle position) {
      BeginInvoke(new Action(() => {
        Point joint = GetJointPosition();
        ScreenshotPreview.Left = position.Left + joint.X - Left;
        ScreenshotPreview.Top = position.Top + joint.Y - Top;
        ScreenshotPreview.Width = position.Width;
        ScreenshotPreview.Height = position.Height;
        ScreenshotPreview.Visible = true;
      }));
    }
    private void OnBitmapReceived(Bitmap bitmap) {
      BeginInvoke(new Action(() => {
        ScreenshotPreview.Image = bitmap;
      }));
    }
    void OnNewFile(string filename) {
      BeginInvoke(new Action(() => {
        AddFileEntry(filename);
      }));
    }


    private void QuitButton_Click(object sender, EventArgs e) {
      this.Close();
    }



    private void SharedgeForm_DragEnter(object sender, DragEventArgs e) {
      DragingOver = true;
      if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
        e.Effect = DragDropEffects.Copy; // Okay
      else
        e.Effect = DragDropEffects.None; // Unknown data, ignore it      
    }

    private void SharedgeForm_DragDrop(object sender, DragEventArgs e) {
      DragingOver = false;
      if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
        string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

        foreach (var File in FileList) {
          TheState.TransferCenter.SendFile(new FileInfo(File));          
        }
      }
      if (e.Data.GetDataPresent(DataFormats.Text)) {
        string Text = (string)e.Data.GetData(DataFormats.Text, false);
        TheState.TransferCenter.SendText(Text);
      }      
    }

    private void OnNewText(string text) {
      BeginInvoke(new Action(() => {
        AddTextEntry(text);
      }));
    }
    private void HandleMouseActivity() {
      Point pos;
      if (!intercepted) {
        pos = Cursor.Position;
        if (IsOutOfBounds(pos)) {
          intercepted = true;
          virtualPosition = interceptedAt = pos;
          lastSeen = GetStasisCursorPos();
          Cursor.Position = lastSeen;
          Cursor.Hide();
        }
      } else {
        Point delta = Point.Subtract(Cursor.Position, new Size(lastSeen));
        virtualPosition = Point.Add(virtualPosition, new Size(delta));
        pos = virtualPosition;
        if (!IsOutOfBounds(virtualPosition)) {
          intercepted = false;
          Cursor.Show();
          Cursor.Position = virtualPosition;
        } else if (!delta.IsEmpty) {
          lastSeen = GetStasisCursorPos();
          Cursor.Position = lastSeen;
        }
        Point relPos = Point.Subtract(virtualPosition, new Size(GetJointPosition()));
        TheState.TransferCenter.SendMousePosition(relPos, DragingOver?CURSOR_KIND.DRAG_COPY:CURSOR_KIND.ARROW);
      }
      
    }
    private void SharedgeForm_DragOver(object sender, DragEventArgs e) {
      HandleMouseActivity();
    }
    
    private bool IsOutOfBounds(Point p) {
      return LayoutStrategy.IsOutOfBounds(p);
      
    }
    private Point GetStasisCursorPos() {
      return new Point(this.Left + this.Width / 2, this.Top + this.Height / 2);
    }
    private void SetProperSize() {
      const int SHRINK_SPEED = 5;
      const int MINIMIZED_WIDTH = 5;
      int MAX_WIDTH = Screen.PrimaryScreen.Bounds.Width;
      const int HOVER_WIDTH = 200;
      int goalWidth = 0;
      if (Bounds.Contains(Cursor.Position) && !DragingOver) {
        Open();
      }
      if(DateTime.Now < OpenDeadline){
        goalWidth = Math.Max(goalWidth,HOVER_WIDTH);
      } 
      if (FakeCursor.Visible) {
        goalWidth = Math.Max(goalWidth, LayoutStrategy.MinWidthForFakeCursor());
      }
      if (ScreenshotPreview.Visible && LayoutStrategy.IsScreenshotVisible()) {
        goalWidth = Math.Max(goalWidth, LayoutStrategy.MinWidthForScreenshot());
      }
      if (goalWidth < Width) {
        goalWidth = Math.Max(MINIMIZED_WIDTH, Width - SHRINK_SPEED);
      }
      goalWidth = Math.Min(goalWidth, MAX_WIDTH);
      LayoutStrategy.SetWidth(goalWidth);      
    }
    private void SetFakeCursorPos(Point pos) {
      FakeCursor.Left = pos.X - this.Left;
      FakeCursor.Top = pos.Y - this.Top;
      FakeCursor.Visible = !IsOutOfBounds(pos);
      SetProperSize();
    }
    private Point GetJointPosition() {
      return LayoutStrategy.GetJointPosition();      
    }
    private void SetFakeCursorRelativePos(Point relPos,CURSOR_KIND cursorKind) {
      BeginInvoke(new Action(() => {
        Point pos = Point.Add(relPos, new Size(GetJointPosition()));
        SetFakeCursorPos(pos);
        switch (cursorKind) {
          case CURSOR_KIND.ARROW:
            this.FakeCursor.Image = global::Sharedge.Properties.Resources.Cursor_arrow_white;
            break;
          default:
            this.FakeCursor.Image = global::Sharedge.Properties.Resources.Cursor_drag_copy;
            break;
        }
      
      }));
    }
    
    private void SharedgeForm_MouseMove(object sender, MouseEventArgs e) {
      HandleMouseActivity();
      SetProperSize();      
    }
    public Bitmap TakeScreenshot(Rectangle rectangle) {
      Bitmap screenShotBMP = new Bitmap(rectangle.Width,
          rectangle.Height, PixelFormat.Format32bppArgb);

      using (Graphics screenShotGraphics = Graphics.FromImage(screenShotBMP)) {
        screenShotGraphics.CopyFromScreen(rectangle.X,
            rectangle.Y, 0, 0, rectangle.Size ,
            CopyPixelOperation.SourceCopy);

        return screenShotBMP;
      }
    }
    private bool IsOutOfBounds(Rectangle rectangle) {
      //TODO:
      return true;
    }
    Bitmap Screenshot;
    IntPtr LastForegroundWindow;
    bool sent = false;
    private void HandleScreenshots() {
      IntPtr fg = Win32.GetForegroundWindow();
      if (this.Handle != fg && IntPtr.Zero != fg && null != fg) {
        WINDOWINFO info = new WINDOWINFO();
        Win32.GetWindowInfo(fg, ref info);
        RECT r = info.rcWindow;
        Rectangle rectangle = new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        if (0 < rectangle.Height && 0 < rectangle.Height) {
          if (fg != LastForegroundWindow) {
            Screenshot = TakeScreenshot(rectangle);
            LastForegroundWindow = fg;
            sent = false;
          }
          if (IsOutOfBounds(rectangle)) {
            if (!sent) {
              sent = true;
              TheState.TransferCenter.SendBitmap(Screenshot);
            }
            Point joint = LayoutStrategy.GetJointPosition();
            rectangle.Offset(-joint.X, -joint.Y);
            TheState.TransferCenter.SendBitmapPosition(rectangle);
          }
          return;
        }
      }
      sent = false;
      Screenshot = null;
      LastForegroundWindow = IntPtr.Zero;
    }
    private void ResizingTimer_Tick(object sender, EventArgs e) {
      HandleMouseActivity();
      SetProperSize();
      HandleScreenshots();   
    }

    private void SharedgeForm_DragLeave(object sender, EventArgs e) {
      DragingOver = false;
    }




    
    private void SharedgeForm_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
      e.Action = DragAction.Continue;
    }

    
        
  }
}
