using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Build.Framework;
using Newtonsoft.Json;

namespace YCsharp.Util {
    /// <summary>
    /// 文件工具
    /// <date>2017-09-27</date>
    /// <author>ychost</author>
    /// </summary>
    public static partial class YUtil {

        /// <summary>
        /// 获取绝对路径，主要处理.\\ 与 ..\\等相对路径
        /// 如果没有出现.\\或者..\\则直接返回原路径视作绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(string path) {
            string absolutePath = path;
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            //上层相对目录
            if (path.StartsWith("..\\")) {
                var relativePathDeepin = path.Split(new String[] { "..\\" }, StringSplitOptions.None);
                absolutePath = exePath.Substring(0, exePath.LastIndexOf("\\"));
                for (int i = 1; i < relativePathDeepin.Length; ++i) {
                    absolutePath = absolutePath.Substring(0, absolutePath.LastIndexOf("\\"));
                }
                absolutePath = absolutePath + "\\" + relativePathDeepin[relativePathDeepin.Length - 1];
                //本层相对目录
            } else if (path.StartsWith(".\\")) {
                //去除.\
                var nextPath = path.Substring(2, path.Length - 2);
                absolutePath = exePath + nextPath;
            }
            return absolutePath;
        }

        /// <summary>
        /// 将json文件变成对象
        /// 自动屏蔽了注释
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T GetJsonObjectFromFile<T>(string path) {
            string txt = ReadTxt(path);
            string json = RemoveStringComment(txt);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 从json中获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T GetJsonObjectFromStr<T>(string json) {
            json = RemoveStringComment(json);
            T config = JsonConvert.DeserializeObject<T>(json);
            return config;
        }


        /// <summary>
        /// json变成字典
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<K, V> GetJsonDictionary<K, V>(string json) {
            Dictionary<K, V> dictionary = JsonConvert.DeserializeObject<Dictionary<K, V>>(json);
            return dictionary;
        }

        /// <summary>
        /// 从路径中读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadTxt(string path) {
            StringBuilder txt = new StringBuilder();
            if (!File.Exists(path)) {
                throw new Exception("文件不存在: " + path);
            }
            System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding("utf-8"));
            string line;
            while ((line = sr.ReadLine()) != null) {
                txt.Append(line + "\n");
            }
            fs.Close();
            return txt.ToString();
        }

        /// <summary>
        /// 去除字符串中的注释
        /// http://www.jb51.net/article/114452.htm
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveStringComment(string str) {
            str = str.Trim();
            str = Regex.Replace(str, @"/\*[\s\S]*?\*/", "", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"^\s*//[\s\S]*?$", "", RegexOptions.Multiline);
            str = Regex.Replace(str, @"^\s*$\n", "", RegexOptions.Multiline);
            str = Regex.Replace(str, @"^\s*//[\s\S]*", "", RegexOptions.Multiline);
            return str;
        }

        /// <summary>
        /// Table转csv字符串
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string Dt2CsvStr(DataTable dt) {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < dt.Rows.Count; i++) {
                for (int j = 0; j < dt.Columns.Count; j++) {
                    if (j > 0)
                        stringBuilder.Append(",");
                    stringBuilder.Append(dt.Rows[i][j].ToString());

                }
                stringBuilder.Append("\r\n");
            }
            return stringBuilder.ToString();

        }

        /// <summary>
        /// 修改AssemblyInfo.cs中版本号，自动递增
        /// 主版本.次版本.递增号.日期[距离2000年的天数]
        /// 也可以使用插件[BuildVersionIncrement]，该方法是运行时修改，所以要加参数区分环境
        /// </summary>
        /// <param name="asmFilePath"></param>
        public static void SetAppVersionAutoInc(string asmFilePath) {
            StringBuilder sb = new StringBuilder();
            FileStream fsRead = new FileStream(asmFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fsRead, System.Text.Encoding.GetEncoding("utf-8"));
            string line;
            while ((line = sr.ReadLine()) != null) {
                sb.Append(line + "\r\n");
            }
            fsRead.Close(); FileStream fsWrite = new FileStream(asmFilePath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite);
            var verPattern = @"\d+(\.\d+){3}";
            var verOld = new Regex(verPattern).Match(sb.ToString());
            var ver = verOld.ToString().Split('.'); var buildTimes = int.Parse(ver[2]);
            var buildDate = DateTime.Now.Subtract(new DateTime(2000, 1, 1));
            ver[2] = (++buildTimes).ToString();
            ver[3] = ((int)buildDate.TotalDays).ToString();
            var verNew = string.Join(".", ver);
            var assemblyInfo = Regex.Replace(sb.ToString(), verPattern, verNew);
            var writeBytes = Encoding.UTF8.GetBytes(assemblyInfo);
            fsWrite.Write(writeBytes, 0, writeBytes.Length);
            fsWrite.Close();
        }

        /// <summary>
        /// 取得当前源码的哪一行
        /// </summary>
        /// <param name="skipFrame">为0表示语句所在行，为1的时候表示调用者的行数，为2的时候表示调用者的调用者的行数...如果不存在则返回0</param>
        /// <returns></returns>
        public static int GetCurCodeLineNum(int skipFrame = 1) {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(skipFrame, true);
            return st.GetFrame(0).GetFileLineNumber();
        }

        /// <summary>
        /// 取当前源码的源文件名
        /// </summary>
        /// <param name="skipFrame">为0表示语句所在行，为1的时候表示调用者的行数，为2的时候表示调用者的调用者的行数...如果不存在则返回0</param>
        /// <returns></returns>
        public static string GetCurCodeFileName(int skipFrame = 1) {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(skipFrame, true);
            return st.GetFrame(0).GetFileName();
        }

        /// <summary>
        /// 将文件转成stream
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Stream FileToStream(string fileName) {
            // 打开文件
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 转换成 Stream
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        /// <summary>
        /// 替换文件名中的不合法字符
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="replaceStr"></param>
        /// <returns></returns>
        public static string ReplaceInvalidFileName(string fileName, string replaceStr) {
            var builder = new StringBuilder(fileName);
            foreach (var invalidChar in Path.GetInvalidFileNameChars()) {
                builder.Replace(invalidChar.ToString(), replaceStr);
            }
            return builder.ToString();
        }
    }
}