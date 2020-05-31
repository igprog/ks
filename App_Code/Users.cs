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
        public string firstName;
        public string lastName;
        public string userType; //***** fizicka / pravna osoba *****
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
        public Address deliveryAddress;
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

    public void Save(NewUser x) {
        try {
            DB.CreateDataBase(G.db.users);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO users VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}')"
                                    , x.id, x.firstName, x.lastName, x.company, x.address, x.postalCode, x.city, x.country, x.pin, x.phone, x.email, x.userName, x.password
                                    , x.deliveryAddress.firstName, x.deliveryAddress.lastName, x.deliveryAddress.phone, x.deliveryAddress.company, x.deliveryAddress.address, x.deliveryAddress.postalCode, x.deliveryAddress.city, x.deliveryAddress.country);
            } else {
                sql = string.Format(@"UPDATE users SET firstName = '{1}', lastName = '{2}', company = '{3}', address = '{4}', postalCode = '{5}', city = '{6}', pin = '{7}', phone = '{8}', email = '{9}', deliveryFirstName = '{10}', deliveryLastName = '{11}'
                                    , deliveryEmail = '{12}', deliveryPhone = '{13}', deliveryCompany = '{14}', deliveryAddress = '{15}', deliveryPostalCode = '{16}', deliveryCity = '{17}', deliveryCountry = '{18}', userName = '{19}', password = '{20}' WHERE id = '{0}'"
                                    , x.id, x.firstName, x.lastName, x.company, x.address, x.postalCode, x.city, x.country, x.pin, x.phone, x.email, x.userName, x.password
                                    , x.deliveryAddress.firstName, x.deliveryAddress.lastName, x.deliveryAddress.phone, x.deliveryAddress.company, x.deliveryAddress.address, x.deliveryAddress.postalCode, x.deliveryAddress.city, x.deliveryAddress.country);
            }
            using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
                connection.Open();
                using (var command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            //return JsonConvert.SerializeObject("Review saved successfully", Formatting.None);
            //return JsonConvert.SerializeObject(GetData(x.sku), Formatting.None);
        } catch (Exception e) {
            //return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

}
