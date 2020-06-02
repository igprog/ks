﻿using System;
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
using System.Diagnostics;
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
    string mainSql = @"SELECT p.id, p.sku, p.style, p.productgroup, p.title, p.shortdesc, p.longdesc, p.brand, p.img, p.price, p.discount, p.stock, p.isnew, p.outlet, p.bestselling, p.isactive, p.features, p.deliverydays, p.productorder, pg.title, b.title, pg.discount
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
        public string style;
        public ProductGroups.NewProductGroup productGroup;
        public string title;
        public string title_seo;
        public string shortdesc;
        public string longdesc;
        public Brands.NewBrands brand;
        public string img;
        //public double discount;
        public Discount discount;
        public int stock;
        public bool isnew;
        public bool outlet;
        public bool bestselling;
        public bool isactive;
        public string[] gallery;
        public List<Features.NewFeature> features;
        public string deliverydays;
        public int productorder;
        public Price price;
        public int qty;
        public Review.ReviewData reviews;
        public Discount pg_discount;
    }

    public class Price {
        public double net;
        public double gross;
        public double discount;
        public double netWithDiscount;
        public double grossWithDiscount;
        //public double netWithDiscountTot;  // total with quantity
        //public double grossWithDiscountTot;  // total with quantity
        //public double net;
        //public double gross;
        //public double net_discount;
        //public double gross_discount;
        //public double net_discount_tot;  // total with quantity
        //public double gross_discount_tot;  // total with quantity
    }

    public class Discount {
        public double coeff;
        public double perc;
    }

    public class ProductsData {
        public List<NewProduct> data;
        //public PriceRange priceRange;
        public double responseTime;
        public Filters filters;
        //public SortTypes sortTypes;
    }

    public class PriceRange {
        public double min;
        public double max;
        public double minVal;
        public double maxVal;
    }

    public class Filters {
        public PriceRange price;
        public FilterItem isnew;
        public FilterItem outlet;
        public FilterItem bestselling;
        public SortBy sortBy;
        public Show show;
    }

    public class FilterItem {
        public string code;
        public string title;
        public bool val;
        public int tot;
    }

    public class SortBy {
        public SortTypes sortTypes;
        public string val;
    }

    public class SortTypes {
        public SortItem nameAZ;
        public SortItem nameZA;
        public SortItem priceLH;
        public SortItem priceHL;
        public SortItem ratingH;
        public SortItem ratingL;
    }

    public class SortItem {
        public string code;
        public string title;
        public bool val;
    }

    public class Show {
        public int[] values;
        public int val;
    }
    #endregion Class

    #region WebMethod
    [WebMethod]
    public string Init() {
        try {
            NewProduct x = new NewProduct();
            x.id = null;
            x.sku = null;
            x.style = null;
            x.productGroup = new ProductGroups.NewProductGroup();
            x.title = null;
            x.title_seo = null;
            x.shortdesc = null;
            x.longdesc = null;
            x.brand = new Brands.NewBrands();
            x.img = null;
            x.price = new Price();
            x.discount = new Discount();
            x.stock = 1000;
            x.isnew = false;
            x.outlet = false;
            x.bestselling = false;
            x.isactive = true;
            x.features = F.Get(G.featureType.product);
            x.deliverydays = null;
            x.productorder = 0;
            x.gallery = null;
            x.price = new Price();
            x.qty = 1;
            x.reviews = new Review.ReviewData();
            return JsonConvert.SerializeObject(x, Formatting.None);    
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load(string lang, string productGroup, string brand, string search) {
        try {
            return JsonConvert.SerializeObject(LoadData(lang, productGroup, brand, search), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Filter(string lang, string productGroup, string brand, string search, Filters filters) {
        try {
            return JsonConvert.SerializeObject(LoadData(lang, productGroup, brand, search, filters), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string LoadProductType(string lang, string productGroup, string type, int limit) {
        try {
            DB.CreateDataBase(G.db.products);
            string sql = string.Format(@"{0} WHERE {1} = 'True' {2} AND p.stock > 0 {3}"
                                    , mainSql
                                    , string.Format("p.{0}", type)
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
            return JsonConvert.SerializeObject("OK", Formatting.None);
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
            x.discount.coeff = x.discount.perc / 100;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO products VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}',  '{17}', {18})"
                                    , x.id, x.sku, x.style, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.net, x.discount.coeff, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.deliverydays, x.productorder);
            } else {
                sql = string.Format(@"UPDATE products SET sku = '{1}', style = '{2}', productgroup = '{3}', title = '{4}', shortdesc = '{5}', longdesc = '{6}', brand = '{7}', img = '{8}', price = '{9}', discount = '{10}', stock = '{11}', isnew = '{12}', outlet = '{13}', bestselling = '{14}', isactive = '{15}', features = '{16}', deliverydays = '{17}', productorder = {18} WHERE id = '{0}'"
                                    , x.id, x.sku, x.style, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.net, x.discount.coeff, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.deliverydays, x.productorder);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(x.id, Formatting.None);
            //return JsonConvert.SerializeObject(LoadData(null, null, null, null), Formatting.None);
            //return JsonConvert.SerializeObject("Spremljeno", Formatting.None);
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
            return JsonConvert.SerializeObject(LoadData(null, null, null, null), Formatting.None);
            //return JsonConvert.SerializeObject("Proizvod izbrisan", Formatting.None);
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
            //return JsonConvert.SerializeObject(LoadData(null, false, x.productGroup.code, x.brand.code, null), Formatting.None);
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion WebMethod

    #region Methods
    public ProductsData LoadData(string lang, string productGroup, string brand, string search) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        DB.CreateDataBase(G.db.products);
        string sql = string.Format(@"{0} {1} {2} {3} {4} {5}"
                   , mainSql
                   , string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) ? "" : "WHERE"
                   , string.IsNullOrEmpty(productGroup) ? "" : string.Format("(p.productGroup = '{0}' OR pg.parent = '{0}')", productGroup)
                   , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("p.brand = '{0}'", brand) : string.Format("AND p.brand = '{0}'", brand))
                   , string.IsNullOrEmpty(search) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) ? string.Format("p.title LIKE '%{0}%' OR p.shortdesc LIKE '%{0}%'", search) : string.Format("AND p.title LIKE '{0}%'", brand))
                   , "ORDER BY p.title DESC LIMIT 16");
        ProductsData xxx = new ProductsData();
        xxx.data = DataCollection(sql, lang, true);
        //xxx.priceRange = new PriceRange();
        //xxx.priceRange.min = xxx.data.Count > 0 ? xxx.data.Min(a => a.price.net_discount) : 0;
        //xxx.priceRange.max = xxx.data.Count > 0 ? xxx.data.Max(a => a.price.net_discount) : 0;
        xxx.responseTime = stopwatch.Elapsed.TotalSeconds;
        xxx.filters = LoadFilters(xxx); // InitFilters();
        //xxx.filters.isnew.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.isnew) : 0;
        //xxx.filters.outlet.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.outlet) : 0;
        //xxx.filters.bestselling.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.bestselling) : 0;
        //xxx.sortTypes = InitSortTypes();
        return xxx;
    }

    public ProductsData LoadData(string lang, string productGroup, string brand, string search, Filters filters) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        DB.CreateDataBase(G.db.products);
        //string where = string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && (filters.price.max == 0 && filters.price.max == 0) ? "" : "WHERE";
        string sql = string.Format(@"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}"
           , mainSql
           , string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && (filters.price.maxVal == 0 && filters.price.maxVal == 0 && !filters.isnew.val && !filters.outlet.val && !filters.bestselling.val) ? "" : "WHERE"
           , string.IsNullOrEmpty(productGroup) ? "" : string.Format("(p.productGroup = '{0}' OR pg.parent = '{0}')", productGroup)
           , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("p.brand = '{0}'", brand) : string.Format("AND p.brand = '{0}'", brand))
           , string.IsNullOrEmpty(search) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) ? string.Format("p.title LIKE '%{0}%' OR p.shortdesc LIKE '%{0}%'", search) : string.Format("AND p.title LIKE '{0}%'", brand))
           , filters.price.minVal <= 0 && filters.price.maxVal <= 0 ? "" : string.Format(@"{0} CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal)) >= {1} AND CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal)) <= {2}", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) ? "" : "AND"), filters.price.minVal, filters.price.maxVal)
           , filters.isnew.val == false ? "" : string.Format(@"{0} p.isnew = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 ? "" : "AND"))
           , filters.outlet.val == false ? "" : string.Format(@"{0} p.outlet = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false ? "" : string.Format("{0}", filters.isnew.val == false ? "AND" : "OR")))
           , filters.bestselling.val == false ? "" : string.Format(@"{0} p.bestselling = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false && filters.outlet.val == false ? "" : string.Format("{0}", filters.isnew.val == false || filters.outlet.val == false ? "AND" : "OR")))
           , string.Format("ORDER BY {0} LIMIT {1}", SortBySql(filters.sortBy.val), filters.show.val)
           );

        ProductsData xxx = new ProductsData();
        xxx.data = DataCollection(sql, lang, true);
        //xxx.priceRange = new PriceRange();
        //xxx.priceRange.min = xxx.data.Count > 0 ? xxx.data.Min(a => a.price.net_discount) : 0;
        //xxx.priceRange.max = xxx.data.Count > 0 ? xxx.data.Max(a => a.price.net_discount) : 0;
        xxx.responseTime = stopwatch.Elapsed.TotalSeconds;
        //xxx.filters = LoadFilters(xxx); // InitFilters();
                                        //xxx.filters.isnew.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.isnew) : 0;
                                        //xxx.filters.outlet.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.outlet) : 0;
                                        //xxx.filters.bestselling.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.bestselling) : 0;
        //xxx.sortTypes = InitSortTypes();
        return xxx;
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
        Review R = new Review();
        x.reviews = R.GetData(x.sku);
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
        x.style = G.ReadS(reader, 2);
        //if (loadAllData) {
        x.productGroup = new ProductGroups.NewProductGroup();
            x.productGroup.code = G.ReadS(reader, 3);
            x.productGroup.title = G.ReadS(reader, 19);
            x.productGroup.title_seo = G.GetSeoTitle(x.productGroup.title);
            x.productGroup.parent = PG.GetParentGroup(x.productGroup.code);
        //}
        List<Tran.NewTran> tran = T.LoadData(x.id, G.recordType.productTitle, lang);
        x.title = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : G.ReadS(reader, 4);
        x.title_seo = G.GetSeoTitle(x.title);
        if (loadAllData) {
            tran = T.LoadData(x.id, G.recordType.productShortDesc, lang);
            x.shortdesc = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : G.ReadS(reader, 5);
            tran = T.LoadData(x.id, G.recordType.productLongDesc, lang);
            x.longdesc = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : G.ReadS(reader, 6);
            x.brand = new Brands.NewBrands();
            x.brand.code = G.ReadS(reader, 7);
            x.brand.title = G.ReadS(reader, 20);
            x.brand.title_seo = G.GetSeoTitle(x.brand.title);
        }
        x.img = G.ReadS(reader, 8);
        x.price = new Price();
        x.price.net = G.ReadD(reader, 9);
        //double productDiscount = G.ReadD(reader, 10);  // Product Discount
        //double pgDisCount = G.ReadI(reader, 20);  // Product Group Discount
        x.discount = GetDiscount(G.ReadD(reader, 10), G.ReadD(reader, 21));
        //x.discount = new Discount();
        //x.discount.coeff = G.ReadD(reader, 10);
        //x.discount.perc = Math.Round(x.discount.coeff * 100, 1);
        //x.price.discount = x.price.net * x.discount.coeff;
        //x.price.netWithDiscount = x.price.net - x.price.discount;
        x.price.gross = x.price.net * 1.25;
        x.price.discount = x.price.gross * x.discount.coeff;  // TODO: Dali se popust racuna na neto ili na bruto ???
        x.price.grossWithDiscount = x.price.gross - x.price.discount;
        //if (loadAllData) {
        x.stock = G.ReadI(reader, 11);
            x.isnew = G.ReadB(reader, 12);
            x.outlet = G.ReadB(reader, 13);
            x.bestselling = G.ReadB(reader, 14);
            x.isactive = G.ReadB(reader, 15);
            x.features = F.GetProductFeatures(features, G.ReadS(reader, 16));
            x.deliverydays = G.ReadS(reader, 17);
            x.productorder = G.ReadI(reader, 18);
            //x.pg_discount = new Discount();
            //x.pg_discount.coeff = G.ReadI(reader, 20);
            //x.discount.perc = Math.Round(x.discount.coeff * 100, 1);
            x.gallery = GetGallery(x.id);
        //}
        x.qty = 1;
        return x;
    }

    private Discount GetDiscount(double productDiscount, double pgDiscount) {
        Discount x = new Discount();
        double discount = productDiscount > 0 ? productDiscount : pgDiscount;
        x.coeff = discount;
        x.perc = Math.Round(x.coeff * 100, 1);
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

    public Filters LoadFilters(ProductsData xxx) {
        Filters x = new Filters();
        x.price = new PriceRange();
        x.price.min = xxx.data.Count > 0 ? xxx.data.Min(a => a.price.grossWithDiscount) : 0;
        x.price.max = xxx.data.Count > 0 ? xxx.data.Max(a => a.price.grossWithDiscount) : 0;
        x.isnew = new FilterItem();
        x.isnew.code = "isnew";
        x.isnew.title = "new";
        x.isnew.val = false;
        x.isnew.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.isnew) : 0;
        x.outlet = new FilterItem();
        x.outlet.code = "outlet";
        x.outlet.title = "outlet";
        x.outlet.val = false;
        x.outlet.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.outlet) : 0;
        x.bestselling = new FilterItem();
        x.bestselling.code = "bestselling";
        x.bestselling.title = "bestselling";
        x.bestselling.val = false;
        x.bestselling.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.bestselling) : 0;
        x.sortBy = InitSortBy();
        x.show = new Show();
        x.show.values = new int[] { 16, 32, 64 };
        x.show.val = 16;
        return x;
    }

    public SortBy InitSortBy() {
        SortBy s = new SortBy();
        SortTypes x = new SortTypes();
        x.nameAZ = new SortItem();
        x.nameAZ.code = "nameAZ";
        x.nameAZ.title = "name (A - Z)";
        x.nameAZ.val = false;
        x.nameZA = new SortItem();
        x.nameZA.code = "nameZA";
        x.nameZA.title = "name (Z - A)";
        x.nameZA.val = false;
        x.priceLH = new SortItem();
        x.priceLH.code = "priceLH";
        x.priceLH.title = "price (Low - Heigh)";
        x.priceLH.val = false;
        x.priceHL = new SortItem();
        x.priceHL.code = "priceHL";
        x.priceHL.title = "price (Heigh - Low)";
        x.priceHL.val = false;
        x.ratingH = new SortItem();
        x.ratingH.code = "ratingH";
        x.ratingH.title = "rating (Highest)";
        x.ratingH.val = false;
        x.ratingL = new SortItem();
        x.ratingL.code = "ratingL";
        x.ratingL.title = "rating (Lowest)";
        x.ratingL.val = false;
        s.sortTypes = x;
        s.val = "nameAZ";
        return s;
    }

    public string SortBySql (string code) {
        //TODO: rating
        string x = null;
        switch (code) {
            case "nameAZ":
                x = "p.title ASC";
                break;
            case "nameZA":
                x = "p.title DESC";
                break;
            case "priceLH":
                x = "(CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal))) ASC";
                break;
            case "priceHL":
                x = "(CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal))) DESC";
                break;
            case "ratingH":
                x = "p.title ASC";
                break;
            case "ratingL":
                x = "p.title DESC";
                break;
            default:
                x = "p.title ASC";
                break;
        }
        return x;
    }
    #endregion Methods

}
