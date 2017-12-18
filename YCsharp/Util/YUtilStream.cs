using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace YCsharp.Util {
    /// <summary>
    /// 对流的操作，压缩等等
    /// <date>2017-09-27</date>
    /// </summary>
    public static partial class YUtil {
        /// <summary>
        /// 压缩字节数组
        /// </summary>
        public static byte[] Compress(byte[] inputBytes) {
            using (MemoryStream outStream = new MemoryStream()) {
                using (GZipStream zipStream = new GZipStream(outStream, CompressionMode.Compress, true)) {
                    zipStream.Write(inputBytes, 0, inputBytes.Length);
                    zipStream.Close(); //很重要，必须关闭，否则无法正确解压
                    return outStream.ToArray();
                }
            }
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        public static byte[] Decompress(byte[] inputBytes) {
            using (MemoryStream inputStream = new MemoryStream(inputBytes)) {
                using (MemoryStream outStream = new MemoryStream()) {
                    using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress)) {
                        zipStream.CopyTo(outStream);
                        zipStream.Close();
                        return outStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns>base64 字符串</returns>
        public static string CompressString(string str) {
            Encoding iso = Encoding.UTF8;
            byte[] bytes = iso.GetBytes(str);
            var compressedBytes = Compress(bytes);
            return Convert.ToBase64String(compressedBytes);
        }

        /// <summary>
        /// 解压字符串
        /// </summary>
        /// <param name="base64Str"></param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string DecompressString(string base64Str) {
            Encoding iso = Encoding.UTF8;
            var bytes = Convert.FromBase64String(base64Str);
            var decompressBytes = Decompress(bytes);
            return iso.GetString(decompressBytes);
        }

        /// <summary>
        /// 压缩对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string CompressObject(object obj) {
            string json = JsonConvert.SerializeObject(obj);
            return CompressString(json);
        }

        /// <summary>
        /// 将压缩的对象进行解压
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="base64Json"></param>
        /// <returns></returns>
        public static T DecompressObject<T>(string base64Json) {
            string json = DecompressString(base64Json);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// MD5加密字符串
        /// </summary>
        /// <returns></returns>
        public static string Md5Encrypt(Object obj) {
            string json = JsonConvert.SerializeObject(obj);
            byte[] buffer = System.Text.Encoding.Default.GetBytes(json);
            System.Security.Cryptography.MD5CryptoServiceProvider check;
            check = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] somme = check.ComputeHash(buffer);
            string ret = "";
            foreach (byte a in somme) {
                if (a < 16)
                    ret += "0" + a.ToString("X");
                else
                    ret += a.ToString("X");
            }
            return ret.ToLower();
        }

        /// <summary>  
        /// 序列化成xml 
        /// </summary>  
        /// <param name="type">类型</param>  
        /// <param name="obj">对象</param>  
        /// <param name="encoding">编码，会写在xml的首行</param>
        /// <param name="xmlNamespaces">xml 命名空间</param>
        /// <returns></returns>  
        public static string SerializeToXml(Type type, object obj, Encoding encoding, XmlSerializerNamespaces xmlNamespaces) {
            MemoryStream memStream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            //设置编码，缩进
            var xmlWriter = XmlWriter.Create(memStream, new XmlWriterSettings() { Encoding = encoding, Indent = true, IndentChars = "\t" });
            xml.Serialize(xmlWriter, obj, xmlNamespaces);
            memStream.Position = 0;
            StreamReader sr = new StreamReader(memStream, encoding);
            string str = sr.ReadToEnd();
            sr.Dispose();
            memStream.Dispose();
            return str;
        }

        /// <summary>
        /// 替换字符串中不可见字符
        /// </summary>
        /// <param name="originStr"></param>
        /// <param name="replaceStr"></param>
        /// <returns></returns>
        public static string ReplaceNoSeeStr(string originStr, string replaceStr) {
            return Regex.Replace(originStr, @"[^/x21-x7E]", replaceStr);
        }

        /// <summary>
        /// 骆驼峰字符串转下划线风格
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CamelToUnderScore(string input) {
            return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

    }
}
