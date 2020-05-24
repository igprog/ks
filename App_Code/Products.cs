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
using Igprog;

/// <summary>
/// Products
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Products : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();
    Tran T = new Tran();
    Features F = new Features();
    ProductGroups PG = new ProductGroups();
    string mainSql = @"SELECT p.id, p.sku, p.productgroup, p.title, p.shortdesc, p.longdesc, p.brand, p.img, p.price, p.discount, p.stock, p.isnew, p.outlet, p.bestselling, p.isactive, p.features, p.productorder, pg.title, b.title
                        FROM products p   
                        LEFT OUTER JOIN productGroups pg
                        ON p.productGroup = pg.code
                        LEFT OUTER JOIN brands b
                        ON p.brand = b.code ";

    public Products () {
    }

    #region Class
    public class NewProduct {
        public string id;
        public string sku;
        public ProductGroups.NewProductGroup productGroup;
        public string title;
        public string title_seo;
        public string shortdesc;
        public string longdesc;
        public Brands.NewBrands brand;
        public string img;
        public double discount;
        public int stock;
        public bool isnew;
        public bool outlet;
        public bool bestselling;
        public bool isactive;
        public string[] gallery;
        public List<Features.NewFeature> features;
        public int productorder;
        public Price price;
        public int qty;
    }

    public class Price {
        public double net;
        public double gross;
        public double net_discount;
        public double gross_discount;
        public double net_discount_tot;  // total with quantity
        public double gross_discount_tot;  // total with quantity
    }
    #endregion Class

    #region WebMethod
    [WebMethod]
    public string Init() {
        try {
            NewProduct x = new NewProduct();
            x.id = null;
            x.productGroup = new ProductGroups.NewProductGroup();
            x.title = null;
            x.title_seo = null;
            x.shortdesc = null;
            x.longdesc = null;
            x.brand = new Brands.NewBrands();
            x.img = null;
            x.price = new Price();
            x.discount = 0;
            x.stock = 1000;
            x.isnew = false;
            x.outlet = false;
            x.bestselling = false;
            x.isactive = true;
            x.features = F.Get(G.featureType.product);
            x.productorder = 0;
            x.gallery = null;
            x.price = new Price();
            x.qty = 1;
            return JsonConvert.SerializeObject(x, Formatting.None);    
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load(string lang, bool order, string productGroup, string brand) {
        try {
            return JsonConvert.SerializeObject(LoadData(lang, order, productGroup, brand), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string LoadOutlet(string lang, int limit) {
        try {
            DB.CreateDataBase(G.db.products);
            string sql = string.Format(@"{0} WHERE p.outlet = 'True' AND p.stock > 0 {1}"
                                    , mainSql
                                    , string.Format("LIMIT {0}", limit));
            List<NewProduct> xx = DataCollection(sql, lang, false);
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string LoadBestSelling(string lang, string productGroup, int limit) {
        try {
            DB.CreateDataBase(G.db.products);
            string sql = string.Format(@"{0} WHERE p.bestselling = 'True' {1} AND p.stock > 0 {2}"
                                    , mainSql
                                    , string.IsNullOrEmpty(productGroup) ? "" : string.Format("AND p.productgroup = '{0}'", productGroup)
                                    , string.Format("LIMIT {0}", limit));
            List<NewProduct> xx = DataCollection(sql, lang, false);
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string LoadProductGallery(string productId) {
        try {
            string[] x = GetGallery(productId);
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch(Exception e) {
            return null;
        }
    }

    [WebMethod]
    public string SetMainImg(NewProduct x, string img) {
        try {
            string img_ = img.Contains('?') ? img.Split('?')[0] : img;
            string sql = string.Format("UPDATE products SET img = '{0}' WHERE id = '{1}'" ,img_ , x.id);
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(null, false, x.productGroup.code, x.brand.code), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string id, string lang) {
        try {
            return JsonConvert.SerializeObject(GetProduct(id, lang), Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(NewProduct x) {
        try {
            DB.CreateDataBase(G.db.products);
            string sql = null;
            string productFeatures = null;
            if (x.features.Count > 0) {
                var pf_ = new List<string>();
                foreach (var pf in x.features) {
                    pf_.Add(string.Format("{0}:{1}", pf.code, pf.val));
                }
                productFeatures = string.Join(";", pf_);
            }
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO products VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', {16})"
                                    , x.id, x.sku, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.net, x.discount, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.productorder);
            } else {
                sql = string.Format(@"UPDATE products SET sku = '{1}', productgroup = '{2}', title = '{3}', shortdesc = '{4}', longdesc = '{5}', brand = '{6}', img = '{7}', price = '{8}', discount = '{9}', stock = '{10}', isnew = '{11}', outlet = '{12}', bestselling = '{13}', isactive = '{14}', features = '{15}', productorder = {16} WHERE id = '{0}'"
                                    , x.id, x.sku, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.net, x.discount, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.productorder);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            //return JsonConvert.SerializeObject(LoadData(null, false, x.productGroup.code, x.brand.code), Formatting.None);
            return JsonConvert.SerializeObject("Spremljeno", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(NewProduct x) {
        try {
            string sql = string.Format("DELETE FROM products WHERE id = '{0}'", x.id);
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(LoadData(null, false, x.productGroup.code, x.brand.code), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string DeleteImg(NewProduct x, string img) {
        try {
            string img_ = img.Contains('?') ? img.Split('?')[0] : img;
            string path = Server.MapPath(string.Format("~/upload/{0}/gallery", x.id));
            if (Directory.Exists(path)) {
                string[] gallery = Directory.GetFiles(path);
                foreach (string file in gallery) {
                    if (Path.GetFileName(file) == img_) {
                        File.Delete(file);
                        RemoveMainImg(x.id, img_);
                    }
                }
            }
            return JsonConvert.SerializeObject(LoadData(null, false, x.productGroup.code, x.brand.code), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethod

    #region Class
    public List<NewProduct> LoadData(string lang, bool order, string productGroup, string brand) {
        DB.CreateDataBase(G.db.products);
        string sql = string.Format(@"{0} {1} {2} {3}"
                    , mainSql
                    , string.IsNullOrEmpty(productGroup) ? "" : string.Format("WHERE p.productGroup = '{0}'", productGroup)
                    , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("WHERE p.brand = '{0}'", brand) : string.Format("AND p.brand = '{0}'", brand))
                    , order == true ? "ORDER BY p.productorder" : "");

        List<NewProduct> xx = DataCollection(sql, lang, true);
        return xx;
    }

    public NewProduct GetProduct(string id, string lang) {
        NewProduct x = new NewProduct();
        List<Features.NewFeature> features = F.Get(G.featureType.product);
        string sql = string.Format(@"{0} WHERE p.id = '{1}'"
                                    , mainSql
                                    , id);
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                var reader = command.ExecuteReader();
                x = new NewProduct();
                while (reader.Read()) {
                    x = ReadDataRow(reader, lang, true, features);
                }
            }
            connection.Close();
        }
        return x;
    }

    public List<NewProduct> DataCollection(string sql, string lang, bool loadAllData) {
        List<NewProduct> xx = new List<NewProduct>();
        List<Features.NewFeature> features = F.Get(G.featureType.product);
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewProduct>();
                    while (reader.Read()) {
                        NewProduct x = ReadDataRow(reader, lang, loadAllData, features);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public NewProduct ReadDataRow(SQLiteDataReader reader, string lang, bool loadAllData, List<Features.NewFeature> features) {
        NewProduct x = new NewProduct();
        x.id = G.ReadS(reader, 0);
        x.sku = G.ReadS(reader, 1);
        //if (loadAllData) {
            x.productGroup = new ProductGroups.NewProductGroup();
            x.productGroup.code = G.ReadS(reader, 2);
            x.productGroup.title = G.ReadS(reader, 17);
            x.productGroup.title_seo = G.GetSeoTitle(x.productGroup.title);
            x.productGroup.parent = PG.GetParentGroup(x.productGroup.code);
        //}
        List<Tran.NewTran> tran = T.LoadData(x.id, G.recordType.productTitle, lang);
        x.title = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : G.ReadS(reader, 3);
        x.title_seo = G.GetSeoTitle(x.title);
        if (loadAllData) {
            tran = T.LoadData(x.id, G.recordType.productShortDesc, lang);
            x.shortdesc = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : G.ReadS(reader, 4);
            tran = T.LoadData(x.id, G.recordType.productLongDesc, lang);
            x.longdesc = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : G.ReadS(reader, 5);
            x.brand = new Brands.NewBrands();
            x.brand.code = G.ReadS(reader, 6);
            x.brand.title = G.ReadS(reader, 18);
            x.brand.title_seo = G.GetSeoTitle(x.brand.title);
        }
        x.img = G.ReadS(reader, 7);
        x.price = new Price();
        x.price.net = G.ReadD(reader, 8);
        x.discount = G.ReadD(reader, 9);
        x.price.net_discount = x.price.net - (x.price.net * x.discount);
        x.price.gross = x.price.net_discount + (x.price.net_discount * 0.25);
        x.price.gross_discount = x.price.gross - (x.price.gross * x.discount);
        if (loadAllData) {
            x.stock = G.ReadI(reader, 10);
            x.isnew = G.ReadB(reader, 11);
            x.outlet = G.ReadB(reader, 12);
            x.bestselling = G.ReadB(reader, 13);
            x.isactive = G.ReadB(reader, 14);
            x.features = F.GetProductFeatures(features, G.ReadS(reader, 15));
            x.productorder = G.ReadI(reader, 16);
            x.gallery = GetGallery(x.id);
        }
        x.qty = 1;
        return x;
    }

    public void RemoveMainImg(string productId, string img) {
        string img_ = img.Contains('?') ? img.Split('?')[0] : img;
        string sql = string.Format("UPDATE products SET img = ''  WHERE id = '{0}' AND img = '{1}'", productId, img_);
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    string[] GetGallery(string id) {
        string[] xx = null;
        string path = Server.MapPath(string.Format("~/upload/{0}/gallery", id));
        if (Directory.Exists(path)) {
            string[] ss = Directory.GetFiles(path);
            xx = ss.Select(a => string.Format("{0}?v={1}", Path.GetFileName(a), DateTime.Now.Ticks)).ToArray();
        }
        return xx;
    }
    #endregion Class

}
