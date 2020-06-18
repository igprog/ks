using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Global
/// </summary>
namespace Igprog {
    public class Global {
        public Global() {
        }

        //public string productGroups = "productGroups";
        //public string products = "products";
        public DB db = new DB();

        public string myEmail = ConfigurationManager.AppSettings["myEmail"];
        public string myEmailName = ConfigurationManager.AppSettings["myEmailName"];
        public string myPassword = ConfigurationManager.AppSettings["myPassword"];
        public int myServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort"]);
        public string myServerHost = ConfigurationManager.AppSettings["myServerHost"];
        public string email = ConfigurationManager.AppSettings["email"];
        public string adminUserName = ConfigurationManager.AppSettings["adminUserName"];
        public string adminPassword = ConfigurationManager.AppSettings["adminPassword"];
        public string supervisorUserName = ConfigurationManager.AppSettings["supervisorUserName"];
        public string supervisorPassword = ConfigurationManager.AppSettings["supervisorPassword"];
        public string dataBase = ConfigurationManager.AppSettings["dataBase"];

        public RecordType recordType = new RecordType();
        //public OptionType optionType = new OptionType();
        public FeatureType featureType = new FeatureType();
        public UserTypes userTypes = new UserTypes();
        public AdminType adminType = new AdminType();
        

        public class DB {
            public string productGroups = "productGroups";
            public string brands = "brands";
            public string products = "products";
            public string review = "review";
            public string users = "users";
            public string orders = "orders";
            public string banners = "banners";
            public string tran = "tran";
        }

        public class RecordType {
            public string services = "services";
            public string about = "about";
            public string productTitle = "productTitle";
            public string productShortDesc = "productShortDesc";
            public string productLongDesc = "productLongDesc";
        }

        public class UserTypes {
            public string legal = "legal";
            public string natural = "natural";
        }

        //public class OptionType {
        //    public string services = "services";
        //    public string product = "product";
        //}
        public class FeatureType {
            public string product = "p";
        }

        public class AdminType {
            public string admin = "admin";
            public string supervisor = "supervisor";
        }

        public class CodeTitle {
            public string code;
            public string title;
        }

        public string ReadS(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? null : reader.GetString(i);
        }

        public int ReadI(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? 0 : reader.GetInt32(i);
        }

        public double ReadD(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(i));
        }

        public bool ReadB(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? false : Convert.ToBoolean(reader.GetString(i));
        }

        public string GetSeoTitle(string title) {
            return !string.IsNullOrEmpty(title) ? title.Replace(" ", "-").Replace("č", "c").Replace("ć", "c").Replace("š", "s").Replace("ž", "z").Replace("đ", "d").Trim().ToLower() : null;
        }
    }
}