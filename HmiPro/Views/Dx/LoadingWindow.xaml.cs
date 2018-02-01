using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;

namespace HmiPro.Views.Dx {
    /// <summary>
    /// LoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWindow : Window, ISplashScreen {
        public LoadingWindow() {
            InitializeComponent();
        }

        public void Progress(double value) {
        }

        public void SetProgressState(bool isIndeterminate) {
        }

        public void CloseSplashScreen() {
            Close();
        }

        /// <summary>
        /// 隐藏窗口
        /// <param name="fadeTimeSpan">消隐时长</param>
        /// </summary>
        public void Hide(TimeSpan fadeTimeSpan) {
            var anim = new DoubleAnimation(0, fadeTimeSpan);
            anim.Completed += (s, _) => base.Hide();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}
