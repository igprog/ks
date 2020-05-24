using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Options
/// </summary>
[WebService(Namespace = "http://dizajn911.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Options : System.Web.Services.WebService {
    Global G = new Global();
    string path = "~/data/options.json";
    string folder = "~/data/";

    public Options() {
    }

    public class NewOption {
        public string code;
        public string title;
        public string desc;
        public string unit;
        public string icon;
        public string faicon;
        public string type;
        public int order;
    }

    [WebMethod]
    public string Load(string type) {
        try {
            return JsonConvert.SerializeObject(GetOptions(type), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(List<NewOption> x) {
        try {
            if (!Directory.Exists(Server.MapPath(folder))) {
                Directory.CreateDirectory(Server.MapPath(folder));
            }
            WriteFile(path, x);
            return Load(null);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    protected void WriteFile(string path, List<NewOption> value) {
        File.WriteAllText(Server.MapPath(path), JsonConvert.SerializeObject(value));
    }

    public List<NewOption> GetOptions(string type) {
        List<NewOption> x = new List<NewOption>();
        string json = ReadFile();
        if (!string.IsNullOrEmpty(json)) {
            x = JsonConvert.DeserializeObject<List<NewOption>>(json);
            if (!string.IsNullOrEmpty(type)) {
                x = x.Where(a => a.type == type).OrderBy(a => a.order).ToList();
            }
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

}
