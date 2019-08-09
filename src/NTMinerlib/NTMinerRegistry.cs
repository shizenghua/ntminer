﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner {
    public static class NTMinerRegistry {
        /// <summary>
        /// 将当前程序设置为windows开机自动启动
        /// </summary>
        /// <param name="valueName"></param>
        /// <param name="isAutoBoot"></param>
        /// <param name="otherParams"></param>
        public static void SetAutoBoot(string valueName, bool isAutoBoot, string otherParams = null) {
            const string AutoRunSubKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
            if (isAutoBoot == true) {
                string value = VirtualRoot.AppFileFullName;
                if (!string.IsNullOrEmpty(otherParams)) {
                    value = value + " " + otherParams;
                }
                Windows.WinRegistry.SetValue(Registry.CurrentUser, AutoRunSubKey, valueName, value);
            }
            else {
                Windows.WinRegistry.DeleteValue(Registry.CurrentUser, AutoRunSubKey, valueName);
            }
        }

        public const string NTMinerRegistrySubKey = @".DEFAULT\Software\NTMiner";

        // 下面这些项是可能需要交换到下层系统从而完成不同进程间信息交换的项
        // 注册表就属于下层系统，文件系统也属于下层系统，具体使用什么方案交换到时候再定

        private const string MinerStudio = "MinerStudio";
        #region Location
        public static string GetLocation() {
            string valueName = "Location";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "Location";
            }
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, valueName);
            if (value != null) {
                return (string)value;
            }
            return string.Empty;
        }

        public static void SetLocation(string location) {
            string valueName = "Location";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "Location";
            }
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, valueName, location);
        }
        #endregion

        #region IsLastIsWork
        public static bool GetIsLastIsWork() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "IsLastIsWork");
            return value != null && value.ToString() == "True";
        }

        public static void SetIsLastIsWork(bool value) {
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "IsLastIsWork", value);
        }
        #endregion

        #region Arguments
        public static string GetArguments() {
            string valueName = "Arguments";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "Arguments";
            }
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, valueName);
            if (value != null) {
                return (string)value;
            }
            return string.Empty;
        }

        public static void SetArguments(string arguments) {
            string valueName = "Arguments";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "Arguments";
            }
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, valueName, arguments);
        }
        #endregion

        #region IsAutoBoot
        public static bool GetIsAutoBoot() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "IsAutoBoot");
            return value == null || value.ToString() == "True";
        }

        public static void SetIsAutoBoot(bool value) {
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "IsAutoBoot", value);
        }
        #endregion

        #region IsAutoStart
        public static bool GetIsAutoStart() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistry.NTMinerRegistrySubKey, "IsAutoStart");
            return value != null && value.ToString() == "True";
        }

        public static void SetIsAutoStart(bool value) {
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistry.NTMinerRegistrySubKey, "IsAutoStart", value);
        }
        #endregion

        #region IsAutoDisableWindowsFirewall
        public static bool GetIsAutoDisableWindowsFirewall() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistry.NTMinerRegistrySubKey, "IsAutoDisableWindowsFirewall");
            return value == null || value.ToString() == "True";
        }

        public static void SetIsAutoDisableWindowsFirewall(bool value) {
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistry.NTMinerRegistrySubKey, "IsAutoDisableWindowsFirewall", value);
        }
        #endregion

        #region CurrentVersion
        public static string GetCurrentVersion() {
            string valueName = "CurrentVersion";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "CurrentVersion";
            }
            string currentVersion = "1.0.0.0";
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, valueName);
            if (value != null) {
                currentVersion = (string)value;
            }
            if (string.IsNullOrEmpty(currentVersion)) {
                return "1.0.0.0";
            }
            return currentVersion;
        }

        public static void SetCurrentVersion(string version) {
            string valueName = "CurrentVersion";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "CurrentVersion";
            }
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, valueName, version);
        }
        #endregion

        #region CurrentVersionTag
        public static string GetCurrentVersionTag() {
            string valueName = "CurrentVersionTag";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "CurrentVersionTag";
            }
            string currentVersionTag = string.Empty;
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, valueName);
            if (value != null) {
                currentVersionTag = (string)value;
            }
            return currentVersionTag;
        }

        public static void SetCurrentVersionTag(string versionTag) {
            string valueName = "CurrentVersionTag";
            if (VirtualRoot.IsMinerStudio) {
                valueName = MinerStudio + "CurrentVersionTag";
            }
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, valueName, versionTag);
        }
        #endregion

        #region ControlCenterHost
        public const string DefaultControlCenterHost = "localhost";
        public static string GetControlCenterHost() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "ControlCenterHost");
            if (value == null) {
                return DefaultControlCenterHost;
            }
            return (string)value;
        }

        public static void SetControlCenterHost(string host) {
            if (string.IsNullOrEmpty(host)) {
                host = DefaultControlCenterHost;
            }
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "ControlCenterHost", host);
        }
        #endregion

        #region ControlCenterHosts
        public static List<string> GetControlCenterHosts() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "ControlCenterHosts");
            if (value == null) {
                return new List<string>();
            }
            return value.ToString().Split(',').ToList();
        }

        public static void SetControlCenterHosts(List<string> hosts) {
            string value = string.Empty;
            if (hosts != null) {
                value = string.Join(",", hosts);
            }
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "ControlCenterHosts", value);
        }
        #endregion

        #region DaemonVersion
        public static string GetDaemonVersion() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "DaemonVersion");
            if (value == null) {
                return string.Empty;
            }
            return (string)value;
        }

        public static void SetDaemonVersion(string version) {
            if (version == null) {
                version = string.Empty;
            }
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "DaemonVersion", version);
        }
        #endregion

        #region DaemonActiveOn
        public static DateTime GetDaemonActiveOn() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "DaemonActiveOn");
            if (value == null) {
                return DateTime.MinValue;
            }
            string str = value.ToString();
            if (!DateTime.TryParse(str, out DateTime dateTime)) {
                return DateTime.MinValue;
            }
            return dateTime;
        }

        public static void SetDaemonActiveOn(DateTime version) {
            Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "DaemonActiveOn", version.ToString());
        }
        #endregion

        #region GetClientId
        public static Guid GetClientId() {
            Guid id;
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "ClientId");
            if (value == null) {
                id = Guid.NewGuid();
                Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "ClientId", id.ToString());
            }
            else if (!Guid.TryParse((string)value, out id)) {
                id = Guid.NewGuid();
                Windows.WinRegistry.SetValue(Registry.Users, NTMinerRegistrySubKey, "ClientId", id.ToString());
            }
            return id;
        }
        #endregion

        #region GetIndexHtmlFileFullName
        public static string GetIndexHtmlFileFullName() {
            object value = Windows.WinRegistry.GetValue(Registry.Users, NTMinerRegistrySubKey, "IndexHtmlFileFullName");
            if (value == null) {
                return string.Empty;
            }
            return (string)value;
        }
        #endregion
    }
}
