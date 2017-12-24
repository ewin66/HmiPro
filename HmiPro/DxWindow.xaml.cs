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
using HmiPro.Config;


namespace HmiPro {
    /// <summary>
    /// Interaction logic for DxWindow.xaml
    /// </summary>
    public partial class DxWindow : DXWindow {
        public DxWindow() {
            InitializeComponent();
            //开发电脑
            if (Environment.UserName.ToLower().Contains("ychost")) {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                Width = 800;
                Height = 600;
                Topmost = false;
                //生产电脑
            } else {
                Topmost = false;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }
    }

}
