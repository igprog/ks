using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Admin
/// </summary>
[WebService(Namespace = "http://dizajn911.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Admin : System.Web.Services.WebService {
    Global G = new Global();
    public Admin() {
    }

    public class AdminType {
        public bool isLogin;
        public string adminType;
    }

    [WebMethod]
    public string Login(string username, string password) {
        try {
            AdminType x = new AdminType();
            x.isLogin = false;
            if (username == G.adminUserName && password == G.adminPassword) {
                x.isLogin = true;
                x.adminType = G.adminType.admin;
            }
            if (username == G.supervisorUserName && password == G.supervisorPassword) {
                x.isLogin = true;
                x.adminType = G.adminType.supervisor;
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch(Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }


    // Test
    [WebMethod]
    public string CreatePage(string page) {

        string template = null;
        string template_path = Server.MapPath("~/index.html");
        if (System.IO.File.Exists(template_path)) {
            template = System.IO.File.ReadAllText(template_path);
        }

        string content = @"<!DOCTYPE html>
<html>
<title>HTML Tutorial</title>
<body>

<h1>This is a heading</h1>
<p>This is a paragraph.</p>

</body>
</html>";
        string path = Server.MapPath(string.Format("~/{0}.html", page));
        if (!System.IO.Directory.Exists(path)) {
            System.IO.File.WriteAllText(path, template);

        }
        return "ok";
    }

}
