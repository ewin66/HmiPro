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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.LayoutControl;

namespace HmiPro.Views.Sys {
    /// <summary>
    /// FormView.xaml 的交互逻辑
    /// </summary>
    public partial class FormView : UserControl {
        public FormView() {
            InitializeComponent();
        }

        /// <summary>
        /// 让 DataLayoutControl 第一个元素聚焦
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataLayoutControl_Loaded(object sender, RoutedEventArgs e) {
            ((DataLayoutItem)LayoutHelper.FindElement(sender as FrameworkElement, elem => elem is DataLayoutItem)).Content.Focus();
        }
    }
}
