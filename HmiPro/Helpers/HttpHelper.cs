using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HmiPro.Config;
using Newtonsoft.Json;

namespace HmiPro.Helpers {
    /// <summary>
    /// 主要用于 json post
    /// <author>ychost</author>
    /// <date>2018-3-26</date>
    /// </summary>
    public class HttpHelper {
        /// <summary>
        /// 向 url 提交 json 数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string Post(string url, string json) {
            try {
                var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
                http.Accept = "application/json";
                http.ContentType = "application/json";
                http.Method = "POST";
                string parsedContent = json;
                ASCIIEncoding encoding = new ASCIIEncoding();
                Byte[] bytes = encoding.GetBytes(parsedContent);
                Stream newStream = http.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                newStream.Close();
                var response = http.GetResponse();
                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();
                return content;
            } catch {
            }
            return "Error";
        }


        public static Task<string> Get(string url, IDictionary<string, string> paramDict) {
            return Task.Run(() => {
                WebClient webClient = new WebClient();
                paramDict["requestType"] = "portal";
                foreach (var pair in paramDict) {
                    webClient.QueryString.Add(pair.Key, pair.Value);
                }
                try {
                    string result = webClient.DownloadString(url);
                    return result;
                } catch (Exception e) {
                } finally {
                    webClient.Dispose();
                }
                return "Error";
            });
        }

        /// <summary>
        /// 更新 grafana 的在线数据，比如 Oee，完成率之类的
        /// </summary>
        /// <param name="dict">更新字典，key 为表中的字段名</param>
        /// <param name="machineCode"></param>
        /// <returns></returns>
        public static string UpdateGrafana(IDictionary<string, string> dict, string machineCode) {
            if (dict == null) {
                return "";
            }
            dict["mac_code"] = machineCode;
            var json = new StringBuilder(JsonConvert.SerializeObject(dict));
            try {
                //json.Replace("{", "%7B");
                //json.Replace("}", "%7D");
                return Get(HmiConfig.UpdateGrafanaUrl, json.ToString());
            } catch (Exception e) {
                return e.ToString();
            }
        }

        public static string Get(string url, string json) {
            using (WebClient client = new WebClient()) {
                StringBuilder builder = new StringBuilder(url + "?requestType=mapuser&beans=" + json);
                Console.WriteLine(builder);
                return Encoding.UTF8.GetString(client.DownloadData(builder.ToString()));
            }
        }

    }
}
