using System;
using BDProject.Core;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace BDProject.Terminal
{
    class Program
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
            Menu(con);
        }
        private static Dictionary<string, Func<string>> menuOption = new Dictionary<string, Func<string>>() {
            {"_TABLES", () => "SELECT * FROM INFORMATION_SCHEMA.TABLES" },
            {"_LINIA", () => {
                Console.Write("nr linii: ");
                string linia = Console.ReadLine();
                return String.Format("SELECT nazwa FROM Przystanek INNER JOIN Przyjazd ON [Przystanek].[id] = [Przyjazd].[id_przyst] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[nr_linii] = {0} AND [Przyjazd].[kierunek] = 0 GROUP BY nazwa,[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]",linia);
                }
            },
            {"_ALL_LINIA", () => {
                return "SELECT nr_linii FROM Linia";
                }
            },
            {"_ALL_PRZYSTANEK", () => {
                return "SELECT nazwa,NZ FROM Przystanek";
                }
            },
            {"_PRZYSTANEK", () => {
                Console.Write("nazwa przystanku: ");
                string przystanek = Console.ReadLine();
                return String.Format("SELECT [Linia].[nr_linii] FROM Linia INNER JOIN Przyjazd ON [Linia].[id] = [Przyjazd].[id_linia] RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] WHERE [Przystanek].[nazwa] = '{0}' AND [Przyjazd].[kierunek] = 0 GROUP BY [Linia].[nr_linii],[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]",przystanek);
                }
            },
            {"_ROZKLAD_PRZYSTANEK", () => {
                Console.Write("nazwa przystanku: ");
                string przystanek = Console.ReadLine();
                Console.Write("do przystanku końcowego (Y/N): ");
                int odp = Console.ReadLine()[0] == 'Y' ? 1 : 0;
                return String.Format("SELECT [Linia].[nr_linii],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Przystanek].[nazwa] = '{0}' AND [Przyjazd].[kierunek] = {1} ORDER BY [Przyjazd].[godzina]",przystanek,odp);
                }
            },
            {"_ROZKLAD_LINIA", () => {
                Console.Write("nr linii: ");
                string przystanek = Console.ReadLine();
                Console.Write("do przystanku końcowego (Y/N): ");
                int odp = Console.ReadLine()[0] == 'Y' ? 1 : 0;
                return String.Format("SELECT [Przystanek].[nazwa],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[nr_linii] = {0} AND [Przyjazd].[kierunek] = {1} ORDER BY [Przyjazd].[godzina] ",przystanek,odp);
                }
            },
        };
        private static Dictionary<string, string> menuInstruction = new Dictionary<string, string>(){
            {"_TABLES", "Zwraca wszystkie tabele w bazie"},
            {"_ALL_LINIA", "Zwraca wszystkie linie autobusowe"},
            {"_ALL_PRZYSTANEK", "Zwraca wszystkie przystanki autobusowe"},
            {"_LINIA", "Zwraca wszystkie przystanki, na których dana linia się zatrzymuje"},
            {"_PRZYSTANEK", "Zwraca wszystkie linie, które zatrzymują się na danym przystanku"},
            {"_ROZKLAD_LINIA", "Zwraca rozklad jazdy dla danej linii"},
            {"_ROZKLAD_PRZYSTANEK", "Zwraca rozkład jazdy dla danego przystanku"}
        };
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
                    command = menuOption[command]();
                }
                try
                {
                    ResponseAgregator response = con.Query(command);
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
                    strColumns += new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1);
                    Console.WriteLine(strColumns);
                    string strResponse = "";
                    foreach(object[] values in response.Values){
                        for(int i=0;i<values.Length;i++)
                        {
                            string valueStr = values[i].ToString();
                            strResponse += "| " + valueStr + new string(' ',columnWidth[i] - valueStr.Length + 1);
                        }
                        strResponse += "|\n";
                    }
                    strResponse += new string('-',columnWidth.Sum() + columnWidth.Length*3 + 1) + "\n";
                    Console.Write(strResponse);
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
    }
}
