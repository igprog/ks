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
        public List<Global.CodeTitle> productgroups;
    }

    [WebMethod]
    public string Init() {
        NewFeature x = new NewFeature();
        x.code = null;
        x.title = null;
        x.val = null;
        x.unit = null;
        x.icon = null;
        x.faicon = null;
        x.type = G.featureType.product;
        x.order = 0;
        x.productgroups = new List<Global.CodeTitle>();
        Global.CodeTitle pg = new Global.CodeTitle();
        x.productgroups.Add(pg);
        return JsonConvert.SerializeObject(x, Formatting.None);
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
    public string GetProductFeatures(ProductGroups.NewProductGroup productGroup) {
        try {
            List<NewFeature> features = Get(G.featureType.product);
            return JsonConvert.SerializeObject(InitProductFeatures(features, productGroup), Formatting.None);
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

    public List<NewFeature> GetProductFeatures(List<NewFeature> features, string data, ProductGroups.NewProductGroup productGroup) {
        var xx = new List<NewFeature>();
        if (!string.IsNullOrEmpty(data)) {
            string[] opt = data.Split(';');
            foreach (var o in opt) {
                string[] o_ = o.Split(':');
                var po = new NewFeature();
                var po_ = features.Find(a => a.code == o_[0]);
                if (po_ != null) {
                    po.code = po_.code;
                    po.title = po_.title;
                    po.val = o_[1];
                    po.unit = po_.unit;
                    po.icon = po_.icon;
                    po.faicon = po_.faicon;
                    po.type = po_.type;
                    po.order = po_.order;
                    po.productgroups = new List<Global.CodeTitle>();
                    xx.Add(po);
                }
            }
            // ****** if not exists in data collection (new features)  *****
            foreach (var f in features) {
                if (!xx.Exists(a => a.code == f.code)) {
                    xx.Add(f);
                }
            }
            // *************************************************************
        } else {
            xx = InitProductFeatures(features, productGroup);
        }
        return xx;
    }

    public List<NewFeature> InitProductFeatures(List<NewFeature> features, ProductGroups.NewProductGroup productGroup) {
        NewFeature x = new NewFeature();
        List<NewFeature> xx = new List<NewFeature>();
        foreach (NewFeature f in features) {
            foreach(var pg in f.productgroups) {
                if (pg.code == productGroup.parent.code) {
                    x = new NewFeature();
                    x.code = f.code;
                    x.title = f.title;
                    x.unit = f.unit;
                    x.productgroups = new List<Global.CodeTitle>();
                    xx.Add(x);
                }
            }
        }
        return xx;
    }



}
