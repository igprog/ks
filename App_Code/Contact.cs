using System.Web.Services;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Contact
/// </summary>
[WebService(Namespace = "http://studiotanya.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Contact : System.Web.Services.WebService {
    Global G = new Global();

    public Contact() {
    }

    public class NewContact {
        public string name;
        public string email;
        public string phone;
        public string file;
        public string msg;
        public Mail.Response response;
    }

    [WebMethod]
    public string Init() {
        NewContact x = new NewContact();
        x.name = null;
        x.email = null;
        x.phone = null;
        x.file = null;
        x.msg = null;
        x.response = new Mail.Response();
        x.response.isSent = false;
        x.response.msg = null;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Send(NewContact x) {
        string subject = string.Format(@"
<h3>Novi upit</h3>
<p>Ime: {0}</p>
<p>Email: {1}</p>
<p>Tel: {2}</p>
<p>Poruka: {3}</p>", x.name, x.email, x.phone, x.msg);
        Mail m = new Mail();
        x.response = m.SendMail(G.email, "Novi upit", subject, x.file);
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

}
