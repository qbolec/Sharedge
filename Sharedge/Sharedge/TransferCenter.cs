using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;


namespace Sharedge {
  public class TransferCenter {
    public DirectoryInfo IncomingDirectoryInfo;
    public string IncomingDirectory;
    private TcpListener Listener;
    private State State;
    
    enum MESSAGE_TYPES {
      MOUSE_MOVE,
      NEW_TEXT,
      NEW_FILE,
      BITMAP,
      BITMAP_POSITION,
      QUIT,
    }
    public TransferCenter(State state) {
      State = state;
      var myDirectory = Application.StartupPath;
      IncomingDirectory = myDirectory + "\\Incoming";
      IncomingDirectoryInfo = Directory.CreateDirectory(IncomingDirectory);
    }
    
    public void Start() {
      const int LIMIT = 5;
      Listener = new TcpListener(State.LocalEndpoint);
      Listener.Start();
      for (int i = 0; i < LIMIT; i++) {
        Thread t = new Thread(new ThreadStart(ServeIncomingConnections));
        t.IsBackground = true;
        t.Start();
      }
    }
    private void ServeMouseMove(BinaryReader sr) {
      int x = sr.ReadInt32();
      int y = sr.ReadInt32();
      int cursorKind = sr.ReadInt32();
      MouseMove(new Point(x, y),(SharedgeForm.CURSOR_KIND)cursorKind);
    }
    private void ServeNewText(BinaryReader sr) {
      const int MAX_TEXT_LENGTH = 1 << 16;
      int length = sr.ReadInt32();
      if (0 < length && length <= MAX_TEXT_LENGTH) {
        var UTF = new UTF8Encoding();
        var text = UTF.GetString(sr.ReadBytes(length));
        NewText(text);
      }
    }
    private static bool IsValidFileName(string filename)
    {
        string sPattern = @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
        return (Regex.IsMatch(filename, sPattern, RegexOptions.CultureInvariant));
    }
    static FileStream CreateFileWithUniqueName(string folder, string fileName,
      int maxAttempts = 1024) {
      // get filename base and extension
      var fileBase = Path.GetFileNameWithoutExtension(fileName);
      var ext = Path.GetExtension(fileName);
      // build hash set of filenames for performance
      var files = new HashSet<string>(Directory.GetFiles(folder));

      for (var index = 0; index < maxAttempts; index++) {
        // first try with the original filename, else try incrementally adding an index
        var name = (index == 0)
            ? fileName
            : String.Format("{0} ({1}){2}", fileBase, index, ext);

        // check if exists
        var fullPath = Path.Combine(folder, name);
        if (files.Contains(fullPath))
          continue;

        // try to open the stream
        try {
          return new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        } catch (DirectoryNotFoundException) { 
          throw; 
        } catch (DriveNotFoundException) { 
          throw; 
        } catch (IOException) { 
        } // ignore this and try the next filename
      }

      throw new Exception("Could not create unique filename in " + maxAttempts + " attempts");
    }
    private void ServeBitmap(BinaryReader sr) {
      const int MAX_BITMAP_LENGTH = 1 << 25;
      const int BUFFER_SIZE = 1 << 12;
      long file_length = sr.ReadInt64();
      if (0 < file_length && file_length <= MAX_BITMAP_LENGTH ) {
        using (var stream = new MemoryStream((int)file_length)) {
          for (long i = 0; i < file_length; i += BUFFER_SIZE) {
             int expected = (int)Math.Min((long)BUFFER_SIZE, (file_length - i));
             stream.Write(sr.ReadBytes(expected), 0, expected);
          }
          BitmapReceived(new Bitmap(stream));
        }
      }
    }
    private void ServeBitmapPosition(BinaryReader sr) {
      int x = sr.ReadInt32();
      int y = sr.ReadInt32();
      int w = sr.ReadInt32();
      int h = sr.ReadInt32();
      BitmapMove(new Rectangle(x, y, w, h));
    }
    private void ServeNewFile(BinaryReader sr) {
      const int MAX_FILENAME_LENGTH = 1 << 10;
      const long MAX_FILE_LENGTH = 1 << 30;
      const int BUFFER_SIZE = 1 << 12;
      int filename_length = sr.ReadInt32();
      if (0 < filename_length && filename_length <= MAX_FILENAME_LENGTH) {
        var UTF = new UTF8Encoding();
        var filename = UTF.GetString(sr.ReadBytes(filename_length));
        long file_length = sr.ReadInt64();
        if(0 < file_length && file_length <= MAX_FILE_LENGTH){
          if (IsValidFileName(filename)) {
            using (FileStream stream = CreateFileWithUniqueName(IncomingDirectory, filename)) {
              for (long i = 0; i < file_length; i += BUFFER_SIZE) {
                int expected = (int)Math.Min((long)BUFFER_SIZE, (file_length - i));
                stream.Write(sr.ReadBytes(expected), 0, expected);
              }
              NewFile(filename);
            }            
          }
        }
      }
    }
    private void ServeIncomingConnection(Stream s) {
      var sr = new BinaryReader(s);
      while (true) {
        int type = BitConverter.ToInt32(sr.ReadBytes(4), 0);
        switch ((MESSAGE_TYPES)type) {
          case MESSAGE_TYPES.MOUSE_MOVE:
            ServeMouseMove(sr);
            break;
          case MESSAGE_TYPES.NEW_TEXT:
            ServeNewText(sr);
            break;
          case MESSAGE_TYPES.NEW_FILE:
            ServeNewFile(sr);
            break;
          case MESSAGE_TYPES.BITMAP:
            ServeBitmap(sr);
            break;
          case MESSAGE_TYPES.BITMAP_POSITION:
            ServeBitmapPosition(sr);
            break;
          case MESSAGE_TYPES.QUIT:
            return;
          default:
            throw new FormatException();
        }
      }
    }
    private void ServeIncomingConnections() {
      while (true) {
        Socket soc = Listener.AcceptSocket();
        try {
          soc.SetSocketOption(
            SocketOptionLevel.Socket,
            SocketOptionName.ReceiveTimeout,
            10000
          );
          Stream s = new NetworkStream(soc);
          try {
            ServeIncomingConnection(s);
          } finally {
            s.Close();
          }
        } catch(Exception){
        } finally {
          soc.Close();
        }
      }
    }
    
