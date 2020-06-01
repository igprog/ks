using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Users
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Users : System.Web.Services.WebService {
    Global G = new Global();
    DataBase DB = new DataBase();
    public Users() {
    }

    public class NewUser {
        public string id;
        public string userType; //***** fizicka / pravna osoba *****
        public Details billingDetails;

        //public string firstName;
        //public string lastName;
        //public string company;
        //public string address;
        //public string postalCode;
        //public string city;
        //public Global.CodeTitle country;
        public string pin;
        //public string phone;
        //public string email;
        public Details deliveryDetails;
        public string emailConfirm;
        public string userName;
        public string password;
        public string passwordConfirm;
    }

    public class Details {
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

    public string Save(NewUser x) {
        try {
            DB.CreateDataBase(G.db.users);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO users VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}')"
                                    , x.id, x.billingDetails.firstName, x.billingDetails.lastName, x.billingDetails.company, x.billingDetails.address, x.billingDetails.postalCode, x.billingDetails.city, x.billingDetails.country.code, x.pin, x.billingDetails.phone, x.billingDetails.email
                                    , x.deliveryDetails.firstName, x.deliveryDetails.lastName, x.deliveryDetails.email, x.deliveryDetails.phone, x.deliveryDetails.company, x.deliveryDetails.address, x.deliveryDetails.postalCode, x.deliveryDetails.city, x.deliveryDetails.country.code, x.userName, x.password);
            } else {
                sql = string.Format(@"UPDATE users SET firstName = '{1}', lastName = '{2}', company = '{3}', address = '{4}', postalCode = '{5}', city = '{6}', pin = '{7}', phone = '{8}', email = '{9}', deliveryFirstName = '{10}', deliveryLastName = '{11}'
                                    , deliveryEmail = '{12}', deliveryPhone = '{13}', deliveryCompany = '{14}', deliveryAddress = '{15}', deliveryPostalCode = '{16}', deliveryCity = '{17}', deliveryCountry = '{18}', userName = '{19}', password = '{20}' WHERE id = '{0}'"
                                    , x.id, x.billingDetails.firstName, x.billingDetails.lastName, x.billingDetails.company, x.billingDetails.address, x.billingDetails.postalCode, x.billingDetails.city, x.billingDetails.country.code, x.pin, x.billingDetails.phone, x.billingDetails.email
                                    , x.deliveryDetails.firstName, x.deliveryDetails.lastName, x.deliveryDetails.email, x.deliveryDetails.phone, x.deliveryDetails.company, x.deliveryDetails.address, x.deliveryDetails.postalCode, x.deliveryDetails.city, x.deliveryDetails.country.code, x.userName, x.password);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return "OK";
            //return JsonConvert.SerializeObject("Review saved successfully", Formatting.None);
            //return JsonConvert.SerializeObject(GetData(x.sku), Formatting.None);
        } catch (Exception e) {
            return e.Message;
            //return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

}
