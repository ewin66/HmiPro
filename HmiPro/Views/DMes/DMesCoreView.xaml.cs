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
using HmiPro.Controls;

namespace HmiPro.Views.DMes {
    /// <summary>
    /// Interaction logic for DMesCoreView.xaml
    /// </summary>
    public partial class DMesCoreView : UserControl {
        SampleSource source;

        public DMesCoreView() {
            InitializeComponent();
            //source = new SampleSource(10);
            //DataContext = source;
        }
        //private void Button_Click_1(object sender, RoutedEventArgs e) {
        //    source.Add();
        //}
        //private void Button_Click_2(object sender, RoutedEventArgs e) {
        //    source.RemoveRandom();
        //}
        //private void Button_Click_3(object sender, RoutedEventArgs e) {
        //    source.RemoveAt(3);
        //}
    }
}
