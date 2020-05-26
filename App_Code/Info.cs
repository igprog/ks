using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Info
/// </summary>
[WebService(Namespace = "http://dizajn911.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Info : System.Web.Services.WebService {
    Global G = new Global();
    Tran T = new Tran();
    string path = "~/data/info.json";
    string folder = "~/data/";

    public Info() {
    }

    public class NewInfo {
        public string company;
        public string address;
        public string country;
        public string pin;
        public string firstName;
        public string lastName;
        public string phone;
        public string email;
        public string services;
        public string about;
        public Social social;
        public PaymentDetails paymentDetails;

    }

    public class Social {
        public string facebook;
        public string instagram;
    }

    public class PaymentDetails {
        public string recipient;
        public string iban;
        public string swift;
        public string bank;
        public string bankAddress;
    }

    [WebMethod]
    public string Load(string lang) {
        try {
            return JsonConvert.SerializeObject(GetInfo(lang), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(NewInfo x) {
        try {
            if (!Directory.Exists(Server.MapPath(folder))) {
                Directory.CreateDirectory(Server.MapPath(folder));
            }
            WriteFile(path, x);
            return Load(null);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string LoadMainGellery() {
        try {
            string[] x = GetMainGallery();
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return null;
        }
    }

    [WebMethod]
    public string DeleteMainImg(string img) {
        try {
            string path = Server.MapPath(string.Format("~/upload/main/{1}", folder, img));
            if (File.Exists(path)) {
                File.Delete(path);
            }
            return JsonConvert.SerializeObject(null, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(img, Formatting.None);
        }
    }

    protected void WriteFile(string path, NewInfo value) {
        File.WriteAllText(Server.MapPath(path), JsonConvert.SerializeObject(value));
    }

    public NewInfo GetInfo(string lang) {
        NewInfo x = new NewInfo();
        x.social = new Social();
        string json = ReadFile();
        if (!string.IsNullOrEmpty(json)) {
            x = JsonConvert.DeserializeObject<NewInfo>(json);
            List<Tran.NewTran> tran = T.LoadData(null, G.recordType.about, lang);
            x.about = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : x.about;
            tran = T.LoadData(null, G.recordType.services, lang);
            x.services = !string.IsNullOrEmpty(lang) && tran.Count > 0 ? tran[0].tran : x.services;
            return x;
        } else {
            return x;
        }
    }

    public string ReadFile() {
        if (File.Exists(Server.MapPath(path))) {
            return File.ReadAllText(Server.MapPath(path));
        } else {
            return null;
        }
    }

    string[] GetMainGallery() {
        string[] xx = null;
        string path = Server.MapPath("~/upload/main");
        if (Directory.Exists(path)) {
            string[] ss = Directory.GetFiles(path);
            xx = ss.Select(a => Path.GetFileName(a)).ToArray();
        }
        return xx;
    }

}
