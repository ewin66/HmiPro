using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using HmiPro.Redux.Models;
using YCsharp.Util;

namespace HmiPro.Converts {
    /// <summary>
    /// JavaTime 类转可视化时间
    /// <author>ychost</author>
    /// <date>2018-1-22</date>
    /// </summary>
    public class JavaTimeConvert : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return "\\";
            }
            try {
                var time = (JavaTime)value;
                var dateTime = YUtil.UtcTimestampToLocalTime(time.time);
                var format = "yyyy-MM-dd HH:mm:ss";
                if (parameter != null && parameter is string formatParam) {
                    format = formatParam;
                }
                return dateTime.ToString(format);
            } catch {
                return "\\";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
