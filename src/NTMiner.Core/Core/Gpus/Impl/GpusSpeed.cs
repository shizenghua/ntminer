﻿using NTMiner.Core.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Core.Gpus.Impl {
    internal class GpusSpeed : IGpusSpeed {
        private readonly Dictionary<int, GpuSpeed> _currentGpuSpeed = new Dictionary<int, GpuSpeed>();
        private Dictionary<int, List<IGpuSpeed>> _gpuSpeedHistory = new Dictionary<int, List<IGpuSpeed>>();
        private readonly Dictionary<int, AverageSpeedWithHistory> _averageGpuSpeed = new Dictionary<int, AverageSpeedWithHistory>();
        private readonly object _gpuSpeedHistoryValuesLocker = new object();

        private readonly INTMinerRoot _root;
        public GpusSpeed(INTMinerRoot root) {
            _root = root;
            VirtualRoot.On<Per10MinuteEvent>("周期清除过期的历史算力", LogEnum.DevConsole,
                action: message => {
                    ClearOutOfDateHistory();
                });

            VirtualRoot.On<MineStopedEvent>("停止挖矿后产生一次0算力", LogEnum.DevConsole,
                action: message => {
                    var now = DateTime.Now;
                    foreach (var gpu in _root.GpuSet) {
                        SetCurrentSpeed(gpuIndex: gpu.Index, speed: 0.0, isDual: false, now: now);
                        if (message.MineContext is IDualMineContext dualMineContext) {
                            SetCurrentSpeed(gpuIndex: gpu.Index, speed: 0.0, isDual: true, now: now);
                        }
                    }
                });

            VirtualRoot.On<MineStartedEvent>("挖矿开始时产生一次0算力0份额", LogEnum.DevConsole,
                action: message => {
                    var now = DateTime.Now;
                    _root.CoinShareSet.UpdateShare(message.MineContext.MainCoin.GetId(), 0, 0, now);
                    foreach (var gpu in _root.GpuSet) {
                        SetCurrentSpeed(gpuIndex: gpu.Index, speed: 0.0, isDual: false, now: now);
                    }
                    if (message.MineContext is IDualMineContext dualMineContext) {
                        _root.CoinShareSet.UpdateShare(dualMineContext.DualCoin.GetId(), 0, 0, now);
                        foreach (var gpu in _root.GpuSet) {
                            SetCurrentSpeed(gpuIndex: gpu.Index, speed: 0.0, isDual: true, now: now);
                        }
                    }
                });
        }

        private bool _isInited = false;
        private readonly object _locker = new object();
        private void InitOnece() {
            if (!_isInited) {
                lock (_locker) {
                    if (!_isInited) {
                        DateTime now = DateTime.Now;
                        foreach (var gpu in _root.GpuSet) {
                            _currentGpuSpeed.Add(gpu.Index, new GpuSpeed(gpu, new Speed() {
                                Value = 0,
                                SpeedOn = now
                            }, new Speed() {
                                Value = 0,
                                SpeedOn = now
                            }));
                            _gpuSpeedHistory.Add(gpu.Index, new List<IGpuSpeed>());
                            _averageGpuSpeed.Add(gpu.Index, new AverageSpeedWithHistory {
                                Speed = 0,
                                DualSpeed = 0
                            });
                        }
                        _isInited = true;
                    }
                }
            }
        }

        public void ClearOutOfDateHistory() {
            InitOnece();
            DateTime now = DateTime.Now;
            lock (_gpuSpeedHistoryValuesLocker) {
                foreach (var averageSpeed in _averageGpuSpeed.Values) {
                    averageSpeed.SpeedHistory.Add(averageSpeed.Speed);
                    averageSpeed.DualSpeedHistory.Add(averageSpeed.DualSpeed);
                }
                foreach (var historyList in _gpuSpeedHistory.Values.ToArray()) {
                    var toRemoves = historyList.Where(a => a.MainCoinSpeed.SpeedOn.AddMinutes(NTMinerRoot.SpeedHistoryLengthByMinute) < now).ToArray();
                    foreach (var item in toRemoves) {
                        historyList.Remove(item);
                    }
                }
            }
        }

        public IGpuSpeed CurrentSpeed(int gpuIndex) {
            InitOnece();
            if (_currentGpuSpeed.TryGetValue(gpuIndex, out GpuSpeed gpuSpeed)) {
                return gpuSpeed;
            }
            return GpuSpeed.Empty;
        }

        public AverageSpeed GetAverageSpeed(int gpuIndex) {
            InitOnece();
            if (_averageGpuSpeed.TryGetValue(gpuIndex, out AverageSpeedWithHistory averageSpeed)) {
                if (averageSpeed.SpeedHistory.Count != 0) {
                    return new AverageSpeed() {
                        Speed = averageSpeed.SpeedHistory.Average(),
                        DualSpeed = averageSpeed.DualSpeedHistory.Average()
                    };
                }
                return averageSpeed.ToAverageSpeed();
            }
            return AverageSpeed.Empty;
        }

        private Guid _mainCoinId;
        public void SetCurrentSpeed(int gpuIndex, double speed, bool isDual, DateTime now) {
            InitOnece();
            GpuSpeed gpuSpeed;
            if (!_currentGpuSpeed.TryGetValue(gpuIndex, out gpuSpeed)) {
                return;
            }
            Guid mainCoinId = _root.MinerProfile.CoinId;
            if (this._mainCoinId != mainCoinId) {
                this._mainCoinId = mainCoinId;
                // 切换币种了，清空历史算力
                lock (_gpuSpeedHistoryValuesLocker) {
                    foreach (var item in _gpuSpeedHistory) {
                        item.Value.Clear();
                    }
                }
                // 切换币种了，将所有显卡的当前算力置为0
                foreach (var item in _currentGpuSpeed.Values) {
                    item.UpdateMainCoinSpeed(0, now);
                    item.UpdateDualCoinSpeed(0, now);
                }
                foreach (var avgSpeed in _averageGpuSpeed.Values) {
                    avgSpeed.Reset();
                }
            }
            lock (_gpuSpeedHistoryValuesLocker) {
                // 将当前的旧算力加入历史列表
                if (_gpuSpeedHistory.TryGetValue(gpuSpeed.Gpu.Index, out List<IGpuSpeed> list)) {
                    list.Add(gpuSpeed.Clone());
                }
            }
            bool isChanged = false;
            // 如果变化幅度大于等于百分之一或者距离上一次算力记录的时间超过了10分钟则视为算力变化
            if (isDual) {
                isChanged = gpuSpeed.DualCoinSpeed.SpeedOn.AddSeconds(10) < now || gpuSpeed.DualCoinSpeed.Value.IsChange(speed, 0.01);
                if (isChanged) {
                    gpuSpeed.UpdateDualCoinSpeed(speed, now);
                }
            }
            else {
                isChanged = gpuSpeed.MainCoinSpeed.SpeedOn.AddSeconds(10) < now || gpuSpeed.MainCoinSpeed.Value.IsChange(speed, 0.01);
                if (isChanged) {
                    gpuSpeed.UpdateMainCoinSpeed(speed, now);
                }
            }
            if (_averageGpuSpeed.TryGetValue(gpuIndex, out AverageSpeedWithHistory averageSpeed)) {
                if (isDual) {
                    var array = _gpuSpeedHistory[gpuIndex].Where(a => a.DualCoinSpeed.Value != 0).ToArray();
                    if (array.Length != 0) {
                        averageSpeed.DualSpeed = array.Average(a => a.DualCoinSpeed.Value);
                    }
                }
                else {
                    var array = _gpuSpeedHistory[gpuIndex].Where(a => a.MainCoinSpeed.Value != 0).ToArray();
                    if (array.Length != 0) {
                        averageSpeed.Speed = array.Average(a => a.MainCoinSpeed.Value);
                    }
                }
            }
            if (isChanged) {
                VirtualRoot.Happened(new GpuSpeedChangedEvent(isDualSpeed: isDual, gpuSpeed: gpuSpeed));
            }
        }

        public List<IGpuSpeed> GetGpuSpeedHistory(int index) {
            InitOnece();
            if (!_gpuSpeedHistory.ContainsKey(index)) {
                return new List<IGpuSpeed>();
            }
            return _gpuSpeedHistory[index];
        }

        public IEnumerator<IGpuSpeed> GetEnumerator() {
            InitOnece();
            return _currentGpuSpeed.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InitOnece();
            return _currentGpuSpeed.Values.GetEnumerator();
        }
    }
}
