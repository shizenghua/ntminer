﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Vms {
    public class CoinSnapshotDataViewModels : ViewModelBase {
        public static readonly CoinSnapshotDataViewModels Current = new CoinSnapshotDataViewModels();

        private readonly Dictionary<string, CoinSnapshotDataViewModel> _dicByCoinCode = new Dictionary<string, CoinSnapshotDataViewModel>(StringComparer.OrdinalIgnoreCase);

        private CoinSnapshotDataViewModels() {
            foreach (var coinVm in CoinViewModels.Current.AllCoins) {
                _dicByCoinCode.Add(coinVm.Code, new CoinSnapshotDataViewModel(new MinerServer.CoinSnapshotData {
                    CoinCode = coinVm.Code,
                    MainCoinMiningCount = 0,
                    MainCoinOnlineCount = 0,
                    DualCoinMiningCount = 0,
                    DualCoinOnlineCount = 0,
                    ShareDelta = 0,
                    RejectShareDelta = 0,
                    Speed = 0,
                    Timestamp = DateTime.MinValue
                }));
            }
        }

        public bool TryGetSnapshotDataVm(string coinCode, out CoinSnapshotDataViewModel vm) {
            return _dicByCoinCode.TryGetValue(coinCode, out vm);
        }

        public List<CoinSnapshotDataViewModel> CoinSnapshotDataVms {
            get {
                return _dicByCoinCode.Values.ToList();
            }
        }
    }
}
