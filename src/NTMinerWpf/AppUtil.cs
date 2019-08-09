﻿using NTMiner.Views;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NTMiner {
    public static class AppUtil {
        #region Init
        public static void Init(Application app) {
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => {
                if (e.ExceptionObject is Exception exception) {
                    Handle(exception);
                }
            };

            app.DispatcherUnhandledException += (object sender, DispatcherUnhandledExceptionEventArgs e) => {
                Handle(e.Exception);
                e.Handled = true;
            };

            UIThread.InitializeWithDispatcher();
            UIThread.StartTimer();
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
        }
        #endregion

        public static void RunOneceOnLoaded(this UserControl uc, Action<Window> action) {
            uc.Loaded += (sender, e) => {
                if (uc.Resources == null) {
                    uc.Resources = new ResourceDictionary();
                }
                if (uc.Resources.Contains("isNotFirstTimeLoaded")) {
                    return;
                }
                uc.Resources.Add("isNotFirstTimeLoaded", true);
                action(Window.GetWindow(uc));
            };
        }

        #region private methods
        private static void Handle(Exception e) {
            if (e == null) {
                return;
            }
            if (e is ValidationException) {
                DialogWindow.ShowDialog(title: "验证失败", message: e.Message, icon: "Icon_Error");
            }
            else {
                Logger.ErrorDebugLine(e);
            }
        }
        #endregion
    }
}
