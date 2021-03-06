﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Linq;

namespace BDProject.Core
{
    public class Connection {
        private const string connectionString = "Server=tcp:{your_server_address},1433;Initial Catalog=stabryla;Persist Security Info=False;User ID={your_login};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private SqlConnection databaseConnection;

        public Connection(string serverAddress, string login,string password){
            Connect(serverAddress,login,password);
        }
        public Connection(){
        }
        public string DatabaseAddress {
            get => databaseConnection != null ? databaseConnection.DataSource : "";
        }
        public ConnectionState ConnectionStatus {
            get => databaseConnection.State;
        }
        public void Connect(string serverAddress, string login,string password){
            if(databaseConnection != null)
                return;
            databaseConnection = new SqlConnection(connectionString.Replace("{your_server_address}",serverAddress).Replace("{your_login}",login).Replace("{your_password}",password));
            databaseConnection.Open();
            
        }
        public ResponseAgregator Query(string sqlquery){
            List<string> columns = new List<string>();
            List<object[]> values = new List<object[]>();
            SqlCommand command = new SqlCommand(sqlquery,databaseConnection);
            using(SqlDataReader reader = command.ExecuteReader())
            {
                for(int i=0;i<reader.VisibleFieldCount;i++)
                {
                    columns.Add(reader.GetName(i));
                }
                while (reader.Read())
                {
                    object[] value = new object[columns.Count];
                    for(int i=0;i<reader.VisibleFieldCount;i++)
                        reader.GetSqlValues(value);
                    values.Add(value);
                }
            }
            return new ResponseAgregator(columns,values);
        }

        public string QueryString(string sqlquery){
            string result = "";
            SqlCommand command = new SqlCommand(sqlquery,databaseConnection);
            using(SqlDataReader reader = command.ExecuteReader())
            {
                for(int i=0;i<reader.VisibleFieldCount;i++)
                {
                    result += reader.GetName(i) + "\t";
                }
                result += "\n";
                while (reader.Read())
                {
                    for(int i=0;i<reader.VisibleFieldCount;i++)
                        result += reader[i].ToString() + ( i<reader.VisibleFieldCount-1 ? "\t" : "");
                    result += "\n";
                }
            }
            return result;
        }
        public void GenerateArriveRecords(DateTime starthour, long id_linia,long przyst_p,long przyst_kon)
        {
            ResponseAgregator listOfStops = Query(String.Format("SELECT * FROM Przystanek WHERE id >= {0} AND id <= {1}",przyst_p,przyst_kon));
            
            string template = "INSERT INTO Przyjazd ([kierunek], [godzina], [kolejność], [id_linia], [id_przyst]) VALUES ({0},'{1}',{2},{3},{4})";
            for(int j=0;j<5;j++){
                for(int i=0;i<listOfStops.Values.Count();i++){
                    long id = ((SqlInt64)listOfStops.Values.ElementAt(i).ElementAt(0)).Value;
                    Query(String.Format(template,0,starthour.ToString(),i + 1,id_linia,id));
                    starthour = starthour.AddMinutes(1);
                }
                starthour = starthour.AddMinutes(20);
                for(int i=listOfStops.Values.Count()-1;i>=0;i--){
                    long id = ((SqlInt64)listOfStops.Values.ElementAt(i).ElementAt(0)).Value;
                    Query(String.Format(template,1,starthour.ToString(),i + 1,id_linia,id));
                    starthour = starthour.AddMinutes(1);
                }
                starthour = starthour.AddMinutes(20);
            }
        }
    }
    public struct ResponseAgregator{
        public ResponseAgregator(IEnumerable<string> _ColumnNames,IEnumerable<object[]> _Values) 
        { ColumnNames = _ColumnNames; Values = _Values; }
        public IEnumerable<string> ColumnNames;
        public IEnumerable<object[]> Values;
    }
}
