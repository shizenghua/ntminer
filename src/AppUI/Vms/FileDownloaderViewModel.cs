﻿using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class FileDownloaderViewModel : ViewModelBase {
        private string _downloadFileUrl;
        private string _downloadMessage;
        private Action _cancel;
        private Visibility _btnCancelVisible = Visibility.Visible;

        public ICommand CancelDownload { get; private set; }

        public FileDownloaderViewModel(string downloadFileUrl, Action<bool, string, string> downloadComplete) {
            if (string.IsNullOrEmpty(downloadFileUrl)) {
                throw new InvalidProgramException();
            }
            _downloadFileUrl = downloadFileUrl;
            this.CancelDownload = new DelegateCommand(() => {
                this._cancel?.Invoke();
            });
            this.Download(downloadComplete);
        }

        public string DownloadFileUrl {
            get {
                return _downloadFileUrl;
            }
        }

        public string DownloadFileName {
            get {
                if (string.IsNullOrEmpty(_downloadFileUrl)) {
                    return string.Empty;
                }
                return Path.GetFileName(_downloadFileUrl);
            }
        }

        private double _downloadPercent;
        public double DownloadPercent {
            get {
                return _downloadPercent;
            }
            set {
                if (_downloadPercent != value) {
                    _downloadPercent = value;
                    OnPropertyChanged(nameof(DownloadPercent));
                }
            }
        }

        public string DownloadMessage {
            get {
                return _downloadMessage;
            }
            set {
                if (_downloadMessage != value) {
                    _downloadMessage = value;
                    OnPropertyChanged(nameof(DownloadMessage));
                }
            }
        }

        public Visibility BtnCancelVisible {
            get => _btnCancelVisible;
            set {
                if (_btnCancelVisible != value) {
                    _btnCancelVisible = value;
                    OnPropertyChanged(nameof(BtnCancelVisible));
                }
            }
        }

        private void Download(Action<bool, string, string> downloadComplete) {
            Logger.InfoDebugLine("下载：" + _downloadFileUrl);
            string saveFileFullName = Path.Combine(SpecialPath.DownloadDirFullName, Guid.NewGuid().ToString());
            using (WebClient webClient = new WebClient()) {
                _cancel = () => {
                    webClient.CancelAsync();
                };
                webClient.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
                    this.DownloadMessage = e.ProgressPercentage + "%";
                    this.DownloadPercent = (double)e.ProgressPercentage / 100;
                };
                webClient.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) => {
                    bool isSuccess = !e.Cancelled && e.Error == null;
                    if (isSuccess) {
                        Logger.OkDebugLine($"{_downloadFileUrl}下载成功");
                    }
                    string message = "下载成功";
                    if (e.Error != null) {
                        message = "下载失败";
                        Logger.ErrorDebugLine(e.Error.Message, e.Error);
                    }
                    if (e.Cancelled) {
                        message = "下载取消";
                    }
                    this.DownloadMessage = message;
                    this.DownloadPercent = 0;
                    this.BtnCancelVisible = Visibility.Collapsed;
                    TimeSpan.FromSeconds(2).Delay().ContinueWith((t) => {
                        UIThread.Execute(() => {
                            downloadComplete?.Invoke(isSuccess, message, saveFileFullName);
                        });
                    });
                };
                webClient.DownloadFileAsync(
                    new Uri(_downloadFileUrl),
                    saveFileFullName);
            }
        }
    }
}
