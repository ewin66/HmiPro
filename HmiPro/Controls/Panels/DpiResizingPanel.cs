using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace HmiPro.Controls.Panels {
    /// <summary>
    /// Dpi操作辅助
    /// <date>2018-01-04</date>
    /// <author>devexpress</author>
    /// </summary>
    public class DpiResizingPanel : ContentControl {
        const double defaultDpi = 96d;
        public DpiResizingPanel() {
            ResizeByDpi();
        }
        static double GetDpiXFactor() { return GetDpiFactor("DpiX"); }
        static double GetDpiYFactor() { return GetDpiFactor("Dpi"); }
        static double GetDpiFactor(string propName) {
            var dpiProperty = typeof(SystemParameters).GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Static);
            var dpi = (int)dpiProperty.GetValue(null, null);
            return dpi / defaultDpi;
        }
        static double CorrectDpiFactor(double factor) {
            return factor > 1.5 ? 1.5 : factor;
        }
        void ResizeByDpi() {
            if (SystemParameters.PrimaryScreenHeight > 1500 && SystemParameters.PrimaryScreenWidth > 2000)
                return;
            var dpiXFactor = CorrectDpiFactor(GetDpiXFactor());
            var dpiYFactor = CorrectDpiFactor(GetDpiYFactor());
            LayoutTransform = new ScaleTransform(1 / dpiXFactor, 1 / dpiYFactor);
            float touchScaleFactor, fontSize;
            DeviceDetector.SuggestHybridDemoParameters(out touchScaleFactor, out fontSize);
            FontSize = 12 * dpiXFactor;
        }


    }

}
