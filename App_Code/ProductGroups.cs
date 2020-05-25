using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// ProductGroups
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
[DataContract(IsReference = true)]
public class ProductGroups : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();
    public ProductGroups () {
    }

    public class NewProductGroup {
        public string id;
        public string code;
        public string title;
        public string title_seo;
        public string parent;
        public int order;
        public List<NewProductGroup> subGroups;
    }

    [WebMethod]
    public string Init() {
        try {
            NewProductGroup x = new NewProductGroup();
            x.id = null;
            x.code = null;
            x.title = null;
            x.title_seo = null;
            x.parent = null;
            x.order = 0;
            x.subGroups = new List<NewProductGroup>();
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

    [WebMethod]
    public string Get(string id) {
        try {
            List<NewProductGroup> xx = new List<NewProductGroup>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT * FROM productGroups WHERE id = '{0}'", id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewProductGroup x = new NewProductGroup();
                            x.id = G.ReadS(reader, 0);
                            x.code = G.ReadS(reader, 1);
                            x.title = G.ReadS(reader, 2);
                            x.parent = G.ReadS(reader, 3);
                            x.order = G.ReadI(reader, 4);
                            x.subGroups = GetSubGroups(x.code);
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
    public string Save(NewProductGroup x) {
        try {
            DB.CreateDataBase(G.db.productGroups);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO productGroups VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", x.id, x.code, x.title, x.parent, x.order);
            } else {
                sql = string.Format(@"UPDATE productGroups SET code = '{1}', title = '{2}', parent = '{3}', pg_order = {4} WHERE id = '{0}'", x.id, x.code, x.title, x.parent, x.order);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("Spremljeno", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(NewProductGroup x) {
        try {
            string sql = string.Format(@"DELETE FROM ProductGroups WHERE {0}"
                        , x.code == x.parent ? string.Format("parent = '{0}'", x.code) : string.Format("code = '{0}'", x.code));
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

    public List<NewProductGroup> LoadData() {
        DB.CreateDataBase(G.db.productGroups);
        string sql = "SELECT id, code, title, parent, pg_order FROM productGroups WHERE code = parent ORDER BY pg_order";
        List<NewProductGroup> xx = new List<NewProductGroup>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewProductGroup>();
                    while (reader.Read()) {
                        NewProductGroup x = new NewProductGroup();
                        x.id = G.ReadS(reader, 0);
                        x.code = G.ReadS(reader, 1);
                        x.title = G.ReadS(reader, 2);
                        x.title_seo = G.GetSeoTitle(x.title);
                        x.parent = G.ReadS(reader, 3);
                        x.order = G.ReadI(reader, 4);
                        x.subGroups = GetSubGroups(x.code);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public List<NewProductGroup> GetSubGroups(string parent) {
        DB.CreateDataBase(G.db.productGroups);
        string sql = string.Format("SELECT id, code, title, parent, pg_order FROM productGroups WHERE code <> '{0}' AND parent = '{0}' ORDER BY pg_order", parent);
        List<NewProductGroup> xx = new List<NewProductGroup>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewProductGroup>();
                    while (reader.Read()) {
                        NewProductGroup x = new NewProductGroup();
                        x.id = G.ReadS(reader, 0);
                        x.code = G.ReadS(reader, 1);
                        x.title = G.ReadS(reader, 2);
                        x.title_seo = G.GetSeoTitle(x.title);
                        x.parent = G.ReadS(reader, 3);
                        x.order = G.ReadI(reader, 4);
                        x.subGroups = null;  // TODO: isprogramirati za vise od 1 podgrupe GetSubGroups(x.code);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public string GetParentGroup(string subGroup) {
        DB.CreateDataBase(G.db.productGroups);
        string x = null;
        string sql = string.Format("SELECT parent FROM productGroups WHERE code = '{0}'", subGroup);
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = G.ReadS(reader, 0);
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

   
}
