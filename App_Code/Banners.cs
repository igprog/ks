using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Banners
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Banners : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();

    public Banners() {
    }

    public class NewBaner {
        public string id;
        public string img;
        public int order;
    }

    [WebMethod]
    public string Init() {
        try {
            NewBaner x = new NewBaner();
            x.id = null;
            x.img = null;
            x.order = 0;
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load() {
        try {
            return JsonConvert.SerializeObject(LoadData(), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public List<NewBaner> LoadData() {
        DB.CreateDataBase(G.db.banners);
        string sql = "SELECT id, img, b_order FROM banners";
        List<NewBaner> xx = new List<NewBaner>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewBaner>();
                    while (reader.Read()) {
                        NewBaner x = new NewBaner();
                        x.id = G.ReadS(reader, 0);
                        x.img = G.ReadS(reader, 1);
                        x.order = G.ReadI(reader, 2);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    [WebMethod]
    public string Get(string id) {
        try {
            List<NewBaner> xx = new List<NewBaner>();
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT * FROM banners WHERE id = '{0}'", id);
                using (var command = new SQLiteCommand(sql, connection)) {
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewBaner x = new NewBaner();
                            x.id = G.ReadS(reader, 0);
                            x.img = G.ReadS(reader, 1);
                            x.order = G.ReadI(reader, 2);
                            xx.Add(x);
                        }
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }

    }

    [WebMethod]
    public string Save(NewBaner x) {
        try {
            DB.CreateDataBase(G.db.banners);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO banners VALUES ('{0}', '{1}', '{2}')", x.id, x.img, x.order);
            } else {
                sql = string.Format(@"UPDATE banners SET img = '{1}', b_order = {2} WHERE id = '{0}'", x.id, x.img, x.order);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(NewBaner x) {
        try {
            string sql = string.Format(" DELETE FROM banners WHERE id = '{0}'", x.id);
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

}
