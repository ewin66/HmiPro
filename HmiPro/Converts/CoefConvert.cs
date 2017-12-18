using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HmiPro.Converts {
    /// <summary>
    /// 系数转换器，比如 某个元素的宽度绑定了另一个元素，但是想乘一系数
    /// <date>2017-07-25</date>
    /// <author>ychost</author>
    /// </summary>
    public class CoefConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null || value == null) {
                return value;
            }
            double bindValue = (double)value;
            double bindCoef = double.Parse(parameter.ToString());

            return bindCoef * bindValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
