using System;
using BDProject.Core;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Linq;

namespace BDProject.Terminal
{
    partial class Program
    {
        private static Dictionary<string, Func<Connection,string>> menuOption = new Dictionary<string, Func<Connection,string>>() {
            {"_TABLES", (con) => "SELECT * FROM INFORMATION_SCHEMA.TABLES" },
            {"_ALL_LINIA", (con) => {
                return "SELECT nr_linii FROM Linia";
                }
            },
            {"_ALL_PRZYSTANEK", (con) => {
                return "SELECT nazwa,NZ FROM Przystanek";
                }
            },
            {"_LINIA", (con) => {
                ResponseAgregator dataCheck;
                dataCheck = con.Query("SELECT [id], [nr_linii] FROM Linia");
                Console.WriteLine("Dostępne linie:");
                Console.Write(ResponseAgregatorToString(dataCheck));
                Console.Write("id linii: ");
                string linia = Console.ReadLine();
                return String.Format("SELECT nazwa FROM Przystanek INNER JOIN Przyjazd ON [Przystanek].[id] = [Przyjazd].[id_przyst] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[id] = {0} AND [Przyjazd].[kierunek] = 0 GROUP BY nazwa,[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]",linia);
                }
            },
            {"_PRZYSTANEK", (con) => {
                ResponseAgregator dataCheck;
                dataCheck = con.Query("SELECT [id], [nazwa] FROM Przystanek");
                Console.WriteLine("Dostępne przystanki:");
                Console.Write(ResponseAgregatorToString(dataCheck));
                Console.Write("id przystanku: ");
                string przystanek = Console.ReadLine();
                return String.Format("SELECT [Linia].[nr_linii] FROM Linia INNER JOIN Przyjazd ON [Linia].[id] = [Przyjazd].[id_linia] RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] WHERE [Przystanek].[id] = {0} AND [Przyjazd].[kierunek] = 0 GROUP BY [Linia].[nr_linii],[Przyjazd].[kolejność] ORDER BY [Przyjazd].[kolejność]",przystanek);
                }
            },
            {"_ROZKLAD_PRZYSTANEK", (con) => {
                ResponseAgregator dataCheck;
                dataCheck = con.Query("SELECT [id], [nazwa] FROM Przystanek");
                Console.WriteLine("Dostępne przystanki:");
                Console.Write(ResponseAgregatorToString(dataCheck));
                Console.Write("id przystanku: ");
                string przystanek = Console.ReadLine();
                Console.Write("do przystanku końcowego (Y/N): ");
                int odp = Console.ReadLine()[0] == 'Y' ? 0 : 1;
                return String.Format("SELECT [Linia].[nr_linii],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Przystanek].[id] = {0} AND [Przyjazd].[kierunek] = {1} ORDER BY [Przyjazd].[godzina]",przystanek,odp);
                }
            },
            {"_ROZKLAD_LINIA", (con) => {
                ResponseAgregator dataCheck;
                dataCheck = con.Query("SELECT [id], [nr_linii] FROM Linia");
                Console.WriteLine("Dostępne linie:");
                Console.Write(ResponseAgregatorToString(dataCheck));
                Console.Write("id linii: ");
                string linia = Console.ReadLine();
                Console.Write("do przystanku końcowego (Y/N): ");
                int odp = Console.ReadLine()[0] == 'Y' ? 0 : 1;
                return String.Format("SELECT [Przystanek].[nazwa],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[id] = {0} AND [Przyjazd].[kierunek] = {1} ORDER BY [Przyjazd].[godzina] ",linia,odp);
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
                    Console.Write("Id przystanku początkowego: ");
                    long przystanek_p = Convert.ToInt64(Console.ReadLine());
                    busStopApproved = dataCheck.Values.Any(x => ((SqlInt64)x.ElementAt(0)).Value == przystanek_p);
                    if(!busStopApproved)
                    {
                        Console.WriteLine("Nie ma takiego przystanku w liście");
                        continue;
                    }
                    dataCheck = con.Query(String.Format("SELECT * FROM Przystanek WHERE [id] = {0}",przystanek_p));
                    if(dataCheck.Values.Count() <= 0){
                        Console.WriteLine("Nie ma takiego przystanku w bazie");
                    }
                } while(dataCheck.Values == null || dataCheck.Values.Count() == 0 || !busStopApproved);
                long przystanek_p_id = ((SqlInt64)dataCheck.Values.ElementAt(0).ElementAt(0)).Value;
                dataCheck = con.Query(String.Format("SELECT [id], [nazwa] FROM Przystanek WHERE [id] > {0}",przystanek_p_id));
                Console.WriteLine("Dostępne przystanki końcowe");
                Console.Write(ResponseAgregatorToString(dataCheck));
                do {
                    Console.Write("Id przystanku końcowego: ");
                    long przystanek_k = Convert.ToInt64(Console.ReadLine());
                    dataCheck = con.Query(String.Format("SELECT * FROM Przystanek WHERE [id] = {0} AND [id] > {1}",przystanek_k,przystanek_p_id));
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
                Console.WriteLine("Dostępne linie:");
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
                        dataCheck = con.Query(String.Format("SELECT [Linia].[nr_linii] FROM Linia INNER JOIN Przyjazd ON [Linia].[id] = [Przyjazd].[id_linia] RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] WHERE [Przystanek].[id] = {0} AND ( [Linia].[przyst_kon] = [Przystanek].[id] OR [Linia].[przyst_pocz] = [Przystanek].[id] ) GROUP BY [Linia].[nr_linii]",id_long));
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
                bool busStopApproved = false,lineApproved = false;
                long id_linia,przyst_id = 0,przyst_k_id = 0,new_przyst_id,new_przyst_k_id;
                dataCheck = con.Query("SELECT [id], [nr_linii] FROM Linia");
                Console.WriteLine("Dostępne linie:");
                Console.Write(ResponseAgregatorToString(dataCheck));
                do{
                    Console.Write("id linii: ");
                    id_linia = Convert.ToInt64(Console.ReadLine());
                    dataCheck = con.Query("SELECT [przyst_pocz], [przyst_kon] FROM Linia WHERE [id] = " + id_linia);
                    lineApproved = dataCheck.Values.Count() > 0;
                    if(!lineApproved)
                        Console.WriteLine("Nie ma linii o podanym id");
                }while(!lineApproved);
                przyst_id = ((SqlInt64) dataCheck.Values.ElementAt(0).ElementAt(0)).Value;
                przyst_k_id = ((SqlInt64) dataCheck.Values.ElementAt(0).ElementAt(1)).Value;
                Console.Write("Chcesz zmienić przystanek początkowy? (Y/N) ");
                if(Console.ReadLine().ToUpper() == "Y")
                {
                    dataCheck = con.Query("SELECT [id], [nazwa] FROM Przystanek WHERE [Przystanek].[id] < " + przyst_k_id);
                    Console.WriteLine("Dostępne przystanki:");
                    Console.Write(ResponseAgregatorToString(dataCheck));
                    do{
                        Console.Write("Id nowego przystanku początkowego: ");
                        new_przyst_id = Convert.ToInt64(Console.ReadLine());
                        dataCheck = con.Query("SELECT * FROM Przystanek WHERE [id] = " + new_przyst_id + " AND [id] < " + przyst_k_id);
                        busStopApproved = dataCheck.Values.Count() > 0;
                        if(!busStopApproved)
                            Console.WriteLine("Nie ma pasującego przystanku o podanym id");
                    }while(!busStopApproved);
                }
                else
                    new_przyst_id = przyst_id;
                busStopApproved = false;
                Console.Write("Chcesz zmienić przystanek końcowy? (Y/N) ");
                if(Console.ReadLine().ToUpper() == "Y")
                {
                    dataCheck = con.Query("SELECT [id], [nazwa] FROM Przystanek WHERE [Przystanek].[id] > " + new_przyst_id);
                    Console.WriteLine("Dostępne przystanki:");
                    Console.Write(ResponseAgregatorToString(dataCheck));
                    do{
                        Console.Write("Id nowego przystanku początkowego: ");
                        new_przyst_k_id = Convert.ToInt64(Console.ReadLine());
                        dataCheck = con.Query("SELECT * FROM Przystanek WHERE [id] = " + new_przyst_k_id + " AND [id] > " + new_przyst_id);
                        busStopApproved = dataCheck.Values.Count() > 0;
                        if(!busStopApproved)
                            Console.WriteLine("Nie ma pasującego przystanku o podanym id");
                    }while(!busStopApproved);
                }
                else
                    new_przyst_k_id = przyst_k_id;
                if(new_przyst_id != przyst_id || new_przyst_k_id != przyst_k_id)
                {
                    dataCheck = con.Query(String.Format("SELECT TOP(1) [godzina] FROM Przyjazd RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[id] = {0};",id_linia));
                    DateTime startHour = new DateTime(((TimeSpan) dataCheck.Values.ElementAt(0).ElementAt(0)).Ticks);
                    con.Query(String.Format("DELETE FROM Przyjazd WHERE [id_linia] = {0}",id_linia));
                    con.GenerateArriveRecords(startHour,id_linia,new_przyst_id,new_przyst_k_id);
                    con.Query(String.Format("UPDATE Linia SET [przyst_pocz] = {0}, [przyst_kon] = {1} WHERE [id] = {2}",new_przyst_id,new_przyst_k_id,id_linia));
                }
                return String.Format("SELECT [Przystanek].[nazwa],[Przyjazd].[godzina] FROM Przyjazd RIGHT JOIN Przystanek ON [Przyjazd].[id_przyst] = [Przystanek].[id] RIGHT JOIN Linia ON [Przyjazd].[id_linia] = [Linia].[id] WHERE [Linia].[id] = {0} ORDER BY [Przyjazd].[godzina] ",id_linia);
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
            {"_UPDATE_LINIA","Umożliwia zmianę trasy linii"}
        };
    }
}