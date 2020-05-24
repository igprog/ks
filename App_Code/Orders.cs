using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Data.SQLite;
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
    string orderOptionsFile = "orderoptions";
    //Invoice i = new Invoice();

    public Orders() {
    }
    public class NewOrder {
        public string id;
        public Users.NewUser user;
        public Cart.NewCart cart;
        public string orderDate;
        public CodeTitle deliveryType;
        public CodeTitle paymentMethod;
        public string note;
        public string number;
        public CodeTitle status;
        public string invoice;
        public string invoiceId;
        public Info.PaymentDetails paymentDetails;
        public Response response;

    }

    public class OrderOption {
        public double deliveryprice;
        public List<CodeTitle> deliverytype = new List<CodeTitle>();
        public List<CodeTitle> paymentmethod = new List<CodeTitle>();
        public List<CodeTitle> orderstatus = new List<CodeTitle>();
        //public List<DiscountCoeff> discountcoeff = new List<DiscountCoeff>();
        //public List<Bank> bank = new List<Bank>();
    }

    public class CodeTitle {
        public string code { get; set; }
        public string title { get; set; }
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
        x.cart = cart;
        x.orderDate = null;
        x.deliveryType = new CodeTitle();
        x.paymentMethod = new CodeTitle();
        x.note = null;
        x.number = null;
        x.status = new CodeTitle();
        x.invoice = null;
        x.invoiceId = null;
        x.paymentDetails = new Info.PaymentDetails();
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
    public string GetOrderOptionsJson() {
        return JsonConvert.SerializeObject(GetOrderOptions(), Formatting.None);
    }

    [WebMethod]
    public string SaveOrderOptions(OrderOption x) {
        return WriteJsonFile(orderOptionsFile, JsonConvert.SerializeObject(x, Formatting.None));
    }

   
    #endregion WebMethods

    #region Methods
    public void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    public string WriteJsonFile(string filename, string json) {
        try {
            CreateFolder("~/App_Data/json/");
            string path = string.Format(@"~/App_Data/json/{0}.json", filename);
            File.WriteAllText(Server.MapPath(path), json);
            return "OK";
        } catch (Exception e) {
            return e.Message;
        }
    }

    public OrderOption GetOrderOptions() {
        try {
            string path = string.Format("~/App_Data/json/{0}.json", orderOptionsFile);
            string json = null;
            if (File.Exists(Server.MapPath(path))) {
                json = File.ReadAllText(Server.MapPath(path));
            }
            return JsonConvert.DeserializeObject<OrderOption>(json);
        } catch (Exception e) {
            return new OrderOption();
        }
    }
    #endregion Methods

}
