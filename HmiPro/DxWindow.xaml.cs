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
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using HmiPro.Config;


namespace HmiPro {
    /// <summary>
    /// Interaction logic for DxWindow.xaml
    /// </summary>
    public partial class DxWindow : DXWindow {
        public DxWindow() {
            Theme theme = new Theme("HmiPro", "DevExpress.Xpf.Themes.HmiPro.v17.1");
            theme.AssemblyName = "DevExpress.Xpf.Themes.HmiPro.v17.1";
            Theme.RegisterTheme(theme);
            ThemeManager.SetTheme(this, theme);
            InitializeComponent();
            //开发电脑
            if (HmiConfig.IsDevUserEnv) {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                //Width = 800;
                //Height = 600;
                Topmost = false;
                //生产电脑
            } else {
                Topmost = true;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            //每一秒回收一次垃圾
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (d, e) => { GC.Collect(); };
            timer.Start();
        }
    }

}
