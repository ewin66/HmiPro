using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HmiPro.Converts {
    /// <summary>
    /// 将浮点转成百分比字符串
    /// <date>2017-12-24</date>
    /// <author>ychost</author>
    /// </summary>
    public class PercentConvert : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var val = double.Parse(value.ToString());
            return (val * 100).ToString("0.00") + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
