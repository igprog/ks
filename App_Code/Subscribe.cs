using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Subscribe
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Subscribe : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();
    string mainSql = "SELECT id, email FROM subscribe";
    public Subscribe() {
    }

       #region Class
    public class NewSubscribe {
        public string id;
        public string email;
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Init() {
        try {
            NewSubscribe x = new NewSubscribe();
            x.id = null;
            x.email = null;
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load() {
        try {
            return JsonConvert.SerializeObject(LoadData(mainSql), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(NewSubscribe x) {
        try {
            DB.CreateDataBase(G.db.subscribe);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
            }
            sql = string.Format(@"INSERT OR REPLACE INTO subscribe VALUES('{0}', '{1}')", x.id, x.email);
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(mainSql), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(NewSubscribe x) {
        try {
            string sql = string.Format("DELETE FROM subscribe WHERE id = '{0}'", x.id);
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(mainSql), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethods

    #region Methods
    //public NewSubscribe GetData(string sku) {
    //    string sql = string.Format("{0} WHERE id = '{1}' ORDER BY rowid DESC", mainSql, sku);
    //    return LoadData(sql);
    //}

    public List<NewSubscribe> LoadData(string sql) {
        DB.CreateDataBase(G.db.subscribe);
        List<NewSubscribe> xx = DataCollection(sql);
        return xx;
    }

    public List<NewSubscribe> DataCollection(string sql) {
        List<NewSubscribe> xx = new List<NewSubscribe>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewSubscribe>();
                    while (reader.Read()) {
                        NewSubscribe x = ReadDataRow(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public NewSubscribe ReadDataRow(SQLiteDataReader reader) {
        NewSubscribe x = new NewSubscribe();
        x.id = G.ReadS(reader, 0);
        x.email = G.ReadS(reader, 1);
        return x;
    }
    #endregion Methods


}
