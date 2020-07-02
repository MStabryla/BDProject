using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Linq;

namespace BDProject.Core.Data
{
    abstract class Convertable
    {
        public virtual void Convert(object[] data){}
    }
    public class Autobus
    {
        public long id;
        public string marka;
        public string model;
        public long id_linia;

        public static object Convert(object[] data)
        {
            var newObject = new Autobus();
            newObject.id = ((SqlInt64)data[0]).Value;
            newObject.marka = ((SqlString)data[1]).Value;
            newObject.model = ((SqlString)data[2]).Value;
            newObject.id_linia = ((SqlInt64)data[3]).Value;
            return newObject;
        }
    }
    public class Linia
    {
        public long id;
        public int nr_linii;
        public long przyst_pocz;
        public long przyst_kon;

        public static object Convert(object[] data)
        {
            var newObject = new Linia();
            newObject.id = ((SqlInt64)data[0]).Value;
            newObject.nr_linii = ((SqlInt32)data[1]).Value;
            newObject.przyst_pocz = ((SqlInt64)data[2]).Value;
            newObject.przyst_kon = ((SqlInt64)data[3]).Value;
            return newObject;
        }
    }
    public class Kierowca
    {
        public long id;
        public string imie;
        public string nazwisko;
        public DateTime zatrudniony;
        public long id_aut;

        public static object Convert(object[] data)
        {
            
            var newObject = new Kierowca();
            newObject.id = ((SqlInt64)data[0]).Value;
            newObject.imie = ((SqlString)data[1]).Value;
            newObject.nazwisko = ((SqlString)data[2]).Value;
            newObject.zatrudniony = (DateTime)data[3];
            newObject.id_aut = ((SqlInt64)data[4]).Value;
            return newObject;
        }
    }
    
    public class Przyjazd
    {
        public long id;
        public byte kierunek;
        public TimeSpan godzina;
        public int kolejnosc;
        public long id_linia;
        public long id_przyst;

        public static object Convert(object[] data)
        {
            var newObject = new Przyjazd();
            newObject.id = ((SqlInt64)data[0]).Value;
            newObject.kierunek = ((SqlByte)data[1]).Value;
            newObject.godzina = (TimeSpan)data[2];
            newObject.kolejnosc = ((SqlInt16)data[3]).Value;
            newObject.id_linia = ((SqlInt64)data[4]).Value;
            newObject.id_przyst = ((SqlInt64)data[5]).Value;
            return newObject;
        }
    }
    
    public class Przystanek
    {
        public long id;
        public string nazwa;
        public byte NZ;

        public static object Convert(object[] data)
        {
            var newObject = new Przystanek();
            newObject.id = ((SqlInt64)data[0]).Value;
            newObject.nazwa = ((SqlString)data[1]).Value;
            newObject.NZ = ((SqlByte)data[2]).Value;
            return newObject;
        }
    }
}