    public void SendBitmap(Bitmap bitmap) {
      EnqueueAction(() => {
        try {
          var connection = new Connection(State.RemoteEndpoint);
          var sw = connection.Writer;
          
          sw.Write((int)MESSAGE_TYPES.BITMAP);

          var memoryStream = new MemoryStream();

          bitmap.SaveJPG100(memoryStream);
          sw.Write((long)memoryStream.Length);
          sw.Flush();
          memoryStream.WriteTo(sw.BaseStream);
          //memoryStream.CopyTo(sw.BaseStream);
          connection.Stream.Flush();
          connection.Stream.Dispose();
          connection.Client.Close();
        } catch {
          //MessageBox.Show("Failed to send bitmap over network :(");
        } finally {

        }
      });
    }
    
    public void SendFile(FileInfo fileInfo) {
      EnqueueAction(() => {
        try {
          var connection = new Connection(State.RemoteEndpoint);
          var sw = connection.Writer;
          
          sw.Write((int)MESSAGE_TYPES.NEW_FILE);
          
          var UTF = new UTF8Encoding();
          string filename = fileInfo.Name;
          sw.Write(UTF.GetByteCount(filename));
          sw.Write(UTF.GetBytes(filename));
          sw.Flush();

          
          long length = fileInfo.Length;
          sw.Write(length);
          using (var stream = new System.IO.FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read)) {
            const int BUFFER_SIZE = 1 << 12;
            byte[] buffer = new byte[BUFFER_SIZE];
            for (long i = 0; i < length; i += BUFFER_SIZE) {
              int expected = (int)Math.Min((long)BUFFER_SIZE, (length - i));
              int read = stream.Read(buffer, 0, expected);
              if (read != expected) {
                throw new FileLoadException();
              }
              sw.Write(buffer, 0, read);
            }
          }
          
          sw.Write((int)MESSAGE_TYPES.QUIT);
          sw.Flush();
          connection.Stream.Dispose();
          connection.Client.Close();
        } catch {
          MessageBox.Show("Failed to send file over network :(");
        } finally {

        }
      });      
    }
    
