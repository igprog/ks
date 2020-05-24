using System;
using System.Web;
using System.Configuration;
using System.IO;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// DataBase
/// </summary>
public class DataBase {
    Global G = new Global();
    public DataBase() {
    }

    public void ProductGroups(string path) {
        string sql = @"CREATE TABLE IF NOT EXISTS productGroups
                    (id VARCHAR(50),
                    code VARCHAR(50) PRIMARY KEY,
                    title NVARCHAR(200),
                    parent VARCHAR(50),
                    pg_order INTEGER)";
        CreateTable(path, sql);
    }

    public void Brands(string path) {
        string sql = @"CREATE TABLE IF NOT EXISTS brands
                    (id VARCHAR(50),
                    code VARCHAR(50) PRIMARY KEY,
                    title NVARCHAR(200),
                    b_order INTEGER)";
        CreateTable(path, sql);
    }

    public void Products(string path) {
        string sql = @"
                CREATE TABLE IF NOT EXISTS products
                (id NVARCHAR(50),
                sku NVARCHAR(50) PRIMARY KEY,
                productgroup VARCHAR(50),
                title NVARCHAR(50),
                shortdesc NVARCHAR(50),
                longdesc NVARCHAR(200),
                brand NVARCHAR(50),
                img NVARCHAR(50),
                price VARCHAR(50),
                discount VARCHAR(50),
                stock INTEGER(50),
                isnew VARCHAR(50),
                outlet VARCHAR(50),
                bestselling VARCHAR(50),
                isactive VARCHAR(50),
                features NVARCHAR(200),
                productorder INTEGER)";
        CreateTable(path, sql);
    }

    public void Tran(string path) {
        string sql = @"CREATE TABLE IF NOT EXISTS tran
                    (id VARCHAR(50),
                    productId VARCHAR(50),
                    tran TEXT,
                    recordType NVARCHAR(50),
                    lang NVARCHAR(50))";
        CreateTable(path, sql);
    }

    public void CreateDataBase(string table) {
            try {
                string path = GetDataBasePath(G.dataBase);
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                if (!File.Exists(path)) {
                    SQLiteConnection.CreateFile(path);
                }
                CreateTables(table, path);
            } catch (Exception e) { }
        }

    public void CreateGlobalDataBase(string path, string table) {
            try {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                if (!File.Exists(path)) {
                    SQLiteConnection.CreateFile(path);
                }
                CreateTables(table, path);
            } catch (Exception e) { }
        }

    private void CreateTables(string table, string path) {
            switch (table) {
                case "productGroups":
                    ProductGroups(path);
                    break;
                case "brands":
                    Brands(path);
                    break;
                case "products":
                    Products(path);
                    break;
                case "tran":
                    Tran(path);
                    break;
            default:
                    break;
            }
        }

    private void CreateTable(string path, string sql) {
        try {
            if (File.Exists(path)) {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + path)) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            };
        }
        catch (Exception e) { }
    }

    public string GetDataBasePath(string dataBase) {
        return HttpContext.Current.Server.MapPath(string.Format("~/data/{0}", dataBase));
    }
}