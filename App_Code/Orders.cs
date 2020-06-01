﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Data.SQLite;
using Igprog;

/// <summary>
///Order
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Orders : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase DB = new DataBase();
    Global G = new Global();
    Users U = new Users();
    string folder = "~/data/json/";
    string orderOptions_json = "orderoptions";
    string countries_json = "countries";
    string mainSql = @"SELECT o.id, o.userId, o.items, netPrice, o.grossPrice, o.deliveryPrice, o.discount, o.currency, o.orderDate, o.deliveryMethod, o.paymentMethod, o.note, o.orderNumber, o.status FROM orders o";

    public Orders() {
    }
    public class NewOrder {
        public string id;
        public Users.NewUser user;
        public Cart.NewCart cart;
        public string currency;
        public string orderDate;
        public Global.CodeTitle deliveryMethod;
        public Global.CodeTitle paymentMethod;
        public string note;
        public string orderNumber;
        public bool confirmTerms;
        public Global.CodeTitle status;
        public string invoice;
        public string invoiceId;
        public Info.PaymentDetails paymentDetails;
        public List<Global.CodeTitle> countries;
        public OrderOptions orderOptions;
        public Response response;

    }

    public class OrderOptions {
        public double deliveryprice;
        public List<Global.CodeTitle> deliverymethod = new List<Global.CodeTitle>();
        public List<Global.CodeTitle> paymentmethod = new List<Global.CodeTitle>();
        public List<Global.CodeTitle> orderstatus = new List<Global.CodeTitle>();
        //public List<DiscountCoeff> discountcoeff = new List<DiscountCoeff>();
        public List<Bank> bank = new List<Bank>();
    }

    //public class CodeTitle {
    //    public string code;
    //    public string title;
    //}

    public class Bank {
        public string code;
        public string title;
        public string link;
    }

    public class Response {
        public bool isSuccess;
        public string msg;
    }

    #region WebMethods
    [WebMethod]
    public string Init(Cart.NewCart cart) {
        NewOrder x = new NewOrder();
        x.id = null;
        x.user = new Users.NewUser();
        x.user.userType = G.userTypes.natural;
        x.user.billingDetails = new Users.Details();
        x.user.deliveryDetails = new Users.Details();
        x.cart = cart;
        x.orderDate = null;
        x.countries = GetCountriesJson();
        x.deliveryMethod = new Global.CodeTitle();
        x.paymentMethod = new Global.CodeTitle();
        x.note = null;
        x.confirmTerms = false;
        x.orderNumber = null;
        x.status = new Global.CodeTitle();
        x.invoice = null;
        x.invoiceId = null;
        x.paymentDetails = new Info.PaymentDetails();
        x.orderOptions = GetOrderOptionsJson();
        x.response = new Response();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string InitTest(Cart.NewCart cart) {
        NewOrder x = new NewOrder();
        x.id = null;
        x.user = new Users.NewUser();
        x.user.billingDetails = new Users.Details();
        x.user.billingDetails.firstName = "Igor";
        x.user.billingDetails.lastName = "Gašparović";
        x.user.billingDetails.company = "IG PROG";
        x.user.billingDetails.address = "Ludvetov breg 5";
        x.user.billingDetails.postalCode = "51000";
        x.user.billingDetails.city = "Rijeka";
        x.user.billingDetails.country = new Global.CodeTitle();
        x.user.billingDetails.country.code = "HR";
        x.user.billingDetails.country.title = "Croatia";
        x.user.billingDetails.phone = "098330966";
        x.user.billingDetails.email = "igprog@yahoo.com";
        x.user.pin = "123456789";
        x.user.userType = G.userTypes.natural;
        x.user.deliveryDetails = new Users.Details();
        x.cart = cart;
        x.orderDate = null;
        x.countries = GetCountriesJson();
        x.deliveryMethod = new Global.CodeTitle();
        x.paymentMethod = new Global.CodeTitle();
        x.note = null;
        x.confirmTerms = false;
        x.orderNumber = null;
        x.status = new Global.CodeTitle();
        x.invoice = null;
        x.invoiceId = null;
        x.paymentDetails = new Info.PaymentDetails();
        x.orderOptions = GetOrderOptionsJson();
        x.response = new Response();
        return JsonConvert.SerializeObject(x, Formatting.None);
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
    public string Confirm(NewOrder x) {
        try {
            string saveUser = U.Save(x.user);
            if (saveUser == "OK") {
                string saveOrder = Save(x);
                x.response.isSuccess = saveOrder == "OK" ? true: false;
                x.response.msg = saveOrder;
                JsonConvert.SerializeObject(x, Formatting.None);
            } else {
                x.response.isSuccess = false;
                x.response.msg = saveUser;
                return JsonConvert.SerializeObject(x, Formatting.None);
            }
        } catch (Exception e) {
            x.response.isSuccess = false;
            x.response.msg = e.Message;
            JsonConvert.SerializeObject(x, Formatting.None);
        }
        //TOOD:
        // Sent mail to customer and me

        // Send response
        Info.NewInfo info = new Info().GetInfo("hr");
        x.paymentDetails = info.paymentDetails;

        x.response.isSuccess = true;
        x.response.msg = "narudžba uspješno poslana";

        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string GetOrderOptions() {
        return JsonConvert.SerializeObject(GetOrderOptionsJson(), Formatting.None);
    }

    //[WebMethod]
    //public string SaveOrderOptions(OrderOptions x) {
    //    WriteFile(orderOptions_json, JsonConvert.SerializeObject(x));
    //    return JsonConvert.SerializeObject(GetOrderOptionsJson(), Formatting.None);
    //    //return WriteJsonFile(orderOptionsFile, JsonConvert.SerializeObject(x, Formatting.None));
    //}

    [WebMethod]
    public string GetCountries() {
        return JsonConvert.SerializeObject(GetCountriesJson(), Formatting.None);
    }

    [WebMethod]
    public string SaveCountries(List<Global.CodeTitle> x) {
        try {
            CreateFolder(folder);
            //if (!Directory.Exists(Server.MapPath(countries_folder))) {
            //    Directory.CreateDirectory(Server.MapPath(countries_folder));
            //}
            WriteFile(countries_json, JsonConvert.SerializeObject(x));
            return GetCountries();
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public List<NewOrder> LoadData(string sql) {
        DB.CreateDataBase(G.db.orders);
        List<NewOrder> xx = DataCollection(sql);
        return xx;
    }

    public List<NewOrder> DataCollection(string sql) {
        List<NewOrder> xx = new List<NewOrder>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewOrder>();
                    while (reader.Read()) {
                        NewOrder x = ReadDataRow(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public NewOrder ReadDataRow(SQLiteDataReader reader) {
        NewOrder x = new NewOrder();
        x.id = G.ReadS(reader, 0);
        x.user = new Users.NewUser();
        //TODO: GetUser
        x.user.id = G.ReadS(reader, 1);
        x.cart = new Cart.NewCart();
        x.cart.items = GetItems(G.ReadS(reader, 2));  // TODO
        x.cart.cartPrice = new Cart.CartPrice();
        x.cart.cartPrice.netPrice = G.ReadD(reader, 3);
        x.cart.cartPrice.grossPrice = G.ReadD(reader, 4);
        x.cart.cartPrice.deliveryPrice = G.ReadD(reader, 5);
        x.cart.cartPrice.discount = G.ReadD(reader, 6);
        x.currency = G.ReadS(reader, 7);
        x.orderDate = G.ReadS(reader, 8);
        x.deliveryMethod = new Global.CodeTitle();
        x.deliveryMethod.code = G.ReadS(reader, 9);
        x.paymentMethod = new Global.CodeTitle();
        x.paymentMethod.code = G.ReadS(reader, 10);
        x.note = G.ReadS(reader, 11);
        x.orderNumber = G.ReadS(reader, 12);
        x.status = new Global.CodeTitle();
        x.status.code = G.ReadS(reader, 13);
        return x;
    }

    public string Save(NewOrder x) {
        try {
            DB.CreateDataBase(G.db.orders);
            string items = SetItems(x.cart);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO orders VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')"
                                    , x.id, x.user.id, items, x.cart.cartPrice.netPrice, x.cart.cartPrice.grossPrice, x.cart.cartPrice.deliveryPrice, x.cart.cartPrice.discount, x.currency, x.orderDate, x.deliveryMethod.code, x.paymentMethod.code, x.note, x.orderNumber, x.status.code);
            } else {
                sql = string.Format(@"UPDATE orders SET userId = '{1}', items = '{2}', netPrice = '{3}', grossPrice = '{4}', deliveryPrice = '{5}', discount = '{6}' currency = '{7}', orderDate = '{8}', deliveryMethod = '{9}', paymentMethod = '{10}', note = '{11}', orderNumber = '{12}', status = '{13}' WHERE id = '{0}'"
                                    , x.id, x.user.id, items, x.cart.cartPrice.netPrice, x.cart.cartPrice.grossPrice, x.cart.cartPrice.discount, x.currency, x.orderDate, x.deliveryMethod.code, x.paymentMethod.code, x.note, x.orderNumber, x.status.code);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return "OK";
        } catch (Exception e) {
            return e.Message;
        }
    }

    public List<Global.CodeTitle> GetCountriesJson() {
        try {
            string json = null;
            string path = string.Format("{0}{1}.json", folder, countries_json);
            if (File.Exists(Server.MapPath(path))) {
                json = File.ReadAllText(Server.MapPath(path));
            }
            return JsonConvert.DeserializeObject<List<Global.CodeTitle>>(json);
        } catch (Exception e) {
            return new List<Global.CodeTitle>();
        }
    }

    private string SetItems(Cart.NewCart cart) {
        List<string> x = new List<string>();
        foreach (var i in cart.items) {
            x.Add(i.product.sku + ":" + i.product.qty + ";");
        }
        return string.Join(";", x);
    }

    private List<Cart.CartItem> GetItems(string items) {
        List<Cart.CartItem> xx = new List<Cart.CartItem>();
        string[] str = items.Split(';');
        foreach(string s in str) {
            if (!string.IsNullOrEmpty(s)){
                Cart.CartItem x = new Cart.CartItem();
                string[] str_ = s.Split(':');
                x.product = new Products.NewProduct();
                x.product.sku = str_[0];
                x.product.qty = Convert.ToInt32(str_[1]);
                //if (str_.Length > 2) {
                //    x.color = str_[2];
                //    x.size = str_[3];
                //    if (str_.Length > 4) {
                //        x.supplier = str_[4];
                //        if (str_.Length > 5) {
                //            x.style = str_[5];
                //            if (str_.Length > 6) {
                //                x.price = Convert.ToDouble(str_[6]);
                //                if (str_.Length > 7) {
                //                    x.discount = Convert.ToDouble(str_[7]);
                //                }
                //            }
                //        }
                //    }
                //}
                xx.Add(x);
            }
        }
        return xx;
    }

    #endregion WebMethods

    #region Methods
    public void CreateFolder(string folder) {
        if (!Directory.Exists(Server.MapPath(folder))) {
            Directory.CreateDirectory(Server.MapPath(folder));
        }
    }

    protected void WriteFile(string path, string json) {
        File.WriteAllText(Server.MapPath(string.Format("{0}{1}.json", folder, path)), json);
    }
    //public string WriteJsonFile(string filename, string json) {
    //    try {
    //        CreateFolder(folder);
    //        string path = string.Format(@"{0}{1}.json", folder, filename);
    //        File.WriteAllText(Server.MapPath(path), json);
    //        return "OK";
    //    } catch (Exception e) {
    //        return e.Message;
    //    }
    //}

    public OrderOptions GetOrderOptionsJson() {
        try {
            string path = string.Format("{0}{1}.json", folder, orderOptions_json);
            string json = null;
            if (File.Exists(Server.MapPath(path))) {
                json = File.ReadAllText(Server.MapPath(path));
            }
            return JsonConvert.DeserializeObject<OrderOptions>(json);
        } catch (Exception e) {
            return new OrderOptions();
        }
    }
    #endregion Methods

}
