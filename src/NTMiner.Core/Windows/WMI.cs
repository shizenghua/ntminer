﻿using System.Collections.Generic;
using System.Management;

namespace NTMiner.Windows {
    public class WMI {
        private static bool _isFirstCall = true;
        private static bool _isWmiEnabled;
        public static bool IsWmiEnabled {
            get {
                if (!_isFirstCall) {
                    return _isWmiEnabled;
                }
                _isFirstCall = false;
                try {
                    using (new ManagementObjectSearcher("root\\CIMV2", "SELECT FreePhysicalMemory FROM Win32_OperatingSystem").Get()) {
                        Write.DevOk("WMI service seems to be running, ManagementObjectSearcher returned success.");
                        _isWmiEnabled = true;
                    }
                }
                catch {
                    Write.DevError("ManagementObjectSearcher not working need WMI service to be running");
                    _isWmiEnabled = false;
                }
                return _isWmiEnabled;
            }
        }

        public static List<string> GetCommandLines(string processName) {
            if (!IsWmiEnabled) {
                return new List<string>();
            }
            List<string> results = new List<string>();
            string wmiQuery = $"select CommandLine from Win32_Process where Name='{processName}'";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery)) {
                using (ManagementObjectCollection retObjectCollection = searcher.Get()) {
                    foreach (ManagementObject retObject in retObjectCollection) {
                        results.Add((string)retObject["CommandLine"]);
                    }
                }
            }

            return results;
        }
    }
}
