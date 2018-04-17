using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HmiPro.Converts {
    public class PictureConverter : IValueConverter {
        //object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        //    Picture picture = value as Picture;
        //    return picture == null ? null : picture.Data;
        //}
        //object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        //    byte[] data = value as byte[];
        //    return data == null ? null : new Picture() { Data = data };
        //}
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
