using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace VkApi.Utility
{
    public class PostData
    {
        public string Method { get; } = string.Empty;
        public string Action { get; } = string.Empty;
        public string Param { get; } = string.Empty;

        public PostData(string sPostString)
        {
            if (sPostString.IndexOf("=") == -1) return;

            Method = sPostString.Substring(0, sPostString.IndexOf("="));
            Action = sPostString.Substring(sPostString.IndexOf("=") + 1);
            if (Action.IndexOf("!") != -1)
            {
                Action = Action.Substring(0, Action.IndexOf("!")); Param = sPostString.Substring(sPostString.IndexOf("!") + 1);
            }
        }

        public static string MultiFormData(string key, string value, string boundary)
        {
            var output = "--" + boundary + "\r\n";
            output += "Content-Disposition: form-data; name=\"" + key + "\"\r\n\r\n";
            output += value + "\r\n";
            return output;
        }
        public static string MultiFormDataFile(string key, string value, string fileName, string fileType, string boundary)
        {
            var output = "--" + boundary + "\r\n";
            output += "Content-Disposition: form-data; name=\"" + key
                      + "\"; filename=\"" + fileName
                      + "\"\r\n";
            output += "Content-Type: " + fileType + " \r\n\r\n";
            output += value + "\r\n";
            return output;
        }
    }

    public class AntiCaptcha
    {
        public string Key;
        public string Filename;
        public int Phrase = 0;
        public int IsRussian = 0;
        public int Numeric = 0;
        public int Calc = 0;
        public int Regsense = 0;
        public int MinLen = 0;
        public int MaxLen = 0;
        public string SoftId = "";

        public AntiCaptcha(string key, string filename)
        {
            Key = key;
            Filename = filename;
        }

        public string GetPage(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var responseReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                {
                    var res = responseReader.ReadToEnd();
                    return res;
                }
            }
        }

        /*public static byte[] imageToBytes(Image imageIn, ImageFormat imgFormat)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imgFormat);
                return ms.ToArray();
            }
        }*/

        /*public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }*/

        /*public static string ImageToBase64(Image image, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                var imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                var base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }*/

        /*public static byte[] Base64ToBytes(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            return imageBytes;
        }*/

        /*public static Image bytesToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }*/

        public string ToMIME(string extension)
        {
            switch (extension.ToLower().Replace(".", ""))
            {
                case "jpg": return "image/jpg";
                case "jpeg": return "image/jpeg";
                case "gif": return "image/gif";
                case "png": return "image/png";
                case "bmp": return "image/bmp";
                case "tiff": return "image/tiff";
                default: return extension;
            }
        }

        /*public static ImageFormat DetectImageFormat(string fmt)
        {
            switch (fmt.ToLower().Replace(".", ""))
            {
                case "image/jpg": return ImageFormat.Jpeg;
                case "image/jpeg": return ImageFormat.Jpeg;
                case "image/gif": return ImageFormat.Gif;
                case "image/png": return ImageFormat.Png;
                case "image/bmp": return ImageFormat.Bmp;
                case "image/tiff": return ImageFormat.Tiff;
                case "jpg": return ImageFormat.Jpeg;
                case "jpeg": return ImageFormat.Jpeg;
                case "gif": return ImageFormat.Gif;
                case "png": return ImageFormat.Png;
                case "bmp": return ImageFormat.Bmp;
                case "tiff": return ImageFormat.Tiff;
                default: return null;
            }
        }*/

        public ImageFormat DetectImageFormat(Image image)
        {
            if (ImageFormat.Jpeg.Equals(image.RawFormat))
            {
                return ImageFormat.Jpeg;
            }
            if (ImageFormat.Png.Equals(image.RawFormat))
            {
                return ImageFormat.Png;
            }
            if (ImageFormat.Gif.Equals(image.RawFormat))
            {
                return ImageFormat.Gif;
            }
            if (ImageFormat.Bmp.Equals(image.RawFormat))
            {
                return ImageFormat.Bmp;
            }
            if (ImageFormat.Tiff.Equals(image.RawFormat))
            {
                return ImageFormat.Tiff;
            }
            return ImageFormat.Jpeg;
        }

        public string GetExtension(byte[] imageBytes)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                var image = Image.FromStream(ms);
                var fmt = DetectImageFormat(image);
                var ext = fmt.ToString().ToLower().Replace("jpeg", "jpg");
                return ext;
            }
        }

        /*public byte[] DownloadImage(string imgURL, out ImageFormat imageFormat)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(imgURL);
            req.UserAgent =
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.04506.590; .NET CLR 3.5.20706)";
            req.Headers.Add("Accept-Language", "ru");
            req.Proxy = new WebProxy();
            req.KeepAlive = true;
            req.AllowAutoRedirect = false;
            req.Method = "GET";

            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream stream = resp.GetResponseStream())
                {
                    Image image = Image.FromStream(stream);
                    imageFormat = DetectImageFormat(image);
                    byte[] imgbytes = imageToBytes(image, imageFormat);
                    return imgbytes;
                }
            }
        }*/

        /*public byte[] DownloadImage(string imgURL)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(imgURL);
            req.Proxy = new WebProxy();
            req.AllowAutoRedirect = true;
            req.Method = "GET";
            req.UserAgent =
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.04506.590; .NET CLR 3.5.20706)";

            using (var resp = (HttpWebResponse)req.GetResponse())
            {
                using (var stream = resp.GetResponseStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        if (stream != null) stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
        }*/

        public string Upload(byte[] imgbytes)
        {
            var acURL = "http://rucaptcha.com/in.php";

            var ext = GetExtension(imgbytes);

            var filename = $"captcha.{ext}";

            var sBoundary = DateTime.Now.Ticks.ToString("x");

            var req = (HttpWebRequest)WebRequest.Create(acURL);
            req.UserAgent = "Mozilla";
            req.Accept = "*/*";
            req.Headers.Add("Accept-Language", "ru");
            req.Proxy = new WebProxy();
            req.KeepAlive = true;
            req.AllowAutoRedirect = false;
            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + sBoundary;

            var sPostMultiString = new StringBuilder();
            sPostMultiString.Append(PostData.MultiFormData("method", "post", sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("key", Key, sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("file", filename, sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("calc", Calc.ToString(), sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("numeric", Numeric.ToString(), sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("phrase", Phrase.ToString(), sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("minlen", MinLen.ToString(), sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("maxlen", MaxLen.ToString(), sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("is_russian", IsRussian.ToString(), sBoundary));
            sPostMultiString.Append(PostData.MultiFormData("soft_id", SoftId, sBoundary));

            var sFileContent = Encoding.Default.GetString(imgbytes);

            sPostMultiString.Append(PostData.MultiFormDataFile("file", sFileContent, filename, ToMIME(ext), sBoundary));
            sPostMultiString.Append($"--{sBoundary}--\r\n\r\n");

            var byteArray = Encoding.Default.GetBytes(sPostMultiString.ToString());
            req.ContentLength = byteArray.Length;
            req.GetRequestStream().Write(byteArray, 0, byteArray.Length);

            using (var response = (HttpWebResponse)req.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.Default))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /*public string UploadImage(Image image)
        {
            return Upload(imageToBytes(image, DetectImageFormat(image)));
        }*/

        public string UploadFile()
        {
            return Upload(File.ReadAllBytes(Filename));
        }

        /*public string UploadURL(string imgURL)
        {
            return Upload(DownloadImage(imgURL));
        }*/

        /*public string UploadBase64(byte[] imgbytes)
        {
            StringBuilder sPostString = new StringBuilder();
            sPostString.Append("method=base64");
            sPostString.Append(String.Format("&key={0}", key));
            sPostString.Append(String.Format("&is_russian={0}", is_russian));
            sPostString.Append(String.Format("&phrase={0}", phrase));
            sPostString.Append(String.Format("&regsense={0}", regsense));
            sPostString.Append(String.Format("&numeric={0}", numeric));
            sPostString.Append(String.Format("&calc={0}", calc));
            sPostString.Append(String.Format("&min_len={0}", min_len));
            sPostString.Append(String.Format("&max_len={0}", max_len));
            sPostString.Append(String.Format("&soft_id={0}", soft_id));

            String ext = GetExtension(imgbytes);

            sPostString.Append(String.Format("&ext={0}", ext));
            sPostString.Append(String.Format("&body={0}", Convert.ToBase64String(imgbytes)));

            string postData = sPostString.ToString();

            byte[] postContentData = Encoding.GetEncoding("windows-1251").GetBytes(postData);
            if (postContentData != null && postContentData.Length > 0)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://2captcha.com/in.php");
                req.Proxy = new WebProxy();
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";

                using (Stream stream = req.GetRequestStream())
                {
                    stream.Write(postContentData, 0, postContentData.Length);
                    stream.CloseDb();
                }

                using (var response = (HttpWebResponse)req.GetResponse())
                {
                    using (
                        StreamReader responseReader = new StreamReader(response.GetResponseStream(),
                                                                       Encoding.GetEncoding("windows-1251")))
                    {
                        return responseReader.ReadToEnd();
                    }
                }
            }
            return "ERROR_IMAGE_UPLOADING";
        }*/

        /*public string UploadImageBase64(Image image)
        {
            return UploadBase64(imageToBytes(image, DetectImageFormat(image)));
        }*/

        /*public string UploadFileBase64(string filename)
        {
            return UploadBase64(File.ReadAllBytes(filename));
        }*/

        /*public string UploadURLBase64(string url)
        {
            return UploadBase64(DownloadImage(url));
        }*/

        /*public string get_balance()
        {
            return GetPage(String.Format("http://2captcha.com/res.php?key={0}&action=getbalance", key));
        }*/

        /*public string get(long capid)
        {
            return GetPage(String.Format("http://2captcha.com/res.php?key={0}&action=get&id={1}", key, capid));
        }*/

        /*public void reportbad(long capid)
        {
            GetPage(String.Format("http://2captcha.com/res.php?key={0}&action=reportbad&id={1}", key, capid));
        }*/

        public string Recognize(string id)
        {
            var url = $"http://rucaptcha.com/res.php?key={Key}&action=get&id={id}";
            var result = "CAPCHA_NOT_READY";

            while (result == "CAPCHA_NOT_READY")
            {
                Thread.Sleep(3000);
                result = GetPage(url);
            }

            return result;
        }
    }
}
