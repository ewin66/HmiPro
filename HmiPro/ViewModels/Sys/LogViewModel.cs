using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using YCsharp.Service;

namespace HmiPro.ViewModels.Sys {
    /// <summary>
    /// 日志页面
    /// <author>ychost</author>
    /// <date>2018-4-24</date>
    /// </summary>
    [POCOViewModel]
    public class LogViewModel : IDocumentContent {
        public ObservableCollection<string> EventsLog { get; set; }
        public virtual IDispatcherService DispatcherService => null;
        Action unsubscribe;

        public LogViewModel() {
            EventsLog = new ObservableCollection<string>();

        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            unsubscribe = LoggerService.Subscribe(content => {
                DispatcherService.BeginInvoke(() => {
                    EventsLog.Add(content.Replace("\r\n", ""));
                    if (EventsLog.Count > 25)
                        EventsLog.RemoveAt(0);
                });

            });
        }

        public void OnClose(CancelEventArgs e) {
            unsubscribe?.Invoke();
        }

        public void OnDestroy() {
        }

        public IDocumentOwner DocumentOwner { get; set; }
        public object Title { get; }
    }
}