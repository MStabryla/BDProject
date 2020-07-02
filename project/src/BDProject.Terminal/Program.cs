using System;
using BDProject.Core;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace BDProject.Terminal
{
    partial class Program
    {
        static void Main(string[] args)
        {
            bool connectionEstablished = false;
            bool useConfig = args.Length > 0;
            Connection con = new Connection();
            while (!connectionEstablished)
            {
                if (useConfig)
                {
                    string[] configFileLines = System.IO.File.ReadAllLines(args[0]);
                    try
                    {
                        con.Connect(
                            configFileLines[1],
                            configFileLines[3],
                            configFileLines[5]);
                        if (con.ConnectionStatus == ConnectionState.Open)
                        {
                            Console.WriteLine("Connected to " + con.DatabaseAddress);
                            connectionEstablished = true;
                            break;
                        }
                    }
                    catch (SqlException exception)
                    {
                        Console.WriteLine(exception.Message);
                        useConfig = false;
                    }
                }
                Console.WriteLine("Podaj parametry (adres serwera, login, hasło):");
                try
                {

                    con.Connect(
                        Console.ReadLine(),
                        Console.ReadLine(),
                        ReadPassword());
                    if (con.ConnectionStatus == ConnectionState.Open)
                    {
                        Console.WriteLine("Connected to " + con.DatabaseAddress);
                        connectionEstablished = true;
                    }
                }
                catch (SqlException exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            if(args.Contains("--linq"))
                MenuLinq(con);
            else
                Menu(con);
        }
        private static void MenuLinq(Connection con)
        {
            LinqQueries.Init(con);
            Console.WriteLine("Specjalne operacje:");
            foreach (KeyValuePair<string, string> elem in LinqQueries.menuInstruction)
                Console.WriteLine(elem.Key + " => '" + elem.Value + "'");
            while(true)
            {
                Console.Write("> ");
                string command = Console.ReadLine();
                IEnumerable<object> response;
                if (LinqQueries.menuOption.ContainsKey(command))
                {
                    response = LinqQueries.menuOption[command](con);
                }
                else
                {
                    response = LinqQueries.Query(command);
                }
                Console.Write(IenumerableAgregatorToString(response));
            }
        }
        private static void Menu(Connection con)
        {
            Console.WriteLine("Specjalne operacje:");
            foreach (KeyValuePair<string, string> elem in menuInstruction)
                Console.WriteLine(elem.Key + " => '" + elem.Value + "'");
            while (true)
            {
                Console.Write("> ");
                string command = Console.ReadLine();
                if (menuOption.ContainsKey(command))
                {
                    command = menuOption[command](con);
                }
                try
                {
                    ResponseAgregator response = con.Query(command);
                    Console.Write(ResponseAgregatorToString(response));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private static string ReadPassword()
        {
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            Console.Write("\n");
            return pass;
        }

        private static string ResponseAgregatorToString(ResponseAgregator response){
            string strResponse = "";
            int[] columnWidth = new int[response.ColumnNames.Count()];
            for(int i=0;i<columnWidth.Length;i++)
            {
                columnWidth[i] = response.ColumnNames.ElementAt(i).Length;
            }
            foreach(object[] values in response.Values){
                for(int i=0;i<values.Length;i++)
                {
                    if(values[i].ToString().Length > columnWidth[i])
                        columnWidth[i] = values[i].ToString().Length;
                }
            }
            string strColumns = new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1); strColumns += '\n';
            for(int i=0;i<columnWidth.Length;i++)
            {
                string columnName = response.ColumnNames.ElementAt(i);
                strColumns += "| " + columnName.ToUpper() + new string(' ',columnWidth[i] - columnName.Length + 1);
            }
            strColumns += "|\n";
            strColumns += new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1) +"\n";
            strResponse += strColumns;
            foreach(object[] values in response.Values){
                for(int i=0;i<values.Length;i++)
                {
                    string valueStr = values[i].ToString();
                    strResponse += "| " + valueStr + new string(' ',columnWidth[i] - valueStr.Length + 1);
                }
                strResponse += "|\n";
            }
            strResponse += new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1) + "\n";
            return strResponse;
        }
        private static string IenumerableAgregatorToString(IEnumerable<object> response){
            string strResponse = "";
            if(response.Count() <= 0)
                return "";
            FieldInfo[] fields = response.ElementAt(0).GetType().GetFields();
            int[] columnWidth = new int[fields.Count()];
            for(int i=0;i<columnWidth.Length;i++)
            {
                columnWidth[i] = fields.ElementAt(i).Name.Length;
            }
            foreach(object values in response){
                var valueType = values.GetType();
                for(int i=0;i<valueType.GetFields().Count();i++)
                {
                    if(valueType.GetField(fields[i].Name).GetValue(values).ToString().Length > columnWidth[i])
                        columnWidth[i] = valueType.GetField(fields[i].Name).GetValue(values).ToString().Length;
                }
            }
            string strColumns = new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1); strColumns += '\n';
            for(int i=0;i<columnWidth.Length;i++)
            {
                string columnName = fields.ElementAt(i).Name;
                strColumns += "| " + columnName.ToUpper() + new string(' ',columnWidth[i] - columnName.Length + 1);
            }
            strColumns += "|\n";
            strColumns += new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1) +"\n";
            strResponse += strColumns;
            foreach(object values in response){
                var valueType = values.GetType();
                for(int i=0;i<fields.Count();i++)
                {
                    string valueStr = valueType.GetField(fields[i].Name).GetValue(values).ToString();
                    strResponse += "| " + valueStr + new string(' ',columnWidth[i] - valueStr.Length + 1);
                }
                strResponse += "|\n";
            }
            strResponse += new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1) + "\n";
            return strResponse;
        }
    }
}
