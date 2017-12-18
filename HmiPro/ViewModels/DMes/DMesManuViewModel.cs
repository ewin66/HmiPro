using System;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using YCsharp.Service;

namespace HmiPro.ViewModels.DMes {
    [POCOViewModel]
    public class DMesManuViewModel {
        public readonly StorePro<AppState> Store;
        public DMesManuViewModel() {
            Store = UnityIocService.ResolveDepend<StorePro<AppState>>();

        }
    }
}