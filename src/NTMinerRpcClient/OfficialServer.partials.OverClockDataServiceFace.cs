﻿using NTMiner.Controllers;
using NTMiner.MinerServer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NTMiner {
    public static partial class OfficialServer {
        public class CalcConfigServiceFace {
            public static readonly CalcConfigServiceFace Instance = new CalcConfigServiceFace();
            private static readonly string SControllerName = ControllerUtil.GetControllerName<IControlCenterController>();

            private CalcConfigServiceFace() { }

            #region GetCalcConfigsAsync
            public void GetCalcConfigsAsync(Action<List<CalcConfigData>> callback) {
                Task.Factory.StartNew(() => {
                    try {
                        CalcConfigsRequest request = new CalcConfigsRequest {
                        };
                        PostAsync(SControllerName, nameof(IControlCenterController.CalcConfigs), null, request, (DataResponse<List<CalcConfigData>> response, Exception e) => {
                            if (response.IsSuccess()) {
                                callback?.Invoke(response.Data);
                            }
                            else {
                                callback?.Invoke(new List<CalcConfigData>());
                            }
                        }, timeountMilliseconds: 10 * 1000);
                    }
                    catch (Exception e) {
                        callback?.Invoke(new List<CalcConfigData>());
                        Logger.ErrorDebugLine(e);
                    }
                });
            }
            #endregion

            #region SaveCalcConfigsAsync
            public void SaveCalcConfigsAsync(List<CalcConfigData> configs, Action<ResponseBase, Exception> callback) {
                if (configs == null || configs.Count == 0) {
                    return;
                }
                SaveCalcConfigsRequest request = new SaveCalcConfigsRequest {
                    Data = configs
                };
                PostAsync(SControllerName, nameof(IControlCenterController.SaveCalcConfigs), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }
            #endregion
        }
    }
}
