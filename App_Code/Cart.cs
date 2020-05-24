using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;


[WebService(Namespace = "http://kaminstudio.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Cart : System.Web.Services.WebService {
    Products P = new Products();

    public Cart() {
    }

    #region Class
    public class NewCart {
        public string id;
        public List<CartItem> items;
        public CartPrice cartPrice;
    }

    public class CartItem {
        public Products.NewProduct product;
        public int qty;
        public double price_net_tot;
    }

    public class CartPrice {
        public double sub_tot;
        public double tot_vat;
        public double shipping;
        public double tot; // total net + vat + shipping
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewCart x = new NewCart();
        x.id = Guid.NewGuid().ToString();
        x.items = new List<CartItem>();
        x.cartPrice = new CartPrice();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string AddToCart(NewCart cart, Products.NewProduct x) {
        //cart.price_net_tot = x.product.price.net_discount * x.qty;
        Products.NewProduct product = P.GetProduct(x.id, "hr");
        CartItem item = new CartItem();
        item.product = product;
        item.product.qty = x.qty;
        //item.qty = x.qty;
        item = c_CalcItemPrice(item); // x.price.net_discount * x.qty;
        cart.items.Add(item);
        return JsonConvert.SerializeObject(c_CalcTotPrice(cart), Formatting.None);
    }

    [WebMethod]
    public string CalcItemPrice(CartItem item) {
        return JsonConvert.SerializeObject(c_CalcItemPrice(item), Formatting.None);
    }

    [WebMethod]
    public string CalcTotPrice(NewCart cart) {
        return JsonConvert.SerializeObject(c_CalcTotPrice(cart), Formatting.None);
    }

    public CartItem c_CalcItemPrice(CartItem x) {
        x.price_net_tot = x.product.price.net_discount * x.product.qty;
        return x;
    }

    public NewCart c_CalcTotPrice (NewCart x) {
        if (x.items != null) {
            x.cartPrice = new CartPrice();
            x.cartPrice.sub_tot = x.items.Sum(a => a.price_net_tot);
            x.cartPrice.tot_vat = x.cartPrice.sub_tot * 0.25;
            x.cartPrice.shipping = 100;  // TODO
            x.cartPrice.tot = x.items.Sum(a => a.price_net_tot) + x.cartPrice.tot_vat + x.cartPrice.shipping;
        }
        return x;
    }
    #endregion WebMethods

}
