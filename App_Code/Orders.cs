using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Igprog;

/// <summary>
///Order
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Orders : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    //string productDataBase = ConfigurationManager.AppSettings["ProductDataBase"];
    DataBase db = new DataBase();
    Global G = new Global();
    Users U = new Users();
    string folder = "~/data/json/";
    string orderOptions_json = "orderoptions";
    string countries_json = "countries";
    //string countries_path = "~/data/countries.json";
    //string countries_folder = "~/data/";
    //Invoice i = new Invoice();

    public Orders() {
    }
    public class NewOrder {
        public string id;
        public Users.NewUser user;
        public Cart.NewCart cart;
        public string orderDate;
        public Global.CodeTitle deliveryMethod;
        public Global.CodeTitle paymentMethod;
        public string note;
        public string number;
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
        x.user.deliveryAddress = new Users.Address();
        x.cart = cart;
        x.orderDate = null;
        x.countries = GetCountriesJson();
        x.deliveryMethod = new Global.CodeTitle();
        x.paymentMethod = new Global.CodeTitle();
        x.note = null;
        x.confirmTerms = false;
        x.number = null;
        x.status = new Global.CodeTitle();
        x.invoice = null;
        x.invoiceId = null;
        x.paymentDetails = new Info.PaymentDetails();
        x.orderOptions = GetOrderOptionsJson();
        x.response = new Response();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Confirm(NewOrder x) {
        //TOOD
        // Save to orders.tbl


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

    [WebMethod]
    public string SaveOrderOptions(OrderOptions x) {
        WriteFile(orderOptions_json, JsonConvert.SerializeObject(x));
        return JsonConvert.SerializeObject(GetOrderOptionsJson(), Formatting.None);
        //return WriteJsonFile(orderOptionsFile, JsonConvert.SerializeObject(x, Formatting.None));
    }

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
