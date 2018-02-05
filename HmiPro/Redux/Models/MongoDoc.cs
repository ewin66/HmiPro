using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace HmiPro.Redux.Models {
    /// <summary>
    /// Mongo的数据都要有Id才行
    /// <date>2017-12-23</date>
    /// <author>ychost</author>
    /// </summary>
    public class MongoDoc {
        //数据要保存在mongo中，所以必须有id才可以
        [JsonIgnore]
        public ObjectId Id { get; set; }
     

    }
}
