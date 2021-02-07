using System;
using System.Web;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;

/// <summary>
/// Global
/// </summary>
namespace Igprog {
    public class Global {
        public Global() {
        }

        //public string productGroups = "productGroups";
        //public string products = "products";
        public DB db = new DB();

        public string siteName = ConfigurationManager.AppSettings["siteName"];
        public string siteUrl = ConfigurationManager.AppSettings["siteUrl"];
        public string myEmail = ConfigurationManager.AppSettings["myEmail"];
        public string myEmailName = ConfigurationManager.AppSettings["myEmailName"];
        public string myPassword = ConfigurationManager.AppSettings["myPassword"];
        public int myServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort"]);
        public string myServerHost = ConfigurationManager.AppSettings["myServerHost"];
        public string email = ConfigurationManager.AppSettings["email"];
        public string adminUserName = ConfigurationManager.AppSettings["adminUserName"];
        public string adminPassword = ConfigurationManager.AppSettings["adminPassword"];
        public string supervisorUserName = ConfigurationManager.AppSettings["supervisorUserName"];
        public string supervisorPassword = ConfigurationManager.AppSettings["supervisorPassword"];
        public string dataBase = ConfigurationManager.AppSettings["dataBase"];

        public RecordType recordType = new RecordType();
        //public OptionType optionType = new OptionType();
        public FeatureType featureType = new FeatureType();
        public UserTypes userTypes = new UserTypes();
        public AdminType adminType = new AdminType();
        

        public class DB {
            public string productGroups = "productGroups";
            public string brands = "brands";
            public string products = "products";
            public string review = "review";
            public string users = "users";
            public string orders = "orders";
            public string banners = "banners";
            public string subscribe = "subscribe";
            public string tran = "tran";
            public string images = "images";
        }

        public class RecordType {
            public string services = "services";
            public string about = "about";
            public string productTitle = "productTitle";
            public string productShortDesc = "productShortDesc";
            public string productLongDesc = "productLongDesc";
        }

        public class UserTypes {
            public string legal = "legal";
            public string natural = "natural";
        }

        //public class OptionType {
        //    public string services = "services";
        //    public string product = "product";
        //}
        public class FeatureType {
            public string product = "p";
        }

        public class AdminType {
            public string admin = "admin";
            public string supervisor = "supervisor";
        }

        public class CodeTitle {
            public string code;
            public string title;
        }

        public string ReadS(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? null : reader.GetString(i);
        }

        public int ReadI(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? 0 : reader.GetInt32(i);
        }

        public double ReadD(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(i));
        }

        public bool ReadB(SQLiteDataReader reader, int i) {
            return reader.GetValue(i) == DBNull.Value ? false : Convert.ToBoolean(reader.GetString(i));
        }

        public string GetSeoTitle(string title) {
            return !string.IsNullOrEmpty(title) ? title.Replace(" ", "-").Replace("č", "c").Replace("ć", "c").Replace("š", "s").Replace("ž", "z").Replace("đ", "d").Trim().ToLower() : null;
        }

         #region ImageCompress
        public static Bitmap GetBitmap(string path) {
            Bitmap source = null;
            try {
                //byte[] imageData = System.IO.File.ReadAllBytes(path);
                //System.IO.MemoryStream stream = new System.IO.MemoryStream(imageData, false);
                //source = new Bitmap(stream);
                //source = new Bitmap(HttpContext.Current.Server.MapPath(path));
                source = new Bitmap(path);
            } catch (Exception e) {
                string err = e.Message;
            }
            return source;
        }

        //To change the compression, change the compression parameter to a value between 0 and 100. The higher number being higher quality and less compression.
        public void CompressImage(string fullFilePath, string fileName, string destinationFolder, long compression) {
            Bitmap bitmap = GetBitmap(fullFilePath);

            using (bitmap) {
                if (bitmap == null) {
                    return;
                }

                bool encoderFound = false;
                System.Drawing.Imaging.ImageCodecInfo encoder = null;

                if (fileName.ToLower().EndsWith(".jpg") || fileName.ToLower().EndsWith(".jpeg")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                } else if (fileName.ToLower().EndsWith(".bmp")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Bmp);
                } else if (fileName.ToLower().EndsWith(".tiff")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Tiff);
                } else if (fileName.ToLower().EndsWith(".gif")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Gif);
                } else if (fileName.ToLower().EndsWith(".png")) {
                    encoderFound = true;
                    encoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Png);
                }

                if (encoderFound) {
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                    System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, compression);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    string path = HttpContext.Current.Server.MapPath(string.Format("~/upload/"));
                    bitmap.Save(System.IO.Path.Combine(destinationFolder, fileName), encoder, myEncoderParameters);
                } else {
                    bitmap.Save(System.IO.Path.Combine(destinationFolder, fileName));
                }
            }
        }

        public static ImageCodecInfo GetEncoder(string mimeType) {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (int x = 0; x < encoders.Length; x++) {
                if (encoders[x].MimeType == mimeType) {
                    return encoders[x];
                }
            }
            return null;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs) {
                if (codec.FormatID == format.Guid) {
                    return codec;
                }
            }
            return null;
        }

        public int CompressionParam(int fileLength, bool isThumb) {
            //TODO
            /***** higher fileLength higher quality and less compression *****/
            int x = 0;
            if (fileLength < 200000) {
                x = 80;
            } else if (fileLength > 200000 && fileLength <= 300000) {
                x = 70;
            } else if (fileLength > 300000 && fileLength <= 500000) {
                x = 60;
            } else if (fileLength > 500000 && fileLength <= 800000) {
                x = 50;
            } else if (fileLength > 800000 && fileLength <= 1000000) {
                x = 45;
            } else if (fileLength > 1000000 && fileLength <= 1500000) {
                x = 40;
            }else if (fileLength > 1500000 && fileLength <= 2000000) {
                x = 35;
            } else {
                x = 30;
            }
            if (isThumb) {
                x = 10 ;
            }
            return x;
        }

        public int KBToByte(int x) {
            return x * 1024;
        }
        #endregion ImageCompress
    }
}