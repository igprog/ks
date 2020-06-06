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
    string mainSql = @"SELECT p.id, p.sku, p.style, p.productgroup, p.title, p.shortdesc, p.longdesc, p.brand, p.img, p.price, p.discount, p.stock, p.isnew, p.outlet, p.bestselling, p.isactive, p.features, p.deliverydays, p.productorder,
                            p.freeshipping, p.bestbuy, p.wifi, p.relatedproducts, p.width, p.height, p.depth, p.power, p.color, p.energyclass, p.datasheet,
                            pg.title, b.title, pg.discount
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

        public bool freeshipping;
        public bool bestbuy;
        public bool wifi;
        public string relatedProductsStr;
        public List<NewProduct> relatedProducts;
        public double width;
        public double height;
        public double depth;
        public double power;
        public string color; 
        public string energyClass;
        public string dataSheet;

        public Price price;
        public int qty;
        public Review.ReviewData reviews;
        public Discount pg_discount;
        public List<NewProduct> styleProducts;
    }

    public class Price {
        public double net;
        public double gross;
        public double discount;
        public double netWithDiscount;
        public double grossWithDiscount;
    }

    public class Discount {
        public double coeff;
        public double perc;
        public string from;
        public string to;
    }

    public class ProductsData {
        public List<NewProduct> data;
        public double responseTime;
        public Filters filters;
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
        public FilterItem freeshipping;
        public FilterItem bestbuy;
        public FilterItem wifi;
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
            x.discount.coeff = 0;
            x.discount.perc = 0;
            x.discount.from = null;
            x.discount.to = null;
            x.stock = 1000;
            x.isnew = false;
            x.outlet = false;
            x.bestselling = false;
            x.isactive = true;
            x.features = F.Get(G.featureType.product);
            x.deliverydays = null;
            x.productorder = 0;
            x.freeshipping = true;
            x.bestbuy = false;
            x.wifi = false;
            x.relatedProducts = new List<NewProduct>();
            x.width = 0;
            x.height = 0;
            x.depth = 0;
            x.power = 0;
            x.color = null;
            x.energyClass = null;
            x.dataSheet = null;
            x.gallery = null;
            x.price = new Price();
            x.qty = 1;
            x.reviews = new Review.ReviewData();
            x.pg_discount = new Discount();
            x.styleProducts = new List<NewProduct>();
            return JsonConvert.SerializeObject(x, Formatting.None);    
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load(string lang, string productGroup, string brand, string search, string type) {
        try {
            return JsonConvert.SerializeObject(LoadData(lang, productGroup, brand, search, type), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Filter(string lang, string productGroup, string brand, string search, string type, Filters filters) {
        try {
            return JsonConvert.SerializeObject(LoadData(lang, productGroup, brand, search, type, filters), Formatting.None);
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
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string sku, string lang) {
        try {
            NewProduct x = new NewProduct();
            if (!string.IsNullOrEmpty(sku)) {
                x = GetProduct(sku, lang);
                x.relatedProducts = GetRelatedProducts(x.relatedProductsStr, lang);
                x.styleProducts = GetStyleProducts(x.style, lang);
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
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
            string relatedProducts = null;
            if (x.relatedProducts.Count > 0) {
                var rp_ = new List<string>();
                foreach (var rp in x.relatedProducts) {
                    rp_.Add(string.Format("{0}", rp.sku));
                }
                relatedProducts = string.Join(";", rp_);
            }
            x.discount.coeff = x.discount.perc / 100;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO products VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}',  '{17}', {18}, '{19}', '{20}', '{21}', '{22}', '{23}', '{24}', '{25}', '{26}', '{27}', '{28}', '{29}')"
                                    , x.id, x.sku, x.style, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.gross, x.discount.coeff, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.deliverydays, x.productorder, x.freeshipping, x.bestbuy, x.wifi, relatedProducts, x.width, x.height, x.depth, x.power, x.color, x.energyClass, x.dataSheet);
            } else {
                sql = string.Format(@"UPDATE products SET sku = '{1}', style = '{2}', productgroup = '{3}', title = '{4}', shortdesc = '{5}', longdesc = '{6}', brand = '{7}', img = '{8}', price = '{9}', discount = '{10}', stock = '{11}', isnew = '{12}', outlet = '{13}', bestselling = '{14}', isactive = '{15}', features = '{16}', deliverydays = '{17}', productorder = {18},
                                    freeshipping = '{19}', bestbuy = '{20}', wifi = '{21}', relatedproducts = '{22}', width = '{23}', height = '{24}', depth = '{25}', power = '{26}', color = '{27}', energyclass = '{28}', datasheet = '{29}' WHERE id = '{0}'"
                                    , x.id, x.sku, x.style, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.gross, x.discount.coeff, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.deliverydays, x.productorder, x.freeshipping, x.bestbuy, x.wifi, relatedProducts, x.width, x.height, x.depth, x.power, x.color, x.energyClass, x.dataSheet);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(x.id, Formatting.None);
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
            return JsonConvert.SerializeObject(LoadData(null, null, null, null, null), Formatting.None);
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
    public ProductsData LoadData(string lang, string productGroup, string brand, string search, string type) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        DB.CreateDataBase(G.db.products);
        string sql = string.Format(@"{0} {1} {2} {3} {4} {5} {6}"
                   , mainSql
                   , string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) ? "" : "WHERE"
                   , string.IsNullOrEmpty(productGroup) ? "" : string.Format("(p.productGroup = '{0}' OR pg.parent = '{0}')", productGroup)
                   , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("p.brand = '{0}'", brand) : string.Format("AND p.brand = '{0}'", brand))
                   , string.IsNullOrEmpty(search) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) ? string.Format("p.title LIKE '%{0}%' OR p.shortdesc LIKE '%{0}%'", search) : string.Format("AND p.title LIKE '{0}%'", brand))
                   , string.IsNullOrEmpty(type) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) ? string.Format("p.{0}='True'", type) : string.Format("AND p.{0}='True''", type))
                   , "ORDER BY p.productorder DESC LIMIT 16");
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

    public ProductsData LoadData(string lang, string productGroup, string brand, string search, string type, Filters filters) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        DB.CreateDataBase(G.db.products);
        string sql = string.Format(@"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}"
           , mainSql
           , string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && (filters.price.maxVal == 0 && filters.price.maxVal == 0 && !filters.isnew.val && !filters.outlet.val && !filters.bestselling.val) ? "" : "WHERE"
           , string.IsNullOrEmpty(productGroup) ? "" : string.Format("(p.productGroup = '{0}' OR pg.parent = '{0}')", productGroup)
           , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("p.brand = '{0}'", brand) : string.Format("AND p.brand = '{0}'", brand))
           , string.IsNullOrEmpty(search) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) ? string.Format("p.title LIKE '%{0}%' OR p.shortdesc LIKE '%{0}%'", search) : string.Format("AND p.title LIKE '{0}%'", brand))
           , string.IsNullOrEmpty(type) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) ? string.Format("p.{0}='True'", type) : string.Format("AND p.{0}='True''", type))
           , filters.price.minVal <= 0 && filters.price.maxVal <= 0 ? ""
                    : string.Format(@"{0} 
                                CASE WHEN p.discount = 0 AND pg.discount > 0 THEN
                                    CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(pg.discount as decimal)) >= {1} AND CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(pg.discount as decimal)) <= {2}
                                ELSE
                                    CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal)) >= {1} AND CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal)) <= {2}
                                END"
           , (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) ? "" : "AND"), filters.price.minVal, filters.price.maxVal)
           , filters.isnew.val == false ? "" : string.Format(@"{0} p.isnew = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 ? "" : "AND"))
           , filters.outlet.val == false ? "" : string.Format(@"{0} p.outlet = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false ? "" : string.Format("{0}", filters.isnew.val == false ? "AND" : "OR")))
           , filters.bestselling.val == false ? "" : string.Format(@"{0} p.bestselling = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false && filters.outlet.val == false ? "" : string.Format("{0}", filters.isnew.val == false || filters.outlet.val == false ? "AND" : "OR")))
           , string.Format("ORDER BY {0} LIMIT {1}", SortBySql(filters.sortBy.val), filters.show.val)
           );

        //string sql = string.Format(@"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}"
        //   , mainSql
        //   , string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && (filters.price.maxVal == 0 && filters.price.maxVal == 0 && !filters.isnew.val && !filters.outlet.val && !filters.bestselling.val) ? "" : "WHERE"
        //   , string.IsNullOrEmpty(productGroup) ? "" : string.Format("(p.productGroup = '{0}' OR pg.parent = '{0}')", productGroup)
        //   , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("p.brand = '{0}'", brand) : string.Format("AND p.brand = '{0}'", brand))
        //   , string.IsNullOrEmpty(search) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) ? string.Format("p.title LIKE '%{0}%' OR p.shortdesc LIKE '%{0}%'", search) : string.Format("AND p.title LIKE '{0}%'", brand))
        //   , string.IsNullOrEmpty(type) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) ? string.Format("p.{0}='True'", type) : string.Format("AND p.{0}='True''", type))
        //   , filters.price.minVal <= 0 && filters.price.maxVal <= 0 ? "" 
        //            : string.Format(@"{0} 
        //                        CASE WHEN p.discount = 0 AND pg.discount > 0 THEN
        //                            CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(pg.discount as decimal)) >= {1} AND CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(pg.discount as decimal)) <= {2}
        //                        ELSE
        //                            CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal)) >= {1} AND CAST(p.price as decimal) - (CAST(p.price as decimal) * CAST(p.discount as decimal)) <= {2}
        //                        END"
        //   , (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) ? "" : "AND"), filters.price.minVal, filters.price.maxVal)
        //   , filters.isnew.val == false ? "" : string.Format(@"{0} p.isnew = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 ? "" : "AND"))
        //   , filters.outlet.val == false ? "" : string.Format(@"{0} p.outlet = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false ? "" : string.Format("{0}", filters.isnew.val == false ? "AND" : "OR")))
        //   , filters.bestselling.val == false ? "" : string.Format(@"{0} p.bestselling = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false && filters.outlet.val == false ? "" : string.Format("{0}", filters.isnew.val == false || filters.outlet.val == false ? "AND" : "OR")))
        //   , string.Format("ORDER BY {0} LIMIT {1}", SortBySql(filters.sortBy.val), filters.show.val)
        //   );

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

    public NewProduct GetProduct(string sku, string lang) {
        NewProduct x = new NewProduct();
        List<Features.NewFeature> features = F.Get(G.featureType.product);
        string sql = string.Format(@"{0} WHERE p.sku = '{1}'"
                                    , mainSql
                                    , sku);
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
        x.productGroup.parent = PG.GetParentGroupData(x.productGroup.parent.code);
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
            x.productGroup.title = G.ReadS(reader, 30);
            x.productGroup.title_seo = G.GetSeoTitle(x.productGroup.title);
            x.productGroup.parent = new ProductGroups.NewProductGroup();
            x.productGroup.parent.code = PG.GetParentGroup(x.productGroup.code);
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
            x.brand.title = G.ReadS(reader, 31);
            x.brand.title_seo = G.GetSeoTitle(x.brand.title);
        }
        x.img = G.ReadS(reader, 8);
        x.price = new Price();
        x.price.gross = G.ReadD(reader, 9);
        x.price.net = x.price.gross - (x.price.gross / 1.25);
        //double productDiscount = G.ReadD(reader, 10);  // Product Discount
        //double pgDisCount = G.ReadI(reader, 20);  // Product Group Discount
        x.discount = GetDiscount(G.ReadD(reader, 10), G.ReadD(reader, 32));
        //x.discount = new Discount();
        //x.discount.coeff = G.ReadD(reader, 10);
        //x.discount.perc = Math.Round(x.discount.coeff * 100, 1);
        //x.price.discount = x.price.net * x.discount.coeff;
        //x.price.netWithDiscount = x.price.net - x.price.discount;
        
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

        x.freeshipping = G.ReadB(reader, 19);
        x.bestbuy = G.ReadB(reader, 20);
        x.wifi = G.ReadB(reader, 21);
        x.relatedProductsStr = G.ReadS(reader, 22);
        x.relatedProducts = new List<NewProduct>();
        x.width = G.ReadD(reader, 23);
        x.height = G.ReadD(reader, 24);
        x.depth = G.ReadD(reader, 25);
        x.power = G.ReadD(reader, 26);
        x.color = G.ReadS(reader, 27);
        x.energyClass = G.ReadS(reader, 28);
        x.dataSheet = G.ReadS(reader, 29);

        //x.pg_discount = new Discount();
        //x.pg_discount.coeff = G.ReadI(reader, 20);
        //x.discount.perc = Math.Round(x.discount.coeff * 100, 1);
        x.gallery = GetGallery(x.id);
        //}
        x.qty = 1;
        return x;
    }

    private List<NewProduct> GetRelatedProducts(string relatedProducts, string lang) {
        List<NewProduct> xx = new List<NewProduct>();
        NewProduct x = new NewProduct();
        if (!string.IsNullOrEmpty(relatedProducts)) {
            string[] list = relatedProducts.Split(';');
            foreach (var sku in list) {
                x = GetProduct(sku, lang);
                xx.Add(x);
            }
        }
        return xx;
    }

    private List<NewProduct> GetStyleProducts(string style, string lang) {
        List<NewProduct> xx = new List<NewProduct>();
        NewProduct x = new NewProduct();
        if (!string.IsNullOrEmpty(style)) {
            string sql = string.Format("{0} WHERE p.style = '{1}'", mainSql, style);
            xx = DataCollection(sql, lang, true);
        }
        return xx;
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

        x.freeshipping = new FilterItem();
        x.freeshipping.code = "freeshipping";
        x.freeshipping.title = "freeshipping";
        x.freeshipping.val = false;
        x.freeshipping.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.freeshipping) : 0;
        x.bestbuy = new FilterItem();
        x.bestbuy.code = "bestbuy";
        x.bestbuy.title = "bestbuy";
        x.bestbuy.val = false;
        x.bestbuy.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.bestbuy) : 0;
        x.wifi = new FilterItem();
        x.wifi.code = "wifi";
        x.wifi.title = "wifi";
        x.wifi.val = false;
        x.wifi.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.wifi) : 0;
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
