using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.IO;
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
        public bool isactive;
        public int order;
    }

    [WebMethod]
    public string Init() {
        try {
            NewBaner x = new NewBaner();
            x.id = null;
            x.img = null;
            x.isactive = true;
            x.order = 0;
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load(bool isactive) {
        try {
            return JsonConvert.SerializeObject(LoadData(isactive), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public List<NewBaner> LoadData(bool isactive) {
        DB.CreateDataBase(G.db.banners);
        string sql = string.Format("SELECT id, img, isactive, b_order FROM banners {0}", isactive ? "WHERE isactive = 'True'" : "");
        List<NewBaner> xx = new List<NewBaner>();
        NewBaner x = new NewBaner();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewBaner>();
                    while (reader.Read()) {
                        x = ReadDataRow(reader);
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
            NewBaner x = new NewBaner();
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT * FROM banners WHERE id = '{0}'", id);
                using (var command = new SQLiteCommand(sql, connection)) {
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x = ReadDataRow(reader);
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
                sql = string.Format(@"INSERT INTO banners VALUES ('{0}', '{1}', '{2}', {3})", x.id, x.img, x.isactive, x.order);
            } else {
                sql = string.Format(@"UPDATE banners SET img = '{1}', isactive = '{2}', b_order = {3} WHERE id = '{0}'", x.id, x.img, x.isactive, x.order);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(false), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(NewBaner x) {
        try {
            string sql = string.Format("DELETE FROM banners WHERE id = '{0}'", x.id);
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            RemoveImg(x.img);
            return JsonConvert.SerializeObject(LoadData(false), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public NewBaner ReadDataRow(SQLiteDataReader reader) {
        NewBaner x = new NewBaner();
        x.id = G.ReadS(reader, 0);
        x.img = G.ReadS(reader, 1);
        x.isactive = G.ReadB(reader, 2);
        x.order = G.ReadI(reader, 3);
        return x;
    }

    public void RemoveImg(string img) {
        string img_ = img.Contains('?') ? img.Split('?')[0] : img;
        string folder = "~/upload/banners";
        string path = Server.MapPath(folder);
        string file = Server.MapPath(string.Format("{0}/{1}", folder, img));
        if (Directory.Exists(path)) {
            File.Delete(file);
        }
    }

}
