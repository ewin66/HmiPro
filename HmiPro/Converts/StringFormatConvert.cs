using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HmiPro.Converts {
    /// <summary>
    /// 解决 Wpf 自带的 StringFormat 不支持对象为字符串
    /// <author>ychost</author>
    /// <date>2018-2-3</date>
    /// </summary>
    public class StringFormatConvert : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string) {
                return value;
            } else if (double.TryParse(value?.ToString(), out var dbValue)) {
                if (parameter is string fmt) {
                    return dbValue.ToString(fmt);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
