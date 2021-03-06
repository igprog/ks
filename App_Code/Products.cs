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
using System.Text;
using System.Text.RegularExpressions;
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
    Colors C = new Colors();
    int defaultLimit = Convert.ToInt32(ConfigurationManager.AppSettings["defaultLimit"]);
    string mainSql = @"SELECT p.id, p.sku, p.style, p.productgroup, p.title, p.shortdesc, p.longdesc, p.brand, p.img, p.price, p.discount, p.discountfrom, p.discountto, p.stock, p.isnew, p.outlet, p.bestselling, p.isactive, p.features, p.deliverydays, p.productorder,
                            p.freeshipping, p.bestbuy, p.wifi, p.relatedproducts, p.width, p.height, p.depth, p.power, p.color, p.energyclass, p.datasheet, p.opportunity, p.keyfeatures, p.fireboxinsert,
                            pg.title, b.title, pg.discount, pg.discountfrom, pg.discountto
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
        public Dimension dimension;
        public string power;
        public Colors.NewColor color; 
        public string energyClass;
        public List<string> dataSheet;
        public bool opportunity;
        public List<Global.CodeTitle> keyFeatures;
        public string fireboxInsert;
        public Price price;
        public int qty;
        public Review.ReviewData reviews;
        public Discount pg_discount;
        public List<NewProduct> styleProducts;
        public List<Dimension> distinctDimensions;
        public List<Colors.NewColor> distinctColors;
        public List<Global.CodeTitle> distinctFireboxInserts;
        public List<Colors.NewColor> colors;
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
        public bool isValid;
    }

    public class ProductsData {
        public List<NewProduct> data;
        public Filters filters;
        public int totRecords;
        public ProductGroups.NewProductGroup parentProductGroup;
        public List<double> responseTime;
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
        public int page;
        public ColorFilter color;
        public FilterItem opportunity;
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

    public class ColorFilter {
        public List<Colors.NewColor> data;
        public Colors.NewColor val;
    }

    public class Dimension {
        public double width;
        public double height;
        public double depth;
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
            x.features = new List<Features.NewFeature>(); // F.Get(G.featureType.product);
            x.deliverydays = null;
            x.productorder = 0;
            x.freeshipping = true;
            x.bestbuy = false;
            x.wifi = false;
            x.relatedProducts = new List<NewProduct>();
            x.dimension = new Dimension();
            //x.width = 0;
            //x.height = 0;
            //x.depth = 0;
            x.power = null;
            x.color = new Colors.NewColor();
            x.color.code = null;
            x.color.title = null;
            x.color.hex = null;
            x.color.img = null;
            x.energyClass = null;
            x.dataSheet = new List<string>();
            x.opportunity = false;
            x.keyFeatures = new List<Global.CodeTitle>();
            x.fireboxInsert = null;
            x.gallery = null;
            x.price = new Price();
            x.qty = 1;
            x.reviews = new Review.ReviewData();
            x.pg_discount = new Discount();
            x.styleProducts = new List<NewProduct>();
            x.distinctDimensions = new List<Dimension>();
            x.distinctColors = new List<Colors.NewColor>();
            x.colors = C.LoadData();
            return JsonConvert.SerializeObject(x, Formatting.None);    
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load(string lang, string productGroup, string brand, string search, string type, bool isDistinctStyle, int? limit) {
        try {
            return JsonConvert.SerializeObject(LoadData(lang, productGroup, brand, search, type, isDistinctStyle, limit), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Filter(string lang, string productGroup, string brand, string search, string type, Filters filters) {
        try {
            return JsonConvert.SerializeObject(LoadData(lang, productGroup, brand, search, type, filters, false), Formatting.None);
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
                //x.styleProducts = GetStyleProducts(x.style, lang);
                x.distinctDimensions = GetDistinctDimensions(x.style);
                x.distinctColors = GetDistinctColors(x.style, x.dimension);
                x.distinctFireboxInserts = GetDistinctFireboxInserts(x.style);
                x.colors = C.LoadData();
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetVarDimProduct(string style, Dimension dimension, string lang) {
        try {
            string sql = string.Format(@"{0} WHERE p.style = '{1}' AND p.width = '{2}' AND p.height = '{3}' AND p.depth = '{4}'"
                                        , mainSql , style , dimension.width, dimension.height, dimension.depth);
            return JsonConvert.SerializeObject(GetProductData(sql, lang), Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetVarColorProduct(string style, string color, Dimension dimension, string lang) {
        try {
            string sql = string.Format(@"{0} WHERE p.style = '{1}' AND p.color = '{2}' AND p.width = '{3}' AND p.height = '{4}' AND p.depth = '{5}'"
                                        , mainSql , style , color, dimension.width, dimension.height, dimension.depth);
            return JsonConvert.SerializeObject(GetProductData(sql, lang), Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetVarFireboxInsertProduct(string style, string fireboxInsert, string lang) {
        try {
            string sql = string.Format(@"{0} WHERE p.style = '{1}' AND p.fireboxinsert = '{2}'", mainSql , style , fireboxInsert);
            return JsonConvert.SerializeObject(GetProductData(sql, lang), Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(NewProduct x) {
        try {
            x = SaveData(x);
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
            return JsonConvert.SerializeObject(LoadData(null, null, null, null, null, true, 100), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string DeleteImg(NewProduct x, string img) {
        try {
            string img_ = img.Contains('?') ? img.Split('?')[0] : img;
            string path = Server.MapPath(string.Format("~/upload/{0}/gallery", x.id));
            DeleteImgFile(x.id, img_, path);

            /***** delete thumb *****/
            path = Server.MapPath(string.Format("~/upload/{0}/gallery/thumb", x.id));
            DeleteImgFile(x.id, img_, path);
            /***** delete thumb *****/

            /***** delete from DB *****/
            //TODO

            string sql = string.Format("DELETE FROM images WHERE fileName = '{0}' AND productId = '{1}'", img_, x.id);
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
            /***** delete from DB *****/

            //return JsonConvert.SerializeObject(LoadData(null, false, x.productGroup.code, x.brand.code, null), Formatting.None);
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    private void DeleteImgFile(string id, string img_, string path) {
        if (Directory.Exists(path)) {
            string[] gallery = Directory.GetFiles(path);
            foreach (string file in gallery) {
                if (Path.GetFileName(file) == img_) {
                    File.Delete(file);
                    RemoveMainImg(id, img_);
                }
            }
        }
    }

    [WebMethod]
    public string DeleteDataSheet(NewProduct x, string file) {
        try {
            string path = Server.MapPath(string.Format("~/upload/{0}/datasheet", x.id));
            if (Directory.Exists(path)) {
                string[] files = Directory.GetFiles(path);
                foreach (string f in files) {
                    if (Path.GetFileName(file) == file) {
                        File.Delete(file);
                        SaveData(x);
                    }
                }
            }
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string ImportProductsCsv() {
        int row = 0;
        int count = 0;
        string sql = null;
        string priceFix = null;
        try {
            DB.CreateDataBase(G.db.products);
            string path = Server.MapPath("~/upload/csv/products.csv");
            List<NewProduct> xx = new List<NewProduct>();
            using (var reader = new StreamReader(path, Encoding.Default)) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    if (row > 1) {
                        //if (row == 2)
                        //{
                        //    string test = null;
                        //}
                        var val = line.Split(';');
                        if (!string.IsNullOrEmpty(val[0])) {
                            NewProduct x = new NewProduct();
                            x.id = Guid.NewGuid().ToString();
                            x.sku = val[0];
                            x.style = string.IsNullOrEmpty(val[1]) ? x.sku : val[1];
                            x.productGroup = new ProductGroups.NewProductGroup();
                            x.productGroup.code = GetProductGroupCode(val[3], val[2]);
                            x.title = val[4].Replace("'", "");
                            x.shortdesc =  ToHtml(val[5]);
                            x.longdesc = ToHtml(val[6]);
                            x.brand = new Brands.NewBrands();
                            x.brand.code = GetBrandCode(val[7]);
                            x.price = new Price();

                            /**** Fix price format: 1.125.15 kn****/
                            priceFix = val[8].Replace("kn", "").Trim();
                            if (priceFix.Count(a => a == '.') > 1) {
                                var regex = new Regex(Regex.Escape("."));
                                priceFix = regex.Replace(priceFix, "", 1);
                            }

                            x.price.gross = string.IsNullOrEmpty(priceFix) ? 0 : Convert.ToDouble(priceFix);
                            x.discount = new Discount();
                            x.discount.coeff = GetVal(val[9]);
                            x.stock = string.IsNullOrEmpty(val[10]) ? 1000 : Convert.ToInt32(GetVal(val[10]));
                            x.isnew = IsTrue(val[11]);
                            x.outlet = IsTrue(val[12]);
                            x.bestselling = IsTrue(val[13]);
                            x.deliverydays = Regex.Replace(val[14], "[^0-9.-]", "");
                            x.productorder = Convert.ToInt32(GetVal(val[15]));
                            x.freeshipping = IsTrue(val[16]);
                            x.bestbuy = IsTrue(val[17]);
                            x.wifi = IsTrue(val[18]);
                            x.dimension = new Dimension();
                            x.dimension.width = GetVal(val[19]);
                            x.dimension.height = GetVal(val[20]);
                            x.dimension.depth = GetVal(val[21]);
                            x.power = val[22];
                            x.color = new Colors.NewColor();
                            x.color.code = ColorName(val[23]);
                            xx.Add(x);
                        }
                    }
                    row++;
                }
            }
            if (1 == 1) {
                using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                    connection.Open();
                    using (var command = new SQLiteCommand()) {
                        command.Connection = connection;
                        using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                            foreach (NewProduct x in xx) {
                                sql = string.Format(@"INSERT OR REPLACE INTO PRODUCTS (id, sku, style, productgroup, title, shortdesc, longdesc, brand, img, price, discount, discountfrom, discountto, stock, isnew, outlet, bestselling, isactive, features, deliverydays, productorder,
                                freeshipping, bestbuy, wifi, relatedproducts, width, height, depth, power, color, energyclass, datasheet, opportunity, keyfeatures, fireboxinsert)
                                                     VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', {20}, '{21}', '{22}', '{23}', '{24}', '{25}', '{26}', '{27}', '{28}', '{29}', '{30}', '{31}', '{32}', '{33}', '{34}')"
                                                    , x.id, x.sku, x.style, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.gross, x.discount.coeff, x.discount.from, x.discount.to, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, null, x.deliverydays, x.productorder, x.freeshipping, x.bestbuy, x.wifi, null, x.dimension.width, x.dimension.height, x.dimension.depth, x.power, x.color.code, x.energyClass, null, x.opportunity, null, x.fireboxInsert);
                                    command.CommandText = sql;
                                    command.Transaction = transaction;
                                    command.ExecuteNonQuery();
                                count++;
                            }

                            transaction.Commit();
                        }
                    }
                    connection.Close();
                }
            }
            

            return JsonConvert.SerializeObject("Spremljeno", Formatting.Indented);
        } catch (Exception e) {
            var test = row;
            var test1 = count;
            var test2 = sql;
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }
    #endregion WebMethod

    #region Methods
    public ProductsData LoadData(string lang, string productGroup, string brand, string search, string type, bool isDistinctStyle, int? limit) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        DB.CreateDataBase(G.db.products);
        string[] productGroup_ = null;
        if (!string.IsNullOrEmpty(productGroup)) {
            productGroup_ = productGroup.Split(';');
        }

        string searchSql = string.Format(@"{0} {1} {2} {3} {4}"
                   , string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) ? "" : "WHERE"
                   , string.IsNullOrEmpty(productGroup) ? "" :
                            productGroup.Split(';').Length == 2 ? string.Format("((LOWER(p.productGroup) = '{0}' OR LOWER(pg.parent) = '{0}') AND (LOWER(p.productGroup) = '{1}' OR LOWER(pg.parent) = '{1}'))", productGroup_[0].ToLower(), productGroup_[1].ToLower()) : 
                            string.Format("(LOWER(p.productGroup) LIKE '%{0}%' OR LOWER(pg.parent) LIKE '%{0}%')", productGroup.ToLower())
                   , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("LOWER(p.brand) = '{0}'", brand.ToLower()) : string.Format("AND p.brand = '{0}'", brand))
                   , string.IsNullOrEmpty(search) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) ? string.Format("p.title LIKE '%{0}%' OR p.shortdesc LIKE '%{0}%' OR p.sku LIKE '{0}%' OR p.style LIKE '{0}%'", search) : string.Format("AND p.title LIKE '{0}%'", brand))
                   , string.IsNullOrEmpty(type) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) ? string.Format("p.{0}='True'", type) : string.Format("AND p.{0}='True''", type)));

        string sql = string.Format(@"{0} {1} ORDER BY p.productorder DESC", mainSql, searchSql);

        ProductsData xxx = new ProductsData();
        xxx.data = DataCollection(sql, lang, false);
        xxx.responseTime = new List<double>();
        xxx.responseTime.Add(Math.Round(stopwatch.Elapsed.TotalSeconds, 5));
        xxx.filters = LoadFilters(xxx);
        xxx.responseTime.Add(Math.Round(stopwatch.Elapsed.TotalSeconds, 5));
        if (isDistinctStyle) {
            List<NewProduct> distinstStyle = (from x in xxx.data
                                              select x).GroupBy(n => new { n.style })
                                       .Select(g => g.FirstOrDefault()).Take(Convert.ToInt32(limit) > 0 ? Convert.ToInt32(limit) : defaultLimit)
                                       .ToList();
            xxx.data = distinstStyle;
        } else if (Convert.ToInt32(limit) > 0) {
            xxx.data = xxx.data.Take(Convert.ToInt32(limit)).ToList();
        }

        xxx.totRecords = GetTotRecords(searchSql);
        xxx.responseTime.Add(Math.Round(stopwatch.Elapsed.TotalSeconds, 5));
        xxx.parentProductGroup = PG.GetParentGroupData(productGroup);
        xxx.responseTime.Add(Math.Round(stopwatch.Elapsed.TotalSeconds, 5));
        return xxx;
    }

    public ProductsData LoadData(string lang, string productGroup, string brand, string search, string type, Filters filters, bool isDistinctStyle) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        //TODO: refaktorirati (AND / OR)
        DB.CreateDataBase(G.db.products);

        string searchSql = string.Format(@"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}"
           , string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && (filters.price.maxVal == 0 && filters.price.maxVal == 0 && !filters.isnew.val && !filters.outlet.val && !filters.bestselling.val && !filters.bestbuy.val && !filters.wifi.val) ? "" : "WHERE"
           , string.IsNullOrEmpty(productGroup) ? "" : string.Format("(LOWER(p.productGroup) LIKE '%{0}%' OR LOWER(pg.parent) LIKE '%{0}%')", productGroup.ToLower())
           , string.IsNullOrEmpty(brand) ? "" : (string.IsNullOrEmpty(productGroup) ? string.Format("LOWER(p.brand) = '{0}'", brand.ToLower()) : string.Format("AND p.brand = '{0}'", brand))
           , string.IsNullOrEmpty(search) ? "" : (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) ? string.Format("(p.title LIKE '%{0}%' OR p.shortdesc LIKE '%{0}%' OR p.sku LIKE '{0}%' OR p.style LIKE '{0}%')", search) : string.Format("AND p.title LIKE '{0}%'", brand))
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
           , filters.bestbuy.val == false ? "" : string.Format(@"{0} p.bestbuy = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false && filters.outlet.val == false && filters.bestselling.val == false ? "" : string.Format("{0}", filters.isnew.val == false || filters.outlet.val == false || filters.bestselling.val == false ? "AND" : "OR")))
           , filters.wifi.val == false ? "" : string.Format(@"{0} p.wifi = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false && filters.outlet.val == false && filters.bestselling.val == false && filters.bestbuy.val == false ? "" : string.Format("{0}", filters.isnew.val == false || filters.outlet.val == false || filters.bestselling.val == false || filters.bestbuy.val == false ? "AND" : "OR")))
           , filters.opportunity.val == false ? "" : string.Format(@"{0} p.opportunity = 'True'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false && filters.outlet.val == false && filters.bestselling.val == false && filters.bestbuy.val == false && filters.wifi.val == false ? "" : string.Format("{0}", filters.isnew.val == false || filters.outlet.val == false || filters.bestselling.val == false || filters.bestbuy.val == false || filters.wifi.val == false ? "AND" : "OR")))
           , string.IsNullOrEmpty(filters.color.val.code)
                                           ? ""
                                           : string.Format(@"{0} p.color = '{1}'", (string.IsNullOrEmpty(productGroup) && string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type) && filters.price.minVal <= 0 && filters.price.maxVal <= 0 && filters.isnew.val == false && filters.outlet.val == false && filters.bestselling.val == false && filters.bestbuy.val == false && filters.wifi.val == false
                                                                ? ""
                                                                : string.Format("{0}", filters.isnew.val == false || filters.outlet.val == false ? "AND" : "OR")), filters.color.val.code)
                                        );
        string sql = string.Format(@"{0} {1} {2} LIMIT {3}  OFFSET {4}"
                   , mainSql
                   , searchSql
                   , string.Format("ORDER BY {0}", SortBySql(filters.sortBy.val))
                   , filters.show.val
                   , (filters.page - 1) * filters.show.val);

        ProductsData xxx = new ProductsData();
        xxx.data = DataCollection(sql, lang, false);
        if (isDistinctStyle) {
            List<NewProduct> distinstStyle = (from x in xxx.data
                                              select x).GroupBy(n => new { n.style })
                                       .Select(g => g.FirstOrDefault())
                                       .ToList();
            xxx.data = distinstStyle;
        }
        xxx.totRecords = GetTotRecords(searchSql);
        xxx.responseTime = new List<double>();
        xxx.responseTime.Add(Math.Round(stopwatch.Elapsed.TotalSeconds, 5));
        return xxx;
    }

    
    public NewProduct SaveData(NewProduct x) {
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
        string keyFeatures = null;
        if (x.keyFeatures.Count > 0) {
            var kf_ = new List<string>();
            foreach (var kf in x.keyFeatures) {
                kf_.Add(string.Format("{0}", kf.title));
            }
            keyFeatures = string.Join(";", kf_);
        }
        string dataSheet = null;
        if (x.dataSheet.Count > 0) {
            var ds_ = new List<string>();
            foreach (var ds in x.dataSheet) {
                if (!string.IsNullOrEmpty(ds)) {
                    ds_.Add(string.Format("{0}", ds));
                }
            }
            dataSheet = string.Join(";", ds_);
        }
        x.discount.coeff = x.discount.perc / 100;
        if (string.IsNullOrEmpty(x.id)) {
            x.id = Guid.NewGuid().ToString();
            sql = string.Format(@"INSERT INTO products VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', {20}, '{21}', '{22}', '{23}', '{24}', '{25}', '{26}', '{27}', '{28}', '{29}', '{30}', '{31}', '{32}', '{33}', '{34}')"
                                , x.id, x.sku, x.style, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.gross, x.discount.coeff, x.discount.from, x.discount.to, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.deliverydays, x.productorder, x.freeshipping, x.bestbuy, x.wifi, relatedProducts, x.dimension.width, x.dimension.height, x.dimension.depth, x.power, x.color.code, x.energyClass, dataSheet, x.opportunity, keyFeatures, x.fireboxInsert);
        } else {
            sql = string.Format(@"UPDATE products SET sku = '{1}', style = '{2}', productgroup = '{3}', title = '{4}', shortdesc = '{5}', longdesc = '{6}', brand = '{7}', img = '{8}', price = '{9}', discount = '{10}', discountfrom = '{11}', discountto = '{12}', stock = '{13}', isnew = '{14}', outlet = '{15}', bestselling = '{16}', isactive = '{17}', features = '{18}', deliverydays = '{19}', productorder = {20},
                                    freeshipping = '{21}', bestbuy = '{22}', wifi = '{23}', relatedproducts = '{24}', width = '{25}', height = '{26}', depth = '{27}', power = '{28}', color = '{29}', energyclass = '{30}', datasheet = '{31}', opportunity = '{32}', keyfeatures = '{33}', fireboxinsert = '{34}' WHERE id = '{0}'"
                                , x.id, x.sku, x.style, x.productGroup.code, x.title, x.shortdesc, x.longdesc, x.brand.code, x.img, x.price.gross, x.discount.coeff, x.discount.from, x.discount.to, x.stock, x.isnew, x.outlet, x.bestselling, x.isactive, productFeatures, x.deliverydays, x.productorder, x.freeshipping, x.bestbuy, x.wifi, relatedProducts, x.dimension.width, x.dimension.height, x.dimension.depth, x.power, x.color.code, x.energyClass, dataSheet, x.opportunity, keyFeatures, x.fireboxInsert);
        }
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
        return x;
    }

    public int GetTotRecords(string searchSql) {
        int x = 0;
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            string sql = string.Format(@"SELECT COUNT(DISTINCT p.style) FROM products p
                                        LEFT OUTER JOIN productGroups pg
                                        ON p.productGroup = pg.code
                                        LEFT OUTER JOIN brands b
                                        ON p.brand = b.code {0}", searchSql);
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = G.ReadI(reader, 0);
                    }
                }
            }
            connection.Close();
        }
        return x;
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

    // refaktorirati
    public NewProduct GetProductData(string sql, string lang) {
        NewProduct x = new NewProduct();
        List<Features.NewFeature> features = F.Get(G.featureType.product);
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                var reader = command.ExecuteReader();
                x = new NewProduct();
                while (reader.Read()) {
                    x = ReadDataRow(reader, lang, true, features);
                }
            }
        }
        Review R = new Review();
        x.reviews = R.GetData(x.sku);
        x.productGroup.parent = PG.GetParentGroupData(x.productGroup.parent.code);
        return x;
    }

    public List<NewProduct> DataCollection(string sql, string lang, bool loadAllData) {
        List<NewProduct> xx = new List<NewProduct>();
        List<Features.NewFeature> features = F.Get(G.featureType.product);
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    xx = new List<NewProduct>();
                    while (reader.Read()) {
                        NewProduct x = ReadDataRow(reader, lang, loadAllData, features);
                        xx.Add(x);
                    }
                }
            }
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
            x.productGroup.title = G.ReadS(reader, 35);
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
            x.brand.title = G.ReadS(reader, 36);
            x.brand.title_seo = G.GetSeoTitle(x.brand.title);
        }
        x.img = G.ReadS(reader, 8);
        x.price = new Price();
        x.price.gross = G.ReadD(reader, 9);
        x.price.net = x.price.gross - (x.price.gross / 1.25);
        //double productDiscount = G.ReadD(reader, 10);  // Product Discount
        //double pgDisCount = G.ReadI(reader, 20);  // Product Group Discount

        x.discount = GetDiscount(G.ReadD(reader, 10), G.ReadS(reader, 11), G.ReadS(reader, 12), G.ReadD(reader, 37), G.ReadS(reader, 38), G.ReadS(reader, 39));

        //x.discount = new Discount();
        //x.discount.coeff = G.ReadD(reader, 10);
        //x.discount.perc = Math.Round(x.discount.coeff * 100, 1);
        //x.price.discount = x.price.net * x.discount.coeff;
        //x.price.netWithDiscount = x.price.net - x.price.discount;
        
        x.price.discount = Math.Round(x.price.gross * (x.discount.isValid ? x.discount.coeff : 0), 2);
        x.price.grossWithDiscount = x.price.gross - x.price.discount;
        //if (loadAllData) {
        x.stock = G.ReadI(reader, 13);
            x.isnew = G.ReadB(reader, 14);
            x.outlet = G.ReadB(reader, 15);
            x.bestselling = G.ReadB(reader, 16);
            x.isactive = G.ReadB(reader, 17);
            x.features = F.GetProductFeatures(features, G.ReadS(reader, 18), x.productGroup);
            x.deliverydays = G.ReadS(reader, 19);
            x.productorder = G.ReadI(reader, 20);

        x.freeshipping = G.ReadB(reader, 21);
        x.bestbuy = G.ReadB(reader, 22);
        x.wifi = G.ReadB(reader, 23);
        x.relatedProductsStr = G.ReadS(reader, 24);
        x.relatedProducts = new List<NewProduct>();
        x.dimension = new Dimension();
        x.dimension.width = G.ReadD(reader, 25);
        x.dimension.height = G.ReadD(reader, 26);
        x.dimension.depth = G.ReadD(reader, 27);
        x.power = G.ReadS(reader, 28);
        x.color = C.GetData(G.ReadS(reader, 29)); // new Colors.NewColor();
        x.energyClass = G.ReadS(reader, 30);
        x.dataSheet = GetDataSheet(G.ReadS(reader, 31));
        x.opportunity = G.ReadB(reader, 32);
        x.keyFeatures = GetKeyFeatures(G.ReadS(reader, 33)); // new List<Global.CodeTitle>(); // G.ReadS(reader, 33); //TODO;
        x.fireboxInsert = G.ReadS(reader, 34);

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

    private Discount GetDiscount(double productDiscount, string pFrom, string pTo, double pgDiscount, string pgFrom, string pgTo) {
        Discount x = new Discount();
        if (productDiscount > 0) {
            DateTime today = DateTime.Now;
            if (productDiscount > 0) {
                x.coeff = productDiscount;
                x.from = pFrom;
                x.to = pTo;
            } else {
                x.coeff = pgDiscount;
                x.from = pgFrom;
                x.to = pgTo;
            }
            x.perc = Math.Round(x.coeff * 100, 1);
            if (!string.IsNullOrEmpty(x.from) && (!string.IsNullOrEmpty(x.to))) {
                if (today >= Convert.ToDateTime(x.from) && today <= Convert.ToDateTime(x.to))
                {
                    x.isValid = true;
                }
            }
        }
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
        x.show.values = new int[] { 6, 12, 24, 48 };
        x.show.val = defaultLimit;
        x.page = 1;
        x.color = C.GetDistinctColors(xxx.data);
        x.opportunity = new FilterItem();
        x.opportunity.code = "opportunity";
        x.opportunity.title = "opportunity";
        x.opportunity.val = false;
        x.opportunity.tot = xxx.data.Count > 0 ? xxx.data.Count(a => a.opportunity) : 0;
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
        s.val = "ratingH";
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

    public List<Dimension> GetDistinctDimensions(string style) {
        List<Dimension> xx = new List<Dimension>();
        //string x, width, height, depth = null;
        string sql = string.Format("SELECT DISTINCT width, height, depth FROM products WHERE style = '{0}'", style);
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        Dimension x = new Dimension();
                        x.width = G.ReadD(reader, 0);
                        x.height = G.ReadD(reader, 1);
                        x.depth = G.ReadD(reader, 2);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public List<Colors.NewColor> GetDistinctColors(string style, Dimension d) {
        List<Colors.NewColor> xx = new List<Colors.NewColor>();
        string sql = null;
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            sql = string.Format(@"SELECT DISTINCT color FROM products WHERE style = '{0}' AND width = '{1}' AND height = '{2}' AND depth = '{3}'"
                                , style, d.width, d.height, d.depth);
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<Colors.NewColor>();
                    while (reader.Read()) {
                        Colors.NewColor x = new Colors.NewColor();
                        x = C.GetData(G.ReadS(reader, 0));
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public List<Global.CodeTitle> GetDistinctFireboxInserts(string style) {
        List<Global.CodeTitle> xx = new List<Global.CodeTitle>();
        Global.CodeTitle x = new Global.CodeTitle();
        string sql = null;
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            sql = string.Format(@"SELECT DISTINCT fireboxinsert FROM products WHERE style = '{0}'"
                                , style);
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        if (!string.IsNullOrEmpty(G.ReadS(reader, 0))) {
                            x = new Global.CodeTitle();
                            x.title = G.ReadS(reader, 0);
                            xx.Add(x);
                        }
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public List<Global.CodeTitle> GetKeyFeatures(string features) {
        List<Global.CodeTitle> xx = new List<Global.CodeTitle>();
        if (!string.IsNullOrEmpty(features)) {
            Global.CodeTitle x = new Global.CodeTitle();
            string[] list = features.Split(';');
            int count = 0;
            foreach (var l in list) {
                x = new Global.CodeTitle();
                x.code = count.ToString();
                x.title = l;
                xx.Add(x);
                count++;
            }
        }
        return xx;
    }

    public List<string> GetDataSheet(string x) {
        List<string> xx = new List<string>();
        string[] list = x.Split(';');
        foreach (var l in list) {
            xx.Add(l);
        }
        return xx;
    }

    /***** Import CSV file *****/
    private string GetProductGroupCode(string pg, string parentGroup) {
        string code = null;
        switch (pg.ToLower().Trim()) {
            case "mill uljni radijatori":
                code = "MILLULJNIRADIJAT";
                break;
            case "mill norveški radijatori":
                code = "MILLNORVESKIRADI";
                break;
            case "mill konvektori":
                code = "MILLKONVEKTORI";
                break;
            case "mill dodaci":
                code = "MILLDODACI";
                break;
            case "inserti":
                code = "INSERTI";
                break;
            case "ugradbeni kamini":
                code = "UGRADBENIEL";
                break;
            case "ugradbeni kamini/zidni kamin":
                code = "UGRADBENIEL;ZIDNIEL";
                break;
            case "ugradbeni kamini/inserti":
                code = "UGRADBENIEL;INSERTI";
                break;
            case "zidni kamini":
                code = "ZIDNIEL";
                break;
            case "samostojeći kamini":
                code = "SAMOSTOJECIEL";
                break;
            case "dimplex dodaci":
                code = "DIMPLEXDODACI";
                break;
            default:
                code = null;
                break;
        }
        return code;
    }

    private string GetBrandCode(string title) {
        string code = null;
        switch (title.ToLower().Trim()) {
            case "mill":
                code = "MILL";
                break;
            case "dimplex":
                code = "DIMPLEX";
                break;
            default:
                code = null;
                break;
        }
        return code;
    }

    private bool IsTrue(string x) {
        return x.ToLower().Trim() == "da" ? true : false;
    }

    private string ColorName(string title) {
        string code = null;
        switch (title.ToLower().Trim()) {
            case "bijela":
                code = "white";
                break;
            case "crna":
                code = "black";
                break;
            default:
                code = null;
                break;
        }
        return code;
    }

    private double GetVal(string x) {
        return !string.IsNullOrEmpty(x) ? Convert.ToDouble(x) : 0;
    }

    private string ToHtml(string x) {
        return x.Replace("m?", "m<sup>2</sup>");
    }
    /***** Import CSV file *****/


    /***** Images *****/
    public class Images {
        public string id;
        public string productId;
        public string fileName;
        public bool isMain;
        public int imageOrder;
    }

    public void SaveImage(Images x) {
        if (!string.IsNullOrEmpty(x.productId)) {
            DB.CreateDataBase(G.db.images);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO images VALUES('{0}', '{1}', '{2}', '{3}', {4})"
                                    , x.id, x.productId, x.fileName, x.isMain, x.imageOrder);
            } else {
                sql = string.Format(@"UPDATE images SET productId = '{1}', fileName = '{2}', isMain = '{3}', imageOrder = {4}, WHERE id = '{0}'"
                                    , x.id, x.productId, x.fileName, x.isMain, x.imageOrder);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
    /***** Images *****/

    #endregion Methods

}
