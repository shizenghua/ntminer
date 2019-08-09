﻿using NTMiner.Controllers;
using NTMiner.MinerServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner {
    public static partial class OfficialServer {
        public class FileUrlServiceFace {
            public static readonly FileUrlServiceFace Instance = new FileUrlServiceFace();
            private static readonly string SControllerName = ControllerUtil.GetControllerName<IFileUrlController>();

            private FileUrlServiceFace() { }

            #region GetNTMinerUrlAsync
            // ReSharper disable once InconsistentNaming
            public void GetNTMinerUrlAsync(string fileName, Action<string, Exception> callback) {
                NTMinerUrlRequest request = new NTMinerUrlRequest {
                    FileName = fileName
                };
                PostAsync(SControllerName, nameof(IFileUrlController.NTMinerUrl), null, request, callback);
            }
            #endregion

            #region GetNTMinerFilesAsync
            // ReSharper disable once InconsistentNaming
            public void GetNTMinerFilesAsync(NTMinerAppType appType, Action<List<NTMinerFileData>, Exception> callback) {
                PostAsync<List<NTMinerFileData>>(SControllerName, nameof(IFileUrlController.NTMinerFiles), null, null, callback: (data, e) => {
                    if (data != null) {
                        data = data.Where(a => a.AppType == appType).ToList();
                    }
                    callback?.Invoke(data, e);
                });
            }
            #endregion

            #region AddOrUpdateNTMinerFileAsync
            // ReSharper disable once InconsistentNaming
            public void AddOrUpdateNTMinerFileAsync(NTMinerFileData entity, Action<ResponseBase, Exception> callback) {
                DataRequest<NTMinerFileData> request = new DataRequest<NTMinerFileData>() {
                    Data = entity
                };
                PostAsync(SControllerName, nameof(IFileUrlController.AddOrUpdateNTMinerFile), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }
            #endregion

            #region RemoveNTMinerFileAsync
            // ReSharper disable once InconsistentNaming
            public void RemoveNTMinerFileAsync(Guid id, Action<ResponseBase, Exception> callback) {
                DataRequest<Guid> request = new DataRequest<Guid>() {
                    Data = id
                };
                PostAsync(SControllerName, nameof(IFileUrlController.RemoveNTMinerFile), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }
            #endregion

            #region GetLiteDbExplorerUrlAsync
            public void GetLiteDbExplorerUrlAsync(Action<string, Exception> callback) {
                PostAsync(SControllerName, nameof(IFileUrlController.LiteDbExplorerUrl), null, null, callback);
            }
            #endregion

            #region GetNTMinerUpdaterUrlAsync
            // ReSharper disable once InconsistentNaming
            public void GetNTMinerUpdaterUrlAsync(Action<string, Exception> callback) {
                PostAsync(SControllerName, nameof(IFileUrlController.NTMinerUpdaterUrl), null, null, callback, timeountMilliseconds: 2000);
            }
            #endregion

            #region GetPackageUrlAsync
            public void GetPackageUrlAsync(string package, Action<string, Exception> callback) {
                PackageUrlRequest request = new PackageUrlRequest {
                    Package = package
                };
                PostAsync(SControllerName, nameof(IFileUrlController.PackageUrl), null, request, callback);
            }
            #endregion
        }
    }
}
