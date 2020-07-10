using System;
using BDProject.Core;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BDProject.Core.Data;

namespace BDProject.Terminal
{
    delegate object Converter(object[] args);
    static class LinqQueries
    {
        public static Dictionary<string,List<object>> localDatabase = new Dictionary<string, List<object>>();
        public static void Init(Connection con){
            ResponseAgregator data = con.Query("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME ");
            string[] columnNames = data.Values.Select(x => x[0].ToString()).ToArray();
            foreach(string columnName in columnNames)
            {
                List<object> newDatabaseTableData = new List<object>();   
                Type acttype = Type.GetType("BDProject.Core.Data." + columnName + ", BDProject.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",true);
                MethodInfo conv = acttype.GetMethod("Convert");
                acttype.GetMethods(BindingFlags.Static);
                data = con.Query("SELECT * FROM " + columnName);
                foreach(var elem in data.Values)
                {
                    newDatabaseTableData.Add(conv.Invoke(null,new object[] { elem } ));
                }
                localDatabase.Add(columnName,newDatabaseTableData);
            }
        }
        public static Dictionary<string, Func<Connection,IEnumerable<object>>> menuOption = new Dictionary<string, Func<Connection,IEnumerable<object>>>() {
            {"_ALL_LINIA", (con) => {
                return localDatabase["Linia"].Select(x => (Linia)x).OrderBy(x => x.nr_linii);
                }
            },
            {"_LINIA", (con) => {
                List<Linia> lines = localDatabase["Linia"].Select(x => (Linia)x).ToList();
                List<Przyjazd> arrives = localDatabase["Przyjazd"].Select(x => (Przyjazd)x).ToList();
                List<Przystanek> stops = localDatabase["Przystanek"].Select(x => (Przystanek)x).ToList();
                Console.Write(Program.IenumerableAgregatorToString(localDatabase["Linia"]));
                Console.Write("id linii: ");
                string linia = Console.ReadLine();
                var result = from l in lines 
                    join a in arrives on l.id equals a.id_linia
                    where l.id == Convert.ToInt64(linia) 
                    join s in stops on a.id_przyst equals s.id
                    select new { NrLinii = l.nr_linii, Stop = s.nazwa };
                return result.GroupBy(x => x.Stop).Select(x => new { Nazwa = x.Key} );
                }
            }
        };
        public static Dictionary<string, string> menuInstruction = new Dictionary<string, string>(){
            {"_ALL_LINIA", "Zwraca wszystkie linie autobusowe"},
            {"_LINIA", "Zwraca wszystkie przystanki, na których dana linia się zatrzymuje"}
        };

        public static IEnumerable<object> Query(string query)
        {
            string[] queryelement = query.Split(' ');
            if(queryelement.Length <= 0)
                return null;
            string table = queryelement[0];
            if(!localDatabase.Keys.Contains(table))
                return null;
            IEnumerable<object> actDataTable = localDatabase[table];
            Type realType = actDataTable.ElementAt(0).GetType();
            for(int i=1;i<queryelement.Length;i++)
            {
                string[] parsedOperation = queryelement[i].Split(":");
                if(parsedOperation.Length <= 1)
                    continue;
                string operation = parsedOperation[0];
                string[] arguments = parsedOperation.Skip(1).ToArray();
                switch(operation)
                {
                    case "Order":
                        actDataTable = actDataTable.OrderBy(x => realType.GetField(arguments[0].ToLower()).GetValue(x));
                        break;
                    case "Contains":
                        actDataTable = actDataTable.Where(x => realType.GetField(arguments[0].ToLower()).GetValue(x).ToString().Contains(arguments[1]));
                        break;
                }
            }
            return actDataTable;
        }
    }
}