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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HmiPro.Config;

namespace HmiPro.Views.DMes {
    /// <summary>
    /// Interaction logic for GrafanaView.xaml
    /// </summary>
    public partial class GrafanaView : UserControl {
        public GrafanaView() {
            InitializeComponent();
            var preUrl = $"http://{HmiConfig.InfluxDbIp}:3001";
            //刷新频率同上传频率
            var refresh = $"{HmiConfig.UploadWebBoardInterval}ms";
            //var dbUpperName = HmiDynamicConfig.DbName.ToUpper();
            var viewName = MachineConfig.MachineDict.FirstOrDefault().Key.ToLower();
            var placeholder = "machine_placeholder";
            var suffixUrl = $"/dashboard/db/{placeholder}?orgId=1&kiosk";
            suffixUrl = suffixUrl.Replace(placeholder, viewName);
            this.WebBrowser.Navigate(preUrl + suffixUrl);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            this.WebBrowser.Dispose();
        }
    }
}
