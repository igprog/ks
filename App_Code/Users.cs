using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Users
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Users : System.Web.Services.WebService {
    //string countries_path = "~/data/countries.json";
    //string countries_folder = "~/data/";
    public Users() {
    }

    public class NewUser {
        public string id;
        public string firstName;
        public string lastName;
        public string userType; // fizicka / pravna osoba
        public string company;
        public string address;
        public string postalCode;
        public string city;
        public Global.CodeTitle country;
        public string pin;
        public string phone;
        public string email;
        public string emailConfirm;
        public string userName;
        public string password;
        public string passwordConfirm;
        public string ipAddress;
        public Address deliveryAddress;
        //public string deliveryFirstName;
        //public string deliveryLastName;
        //public string deliveryCompanyName;
        //public string deliveryAddress;
        //public string deliveryPostalCode;
        //public string deliveryCity;
        //public Orders.CodeTitle deliveryCountry;
        //public string deliveryMethod;
        //public string paymentMethod;
        //public List<Country> countries;
        //public Orders.DiscountCoeff discount = new Orders.DiscountCoeff();

    }

    public class Address {
        public string firstName;
        public string lastName;
        public string email;
        public string phone;
        public string company;
        public string address;
        public string postalCode;
        public string city;
        public Global.CodeTitle country;
    }

    //public class Country {
    //    public string code;
    //    public string name;
    //}

    #region WebMethods
    //[WebMethod]
    //public string GetCountries() {
    //    return JsonConvert.SerializeObject(GetCountriesJson(), Formatting.None);
    //}

    //[WebMethod]
    //public string SaveCountries(List<Country> x) {
    //    try {
    //        if (!Directory.Exists(Server.MapPath(countries_folder))) {
    //            Directory.CreateDirectory(Server.MapPath(countries_folder));
    //        }
    //        WriteFile(countries_path, x);
    //        return GetCountries();
    //    } catch (Exception e) {
    //        return JsonConvert.SerializeObject(e.Message, Formatting.None);
    //    }
    //}

    //public List<Country> GetCountriesJson() {
    //    try {
    //        string json = null;
    //        if (File.Exists(Server.MapPath(countries_path))) {
    //            json = File.ReadAllText(Server.MapPath(countries_path));
    //        }
    //        return JsonConvert.DeserializeObject<List<Country>>(json);
    //    } catch (Exception e) {
    //        return new List<Country>();
    //    }
    //}

    //protected void WriteFile(string path, List<Country> value) {
    //    File.WriteAllText(Server.MapPath(path), JsonConvert.SerializeObject(value));
    //}
    //public string WriteJsonFile(string filename, string json) {
    //    try {
    //        CreateFolder("~/App_Data/json/");
    //        string path = string.Format(@"~/App_Data/json/{0}.json", filename);
    //        File.WriteAllText(Server.MapPath(path), json);
    //        return "OK";
    //    } catch (Exception e) {
    //        return e.Message;
    //    }
    //}

    //public void CreateFolder(string path) {
    //    if (!Directory.Exists(Server.MapPath(path))) {
    //        Directory.CreateDirectory(Server.MapPath(path));
    //    }
    //}
    #endregion WebMethods

}
