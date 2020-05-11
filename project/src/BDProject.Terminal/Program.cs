using System;
using BDProject.Core;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
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
        private static Dictionary<string, Func<Connection,string>> menuOption = new Dictionary<string, Func<Connection,string>>() {
            {"_TABLES", (con) => "SELECT * FROM INFORMATION_SCHEMA.TABLES" },
            {"_LINIA", (con) => {
                Console.Write("nr linii: ");
                string linia = Console.ReadLine();
                return String.Format("SELECT nazwa FROM Przystanek INNER JOIN Przyjazd ON [Przystanek].[id] = [Przyjazd].[id_przyst] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[nr_linii] = {0} AND [Przyjazd].[kierunek] = 0 GROUP BY nazwa,[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]",linia);
                }
            },
            {"_ALL_LINIA", (con) => {
                return "SELECT nr_linii FROM Linia";
                }
            },
            {"_ALL_PRZYSTANEK", (con) => {
                return "SELECT nazwa,NZ FROM Przystanek";
                }
            },
            {"_PRZYSTANEK", (con) => {
                Console.Write("nazwa przystanku: ");
                string przystanek = Console.ReadLine();
                return String.Format("SELECT [Linia].[nr_linii] FROM Linia INNER JOIN Przyjazd ON [Linia].[id] = [Przyjazd].[id_linia] RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] WHERE [Przystanek].[nazwa] = '{0}' AND [Przyjazd].[kierunek] = 0 GROUP BY [Linia].[nr_linii],[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]",przystanek);
                }
            },
            {"_ROZKLAD_PRZYSTANEK", (con) => {
                Console.Write("nazwa przystanku: ");
                string przystanek = Console.ReadLine();
                Console.Write("do przystanku końcowego (Y/N): ");
                int odp = Console.ReadLine()[0] == 'Y' ? 0 : 1;
                return String.Format("SELECT [Linia].[nr_linii],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Przystanek].[nazwa] = '{0}' AND [Przyjazd].[kierunek] = {1} ORDER BY [Przyjazd].[godzina]",przystanek,odp);
                }
            },
            {"_ROZKLAD_LINIA", (con) => {
                Console.Write("nr linii: ");
                string linia = Console.ReadLine();
                Console.Write("do przystanku końcowego (Y/N): ");
                int odp = Console.ReadLine()[0] == 'Y' ? 0 : 1;
                return String.Format("SELECT [Przystanek].[nazwa],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[nr_linii] = {0} AND [Przyjazd].[kierunek] = {1} ORDER BY [Przyjazd].[godzina] ",linia,odp);
                }
            },
            {"_NEW_LINIA", (con) => {
                Console.Write("nowy nr linii: ");
                string nr_linii = Console.ReadLine();
                bool busStopApproved = false;
                ResponseAgregator dataCheck;
                dataCheck = con.Query("DECLARE @PrzystanekCount INT;SELECT @PrzystanekCount = Count(id) FROM Przystanek;SELECT TOP (@PrzystanekCount - 1) [id], [nazwa] FROM Przystanek;");
                Console.WriteLine("Dostępne przystanki");
                Console.Write(ResponseAgregatorToString(dataCheck));
                do {
                    Console.Write("Przystanek początkowy: ");
                    string przystanek_p = Console.ReadLine();
                    busStopApproved = dataCheck.Values.Any(x => ((SqlString)x.ElementAt(1)).Value == przystanek_p);
                    if(!busStopApproved)
                    {
                        Console.WriteLine("Nie ma takiego przystanku w liście");
                        continue;
                    }
                    dataCheck = con.Query(String.Format("SELECT * FROM Przystanek WHERE [nazwa] = '{0}'",przystanek_p));
                    if(dataCheck.Values.Count() <= 0){
                        Console.WriteLine("Nie ma takiego przystanku w bazie");
                    }
                } while(dataCheck.Values == null || dataCheck.Values.Count() == 0 || !busStopApproved);
                long przystanek_p_id = ((SqlInt64)dataCheck.Values.ElementAt(0).ElementAt(0)).Value;
                dataCheck = con.Query(String.Format("SELECT [id], [nazwa] FROM Przystanek WHERE [id] > {0}",przystanek_p_id));
                Console.WriteLine("Dostępne przystanki końcowe");
                Console.Write(ResponseAgregatorToString(dataCheck));
                do {
                    Console.Write("Przystanek końcowy: ");
                    string przystanek_k = Console.ReadLine();
                    dataCheck = con.Query(String.Format("SELECT * FROM Przystanek WHERE [nazwa] = '{0}' AND [id] > {1}",przystanek_k,przystanek_p_id));
                    if(dataCheck.Values.Count() <= 0){
                        Console.WriteLine("Nie ma takiego odpowiedniego przystanku");
                    }
                } while(dataCheck.Values == null || dataCheck.Values.Count() == 0);
                long przystanek_k_id = ((SqlInt64)dataCheck.Values.ElementAt(0).ElementAt(0)).Value;
                dataCheck = con.Query(String.Format("INSERT INTO Linia ([nr_linii], [przyst_pocz], [przyst_kon]) VALUES ({0},{1},{2})",nr_linii,przystanek_p_id,przystanek_k_id));
                dataCheck = con.Query(String.Format("SELECT * FROM Linia WHERE [nr_linii] = {0}",nr_linii));
                long id_linii = ((SqlInt64)dataCheck.Values.ElementAt(0).ElementAt(0)).Value;
                Console.Write("Godzina (%H:%M):");
                string godzinastr = Console.ReadLine();
                int minuta = godzinastr.Split(':').Count() >= 2 ? Convert.ToInt16(godzinastr.Split(':')[1]) : 0;
                int godzina = Convert.ToInt16(godzinastr.Split(':')[0]);
                con.GenerateArriveRecords(new DateTime(1,1,1,godzina, minuta,0),id_linii,przystanek_p_id,przystanek_k_id);
                return String.Format("SELECT [Przystanek].[nazwa],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[nr_linii] = {0} ORDER BY [Przyjazd].[godzina] ",nr_linii);
                }
            },
            {"_NEW_PRZYSTANEK", (con) => {
                Console.Write("nowa nazwa przystanku: ");
                string nazwa = Console.ReadLine();
                Console.Write("Na żądanie (Y/N): ");
                bool nz = Console.ReadLine() == "Y";
                con.Query(String.Format("INSERT INTO Przystanek ([nazwa], [NZ]) VALUES ('{0}', {1})",nazwa,nz ? 1 : 0));
                return String.Format("SELECT * FROM przystanek WHERE [nazwa] = '{0}'",nazwa);
                }
            },
            {"_REMOVE_LINIA", (con) => {
                ResponseAgregator dataCheck;
                dataCheck = con.Query("SELECT [id], [nr_linii] FROM Linia");
                Console.WriteLine("Dostępne linie");
                Console.Write(ResponseAgregatorToString(dataCheck));
                Console.Write("Id linii: ");
                string id = Console.ReadLine();
                con.Query(String.Format("DELETE FROM Przyjazd WHERE [id_linia] = {0}",id));
                con.Query(String.Format("DELETE FROM Linia WHERE [id] = {0}",id));
                return "SELECT [id], [nr_linii] FROM Linia";
                }
            },
            {"_REMOVE_PRZYSTANEK", (con) => {
                ResponseAgregator dataCheck;
                bool busStopApproved = false;
                long id_long;
                dataCheck = con.Query("SELECT [id], [nazwa] FROM Przystanek");
                Console.WriteLine("Dostępne przystanki");
                Console.Write(ResponseAgregatorToString(dataCheck));
                do{
                    Console.Write("Id przystanku: ");
                    string id = Console.ReadLine();
                    id_long = Convert.ToInt64(id);
                    dataCheck = con.Query("SELECT [Przystanek].[id], nazwa FROM Przystanek INNER JOIN Przyjazd ON [Przystanek].[id] = [Przyjazd].[id_przyst] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[przyst_pocz] = [Przystanek].[id] OR [Linia].[przyst_kon] = [Przystanek].[id] GROUP BY [Przystanek].[id],nazwa");
                    busStopApproved = !dataCheck.Values.Any( x => ((SqlInt64)x.ElementAt(0)).Value == id_long);
                    if(!busStopApproved)
                    {
                        Console.WriteLine("Nie można usunąć przystanku początkowego i końcowego. Zmień ustawienia linii związanych z tym przystankiem");
                        dataCheck = con.Query(String.Format("SELECT [Linia].[nr_linii] FROM Linia INNER JOIN Przyjazd ON [Linia].[id] = [Przyjazd].[id_linia] RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] WHERE [Przystanek].[id] = 4 AND ( [Linia].[przyst_kon] = [Przystanek].[id] OR [Linia].[przyst_pocz] = [Przystanek].[id] ) GROUP BY [Linia].[nr_linii]",id_long));
                        Console.Write(ResponseAgregatorToString(dataCheck));
                    }
                }while(!busStopApproved);
                con.Query(String.Format("DELETE FROM Przyjazd WHERE [id_przyst] = {0}",id_long));
                con.Query(String.Format("DELETE FROM Przystanek WHERE [id] = {0}",id_long));
                return "SELECT [id], [nazwa] FROM Przystanek";
                }
            },
            {"_UPDATE_LINIA", (con) => {
                ResponseAgregator dataCheck;
                bool busStopApproved = false;
                long id_long;
                
                return "SELECT [id], [nazwa] FROM Przystanek";
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
            {"_ROZKLAD_PRZYSTANEK", "Zwraca rozkład jazdy dla danego przystanku"},
            {"_NEW_LINIA","Tworzy nową linię"},
            {"_NEW_PRZYSTANEK","Tworzy nowy przystanek"},
            {"_REMOVE_LINIA","Usuwa linię"},
            {"_REMOVE_PRZYSTANEK","Usuwa przystanek, który nie jest początkowym i końcowym przystankiem dowolnej linii"},
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
    }
}
