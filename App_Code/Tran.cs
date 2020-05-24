using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Igprog;

/// <summary>
/// Tran
/// </summary>
[WebService(Namespace = "http://dizajn911.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Tran : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();

    public Tran() {
    }

    public class NewTran {
        public string id;
        public string productId;
        public string tran;
        public string recordType;
        public string lang;
    }

    [WebMethod]
    public string Init() {
        try {
            NewTran x = new NewTran();
            x.id = null;
            x.productId = null;
            x.tran = null;
            x.recordType = null;
            x.lang = null;
            return JsonConvert.SerializeObject(x, Formatting.None);    
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load() {
        try {
            return JsonConvert.SerializeObject(LoadData(null, null, null), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string productId, string recordType, string lang) {
        try {
            return JsonConvert.SerializeObject(LoadData(productId, recordType, lang), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public List<NewTran> LoadData(string productId, string recordType, string lang) {
        DB.CreateDataBase(G.db.tran);
        string sql = string.Format(@"SELECT id, productId, tran, recordType, lang FROM tran {0}"
                    , string.IsNullOrEmpty(productId) ? string.Format("WHERE recordType = '{0}' AND lang = '{1}'", recordType, lang) : string.Format("WHERE productId = '{0}' AND recordType = '{1}' AND lang = '{2}'", productId, recordType, lang));
        List<NewTran> xx = new List<NewTran>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewTran>();
                    while (reader.Read()) {
                        NewTran x = new NewTran();
                        x.id = G.ReadS(reader, 0);
                        x.productId = G.ReadS(reader, 1);
                        x.tran = G.ReadS(reader, 2);
                        x.recordType = G.ReadS(reader, 3);
                        x.lang = G.ReadS(reader, 4);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    [WebMethod]
    public string Save(NewTran x) {
        try {
            DB.CreateDataBase(G.db.tran);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO tran VALUES('{0}', '{1}', '{2}', '{3}', '{4}')"
                                    , x.id, x.productId, x.tran, x.recordType, x.lang);
            } else {
                sql = string.Format(@"UPDATE tran SET productId = '{1}', tran = '{2}', recordType = '{3}', lang = '{4}' WHERE id = '{0}'"
                                    , x.id, x.productId, x.tran, x.recordType, x.lang);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(x.productId, x.recordType, x.lang), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }


    #region JSON translate
    public string TranJson(string title, string lang) {
            try {
                string path = string.Format("~/assets/json/translations/{0}/main.json", lang); ;
                string path1 = HttpContext.Current.Server.MapPath(path);
                string[] ss = Translations(lang);
                if (ss!=null){
                    foreach (string s in ss) {
                        string[] _s = s.Split(':');
                        if (_s.Count() == 2) {
                            if (_s[0].Replace("\"", "").Replace("\r", "").Replace("\n", "").Replace("{\r\n", "").Replace("{ ", "").Trim().ToLower().ToLower().ToString() == title.ToLower()) {
                                title = s.Split(':')[1].Replace("\"", "").Replace("\r\n}", "").Trim().ToString();
                            }
                        }
                    }
                    return title;
                } else {
                    return title;
                }
            } catch (Exception e) { return title; }
        }

        public string[] Translations(string lang) {
            string[] ss = null;
            string path = string.Format("~/assets/json/translations/{0}/main.json", lang);
            string path1 = HttpContext.Current.Server.MapPath(path);
            if (File.Exists(HttpContext.Current.Server.MapPath(path))) {
                string json = File.ReadAllText(HttpContext.Current.Server.MapPath(path));
                ss = Regex.Split(json, ",\r\n");
            }
            return ss;
        }
    #endregion JSON translate

}
