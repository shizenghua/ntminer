﻿using NTMiner.MinerServer;
using System;
using System.Windows.Media;

namespace NTMiner.Vms {
    public class CoinIncomeViewModel : ViewModelBase {
        private string _incomePerDaySumText;
        private string _incomeCnyPerDaySumText;
        private string _coinPriceCnyText;
        private string _incomePerDayText;
        private string _incomeCnyPerDayText;
        private readonly CoinViewModel _coinVm;
        private SpeedUnitViewModel _speedUnitVm = null;
        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _dayWaveBrush;
        private DateTime _modifiedOn;
        private string _netSpeedText;
        private string _netSpeedUnit;
        private string _dayWaveText;

        public CoinIncomeViewModel(CoinViewModel coinVm) {
            _coinVm = coinVm;
        }

        private static readonly SolidColorBrush LightRed = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x00));
        public void Refresh() {
            if (NTMinerRoot.Instance.CalcConfigSet.TryGetCalcConfig(_coinVm, out ICalcConfig calcConfig)) {
                NetSpeedText = calcConfig.NetSpeed > 0 ? calcConfig.NetSpeed.ToString() : string.Empty;
                NetSpeedUnit = calcConfig.NetSpeedUnit;
                if (calcConfig.DayWave > 0) {
                    DayWaveText = $"+{(calcConfig.DayWave * 100).ToString("f2")}%";
                    DayWaveBrush = Wpf.Util.GreenBrush;
                }
                else if (calcConfig.DayWave == 0) {
                    DayWaveText = "0%";
                    DayWaveBrush = LightRed;
                }
                else {
                    DayWaveText = $"{(calcConfig.DayWave * 100).ToString("f2")}%";
                    DayWaveBrush = Wpf.Util.RedBrush;
                }
                var incomePerDay = NTMinerRoot.Instance.CalcConfigSet.GetIncomePerHashPerDay(_coinVm.Code);
                var v = this.Speed.FromUnitSpeed(this.SpeedUnitVm.Unit) * incomePerDay.IncomeCoin;
                if (v >= 100) {
                    IncomePerDaySumText = v.ToString("f2");
                }
                else {
                    IncomePerDaySumText = v.ToString("f7");
                }
                v = 1.0.FromUnitSpeed(this.SpeedUnitVm.Unit) * incomePerDay.IncomeCoin;
                if (v >= 100) {
                    IncomePerDayText = v.ToString("f2");
                }
                else {
                    IncomePerDayText = v.ToString("f7");
                }
                IncomeCnyPerDaySumText = (this.Speed.FromUnitSpeed(this.SpeedUnitVm.Unit) * incomePerDay.IncomeCny).ToString("f2");
                IncomeCnyPerDayText = (1.0.FromUnitSpeed(this.SpeedUnitVm.Unit) * incomePerDay.IncomeCny).ToString("f2");
                CoinPriceCnyText = (incomePerDay.IncomeCny / incomePerDay.IncomeCoin).ToString("f2");
                ModifiedOn = incomePerDay.ModifiedOn;
                if (ModifiedOn.AddMinutes(15) < DateTime.Now) {
                    BackgroundBrush = LightRed;
                }
                else {
                    BackgroundBrush = Wpf.Util.TransparentBrush;
                }
                OnPropertyChanged(nameof(SpeedUnitVm));
            }
            else {
                IncomePerDaySumText = "0";
                IncomeCnyPerDaySumText = "0";
                IncomePerDayText = "0";
                IncomeCnyPerDayText = "0";
                CoinPriceCnyText = "0";
                NetSpeedText = string.Empty;
                NetSpeedUnit = string.Empty;
                DayWaveText = string.Empty;
                ModifiedOn = DateTime.MinValue;
                BackgroundBrush = Wpf.Util.RedBrush;
            }
        }

        public double Speed {
            get {
                double input = _coinVm.CoinProfile.CalcInput;
                if (input == 0) {
                    return 1;
                }
                return input;
            }
            set {
                if (_coinVm.CoinProfile.CalcInput != value) {
                    _coinVm.CoinProfile.CalcInput = value;
                    Refresh();
                    OnPropertyChanged(nameof(Speed));
                }
            }
        }


        public SpeedUnitViewModel SpeedUnitVm {
            get {
                if (_speedUnitVm == null && NTMinerRoot.Instance.CalcConfigSet.TryGetCalcConfig(_coinVm, out ICalcConfig calcConfig)) {
                    _speedUnitVm = SpeedUnitViewModel.GetSpeedUnitVm(calcConfig.SpeedUnit);
                }
                return _speedUnitVm;
            }
            set {
                if (_speedUnitVm != value) {
                    _speedUnitVm = value;
                    Refresh();
                    OnPropertyChanged(nameof(SpeedUnitVm));
                }
            }
        }

        public string IncomePerDayText {
            get {
                return _incomePerDayText;
            }
            set {
                _incomePerDayText = value;
                OnPropertyChanged(nameof(IncomePerDayText));
            }
        }

        public string IncomeCnyPerDayText {
            get {
                return _incomeCnyPerDayText;
            }
            set {
                _incomeCnyPerDayText = value;
                OnPropertyChanged(nameof(IncomeCnyPerDayText));
            }
        }

        public string IncomePerDaySumText {
            get {
                return _incomePerDaySumText;
            }
            set {
                _incomePerDaySumText = value;
                OnPropertyChanged(nameof(IncomePerDaySumText));
            }
        }

        public string IncomeCnyPerDaySumText {
            get {
                return _incomeCnyPerDaySumText;
            }
            set {
                _incomeCnyPerDaySumText = value;
                OnPropertyChanged(nameof(IncomeCnyPerDaySumText));
            }
        }

        public string CoinPriceCnyText {
            get => _coinPriceCnyText;
            set {
                _coinPriceCnyText = value;
                OnPropertyChanged(nameof(CoinPriceCnyText));
            }
        }

        public string NetSpeedText {
            get => _netSpeedText;
            set {
                if (_netSpeedText != value) {
                    _netSpeedText = value;
                    OnPropertyChanged(nameof(NetSpeedText));
                }
            }
        }

        public string NetSpeedUnit {
            get => _netSpeedUnit;
            set {
                if (_netSpeedUnit != value) {
                    _netSpeedUnit = value;
                    OnPropertyChanged(nameof(NetSpeedUnit));
                }
            }
        }
        public string DayWaveText {
            get => _dayWaveText;
            set {
                if (_dayWaveText != value) {
                    _dayWaveText = value;
                    OnPropertyChanged(nameof(DayWaveText));
                }
            }
        }

        public SolidColorBrush DayWaveBrush {
            get { return _dayWaveBrush; }
            set {
                if (_dayWaveBrush != value) {
                    _dayWaveBrush = value;
                    OnPropertyChanged(nameof(DayWaveBrush));
                }
            }
        }

        public DateTime ModifiedOn {
            get => _modifiedOn;
            set {
                _modifiedOn = value;
                OnPropertyChanged(nameof(ModifiedOn));
                OnPropertyChanged(nameof(ModifiedOnText));
            }
        }

        public string ModifiedOnText {
            get {
                return ModifiedOn.ToString("HH:mm");
            }
        }

        public SolidColorBrush BackgroundBrush {
            get => _backgroundBrush;
            set {
                _backgroundBrush = value;
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }
    }
}
