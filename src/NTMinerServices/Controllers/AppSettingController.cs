﻿using NTMiner.MinerServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace NTMiner.Controllers {
    public class AppSettingController : ApiControllerBase, IAppSettingController {
        public DateTime GetTime() {
            return DateTime.Now;
        }

        [HttpPost]
        public string GetJsonFileVersion(AppSettingRequest request) {
            string jsonVersion = string.Empty;
            string minerClientVersion = string.Empty;
            try {
                var fileData = HostRoot.Instance.NTMinerFileSet.LatestMinerClientFile;
                minerClientVersion = fileData != null ? fileData.Version : string.Empty;
                if (!HostRoot.Instance.AppSettingSet.TryGetAppSetting(request.Key, out IAppSetting data) || data.Value == null) {
                    jsonVersion = string.Empty;
                }
                else {
                    jsonVersion = data.Value.ToString();
                }
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
            }
            return $"{jsonVersion}|{minerClientVersion}";
        }

        [HttpPost]
        public DataResponse<AppSettingData> AppSetting([FromBody]AppSettingRequest request) {
            try {
                if (!HostRoot.Instance.AppSettingSet.TryGetAppSetting(request.Key, out IAppSetting data)) {
                    data = null;
                }
                return DataResponse<AppSettingData>.Ok(AppSettingData.Create(data));
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError<DataResponse<AppSettingData>>(e.Message);
            }
        }

        [HttpPost]
        public DataResponse<List<AppSettingData>> AppSettings([FromBody]AppSettingsRequest request) {
            try {
                var data = HostRoot.Instance.AppSettingSet;
                return DataResponse<List<AppSettingData>>.Ok(data.Select(a => AppSettingData.Create(a)).ToList());
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError<DataResponse<List<AppSettingData>>>(e.Message);
            }
        }

        [HttpPost]
        public ResponseBase SetAppSetting([FromBody]DataRequest<AppSettingData> request) {
            if (request == null || request.Data == null) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                if (!request.IsValid(User, Sign, Timestamp, ClientIp, out ResponseBase response)) {
                    return response;
                }
                VirtualRoot.Execute(new ChangeLocalAppSettingCommand(request.Data));
                Logger.InfoDebugLine($"{nameof(SetAppSetting)}({request.Data.Key}, {request.Data.Value})");
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }

        [HttpPost]
        public ResponseBase SetAppSettings([FromBody]DataRequest<List<AppSettingData>> request) {
            if (request == null || request.Data == null) {
                return ResponseBase.InvalidInput("参数错误");
            }
            try {
                if (!request.IsValid(User, Sign, Timestamp, ClientIp, out ResponseBase response)) {
                    return response;
                }
                foreach (var item in request.Data) {
                    VirtualRoot.Execute(new ChangeLocalAppSettingCommand(item));
                }
                Logger.InfoDebugLine($"{nameof(SetAppSettings)} {string.Join(",", request.Data.Select(a => $"{a.Key}:{a.Value}"))}");
                return ResponseBase.Ok();
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return ResponseBase.ServerError(e.Message);
            }
        }
    }
}
