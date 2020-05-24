using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Brands
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Brands : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();

    public Brands() {
    }

    public class NewBrands {
        public string id;
        public string code;
        public string title;
        public string title_seo;
        public int order;
    }

    [WebMethod]
    public string Init() {
        try {
            NewBrands x = new NewBrands();
            x.id = null;
            x.code = null;
            x.title = null;
            x.title_seo = null;
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

    public List<NewBrands> LoadData() {
        DB.CreateDataBase(G.db.brands);
        string sql = "SELECT id, code, title, b_order FROM brands";
        List<NewBrands> xx = new List<NewBrands>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewBrands>();
                    while (reader.Read()) {
                        NewBrands x = new NewBrands();
                        x.id = G.ReadS(reader, 0);
                        x.code = G.ReadS(reader, 1);
                        x.title = G.ReadS(reader, 2);
                        x.title_seo = G.GetSeoTitle(x.title);
                        x.order = G.ReadI(reader, 3);
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
            List<NewBrands> xx = new List<NewBrands>();
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT * FROM brands WHERE id = '{0}'", id);
                using (var command = new SQLiteCommand(sql, connection)) {
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewBrands x = new NewBrands();
                            x.id = G.ReadS(reader, 0);
                            x.code = G.ReadS(reader, 1);
                            x.title = G.ReadS(reader, 2);
                            x.order = G.ReadI(reader, 3);
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
    public string Save(NewBrands x) {
        try {
            DB.CreateDataBase(G.db.brands);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO brands VALUES ('{0}', '{1}', '{2}', '{3}')", x.id, x.code, x.title, x.order);
            } else {
                sql = string.Format(@"UPDATE brands SET code = '{1}', title = '{2}', b_order = {3} WHERE id = '{0}'", x.id, x.code, x.title, x.order);
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
    public string Delete(NewBrands x) {
        try {
            string sql = string.Format(" DELETE FROM brands WHERE id = '{0}'", x.id);
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
