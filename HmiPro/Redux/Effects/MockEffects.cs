using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmiPro.Redux.Actions;
using HmiPro.Redux.Patches;
using HmiPro.Redux.Reducers;
using HmiPro.Redux.Services;
using Newtonsoft.Json;

namespace HmiPro.Redux.Effects {
    /// <summary>
    /// <date>2017-12-19</date>
    /// <author>ychost</author>
    /// </summary>
    public class MockEffects {
        private readonly MqService mqService;
        public StorePro<AppState>.AsyncActionNeedsParam<MockActions.MockSchTaskAccpet> MockSchTaskAccept;
        public MockEffects(MqService mqService) {
            this.mqService = mqService;
            MockSchTaskAccept =
                App.Store.asyncActionVoid<MockActions.MockSchTaskAccpet>(async (dispatch, getState, instance) => {
                    await Task.Run(() => {
                        mqService.SchTaskAccept(JsonConvert.SerializeObject(instance.SchTask));
                    });
                });
        }

    }
}
