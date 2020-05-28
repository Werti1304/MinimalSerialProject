using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MinimalSerialProjekt
{
  class XamppManager
  {
    private readonly string _xamppFolder;

    public const string XamppDefFolder = @"C:\xampp";

    public XamppManager(string xamppFolder = XamppDefFolder) => this._xamppFolder = xamppFolder;

    private Process xamppControlProcess;
    private Process apacheProcess;
    private Process mysqlProcess;

    /// <summary>
    /// Starts Xampp with PHPMyAdmin and MySQL
    /// </summary>
    public void Start()
    { 
      // Source: https://www.apachefriends.org/faq_windows.html
      Console.WriteLine("Starting Xampp Control..");
      xamppControlProcess = ProgramStart("xampp-control.exe");

      if (!CrudePing("localhost", 80))
      {
        Console.WriteLine("Apache not running, starting..");
        apacheProcess = ProgramStart("apache_start.bat", hidden: true);
      }

      if (!CrudePing("localhost", 3306))
      {
        Console.WriteLine("MySQL not running, starting..");
        mysqlProcess = ProgramStart("mysql_start.bat", hidden: true);
      }
    }

    /// <summary>
    /// Stops Xampp, PHPMyAdmin and MySQL
    /// </summary>
    public void Stop(bool closeServices)
    {
      if (closeServices)
      {
        // We don't know if these processes have been created
        if (apacheProcess != null)
        {
          apacheProcess.CloseMainWindow();
          apacheProcess.Close();
        }

        if (mysqlProcess != null)
        {
          mysqlProcess.CloseMainWindow();
          mysqlProcess.Close();
        }

        // Due to problems with closing programs that are started with the CreateNoWindow attribute is this our "failsafe"
        ProgramStart("apache_stop.bat");
        ProgramStart("mysql_stop.bat");
      }

      xamppControlProcess.CloseMainWindow();
      xamppControlProcess.Kill();      // Will attempt to minimize if closed, but we don't want that
      xamppControlProcess.Close();
    }

    private Process ProgramStart(string relativePath, bool hidden = false)
    {
      Process process = new Process();

      ProcessStartInfo processStartInfo =
        new ProcessStartInfo
        {
          FileName = Path.Combine(_xamppFolder, relativePath)
        };

      if (hidden)
      {
        processStartInfo.CreateNoWindow = true;
        processStartInfo.UseShellExecute = false;
      }

      process.StartInfo = processStartInfo;

      process.Start();

      return process;
    }

    private static bool CrudePing(string hostUri, int portNumber)
    {
      try
      {
        using (TcpClient client = new TcpClient(hostUri, portNumber))
          return true;
      }
      catch (SocketException)
      {
        return false;
      }
    }
  }
}
