using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Features
/// </summary>
[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Features : System.Web.Services.WebService {
    Global G = new Global();
    string path = "~/data/json/features.json";
    string folder = "~/data/json/";

    public Features() {
    }

    public class NewFeature {
        public string code;
        public string title;
        public string val;
        public string unit;
        public string icon;
        public string faicon;
        public string type;
        public int order;
    }

    [WebMethod]
    public string Load(string type) {
        try {
            return JsonConvert.SerializeObject(Get(type), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(List<NewFeature> x) {
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

    protected void WriteFile(string path, List<NewFeature> value) {
        File.WriteAllText(Server.MapPath(path), JsonConvert.SerializeObject(value));
    }

    public List<NewFeature> Get(string type) {
        List<NewFeature> x = new List<NewFeature>();
        string json = ReadFile();
        if (!string.IsNullOrEmpty(json)) {
            x = JsonConvert.DeserializeObject<List<NewFeature>>(json);
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

    public List<NewFeature> GetProductFeatures(List<NewFeature> features, string data) {
        var xx = new List<NewFeature>();
        if (!string.IsNullOrEmpty(data)) {
            string[] opt = data.Split(';');
            foreach (var o in opt) {
                string[] o_ = o.Split(':');
                var po = new NewFeature();
                var po_ = features.Find(a => a.code == o_[0]);
                po.code = po_.code;
                po.title = po_.title;
                po.val = o_[1];
                po.unit = po_.unit;
                po.icon = po_.icon;
                po.faicon = po_.faicon;
                po.type = po_.type;
                po.order = po_.order;
                xx.Add(po);
            }
            // ****** if not exists in data collection (new features)  *****
            foreach (var f in features) {
                if (!xx.Exists(a => a.code == f.code)) {
                    xx.Add(f);
                }
            }
            // *************************************************************
        } else {
            xx = InitProductFeatures(features);
        }
        return xx;
    }

    public List<NewFeature> InitProductFeatures(List<NewFeature> features) {
        NewFeature x = new NewFeature();
        List<NewFeature> xx = new List<NewFeature>();
        foreach (NewFeature o in features) {
            x = new NewFeature();
            x.code = o.code;
            x.title = o.title;
            x.unit = o.unit;
            xx.Add(x);
        }
        return xx;
    }



}
