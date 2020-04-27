using System;
using BDProject.Core;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace BDProject.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            bool connectionEstablished = false;
            bool useConfig = args.Length > 0;
            Connection con = new Connection();
            while(!connectionEstablished){
                if(useConfig){
                    string[] configFileLines = System.IO.File.ReadAllLines(args[0]);
                    try{
                        con.Connect(
                            configFileLines[1],
                            configFileLines[3],
                            configFileLines[5]);
                        if(con.ConnectionStatus == ConnectionState.Open){
                            Console.WriteLine("Connected to " + con.DatabaseAddress);
                            connectionEstablished = true;
                            break;
                        }
                    }
                    catch(SqlException exception){
                        Console.WriteLine(exception.Message);
                        useConfig = false;
                    }
                }
                Console.WriteLine("Podaj parametry (adres serwera, login, hasło):");
                try{
                    
                    con.Connect(
                        Console.ReadLine(),
                        Console.ReadLine(),
                        ReadPassword());
                    if(con.ConnectionStatus == ConnectionState.Open){
                        Console.WriteLine("Connected to " + con.DatabaseAddress);
                        connectionEstablished = true;
                    }
                }
                catch(SqlException exception){
                    Console.WriteLine(exception.Message);
                }
            }
            Menu(con);
        }
        private static Dictionary<string,string> menuOption = new Dictionary<string, string>() {
            {"_TABLES","SELECT * FROM INFORMATION_SCHEMA.TABLES"}
        };
        private static void Menu(Connection con){
            Console.WriteLine("Specjalne operacje:");
            foreach(KeyValuePair<string,string> elem in menuOption)
                Console.WriteLine(elem.Key + " => '" + elem.Value +"'");
            while(true){
                Console.Write("> ");
                string command = Console.ReadLine();
                if(menuOption.ContainsKey(command)){
                    command = menuOption[command];
                }
                try{
                    Console.Write(con.Query(command));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private static string ReadPassword(){
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
                    else if(key.Key == ConsoleKey.Enter)
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
