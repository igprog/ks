using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Review
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Review : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();
    string mainSql = "SELECT r.id, r.sku, r.name, r.email, r.desc, r.rating, r.reviewdate, r.isactive, r.lang FROM review r";
    public Review() {
    }

    #region Class
    public class NewReview {
        public string id;
        public string sku;
        public string name;
        public string email;
        public string desc;
        public double rating;
        public string reviewdate;
        public bool isactive;
        public string lang;
    }

    public class ReviewData {
        public List<NewReview> data;
        public double rating;
        public int count;
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Init(string sku) {
        try {
            NewReview x = new NewReview();
            x.id = null;
            x.sku = sku;
            x.name = null;
            x.email = null;
            x.desc = null;
            x.rating = 0;
            x.reviewdate = null;
            x.isactive = true;
            x.lang = null;
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
    public string LoadLastReviews(string lang, int limit) {
        try {
            string sql = string.Format("{0} {1}", mainSql, string.Format("WHERE CAST(r.rating AS DECIMAL)>=4 AND r.lang = '{0}' AND r.isactive = 'True' ORDER BY rowid DESC LIMIT {1}", lang, limit));
            return JsonConvert.SerializeObject(LoadData(sql), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string sku) {
        try {
            return JsonConvert.SerializeObject(GetData(sku), Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(NewReview x) {
        try {
            DB.CreateDataBase(G.db.review);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO review VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')"
                                    , x.id, x.sku, x.name, x.email, x.desc, x.rating, x.reviewdate, x.isactive, x.lang);
            } else {
                sql = string.Format(@"UPDATE review SET sku = '{1}', name = '{2}', email = '{3}', desc = '{4}', rating = '{5}', reviewdate = '{6}', isactive = '{7}', lang = '{8}' WHERE id = '{0}'"
                                    , x.id, x.sku, x.name, x.email, x.desc, x.rating, x.reviewdate, x.isactive, x.lang);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            //return JsonConvert.SerializeObject("Review saved successfully", Formatting.None);
            return JsonConvert.SerializeObject(GetData(x.sku), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(NewReview x) {
        try {
            string sql = string.Format("DELETE FROM review WHERE id = '{0}'", x.id);
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
    public ReviewData GetData(string sku) {
        string sql = string.Format("{0} WHERE r.sku = '{1}' AND r.isactive = 'True' ORDER BY r.rowid DESC", mainSql, sku);
        return LoadData(sql);
    }

    public ReviewData LoadData(string sql) {
        DB.CreateDataBase(G.db.review);
        ReviewData xxx = new ReviewData();
        List<NewReview> xx = DataCollection(sql);
        xxx.data = xx;
        if (xx.Count > 0) {
            xxx.rating = Math.Round(xx.Average(a => a.rating), 1);
            xxx.count = xx.Count(a => a.id != null);
        }
        return xxx;
    }

    public List<NewReview> DataCollection(string sql) {
        List<NewReview> xx = new List<NewReview>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewReview>();
                    while (reader.Read()) {
                        NewReview x = ReadDataRow(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public NewReview ReadDataRow(SQLiteDataReader reader) {
        NewReview x = new NewReview();
        x.id = G.ReadS(reader, 0);
        x.sku = G.ReadS(reader, 1);
        x.name = G.ReadS(reader, 2);
        x.email = G.ReadS(reader, 3);
        x.desc = G.ReadS(reader, 4);
        x.rating = G.ReadD(reader, 5);
        x.reviewdate = G.ReadS(reader, 6);
        x.isactive = G.ReadB(reader, 7);
        x.lang = G.ReadS(reader, 8);
        return x;
    }
    #endregion Methods







}
