using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MinimalSerialProjekt
{
  class MySQLManager
  {
    private MySqlConnection connection;
    private string server;
    private string database;
    private string uid;
    private string password;

    public MySQLManager()
    {
      Initialise();
    }

    private void Initialise()
    {
      server = "localhost";
      database = "minimalserialdatabase";
      uid = "root";
      password = "";
      string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                                database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

      connection = new MySqlConnection(connectionString);
    }

    public bool Open()
    {
      Console.WriteLine("Opening MySQL connection..");

      try
      {
        connection.Open();

        // State will change eventually, for simplicitys' sake no timout is implemented
        while (connection.State == ConnectionState.Connecting);

        return true;
      }
      catch (MySqlException ex)
      {
        //When handling errors, you can your application's response based 
        //on the error number.
        //The two most common error numbers when connecting are as follows:
        //0: Cannot connect to server.
        //1045: Invalid user name and/or password.
        switch (ex.Number)
        {
          case 0:
            Console.WriteLine("Cannot connect to server.  Contact administrator");
            break;

          case 1045:
            Console.WriteLine("Invalid username/password, please try again");
            break;
        }

        return false;
      }
    }

    /// <summary>
    /// Closes connection
    /// </summary>
    /// <returns>Whether it could be closed</returns>
    public bool Close()
    {
      try
      {
        connection.Close();
        return true;
      }
      catch (MySqlException ex)
      {
        Console.WriteLine(ex.Message);
        return false;
      }
    }

    /// <summary>
    /// Insert statement
    /// </summary>
    public void InsertData(string temperature, string timestamp)
    {
      string query = $"INSERT INTO temperatures (Temperature, Time) VALUES('{temperature}', '{timestamp}')";

      if (connection.State != ConnectionState.Open)
      {
        Console.WriteLine($"Query not possible, because MySQL connection state is '{connection.State}'");
        return;
      }

      //create command and assign the query and connection from the constructor
      MySqlCommand cmd = new MySqlCommand(query, connection);

      //Execute command
      int result = cmd.ExecuteNonQuery();
      switch (result)
      {
        case 1:
          Console.WriteLine($"Successfully wrote {temperature} to the table!");
          break;
        default:
          Console.WriteLine($"ERROR: Unknown errorcode while trying to insert the temperature into the table: {result}");
        break;
      }
    }
  }
}
