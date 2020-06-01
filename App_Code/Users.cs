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
    string mainSql = @"SELECT u.id, u.firstName, u.lastName, u.company, u.address, u.postalCode, u.city, u.country, u.pin, u.phone, u.email, u.deliveryFirstName, u.deliveryLastName,
                              u.deliveryCompany, u.deliveryAddress, u.deliveryPostalCode, u.deliveryCity, u.deliveryCountry, u.deliveryEmail, u.deliveryPhone, u.userName, u.password
                        FROM users u";

    public Users() {
    }

    public class NewUser {
        public string id;
        public string userType; //***** fizicka / pravna osoba *****
        public Details billingDetails;
        public string pin;
        public Details deliveryDetails;
        public string emailConfirm;
        public string userName;
        public string password;
        public string passwordConfirm;
    }

    public class Details {
        public string firstName;
        public string lastName;
        public string company;
        public string address;
        public string postalCode;
        public string city;
        public Global.CodeTitle country;
        public string phone;
        public string email;
    }

    public string Save(NewUser x) {
        try {
            DB.CreateDataBase(G.db.users);
            string sql = null;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                sql = string.Format(@"INSERT INTO users VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}')"
                                                                      , x.id, x.billingDetails.firstName, x.billingDetails.lastName, x.billingDetails.company, x.billingDetails.address, x.billingDetails.postalCode, x.billingDetails.city, x.billingDetails.country.code, x.pin, x.billingDetails.phone, x.billingDetails.email
                                    , x.deliveryDetails.firstName, x.deliveryDetails.lastName, x.deliveryDetails.company, x.deliveryDetails.address, x.deliveryDetails.postalCode, x.deliveryDetails.city, x.deliveryDetails.country.code, x.deliveryDetails.email, x.deliveryDetails.phone, x.userName, x.password);
            } else {
                sql = string.Format(@"UPDATE users SET firstName = '{1}', lastName = '{2}', company = '{3}', address = '{4}', postalCode = '{5}', city = '{6}', country = '{7}', pin = '{8}', phone = '{9}', email = '{10}'
                                    , deliveryFirstName = '{11}', deliveryLastName = '{12}' , deliveryCompany = '{13}', deliveryAddress = '{14}', deliveryPostalCode = '{15}', deliveryCity = '{16}', deliveryCountry = '{17}', deliveryEmail = '{18}', deliveryPhone = '{19}', userName = '{20}', password = '{21}' WHERE id = '{0}'"
                                    , x.id, x.billingDetails.firstName, x.billingDetails.lastName, x.billingDetails.company, x.billingDetails.address, x.billingDetails.postalCode, x.billingDetails.city, x.billingDetails.country.code, x.pin, x.billingDetails.phone, x.billingDetails.email
                                    , x.deliveryDetails.firstName, x.deliveryDetails.lastName, x.deliveryDetails.company, x.deliveryDetails.address, x.deliveryDetails.postalCode, x.deliveryDetails.city, x.deliveryDetails.country.code, x.deliveryDetails.email, x.deliveryDetails.phone, x.userName, x.password);
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

    [WebMethod]
    public string Get(string id) {
        try {
            return JsonConvert.SerializeObject(GetData(id), Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    #region Methods
    public NewUser GetData(string id) {
        NewUser x = new NewUser();
        string sql = string.Format("{0} WHERE u.id = '{1}'", mainSql, id);
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    x = new NewUser();
                    while (reader.Read()) {
                        x = ReadDataRow(reader);
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public List<NewUser> LoadData(string sql) {
        DB.CreateDataBase(G.db.users);
        List<NewUser> xx = DataCollection(sql);
        return xx;
    }

    public List<NewUser> DataCollection(string sql) {
        List<NewUser> xx = new List<NewUser>();
        using (var connection = new SQLiteConnection("Data Source=" + DB.GetDataBasePath(G.dataBase))) {
            connection.Open();
            using (var command = new SQLiteCommand(sql, connection)) {
                using (var reader = command.ExecuteReader()) {
                    xx = new List<NewUser>();
                    while (reader.Read()) {
                        NewUser x = ReadDataRow(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public NewUser ReadDataRow(SQLiteDataReader reader) {
        NewUser x = new NewUser();
        x.id = G.ReadS(reader, 0);
        x.billingDetails = new Details();
        x.billingDetails.firstName = G.ReadS(reader, 1);
        x.billingDetails.lastName = G.ReadS(reader, 2);
        x.billingDetails.company = G.ReadS(reader, 3);
        x.billingDetails.address = G.ReadS(reader, 4);
        x.billingDetails.postalCode = G.ReadS(reader, 5);
        x.billingDetails.city = G.ReadS(reader, 6);
        x.billingDetails.country = new Global.CodeTitle();
        x.billingDetails.country.code = G.ReadS(reader, 7);
        x.pin = G.ReadS(reader, 8);
        x.billingDetails.phone = G.ReadS(reader, 9);
        x.billingDetails.email = G.ReadS(reader, 10);
        x.deliveryDetails = new Details();
        x.deliveryDetails.firstName = G.ReadS(reader, 11);
        x.deliveryDetails.lastName = G.ReadS(reader, 12);
        x.deliveryDetails.company = G.ReadS(reader, 13);
        x.deliveryDetails.address = G.ReadS(reader, 14);
        x.deliveryDetails.postalCode = G.ReadS(reader, 15);
        x.deliveryDetails.city = G.ReadS(reader, 16);
        x.deliveryDetails.country = new Global.CodeTitle();
        x.deliveryDetails.country.code = G.ReadS(reader, 17);
        x.deliveryDetails.phone = G.ReadS(reader, 18);
        x.deliveryDetails.email = G.ReadS(reader, 19);
        x.userName = G.ReadS(reader, 20);
        x.password = G.ReadS(reader, 21);
        return x;
    }
    #endregion Methods

}
