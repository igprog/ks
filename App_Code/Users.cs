using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Summary description for Users
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Users : System.Web.Services.WebService {

    public Users() {
    }

    public class NewUser {
        public string id;
        public string firstName;
        public string lastName;
        public string companyName;
        public string address;
        public string postalCode;
        public string city;
        public Country country = new Country();
        public string pin;
        public string phone;
        public string email;
        public string emailConfirm;
        public string userName;
        public string password;
        public string passwordConfirm;
        public string ipAddress;
        public string deliveryFirstName;
        public string deliveryLastName;
        public string deliveryCompanyName;
        public string deliveryAddress;
        public string deliveryPostalCode;
        public string deliveryCity;
        public Country deliveryCountry;
        public string deliveryType;
        public string paymentMethod;
        //public Orders.DiscountCoeff discount = new Orders.DiscountCoeff();

    }

    public class Country {
        public string code;
        public string name;
    }

}
