using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Effects;

namespace HmiPro.Redux.Sbuscribers {
    /// <summary>
    /// <date>2017-12-19</date>
    /// </summary>
    public class CpmSubscribers {
        private DbEffects dbEffects;
        public CpmSubscribers(DbEffects dbEffects) {
            this.dbEffects = dbEffects;

        }

        public void Init() {
            App.Store.Subscribe(s => {
                //保存机台数据
                if (s.Type == CpmActions.CPMS_UPDATED_ALL) {
                    var machineCode = s.CpmState.MachineCode;
                    var updatedCpms = s.CpmState.UpdatedCpmsAllDict[machineCode];
                    App.Store.Dispatch(
                        dbEffects.UploadCpmsInfluxDb(new DbActions.UploadCpmsInfluxDb(machineCode, updatedCpms)));
                }
            });
        }
    }
}
