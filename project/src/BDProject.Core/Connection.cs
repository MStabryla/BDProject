using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

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
    }
    public struct ResponseAgregator{
        public ResponseAgregator(IEnumerable<string> _ColumnNames,IEnumerable<object[]> _Values) 
        { ColumnNames = _ColumnNames; Values = _Values; }
        public IEnumerable<string> ColumnNames;
        public IEnumerable<object[]> Values;
    }
}
