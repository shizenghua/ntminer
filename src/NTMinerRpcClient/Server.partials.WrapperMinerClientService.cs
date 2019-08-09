﻿using NTMiner.Controllers;
using NTMiner.Daemon;
using NTMiner.MinerClient;
using NTMiner.MinerServer;
using System;

namespace NTMiner {
    public partial class Server {
        public class WrapperMinerClientServiceFace {
            public static readonly WrapperMinerClientServiceFace Instance = new WrapperMinerClientServiceFace();
            private static readonly string SControllerName = ControllerUtil.GetControllerName<IWrapperMinerClientController>();

            private WrapperMinerClientServiceFace() {
            }

            public void RestartWindowsAsync(IClientData client, Action<ResponseBase, Exception> callback) {
                SignRequest innerRequest = new SignRequest {
                };
                WrapperRequest<SignRequest> request = new WrapperRequest<SignRequest> {
                    ObjectId = client.GetId(),
                    ClientId = client.ClientId,
                    InnerRequest = innerRequest,
                    ClientIp = client.MinerIp
                };
                PostAsync(SControllerName, nameof(IWrapperMinerClientController.RestartWindows), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }

            public void ShutdownWindowsAsync(IClientData client, Action<ResponseBase, Exception> callback) {
                SignRequest innerRequest = new SignRequest {
                };
                WrapperRequest<SignRequest> request = new WrapperRequest<SignRequest> {
                    ObjectId = client.GetId(),
                    ClientId = client.ClientId,
                    InnerRequest = innerRequest,
                    ClientIp = client.MinerIp
                };
                PostAsync(SControllerName, nameof(IWrapperMinerClientController.ShutdownWindows), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }

            // ReSharper disable once InconsistentNaming
            public void RestartNTMinerAsync(IClientData client, Action<ResponseBase, Exception> callback) {
                WorkRequest innerRequest = new WorkRequest {
                    WorkId = client.WorkId
                };
                WrapperRequest<WorkRequest> request = new WrapperRequest<WorkRequest> {
                    ObjectId = client.GetId(),
                    ClientId = client.ClientId,
                    InnerRequest = innerRequest,
                    ClientIp = client.MinerIp
                };
                PostAsync(SControllerName, nameof(IWrapperMinerClientController.RestartNTMiner), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }

            // ReSharper disable once InconsistentNaming
            public void UpgradeNTMinerAsync(IClientData client, string ntminerFileName, Action<ResponseBase, Exception> callback) {
                UpgradeNTMinerRequest innerRequest = new UpgradeNTMinerRequest {
                    NTMinerFileName = ntminerFileName
                };
                WrapperRequest<UpgradeNTMinerRequest> request = new WrapperRequest<UpgradeNTMinerRequest> {
                    ObjectId = client.GetId(),
                    ClientId = client.ClientId,
                    InnerRequest = innerRequest,
                    ClientIp = client.MinerIp
                };
                PostAsync(SControllerName, nameof(IWrapperMinerClientController.UpgradeNTMiner), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }

            public void StartMineAsync(IClientData client, Guid workId, Action<ResponseBase, Exception> callback) {
                WorkRequest innerRequest = new WorkRequest {
                    WorkId = workId
                };
                WrapperRequest<WorkRequest> request = new WrapperRequest<WorkRequest> {
                    ObjectId = client.GetId(),
                    ClientId = client.ClientId,
                    ClientIp = client.MinerIp,
                    InnerRequest = innerRequest
                };
                PostAsync(SControllerName, nameof(IWrapperMinerClientController.StartMine), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }

            public void StopMineAsync(IClientData client, Action<ResponseBase, Exception> callback) {
                SignRequest innerRequest = new SignRequest {
                };
                WrapperRequest<SignRequest> request = new WrapperRequest<SignRequest> {
                    ObjectId = client.GetId(),
                    ClientId = client.ClientId,
                    ClientIp = client.MinerIp,
                    InnerRequest = innerRequest
                };
                PostAsync(SControllerName, nameof(IWrapperMinerClientController.StopMine), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }

            public void SetClientMinerProfilePropertyAsync(IClientData client, string propertyName, object value, Action<ResponseBase, Exception> callback) {
                SetClientMinerProfilePropertyRequest innerRequest = new SetClientMinerProfilePropertyRequest {
                    PropertyName = propertyName,
                    Value = value
                };
                WrapperRequest<SetClientMinerProfilePropertyRequest> request = new WrapperRequest<SetClientMinerProfilePropertyRequest> {
                    ObjectId = client.GetId(),
                    ClientId = client.ClientId,
                    ClientIp = client.MinerIp,
                    InnerRequest = innerRequest
                };
                PostAsync(SControllerName, nameof(IWrapperMinerClientController.SetClientMinerProfileProperty), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }
        }
    }
}
