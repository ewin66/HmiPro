using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Redux.Actions;

namespace HmiPro.ViewModels.DMes {
    [POCOViewModel]
    public class GrafanaViewModel {
        public GrafanaViewModel() {

        }

        [Command(Name = "OnLoadedCommand")]
        public void OnLoaded() {
            App.Store.Dispatch(new SysActions.CloseLoadingSplash());
        }
    }
}