﻿using NTMiner.Core;
using NTMiner.MinerServer;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class OverClockDataViewModel : ViewModelBase, IOverClockData, IEditableViewModel {
        private Guid _id;
        private Guid _coinId;
        private string _name;
        private GpuType _gpuType;
        private int _coreClockDelta;
        private int _memoryClockDelta;
        private int _powerCapacity;
        private int _cool;
        private int _tempLimit;
        private bool _isAutoFanSpeed;

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public OverClockDataViewModel() {
            if (!Design.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public OverClockDataViewModel(Guid id) {
            _id = id;
            this.Save = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                IOverClockData group;
                if (NTMinerRoot.Instance.OverClockDataSet.TryGetOverClockData(this.Id, out group)) {
                    VirtualRoot.Execute(new UpdateOverClockDataCommand(this));
                }
                else {
                    VirtualRoot.Execute(new AddOverClockDataCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand<FormType?>((formType) => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                VirtualRoot.Execute(new OverClockDataEditCommand(formType ?? FormType.Edit, this));
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                this.ShowDialog(message: $"您确定删除{this.Name}吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new RemoveOverClockDataCommand(this.Id));
                }, icon: IconConst.IconConfirm);
            });
        }

        public OverClockDataViewModel(IOverClockData data) : this(data.GetId()) {
            _coinId = data.CoinId;
            _name = data.Name;
            _gpuType = data.GpuType;
            _coreClockDelta = data.CoreClockDelta;
            _memoryClockDelta = data.MemoryClockDelta;
            _powerCapacity = data.PowerCapacity;
            _tempLimit = data.TempLimit;
            _cool = data.Cool;
        }

        public Guid GetId() {
            return this.Id;
        }

        public Guid Id {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public Guid CoinId {
            get => _coinId;
            set {
                _coinId = value;
                OnPropertyChanged(nameof(CoinId));
            }
        }

        public string Name {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public GpuType GpuType {
            get => _gpuType;
            set {
                _gpuType = value;
                OnPropertyChanged(nameof(GpuType));
                OnPropertyChanged(nameof(IsNvidiaIconVisible));
                OnPropertyChanged(nameof(IsAMDIconVisible));
            }
        }

        public Visibility IsNvidiaIconVisible {
            get {
                if (GpuType == GpuType.NVIDIA) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility IsAMDIconVisible {
            get {
                if (GpuType == GpuType.AMD) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public EnumItem<GpuType> GpuTypeEnumItem {
            get {
                return EnumSet.GpuTypeEnumItems.FirstOrDefault(a => a.Value == GpuType);
            }
            set {
                if (GpuType != value.Value) {
                    GpuType = value.Value;
                    OnPropertyChanged(nameof(GpuTypeEnumItem));
                }
            }
        }

        public int CoreClockDelta {
            get => _coreClockDelta;
            set {
                _coreClockDelta = value;
                OnPropertyChanged(nameof(CoreClockDelta));
                OnPropertyChanged(nameof(Tooltip));
            }
        }

        public int MemoryClockDelta {
            get => _memoryClockDelta;
            set {
                _memoryClockDelta = value;
                OnPropertyChanged(nameof(MemoryClockDelta));
                OnPropertyChanged(nameof(Tooltip));
            }
        }

        public int PowerCapacity {
            get => _powerCapacity;
            set {
                _powerCapacity = value;
                OnPropertyChanged(nameof(PowerCapacity));
                OnPropertyChanged(nameof(Tooltip));
            }
        }

        public int TempLimit {
            get => _tempLimit;
            set {
                _tempLimit = value;
                OnPropertyChanged(nameof(TempLimit));
            }
        }

        public bool IsAutoFanSpeed {
            get => _isAutoFanSpeed;
            set {
                _isAutoFanSpeed = value;
                OnPropertyChanged(nameof(IsAutoFanSpeed));
            }
        }

        public int Cool {
            get => _cool;
            set {
                _cool = value;
                OnPropertyChanged(nameof(Cool));
                OnPropertyChanged(nameof(Tooltip));
            }
        }

        private CoinViewModel _coinVm;
        public CoinViewModel CoinVm {
            get {
                if (_coinVm == null) {
                    if (!AppContext.Instance.CoinVms.TryGetCoinVm(this.CoinId, out _coinVm)) {
                        _coinVm = CoinViewModel.Empty;
                    }
                }
                return _coinVm;
            }
        }

        public string Tooltip {
            get {
                return $"核心{CoreClockDelta}M, 显存{MemoryClockDelta}M, 功耗{PowerCapacity}%, 风扇{(IsAutoFanSpeed ? "自动" : Cool + "%")}, 温度阈值{TempLimit}℃";
            }
        }
    }
}
