using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
namespace Sharedge {
  static class Program {
    static IDictionary<string, string> ParseArgs(string[] args) {
      Regex longArg = new Regex("^--(.*)$");
      string key = null;
      var parsed = new Dictionary<string, string>();
      foreach (String arg in args) {
        if (longArg.IsMatch(arg)) {
          key = longArg.Match(arg).Groups[1].Value;
        } else {
          if (null == key) {
            MessageBox.Show("What do you mean by " + arg + " ?");
          } else {
            parsed.Add(key, arg);
          }
          key = null;
        }
      }

      return parsed;
    }
    static IDictionary<string, IDictionary<string, string>> GetDefaultSettings() {
      var left = new Dictionary<string, string>();
      left.Add("remoteHost", null);
      left.Add("remotePort", "12344");
      left.Add("localHost", null);
      left.Add("localPort", "12345");

      var right = new Dictionary<string, string>();
      right.Add("remoteHost", null);
      right.Add("remotePort", "12345");
      right.Add("localHost", null);
      right.Add("localPort", "12344");

      var all = new Dictionary<string, IDictionary<string, string>>();
      all.Add("left", left);
      all.Add("right", right);

      return all;

    }
    static string GetDefaultSetting(string key, string side) {
      var defaultSettings = GetDefaultSettings();
      return defaultSettings[side][key];
    }
    static IPAddress Parse(string localHost) {
      try {
        return IPAddress.Parse(localHost);
      } catch (Exception) {
        var addresses = Dns.GetHostAddresses(localHost);
        return addresses[0];

      }

    }
    static string GetMyIPs() {
      IPHostEntry host;
      string localIP = "";
      host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress ip in host.AddressList) {
        if (ip.AddressFamily == AddressFamily.InterNetwork) {
          localIP = (0 < localIP.Length ? (localIP + " or ") : "") + ip.ToString();
        }
      }
      return localIP;
    }
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args) {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      String installedDirectory = Application.StartupPath;
      State state = new State();
      if (0 < args.Length) {
        var parsed = ParseArgs(args);
        state.Side = parsed.ContainsKey("side") ? parsed["side"] : "left";
        string remoteHost = parsed.ContainsKey("remoteHost") ? parsed["remoteHost"] : GetDefaultSetting("remoteHost", state.Side);
        string remotePort = parsed.ContainsKey("remotePort") ? parsed["remotePort"] : GetDefaultSetting("remotePort", state.Side);
        string localPort = parsed.ContainsKey("localPort") ? parsed["localPort"] : GetDefaultSetting("localPort", state.Side);
        string localHost = parsed.ContainsKey("localHost") ? parsed["localHost"] : GetDefaultSetting("localHost", state.Side);

        if (localHost == null) {
          state.LocalAddress = IPAddress.Any;
        } else {
          try {
            state.LocalAddress = Parse(localHost);
          } catch (Exception) {
            MessageBox.Show("Could not resolve --localHost " + localHost + "\n" +
                            "Your IP is " + GetMyIPs() + "\n" +
                            "Specify it, or drop the --localHost argument completely\n");
            return;
          }
        }
        if (remoteHost == null) {

          MessageBox.Show("You must specify --remoteHost \n" +
                          "Your IP is " + GetMyIPs() + "\n" +
                          "Provide this as the --remoteHost on the other machine\n");
          return;
        } else {
          try {
            state.RemoteAddress = Parse(remoteHost);
          } catch (Exception) {
            MessageBox.Show("Could not resolve remoteHost: " + remoteHost);
            return;
          }
        }
        try {
          state.RemotePort = Int32.Parse(remotePort);
        } catch (Exception) {
          MessageBox.Show("Could not parse --remotePort " + remotePort);
        }
        try {

          state.LocalPort = Int32.Parse(localPort);
        } catch (Exception) {
          MessageBox.Show("Could not parse --localPort " + localPort);
        }
        Application.Run(new SharedgeForm(state));
      } else {

      }


    }
  }
}
