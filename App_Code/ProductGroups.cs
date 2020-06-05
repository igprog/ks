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
    string mainSql = "SELECT id, code, title, parent, discount, discount_from, discount_to, img, pg_order FROM productGroups WHERE";
    public ProductGroups () {
    }

    public class NewProductGroup {
        public string id;
        public string code;
        public string title;
        public string title_seo;
        //public string parent;
        public NewProductGroup parent;
        public Products.Discount discount;
        public string img;
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
            x.parent = new NewProductGroup();
            x.img = null;
            x.order = 0;
            x.discount = new Products.Discount();
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
                string sql = string.Format("{0} {1}", mainSql, "id = '{0}'", id);
                xx = DataCollection(sql);
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
            x.discount.coeff = x.discount.perc / 100;
            bool isUpdateDiscount = false;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO productGroups VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8})", x.id, x.code, x.title, x.parent.code, x.discount.coeff, x.discount.from, x.discount.to, x.img, x.order);
            } else {
                sql = string.Format(@"UPDATE productGroups SET code = '{1}', title = '{2}', parent = '{3}', discount = '{4}', discount_from = '{5}', discount_to = '{6}', img = '{7}', pg_order = {8} WHERE id = '{0}'", x.id, x.code, x.title, x.parent.code, x.discount.coeff, x.discount.from, x.discount.to, x.img, x.order);
                if (x.code == x.parent.code) {
                    isUpdateDiscount = true;  //***** Update children groups discount (same sa parent group) *****
                }
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            if (isUpdateDiscount) {
                sql = string.Format(@"UPDATE productGroups SET discount = '{0}' WHERE parent = '{1}'", x.discount.coeff, x.code);
                using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(NewProductGroup x) {
        try {
            string sql = string.Format(@"DELETE FROM ProductGroups WHERE {0}"
                        , x.code == x.parent.code ? string.Format("parent = '{0}'", x.code) : string.Format("code = '{0}'", x.code));
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
        string sql = string.Format("{0} {1}", mainSql, "code = parent ORDER BY pg_order");
        List<NewProductGroup> xx = DataCollection(sql);
        return xx;
    }

    public List<NewProductGroup> DataCollection(string sql) {
        List<NewProductGroup> xx = new List<NewProductGroup>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewProductGroup>();
                    while (reader.Read()) {
                        NewProductGroup x = ReadDataRow(reader);
                        //x.parent = GetParentGroupData(x.parent.code);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }

        return xx;
    }

    public NewProductGroup GetParentGroupData(string parent) {
        NewProductGroup x = new NewProductGroup();
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            string sql = string.Format("{0} code = '{1}'", mainSql, parent);
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = ReadDataRow(reader);
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public NewProductGroup ReadDataRow(SQLiteDataReader reader) {
        NewProductGroup x = new NewProductGroup();
        x.id = G.ReadS(reader, 0);
        x.code = G.ReadS(reader, 1);
        x.title = G.ReadS(reader, 2);
        x.title_seo = G.GetSeoTitle(x.title);
        x.parent = new NewProductGroup();
        x.parent.code = G.ReadS(reader, 3);
        x.discount = new Products.Discount();
        x.discount.coeff = G.ReadD(reader, 4);
        x.discount.perc = Math.Round(x.discount.coeff * 100, 1);
        x.discount.from = G.ReadS(reader, 5);
        x.discount.to = G.ReadS(reader, 6);
        x.img = G.ReadS(reader, 7);
        x.order = G.ReadI(reader, 8);
        x.subGroups = GetSubGroups(x.code);
        return x;
    }

    public List<NewProductGroup> GetSubGroups(string parent) {
        DB.CreateDataBase(G.db.productGroups);
        string sql = string.Format("{0} {1}", mainSql, string.Format("code <> '{0}' AND parent = '{0}' ORDER BY pg_order", parent));
        List<NewProductGroup> xx = DataCollection(sql);
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
