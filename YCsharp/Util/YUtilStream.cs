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

        public static Func<double> RandGenerator = GetRandomGen();




        private static int randIndex = 0;
        /// <summary>
        /// 获取随机长度字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length) {
            const string key = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            if (length < 1)
                return string.Empty;
            byte[] buffer = new byte[8];
            var rnd = new Random(int.Parse(DateTime.Now.ToString("HHssffff") + randIndex++));
            ulong bit = 31;
            ulong result = 0;
            int index = 0;
            StringBuilder sb = new StringBuilder((length / 5 + 1) * 5);

            while (sb.Length < length) {
                rnd.NextBytes(buffer);

                buffer[5] = buffer[6] = buffer[7] = 0x00;
                result = BitConverter.ToUInt64(buffer, 0);

                while (result > 0 && sb.Length < length) {
                    index = (int)(bit & result);
                    sb.Append(key[index]);
                    result = result >> 5;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 得到随机日期
        /// </summary>
        /// <param name="time1">起始日期</param>
        /// <param name="time2">结束日期</param>
        /// <returns>间隔日期之间的 随机日期</returns>
        public static DateTime GetRandomTime(DateTime time1, DateTime time2) {
            Random random = new Random();
            DateTime minTime = new DateTime();
            DateTime maxTime = new DateTime();

            System.TimeSpan ts = new System.TimeSpan(time1.Ticks - time2.Ticks);

            // 获取两个时间相隔的秒数
            double dTotalSecontds = ts.TotalSeconds;
            int iTotalSecontds = 0;

            if (dTotalSecontds > System.Int32.MaxValue) {
                iTotalSecontds = System.Int32.MaxValue;
            } else if (dTotalSecontds < System.Int32.MinValue) {
                iTotalSecontds = System.Int32.MinValue;
            } else {
                iTotalSecontds = (int)dTotalSecontds;
            }


            if (iTotalSecontds > 0) {
                minTime = time2;
                maxTime = time1;
            } else if (iTotalSecontds < 0) {
                minTime = time1;
                maxTime = time2;
            } else {
                return time1;
            }

            int maxValue = iTotalSecontds;

            if (iTotalSecontds <= System.Int32.MinValue)
                maxValue = System.Int32.MinValue + 1;

            int i = random.Next(System.Math.Abs(maxValue));

            return minTime.AddSeconds(i);
        }


    }
}
