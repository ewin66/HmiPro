using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;

namespace HmiPro.Views.Dx {
    /// <summary>
    /// Interaction logic for SplashScreenView.xaml
    /// </summary>
    public partial class SplashScreenView : UserControl {
        public SplashScreenView() {
            InitializeComponent();
        }
    }

    public class SplashState
    {
        public string LoadingTxt { get; set; }
        public string Copyright { get; set; }

        public static SplashState Default = new SplashState()
        {
            LoadingTxt = "加载中...",
            Copyright = "Copyright @ 2017-2017 电科智联"
        };

    }
}