    private object syncRoot = new object();
    private Task latestTask;
    private void EnqueueAction(System.Action action) {
      lock (syncRoot) {
        if (latestTask == null) {
          latestTask = Task.Factory.StartNew(action);
        } else {
          latestTask = latestTask.ContinueWith(tsk => action());
        }
      }
    }
    
    private class Connection {
      public TcpClient Client;
      public Stream Stream;
      public BinaryWriter Writer;
      public Connection(IPEndPoint endpoint) {
        Client = new TcpClient();
        Client.SendTimeout = 10000;
        Client.ReceiveTimeout = 10000;
        Client.Connect(endpoint);
        Stream = Client.GetStream();
        Writer = new BinaryWriter(Stream);

      }      
    }
    private Connection MouseConnection;
    private bool MouseConnectionBussy =false ;
    private Connection GetMouseConnection() {
      if (null == MouseConnection) {
        MouseConnection = new Connection(State.RemoteEndpoint);        
      }
      return MouseConnection;      
    }
    
    
    public void SendMousePosition(Point relativePosition,SharedgeForm.CURSOR_KIND cursorKind) {
      lock (syncRoot) {
        if (!MouseConnectionBussy) {
          MouseConnectionBussy = true;
          EnqueueAction(() => {
            try {
              var sw = GetMouseConnection().Writer;
              sw.Write((int)MESSAGE_TYPES.MOUSE_MOVE);
              sw.Write(relativePosition.X);
              sw.Write(relativePosition.Y);
              sw.Write((int)cursorKind);
              sw.Flush();
            } catch (Exception) {
              MouseConnection = null;
            } finally {
              MouseConnectionBussy = false;
            }
          });
        }
      }
    }

    private Connection BitmapPositionConnection;
    private bool BitmapPositionConnectionBussy = false;
    private Connection GetBitmapPositionConnection() {
      if (null == BitmapPositionConnection) {
        BitmapPositionConnection = new Connection(State.RemoteEndpoint);
      }
      return BitmapPositionConnection;
    }


    public void SendBitmapPosition(Rectangle position) {
      lock (syncRoot) {
        if (!BitmapPositionConnectionBussy) {
          BitmapPositionConnectionBussy = true;
          EnqueueAction(() => {
            try {
              var sw = GetBitmapPositionConnection().Writer;
              sw.Write((int)MESSAGE_TYPES.BITMAP_POSITION);
              sw.Write(position.X);
              sw.Write(position.Y);
              sw.Write(position.Width);
              sw.Write(position.Height);
              sw.Flush();
            } catch (Exception) {
              BitmapPositionConnection = null;
            } finally {
              BitmapPositionConnectionBussy = false;
            }
          });
        }
      }
    }

    public void SendText(string text) {
      EnqueueAction(() => {
        try {
          var connection = new Connection(State.RemoteEndpoint);
          var sw = connection.Writer;
          sw.Write((int)MESSAGE_TYPES.NEW_TEXT);
          var UTF = new UTF8Encoding();
          sw.Write(UTF.GetByteCount(text));
          sw.Write(UTF.GetBytes(text));
          sw.Write((int)MESSAGE_TYPES.QUIT);
          sw.Flush();
          connection.Stream.Dispose();
          connection.Client.Close();
        } catch (Exception) {
          MessageBox.Show("Failed to send text over network :(");
        } finally {
          
        }        
      });
    }

    public IEnumerable<FileInfo> EnumerateIncomingFiles() {
      return IncomingDirectoryInfo.EnumerateFiles();
    }

    public delegate void NewTextEventHandler(string message);
    public delegate void MouseMoveEventHandler(Point point,SharedgeForm.CURSOR_KIND cursorKind);
    public delegate void BitmapReceivedEventHandler(Bitmap bitmap);
    public delegate void BitmapMoveEventHandler(Rectangle position);
    public delegate void NewFileEventHandler(string filename);
    
    public NewTextEventHandler NewText;
    public MouseMoveEventHandler MouseMove;
    public BitmapReceivedEventHandler BitmapReceived;
    public BitmapMoveEventHandler BitmapMove;
    public NewFileEventHandler NewFile;

  }
}
