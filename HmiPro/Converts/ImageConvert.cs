using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DevExpress.Utils;

namespace HmiPro.Converts {
    public class ImageConvert : IValueConverter {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
                var uri = AssemblyHelper.GetResourceUri(typeof(ImageConvert).Assembly, value.ToString());
                return new System.Windows.Media.Imaging.BitmapImage(uri);
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
