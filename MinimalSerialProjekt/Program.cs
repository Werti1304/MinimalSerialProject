using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static System.String;

namespace MinimalSerialProjekt
{
  class Program
  {
    private const int xamppWaitTime = 2000;

    private static SerialPort serialPort;

    private static XamppManager xamppManager;

    private static MySQLManager mySqlManager;

    private static bool closeXamppServices = true;

    static void Main(string[] args)
    {
      // Prompt user for Xampp folder-name
      Console.Write($"XAMPP Folder [{XamppManager.XamppDefFolder}]: ");
      string xamppFolder = Console.ReadLine();

      // Sets Exit-Handler
      SetConsoleCtrlHandler(Handler, true);

      // Start Xampp
      xamppManager = new XamppManager(xamppFolder);
      xamppManager.Start();

      string[] openPorts = SerialPort.GetPortNames();

      string lastPort = openPorts.LastOrDefault();

      string openPortsString = "";

      // Creates string that creates all COM-Port options
      openPortsString += "COM-Port [";
      foreach (var openPort in SerialPort.GetPortNames())
      {
        openPortsString += openPort;

        if(openPort != lastPort)
        {
          openPortsString += ", ";
        }
      }
      openPortsString += "]: ";

      // Prompt user for COM-Port name
      Console.Write(openPortsString);
      var comPort = Console.ReadLine();

      if (comPort == "")
      {
        comPort = lastPort;
      }

      // Prompt user for Baudrate
      const int defBaudRate = 9600;
      Console.Write($"Baudrate [{defBaudRate}]: ");
      var baudRateString = Console.ReadLine();

      // If entered string isn't a number, the baudrate is set to a default value
      if (!IsNumber(baudRateString, out var baudRate))
      {
        Console.WriteLine($"Defaulting to default baudrate: {defBaudRate}");
        baudRate = defBaudRate;
      }

      Console.Write($"Disable MySQL and Apache after closing [{closeXamppServices}]: ");
      var inp = Console.ReadLine()?.ToLower();
      if (inp == "false" || inp == "f")
      {
        closeXamppServices = false;
      }

      if (!SerialPort.GetPortNames().Contains(comPort))
      {
        Console.WriteLine("COM-Port wasn't found. Please try again");
        Stop();
      }

      // Create and open serial port with newly acquired information
      Console.WriteLine("Opening serial port..");
      serialPort = new SerialPort(comPort, baudRate);
      serialPort.Open();

      // Set an event to fire when receiving anything new
      serialPort.DataReceived += SerialPortOnDataReceived;

      // Starting mysql
      mySqlManager = new MySQLManager();

      // Open connection to mysql server
      mySqlManager.Open();

      System.Diagnostics.Process.Start("http://localhost/Temperature/simple.php");

      // Wait for Enter Key Press
      Console.ReadLine();

      // Shuts down all services (program closes automatically after this point)
      Stop();
    }
    
    /// <summary>
    /// Closes all started services, making the application ready for a clean exit
    /// </summary>
    private static void Stop()
    {
      xamppManager?.Stop(closeXamppServices);

      serialPort?.Close();

      mySqlManager?.Close();

      Console.Read();

      Environment.Exit(0);
    }

    private static readonly Regex RegexTempMatch = new Regex(@"([0-9]{2})+\.+([0-9]{2})");

    /// <summary>
    /// Serial-Port Event, triggered when receiving (any) data
    /// </summary>
    /// <param name="sender">Event parameter, not used</param>
    /// <param name="e">Event parameter, not used</param>
    private static void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      string temperature = serialPort.ReadLine();

      // Not a problem multi-threading wise, because the windows console has an integrated check for Console requests
      Console.WriteLine(temperature);

      var match = RegexTempMatch.Match(temperature);

      if (!match.Success)
      {
        Console.WriteLine("Last temperature didn't match format and will not be inserted into the database!");
        return;
      }

      var alteredTemp = match.Value;

      DateTime timestamp = DateTime.Now;

      // Current time in MySQL DateTime format
      string sqlFormattedDate = timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");

      mySqlManager.InsertData(alteredTemp, sqlFormattedDate);
    }

    /// <summary>
    /// Returns whether the string is a number
    /// </summary>
    /// <param name="str">String to be checked</param>
    /// <param name="number">Result of the conversion</param>
    /// <returns></returns>
    private static bool IsNumber(string str, out int number)
    {
      return int.TryParse(str, out number);
    }

    #region CloseHandler
    // https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms686016.aspx
    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

    // https://msdn.microsoft.com/fr-fr/library/windows/desktop/ms683242.aspx
    private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);

    private enum CtrlType
    {
      CTRL_C_EVENT = 0,
      CTRL_BREAK_EVENT = 1,
      CTRL_CLOSE_EVENT = 2,
      CTRL_LOGOFF_EVENT = 5,
      CTRL_SHUTDOWN_EVENT = 6
    }

    private static bool Handler(CtrlType signal)
    {
      switch (signal)
      {
        case CtrlType.CTRL_BREAK_EVENT:
        case CtrlType.CTRL_C_EVENT:
        case CtrlType.CTRL_LOGOFF_EVENT:
        case CtrlType.CTRL_SHUTDOWN_EVENT:
        case CtrlType.CTRL_CLOSE_EVENT:
          Console.WriteLine("\n\n----------------\nCLOSING\n----------------\n\n");
          OnExit();
          Environment.Exit(0);
          break;
        default:
          return false;
      }

      return false;
    }

    /// <summary>
    /// OnExit-"Event", triggered when closing the program (in any form, except TMGR)
    /// </summary>
    private static void OnExit()
    {
      Stop();
    }
    #endregion
  }
}

