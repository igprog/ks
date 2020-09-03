using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Colors
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Colors : System.Web.Services.WebService {
    Global G = new Global();
    string path = "~/data/json/colors.json";
    string folder = "~/data/json/";

    public Colors() {
    }

    public class NewColor {
        public string code;
        public string title;
        public string hex;
        public string img;
    }

    [WebMethod]
    public string Init() {
        return JsonConvert.SerializeObject(InitData(), Formatting.None);
    }

    [WebMethod]
    public string Load() {
        try {
            return JsonConvert.SerializeObject(LoadData(), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(List<NewColor> x) {
        try {
            if (!Directory.Exists(Server.MapPath(folder))) {
                Directory.CreateDirectory(Server.MapPath(folder));
            }
            WriteFile(path, x);
            return JsonConvert.SerializeObject(LoadData(), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Get(string code) {
        try {
            return JsonConvert.SerializeObject(GetData(code), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    protected void WriteFile(string path, List<NewColor> value) {
        File.WriteAllText(Server.MapPath(path), JsonConvert.SerializeObject(value));
    }

    public NewColor InitData() {
        NewColor x = new NewColor();
        x.code = null;
        x.title = null;
        x.hex = null;
        x.img = null;
        return x;
    }

    public List<NewColor> LoadData() {
        List<NewColor> xx = new List<NewColor>();
        string json = ReadFile();
        if (!string.IsNullOrEmpty(json)) {
            xx = JsonConvert.DeserializeObject<List<NewColor>>(json);
        } else {
            NewColor x = InitData();
            xx.Add(x);
        }
        return xx;
    }

     public NewColor GetData(string code) {
        List<NewColor> xx = LoadData();
        NewColor x = new NewColor();
        if (!string.IsNullOrEmpty(code)) {
            x = xx.Find(a => a.code == code);
            if (x == null) {
                x = new NewColor();
            }
        }
        return x;
    }

    public string ReadFile() {
        if (File.Exists(Server.MapPath(path))) {
            return File.ReadAllText(Server.MapPath(path));
        } else {
            return null;
        }
    }

    public Products.ColorFilter GetDistinctColors(List<Products.NewProduct> products) {
        var x = new Products.ColorFilter();
        x.data = new List<NewColor>();
        x.val = new NewColor();
        List<NewColor> xx = new List<NewColor>();
        if (products.Count > 0) {
            List<NewColor> cc = new List<NewColor>();
            NewColor c = new NewColor();
            foreach (var p in products) {
                if (p.color.code != null) {
                    c = new NewColor();
                    if (cc.Count == 0) {
                        cc.Add(p.color);
                    } else {
                        if (!cc.Any(a => a.code == p.color.code && a.hex == p.color.hex)) {
                            cc.Add(p.color);
                            break;
                        }
                    }
                }
            }
            x.data = cc;
        }
        return x;
    }

}
