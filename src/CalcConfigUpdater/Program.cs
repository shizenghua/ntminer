﻿using NTMiner.MinerServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NTMiner {
    class Program {
        static void Main(string[] args) {
            VirtualRoot.StartTimer();
            try {
                // 将服务器地址设为localhost从而使用内网ip访问免于验证用户名密码
                AssemblyInfo.OfficialServerHost = "localhost";
                NTMinerRegistry.SetAutoBoot("NTMiner.CalcConfigUpdater", true);
                VirtualRoot.On<Per10MinuteEvent>("每10分钟更新收益计算器", LogEnum.DevConsole,
                    action: message => {
                        UpdateAsync();
                    });
                UpdateAsync();
                Write.UserInfo("输入exit并回车可以停止服务！");

                while (Console.ReadLine() != "exit") {
                }

                Write.UserOk($"服务停止成功: {DateTime.Now}.");
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
            }

            System.Threading.Thread.Sleep(1000);
        }


        private static void UpdateAsync() {
            Task.Factory.StartNew(() => {
                try {
                    var vdsUUDataTask = GetHtmlAsync("https://uupool.cn/api/getAllInfo.php");
                    var vdsZtDataTask = GetHtmlAsync("https://www.zt.com/api/v1/symbol");
                    var htmlDataTask = GetHtmlAsync("https://www.f2pool.com/");
                    byte[] htmlData = null;
                    byte[] vdsUUData = null;
                    byte[] vdsZtData = null;
                    try {
                        Task.WaitAll(new Task[] { vdsUUDataTask, vdsZtDataTask, htmlDataTask }, 30 * 1000);
                        htmlData = htmlDataTask.Result;
                        vdsUUData = vdsUUDataTask.Result;
                        vdsZtData = vdsZtDataTask.Result;
                    }
                    catch {
                    }
                    if (htmlData != null && htmlData.Length != 0) {
                        Write.UserOk($"{DateTime.Now} - 鱼池首页html获取成功");
                        string html = Encoding.UTF8.GetString(htmlData);
                        string vdsUUHtml = string.Empty;
                        string vdsZtHtml = string.Empty;
                        if (vdsUUData != null && vdsUUData.Length != 0) {
                            vdsUUHtml = Encoding.UTF8.GetString(vdsUUData);
                        }
                        if (vdsZtData != null && vdsZtData.Length != 0) {
                            vdsZtHtml = Encoding.UTF8.GetString(vdsZtData);
                        }
                        double usdCny = PickUsdCny(html);
                        Write.UserInfo($"usdCny={usdCny}");
                        List<IncomeItem> incomeItems = PickIncomeItems(html);
                        IncomeItem vdsIncomeItem = PickVDSIncomeItem(vdsUUHtml, vdsZtHtml, usdCny);
                        if (vdsIncomeItem != null && incomeItems.All(a => a.CoinCode != "VDS")) {
                            incomeItems.Add(vdsIncomeItem);
                        }
                        Write.UserInfo($"鱼池首页有{incomeItems.Count}个币种");
                        FillCny(incomeItems, usdCny);
                        NeatenSpeedUnit(incomeItems);
                        if (incomeItems != null && incomeItems.Count != 0) {
                            Login();
                            OfficialServer.CalcConfigService.GetCalcConfigsAsync(data => {
                                Write.UserInfo($"NTMiner有{data.Count}个币种");
                                HashSet<string> coinCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                foreach (CalcConfigData calcConfigData in data) {
                                    IncomeItem incomeItem = incomeItems.FirstOrDefault(a => string.Equals(a.CoinCode, calcConfigData.CoinCode, StringComparison.OrdinalIgnoreCase));
                                    if (incomeItem != null) {
                                        coinCodes.Add(calcConfigData.CoinCode);
                                        calcConfigData.Speed = incomeItem.Speed;
                                        calcConfigData.SpeedUnit = incomeItem.SpeedUnit;
                                        calcConfigData.NetSpeed = incomeItem.NetSpeed;
                                        calcConfigData.NetSpeedUnit = incomeItem.NetSpeedUnit;
                                        calcConfigData.IncomePerDay = incomeItem.IncomeCoin;
                                        calcConfigData.IncomeUsdPerDay = incomeItem.IncomeUsd;
                                        calcConfigData.IncomeCnyPerDay = incomeItem.IncomeCny;
                                        calcConfigData.ModifiedOn = DateTime.Now;
                                        if (calcConfigData.ModifiedOn.AddMinutes(15) > calcConfigData.ModifiedOn.Date.AddDays(1)) {
                                            calcConfigData.BaseNetSpeed = calcConfigData.NetSpeed;
                                        }
                                        else if(calcConfigData.BaseNetSpeed != 0) {
                                            calcConfigData.DayWave = (calcConfigData.NetSpeed - calcConfigData.BaseNetSpeed) / calcConfigData.BaseNetSpeed;
                                        }
                                    }
                                }
                                OfficialServer.CalcConfigService.SaveCalcConfigsAsync(data, callback: (res, e) => {
                                    if (!res.IsSuccess()) {
                                        Write.UserFail(res.ReadMessage(e));
                                    }
                                });
                                foreach (IncomeItem incomeItem in incomeItems) {
                                    if (coinCodes.Contains(incomeItem.CoinCode)) {
                                        continue;
                                    }
                                    Write.UserInfo(incomeItem.ToString());
                                }

                                foreach (var incomeItem in incomeItems) {
                                    if (!coinCodes.Contains(incomeItem.CoinCode)) {
                                        continue;
                                    }
                                    Write.UserOk(incomeItem.ToString());
                                }

                                Write.UserOk($"更新了{coinCodes.Count}个币种：{string.Join(",", coinCodes)}");
                                int unUpdatedCount = data.Count - coinCodes.Count;
                                Write.UserWarn($"{unUpdatedCount}个币种未更新{(unUpdatedCount == 0 ? string.Empty : "：" + string.Join(",", data.Select(a => a.CoinCode).Except(coinCodes)))}");
                            });
                        }
                    }
                }
                catch (Exception e) {
                    Logger.ErrorDebugLine(e);
                }
            });
        }

        private static void Login() {
            // 本机运行，不验证用户名密码
            SingleUser.LoginName = "CalcConfigUpdater";
            SingleUser.SetPasswordSha1("123");
            Console.WriteLine($"LoginName:CalcConfigUpdater");
            Console.Write($"Password:");
            Console.ForegroundColor = Console.BackgroundColor;
            Console.WriteLine("123");
            Console.ResetColor();
        }

        private static void FillCny(List<IncomeItem> incomeItems, double usdCny) {
            foreach (var incomeItem in incomeItems) {
                incomeItem.IncomeCny = usdCny * incomeItem.IncomeUsd;
            }
        }

        private static void NeatenSpeedUnit(List<IncomeItem> incomeItems) {
            foreach (var incomeItem in incomeItems) {
                if (!string.IsNullOrEmpty(incomeItem.SpeedUnit)) {
                    incomeItem.SpeedUnit = incomeItem.SpeedUnit.ToLower();
                    incomeItem.SpeedUnit = incomeItem.SpeedUnit.Replace("sol/s", "h/s");
                }
                if (!string.IsNullOrEmpty(incomeItem.NetSpeedUnit)) {
                    incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.ToLower();
                    incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("sol/s", "h/s");
                }
            }
        }

        private static IncomeItem PickVDSIncomeItem(string vdsUUHtml, string vdsZtHtml, double usdCny) {
            string pattern = "\"symbol\":\"vds\",.+,\"hr\":\"(?<netSpeed>[\\d\\.]+)\\s(?<netSpeedUnit>\\w+)\",\"est\":\"(?<incomeCoin>[\\d\\.]+) VDS\\\\/(?<speedUnit>\\w+)\",";
            IncomeItem result = new IncomeItem {
                CoinCode = "VDS",
                DataCode = "VDS",
                Speed = 1,
            };
            var match = Regex.Match(vdsUUHtml, pattern);
            if (match.Success) {
                string incomeCoinText = match.Groups["incomeCoin"].Value;
                result.SpeedUnit = match.Groups["speedUnit"].Value + "h/s";
                string netSpeedText = match.Groups["netSpeed"].Value;
                result.NetSpeedUnit = match.Groups["netSpeedUnit"].Value + "h/s";
                double incomeCoin;
                if (double.TryParse(incomeCoinText, out incomeCoin)) {
                    result.IncomeCoin = incomeCoin;
                }
                double netSpeed;
                if (double.TryParse(netSpeedText, out netSpeed)) {
                    result.NetSpeed = netSpeed;
                }
            }
            pattern = "\"VDS\",.+?,\"last\":\"(?<incomeCny>[\\d\\.]+)\"";
            match = Regex.Match(vdsZtHtml, pattern);
            if (match.Success) {
                string incomeCnyText = match.Groups["incomeCny"].Value;
                double incomeCny;
                if (double.TryParse(incomeCnyText, out incomeCny)) {
                    result.IncomeCny = incomeCny * result.IncomeCoin;
                    result.IncomeUsd = result.IncomeCny / usdCny;
                }
            }

            return result;
        }

        private static List<IncomeItem> PickIncomeItems(string html) {
            try {
                List<IncomeItem> results = new List<IncomeItem>();
                if (string.IsNullOrEmpty(html)) {
                    return results;
                }
                string patternFileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pattern.txt");
                if (!File.Exists(patternFileFullName)) {
                    return results;
                }
                string pattern = File.ReadAllText(patternFileFullName);
                if (string.IsNullOrEmpty(pattern)) {
                    return results;
                }
                List<int> indexList = new List<int>();
                const string splitText = "<tr class=\"row-common";
                int index = html.IndexOf(splitText);
                while (index != -1) {
                    indexList.Add(index);
                    index = html.IndexOf(splitText, index + splitText.Length);
                }
                Regex regex = new Regex(pattern, RegexOptions.Compiled);
                for (int i = 0; i < indexList.Count; i++) {
                    IncomeItem incomeItem;
                    if (i + 1 < indexList.Count) {
                        incomeItem = PickIncomeItem(regex, html.Substring(indexList[i], indexList[i + 1] - indexList[i]));
                    }
                    else {
                        string content = html.Substring(indexList[i]);
                        incomeItem = PickIncomeItem(regex, content.Substring(0, content.IndexOf("<span class=\"info-title\">币价</span>")));
                    }
                    if (incomeItem != null) {
                        results.Add(incomeItem);
                    }
                }
                return results;
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return new List<IncomeItem>();
            }
        }

        private static IncomeItem PickIncomeItem(Regex regex, string html) {
            Match match = regex.Match(html);
            if (match.Success) {
                IncomeItem incomeItem = new IncomeItem() {
                    DataCode = match.Groups["dataCode"].Value,
                    CoinCode = match.Groups["coinCode"].Value,
                    SpeedUnit = match.Groups["speedUnit"].Value,
                    NetSpeedUnit = match.Groups["netSpeedUnit"].Value,
                };
                if (incomeItem.DataCode == "grin-29") {
                    incomeItem.CoinCode = "grin";
                    incomeItem.SpeedUnit = "h/s";
                    if (incomeItem.NetSpeedUnit != null) {
                        incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("g/s", "h/s");
                    }
                }
                else if (incomeItem.DataCode == "grin-31") {
                    incomeItem.CoinCode = "grin31";
                    incomeItem.SpeedUnit = "h/s";
                    if (incomeItem.NetSpeedUnit != null) {
                        incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("g/s", "h/s");
                    }
                }
                double.TryParse(match.Groups["speed"].Value, out double speed);
                incomeItem.Speed = speed;
                double.TryParse(match.Groups["netSpeed"].Value, out double netSpeed);
                incomeItem.NetSpeed = netSpeed;
                double.TryParse(match.Groups["incomeCoin"].Value, out double incomeCoin);
                incomeItem.IncomeCoin = incomeCoin;
                double.TryParse(match.Groups["incomeUsd"].Value, out double incomeUsd);
                incomeItem.IncomeUsd = incomeUsd;
                if (incomeItem.DataCode == "ae") {
                    incomeItem.SpeedUnit = "h/s";
                    if (incomeItem.NetSpeedUnit != null) {
                        incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("g/s", "h/s");
                    }
                }
                return incomeItem;
            }
            return null;
        }

        private static double PickUsdCny(string html) {
            try {
                double result = 0;
                Regex regex = new Regex(@"CURRENCY_CONF\.usd_cny = Number\('(\d+\.?\d*)' \|\| \d+\.?\d*\);");
                var matchs = regex.Match(html);
                if (matchs.Success) {
                    double.TryParse(matchs.Groups[1].Value, out result);
                }
                return result;
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return 0;
            }
        }

        private static async Task<byte[]> GetHtmlAsync(string url) {
            try {
                using (HttpClient client = new HttpClient()) {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    return await client.GetByteArrayAsync(url);
                }
            }
            catch (Exception e) {
                Logger.ErrorDebugLine(e);
                return new byte[0];
            }
        }
    }
}
