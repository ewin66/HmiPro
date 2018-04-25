using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using HmiPro.Config;
using HmiPro.Helpers;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Models;
using HmiPro.Redux.Reducers;
using HmiPro.ViewModels.DMes.Form;
using Reducto;
using YCsharp.Util;

namespace HmiPro.ViewModels.Func {
    [POCOViewModel]
    public class WorkMgmtViewModel : IDocumentContent {
        public virtual ObservableCollection<Employee> Employees { get; set; }
        public virtual Employee SelectEmployee { get; set; }
        private Unsubscribe unsubscribe;
        readonly IDictionary<string, Action<AppState, IAction>> actionExecDict = new Dictionary<string, Action<AppState, IAction>>();
        public WorkMgmtViewModel() {
            Employees = new ObservableCollection<Employee>();
            foreach (var pair in App.Store.GetState().DMesState.MqEmpRfidDict) {
                foreach (var mqEmpRfid in pair.Value) {
                    Employee emp = new Employee() {
                        MachineCode = mqEmpRfid.macCode,
                        Rfid = mqEmpRfid.employeeCode,
                        Name = mqEmpRfid.name,
                        Photo = $"{HmiConfig.StaticServerUrl}/images/{mqEmpRfid.name}.png",
                        PrintCardTime = mqEmpRfid.PrintTime
                    };
                    Employees.Add(emp);
                }
            }
            actionExecDict[DMesActions.RFID_ACCPET] = whenRfidAccept;
            unsubscribe = App.Store.Subscribe(actionExecDict);
        }

        /// <summary>
        /// 更新上下班
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void whenRfidAccept(AppState state, IAction action) {
            var mqRfid = (DMesActions.RfidAccpet)action;
            if (mqRfid.RfidType != DMesActions.RfidType.EmpStartMachine &&
                mqRfid.RfidType != DMesActions.RfidType.EmpEndMachine) {
                return;
            }
            Application.Current.Dispatcher.Invoke(() => {
                if (mqRfid.RfidType == DMesActions.RfidType.EmpStartMachine && mqRfid.MqData != null) {
                    var mqData = (MqEmpRfid)mqRfid.MqData;
                    //重复打卡无效
                    if (Employees.Count(e => e.Rfid == mqData.employeeCode) > 0) {
                        return;
                    }
                    Employee emp = new Employee() {
                        MachineCode = mqData.macCode,
                        Name = mqData.name,
                        Photo = $"{HmiConfig.StaticServerUrl}/images/{mqData.name}.png",
                        Rfid = mqData.employeeCode,
                        PrintCardTime = mqData.PrintTime
                    };
                    Employees.Add(emp);
                } else if (mqRfid.RfidType == DMesActions.RfidType.EmpEndMachine && mqRfid.MqData != null) {
                    var mqData = (MqEmpRfid)mqRfid.MqData;
                    var emp = Employees.FirstOrDefault(e => e.Rfid == mqData.employeeCode);
                    if (emp != null) {
                        Employees.Remove(emp);
                    }
                }
                App.Logger.Info("更新上班人员，当前上班人员数量：" + Employees.Count);
            });
        }

        async void updatePhoto() {
            foreach (var employee in this.Employees) {
                var url = $"{HmiConfig.StaticServerUrl}/images/{employee.Name}.png";
                var isExist = await YUtil.CheckHttpFileExist(url);
                Application.Current.Dispatcher.Invoke(() => {
                    if (isExist) {
                        employee.Photo = url;
                    } else {
                        employee.Photo = null;
                    }
                });
            }
        }

        /// <summary>
        /// 双击头像，确认更新员工上下班状态
        /// </summary>
        [Command(Name = "ConfirmEmpStatusCommand")]
        public void ConfirmEmpStatus(object data) {
            if (data == null) {
                return;
            }
            var emp = data as Employee;
            var frm = new ConfirmEndMachine(emp.Name + $" 确认打机台 {emp.MachineCode} 下机卡？") {
                OnOkPressed = async (f) => {
                    //请求服务器相关 Rfid 的信息
                    //服务器会自动通知
                    IDictionary<string, string> dict = new Dictionary<string, string>();
                    dict["rfid"] = emp.Rfid;
                    dict["type"] = MqRfidType.EmpEndMachine;
                    dict["macCode"] = emp.MachineCode;
                    var rep = await HttpHelper.Get(HmiConfig.WebUrl + "/mes/rest/mauEmployeeManageAction/saveMauEmployeeRecord", dict);
                    if (!rep.Contains("\"success\":true")) {
                        App.Store.Dispatch(new SysActions.ShowNotification(new SysNotificationMsg() {
                            Title = "警告",
                            Content = "下机打卡失败，请稍后重试"
                        }));
                    }
                }
            };
            App.Store.Dispatch(new SysActions.ShowFormView("下机", frm, false));
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