using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using HmiPro.Config;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Reducers;


namespace HmiPro {
    /// <summary>
    /// Interaction logic for DxWindow.xaml
    /// </summary>
    public partial class DxWindow : DXWindow {
        public DxWindow() {
            //设置自定义主题
            Theme theme = new Theme("HmiPro", "DevExpress.Xpf.Themes.HmiPro.v17.1");
            theme.AssemblyName = "DevExpress.Xpf.Themes.HmiPro.v17.1";
            Theme.RegisterTheme(theme);
            ThemeManager.SetTheme(this, theme);
            InitializeComponent();
            //开发电脑
            if (HmiConfig.IsDevUserEnv) {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                Topmost = false;
                //生产电脑
            } else {
                Topmost =false;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            //每一秒回收一次垃圾
            //DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            //timer.Tick += (d, e) => { GC.Collect(); };
            //timer.Start();
        }

        #region  模糊背景
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);


        internal void EnableBlur(Window window) {
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DragMove();
        }
    }
    internal enum AccentState {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }
    #endregion

}
