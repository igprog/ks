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
        //public double price_net_tot;
        //public double grossWithDiscountTot;
        //public double discount;
        public CartPrice price;
    }

    public class CartPrice {
        public double net;
        public double vat;
        public double gross;
        public double delivery;
        public double discount;
        public double netWithDiscount;
        public double grossWithDiscount;
        public double total; // total net + vat + shipping
        //public double sub_tot;
        //public double tot_vat;
        //public double shipping;
        //public double tot; // total net + vat + shipping
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
        Products.NewProduct product = P.GetProduct(x.sku, "hr");
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
        x.price = new CartPrice();
        //x.grossWithDiscountTot = x.product.price.grossWithDiscount * x.product.qty;
        x.price.net = x.product.price.net * x.product.qty;
        x.price.gross = x.product.price.gross * x.product.qty;
        x.price.discount = x.product.price.discount * x.product.qty;
        x.price.grossWithDiscount = x.product.price.grossWithDiscount * x.product.qty;
        //x.discount = Math.Round(x.product.price.grossWithDiscountTot / x.product.discount.coeff);
        //x.price.discount = Math.Round(x.price.grossWithDiscount * x.product.discount.coeff);
        return x;
    }

    public NewCart c_CalcTotPrice (NewCart x) {
        if (x.items != null) {
            x.cartPrice = new CartPrice();
            x.cartPrice.net = x.items.Sum(a => a.price.net);
            //x.cartPrice.vat = x.cartPrice.net * 0.25;  // TODO VAT
            x.cartPrice.gross = x.items.Sum(a => a.price.gross);
            x.cartPrice.discount = x.items.Sum(a => a.price.discount);
            x.cartPrice.delivery = 100;  // TODO
            //x.cartPrice.netWithDiscount = x.cartPrice.net - x.cartPrice.discount;
            x.cartPrice.grossWithDiscount = x.cartPrice.gross - x.cartPrice.discount;
            x.cartPrice.vat = x.cartPrice.grossWithDiscount - (x.cartPrice.grossWithDiscount / 1.25);
            x.cartPrice.total = x.cartPrice.grossWithDiscount + x.cartPrice.delivery;
            //x.cartPrice.sub_tot = x.items.Sum(a => a.price_net_tot);
            //x.cartPrice.tot_vat = x.cartPrice.sub_tot * 0.25;
            //x.cartPrice.shipping = 100;  // TODO
            //x.cartPrice.tot = x.items.Sum(a => a.price_net_tot) + x.cartPrice.tot_vat + x.cartPrice.shipping;
        }
        return x;
    }
    #endregion WebMethods

}
