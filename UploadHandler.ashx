<%@ WebHandler Language="C#" Class="UploadHandler" %>

using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Configuration;
using Igprog;

public class UploadHandler : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string imgId = context.Request.Form["imgId"];

        string imgFolder = context.Request.Form["imgFolder"];

        if (context.Request.Files.Count > 0) {
            HttpFileCollection files = context.Request.Files;
            for (int i = 0; i < files.Count; i++) {
                HttpPostedFile file = files[i];
                //string fname = context.Server.MapPath(string.Format("~/upload/{0}/gallery/{1}", imgId, file.FileName));

                //if (string.IsNullOrEmpty(imgId) && imgFolder == "temp") {
                //    imgId = file.FileName;
                //}
                if (string.IsNullOrEmpty(imgId) && !string.IsNullOrEmpty(imgFolder)) {
                    imgId = file.FileName;
                }


                string fname = null;
                if (string.IsNullOrEmpty(imgFolder)) {
                    fname = context.Server.MapPath(string.Format("~/upload/{0}/gallery/{1}", imgId, file.FileName));
                } else {
                    //FileInfo fi = new FileInfo(file.FileName);
                    //string ext = fi.Extension;
                    //    string img = string.Format("{0}.{1}", imgId, ext);

                    fname = context.Server.MapPath(string.Format("~/upload/{0}/{1}", imgFolder, imgId));
                    //fname = context.Server.MapPath(string.Format("~/upload/{0}/{1}", imgFolder, img));
                    if (imgFolder == "datasheet") {
                        fname = context.Server.MapPath(string.Format("~/upload/{0}/{1}/{2}", imgId, imgFolder, file.FileName));
                    }
                }

                if (!string.IsNullOrEmpty(file.FileName)) {
                    string folderPath = null;
                    string versionPath = null;
                    if (string.IsNullOrEmpty(imgFolder)) {
                        folderPath = context.Server.MapPath(string.Format("~/upload/{0}/gallery", imgId));

                        /***** save Img to DB *****/
                        Products.Images img = new Products.Images();
                        img.productId = imgId;
                        img.fileName = file.FileName;
                        img.isMain = false;
                        img.imageOrder = 0;
                        Products P = new Products();
                        P.SaveImage(img);
                        /***** save Img to DB *****/

                    } else {
                        folderPath = context.Server.MapPath(string.Format("~/upload/{0}", imgFolder));
                        versionPath = context.Server.MapPath(string.Format("~/upload/{0}/version.txt", imgFolder));

                        if (imgFolder == "datasheet") {
                            folderPath = context.Server.MapPath(string.Format("~/upload/{0}/{1}", imgId, imgFolder));
                        }

                        //TODO:
                        //if (Directory.Exists(folderPath) && imgFolder == "temp") {
                        //    foreach (string f in Directory.GetFiles(folderPath)) {
                        //        File.Delete(f);
                        //    }
                        //}
                    }

                    if (!Directory.Exists(folderPath)) {
                        Directory.CreateDirectory(folderPath);
                    }
                    if (CheckGalleryLimit(folderPath)) {
                        //file.SaveAs(fname);
                        /***** Compress Image *****/
                        Global G = new Global();
                        int fileLength = file.ContentLength;
                        string folderPath_temp = context.Server.MapPath("~/upload/temp");
                        string fname_temp = context.Server.MapPath(string.Format("~/upload/temp/{0}", file.FileName));
                        string folderThumbPath = context.Server.MapPath(string.Format("~/upload/{0}/gallery/thumb", imgId));
                        if (!Directory.Exists(folderPath_temp)) {
                            Directory.CreateDirectory(folderPath_temp);
                        }
                        if (!Directory.Exists(folderThumbPath)) {
                            Directory.CreateDirectory(folderThumbPath);
                        }
                        file.SaveAs(fname_temp);
                        if (fileLength <= G.KBToByte(150)) {
                            file.SaveAs(fname);
                            G.CompressImage(fname_temp, file.FileName, folderThumbPath, G.CompressionParam(fileLength, true));
                        } else {
                            G.CompressImage(fname_temp, file.FileName, folderPath, G.CompressionParam(fileLength, false));
                            G.CompressImage(fname_temp, file.FileName, folderThumbPath, G.CompressionParam(fileLength, true));
                        }
                        if (Directory.Exists(folderPath_temp)) {
                            Directory.Delete(folderPath_temp, true);
                        }
                        /***** Compress Image *****/

                        //***** TODO: save version.txt  *****
                        //if (!string.IsNullOrEmpty(versionPath)) {
                        //    File.WriteAllText(versionPath, DateTime.Now.Ticks.ToString());
                        //}

                        context.Response.Write(string.Format("{0}?v={1}", file.FileName, DateTime.Now.Ticks));
                    } else {
                        context.Response.Write("product limit exceeded");  //TODO
                    }
                } else {
                    context.Response.Write("please choose a file to upload");
                }
            }
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

    private bool CheckGalleryLimit(string path) {
        int count = 0;
        int productsLimit = Convert.ToInt32(ConfigurationManager.AppSettings["galleryLimit"]);
        if (Directory.Exists(path)) {
            string[] ss = Directory.GetFiles(path);
            count = ss.Count();
        }
        return count <= productsLimit ? true : false;
    }

}