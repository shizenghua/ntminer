﻿using NTMiner.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class CoinKernelViewModel : ViewModelBase, ICoinKernel, IEditableViewModel {
        private Guid _id;
        private Guid _coinId;
        private Guid _kernelId;
        private int _sortNumber;
        private Guid _dualCoinGroupId;
        private bool _isSupportPool1;
        private string _args;
        private string _notice;
        private bool _isHot;
        private bool _isRecommend;
        private SupportedGpu _supportedGpu;
        private GroupViewModel _selectedDualCoinGroup;
        private List<EnvironmentVariable> _environmentVariables = new List<EnvironmentVariable>();
        private List<InputSegment> _inputSegments = new List<InputSegment>();
        private List<InputSegmentViewModel> _inputSegmentVms = new List<InputSegmentViewModel>();
        private List<Guid> _fileWriterIds = new List<Guid>();
        private List<Guid> _fragmentWriterIds = new List<Guid>();
        private List<FileWriterViewModel> _fileWriterVms = new List<FileWriterViewModel>();
        private List<FragmentWriterViewModel> _fragmentWriterVms = new List<FragmentWriterViewModel>();
        private CoinViewModel _coinVm;

        public Guid GetId() {
            return this.Id;
        }

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand SortUp { get; private set; }
        public ICommand SortDown { get; private set; }
        public ICommand Save { get; private set; }

        public ICommand AddEnvironmentVariable { get; private set; }
        public ICommand EditEnvironmentVariable { get; private set; }
        public ICommand RemoveEnvironmentVariable { get; private set; }
        public ICommand AddSegment { get; private set; }
        public ICommand EditSegment { get; private set; }
        public ICommand RemoveSegment { get; private set; }
        public ICommand RemoveFileWriter { get; private set; }
        public ICommand RemoveFragmentWriter { get; private set; }

        public Action CloseWindow { get; set; }

        public CoinKernelViewModel() {
            if (!Design.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public CoinKernelViewModel(ICoinKernel data) : this(data.GetId()) {
            _coinId = data.CoinId;
            _kernelId = data.KernelId;
            _sortNumber = data.SortNumber;
            _dualCoinGroupId = data.DualCoinGroupId;
            _args = data.Args;
            _notice = data.Notice;
            _supportedGpu = data.SupportedGpu;
            _isSupportPool1 = data.IsSupportPool1;
            // 复制，视为值对象，防止直接修改引用
            _environmentVariables.AddRange(data.EnvironmentVariables.Select(a => new EnvironmentVariable(a)));
            // 复制，视为值对象，防止直接修改引用
            _inputSegments.AddRange(data.InputSegments.Select(a => new InputSegment(a)));
            _inputSegmentVms.AddRange(_inputSegments.Select(a => new InputSegmentViewModel(a)));
            _fileWriterIds = data.FileWriterIds;
            _fragmentWriterIds = data.FragmentWriterIds;
            _isHot = data.IsHot;
            _isRecommend = data.IsRecommend;
            foreach (var writerId in _fileWriterIds) {
                if (AppContext.Instance.FileWriterVms.TryGetFileWriterVm(writerId, out FileWriterViewModel writerVm)) {
                    _fileWriterVms.Add(writerVm);
                }
            }
            foreach (var writerId in _fragmentWriterIds) {
                if (AppContext.Instance.FragmentWriterVms.TryGetFragmentWriterVm(writerId, out FragmentWriterViewModel writerVm)) {
                    _fragmentWriterVms.Add(writerVm);
                }
            }
        }

        public CoinKernelViewModel(Guid id) {
            _id = id;
            this.AddEnvironmentVariable = new DelegateCommand(() => {
                VirtualRoot.Execute(new EnvironmentVariableEditCommand(this, new EnvironmentVariable()));
            });
            this.EditEnvironmentVariable = new DelegateCommand<EnvironmentVariable>(environmentVariable => {
                VirtualRoot.Execute(new EnvironmentVariableEditCommand(this, environmentVariable));
            });
            this.RemoveEnvironmentVariable = new DelegateCommand<EnvironmentVariable>(environmentVariable => {
                this.ShowDialog(message: $"您确定删除环境变量{environmentVariable.Key}吗？", title: "确认", onYes: () => {
                    this.EnvironmentVariables.Remove(environmentVariable);
                    EnvironmentVariables = EnvironmentVariables.ToList();
                }, icon: IconConst.IconConfirm);
            });
            this.AddSegment = new DelegateCommand(() => {
                VirtualRoot.Execute(new InputSegmentEditCommand(this, new InputSegment()));
            });
            this.EditSegment = new DelegateCommand<InputSegment>((segment) => {
                VirtualRoot.Execute(new InputSegmentEditCommand(this, segment));
            });
            this.RemoveSegment = new DelegateCommand<InputSegment>((segment) => {
                this.ShowDialog(message: $"您确定删除片段{segment.Name}吗？", title: "确认", onYes: () => {
                    this.InputSegments.Remove(segment);
                    InputSegments = InputSegments.ToList();
                }, icon: IconConst.IconConfirm);
            });
            this.RemoveFileWriter = new DelegateCommand<FileWriterViewModel>((writer) => {
                this.ShowDialog(message: $"您确定删除文件书写器{writer.Name}吗？", title: "确认", onYes: () => {
                    this.FileWriterVms.Remove(writer);
                    List<Guid> writerIds = new List<Guid>(this.FileWriterIds);
                    writerIds.Remove(writer.Id);
                    this.FileWriterIds = writerIds;
                }, icon: IconConst.IconConfirm);
            });
            this.RemoveFragmentWriter = new DelegateCommand<FragmentWriterViewModel>((writer) => {
                this.ShowDialog(message: $"您确定删除文件书写器{writer.Name}吗？", title: "确认", onYes: () => {
                    this.FragmentWriterVms.Remove(writer);
                    List<Guid> writerIds = new List<Guid>(this.FragmentWriterIds);
                    writerIds.Remove(writer.Id);
                    this.FragmentWriterIds = writerIds;
                }, icon: IconConst.IconConfirm);
            });
            this.Save = new DelegateCommand(() => {
                if (NTMinerRoot.Instance.CoinKernelSet.Contains(this.Id)) {
                    VirtualRoot.Execute(new UpdateCoinKernelCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand<FormType?>((formType) => {
                VirtualRoot.Execute(new CoinKernelEditCommand(formType ?? FormType.Edit, this));
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                this.ShowDialog(message: $"您确定删除{Kernel.Code}币种内核吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new RemoveCoinKernelCommand(this.Id));
                    Kernel.OnPropertyChanged(nameof(Kernel.SupportedCoins));
                }, icon: IconConst.IconConfirm);
            });
            this.SortUp = new DelegateCommand(() => {
                CoinKernelViewModel upOne = AppContext.Instance.CoinKernelVms.AllCoinKernels.OrderByDescending(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber < this.SortNumber);
                if (upOne != null) {
                    int sortNumber = upOne.SortNumber;
                    upOne.SortNumber = this.SortNumber;
                    VirtualRoot.Execute(new UpdateCoinKernelCommand(upOne));
                    this.SortNumber = sortNumber;
                    VirtualRoot.Execute(new UpdateCoinKernelCommand(this));
                    if (AppContext.Instance.CoinVms.TryGetCoinVm(this.CoinId, out CoinViewModel coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.CoinKernels));
                    }
                    this.Kernel.OnPropertyChanged(nameof(this.Kernel.CoinKernels));
                    AppContext.Instance.CoinVms.OnPropertyChanged(nameof(AppContext.CoinViewModels.MainCoins));
                }
            });
            this.SortDown = new DelegateCommand(() => {
                CoinKernelViewModel nextOne = AppContext.Instance.CoinKernelVms.AllCoinKernels.OrderBy(a => a.SortNumber).FirstOrDefault(a => a.CoinId == this.CoinId && a.SortNumber > this.SortNumber);
                if (nextOne != null) {
                    int sortNumber = nextOne.SortNumber;
                    nextOne.SortNumber = this.SortNumber;
                    VirtualRoot.Execute(new UpdateCoinKernelCommand(nextOne));
                    this.SortNumber = sortNumber;
                    VirtualRoot.Execute(new UpdateCoinKernelCommand(this));
                    if (AppContext.Instance.CoinVms.TryGetCoinVm(this.CoinId, out CoinViewModel coinVm)) {
                        coinVm.OnPropertyChanged(nameof(coinVm.CoinKernels));
                    }
                    this.Kernel.OnPropertyChanged(nameof(this.Kernel.CoinKernels));
                    AppContext.Instance.CoinVms.OnPropertyChanged(nameof(AppContext.CoinViewModels.MainCoins));
                }
            });
        }

        public Guid Id {
            get => _id;
            private set {
                if (_id != value) {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public Guid CoinId {
            get {
                return _coinId;
            }
            set {
                if (_coinId != value) {
                    _coinId = value;
                    OnPropertyChanged(nameof(CoinId));
                    OnPropertyChanged(nameof(CoinCode));
                }
            }
        }

        public string CoinCode {
            get {
                return CoinVm.Code;
            }
        }

        public CoinViewModel CoinVm {
            get {
                if (_coinVm == null || this.CoinId != _coinVm.Id) {
                    AppContext.Instance.CoinVms.TryGetCoinVm(this.CoinId, out _coinVm);
                    if (_coinVm == null) {
                        _coinVm = CoinViewModel.Empty;
                    }
                }
                return _coinVm;
            }
        }

        public Guid KernelId {
            get => _kernelId;
            set {
                if (_kernelId != value) {
                    _kernelId = value;
                    OnPropertyChanged(nameof(KernelId));
                }
            }
        }

        public string DisplayName {
            get {
                return $"{Kernel.Code}{Kernel.Version}";
            }
        }

        public KernelViewModel Kernel {
            get {
                if (AppContext.Instance.KernelVms.TryGetKernelVm(this.KernelId, out KernelViewModel kernel)) {
                    return kernel;
                }
                return KernelViewModel.Empty;
            }
        }

        public int SortNumber {
            get => _sortNumber;
            set {
                if (_sortNumber != value) {
                    _sortNumber = value;
                    OnPropertyChanged(nameof(SortNumber));
                }
            }
        }

        public Guid DualCoinGroupId {
            get => _dualCoinGroupId;
            set {
                if (_dualCoinGroupId != value) {
                    _dualCoinGroupId = value;
                    OnPropertyChanged(nameof(DualCoinGroupId));
                }
            }
        }

        public GroupViewModel SelectedDualCoinGroup {
            get {
                if (this.DualCoinGroupId == Guid.Empty) {
                    return GroupViewModel.PleaseSelect;
                }
                if (_selectedDualCoinGroup == null || _selectedDualCoinGroup.Id != this.DualCoinGroupId) {
                    AppContext.Instance.GroupVms.TryGetGroupVm(DualCoinGroupId, out _selectedDualCoinGroup);
                    if (_selectedDualCoinGroup == null) {
                        _selectedDualCoinGroup = GroupViewModel.PleaseSelect;
                    }
                }
                return _selectedDualCoinGroup;
            }
            set {
                if (DualCoinGroupId != value.Id) {
                    DualCoinGroupId = value.Id;
                    OnPropertyChanged(nameof(DualCoinGroup));
                    OnPropertyChanged(nameof(SelectedDualCoinGroup));
                    OnPropertyChanged(nameof(IsSupportDualMine));
                }
            }
        }

        public GroupViewModel DualCoinGroup {
            get {
                if (!this.Kernel.KernelInputVm.IsSupportDualMine) {
                    return GroupViewModel.PleaseSelect;
                }
                if (this.DualCoinGroupId == Guid.Empty) {
                    return this.Kernel.KernelInputVm.DualCoinGroup;
                }
                return SelectedDualCoinGroup;
            }
        }

        public AppContext.GroupViewModels GroupVms {
            get {
                return AppContext.Instance.GroupVms;
            }
        }

        public string Args {
            get { return _args; }
            set {
                if (_args != value) {
                    _args = value;
                    OnPropertyChanged(nameof(Args));
                }
            }
        }

        public string Notice {
            get { return _notice; }
            set {
                if (_notice != value) {
                    _notice = value;
                    OnPropertyChanged(nameof(Notice));
                }
            }
        }

        public bool IsHot {
            get { return _isHot; }
            set {
                _isHot = value;
                OnPropertyChanged(nameof(IsHot));
            }
        }

        public bool IsRecommend {
            get { return _isRecommend; }
            set {
                _isRecommend = value;
                OnPropertyChanged(nameof(IsRecommend));
            }
        }

        public List<EnvironmentVariable> EnvironmentVariables {
            get => _environmentVariables;
            set {
                _environmentVariables = value;
                OnPropertyChanged(nameof(EnvironmentVariables));
            }
        }

        public List<InputSegment> InputSegments {
            get => _inputSegments;
            set {
                _inputSegments = value;
                OnPropertyChanged(nameof(InputSegments));
                if (value != null) {
                    this.InputSegmentVms = value.Select(a => new InputSegmentViewModel(a)).ToList();
                }
                else {
                    this.InputSegmentVms = new List<InputSegmentViewModel>();
                }
            }
        }

        public List<InputSegmentViewModel> InputSegmentVms {
            get {
                return _inputSegmentVms;
            }
            set {
                _inputSegmentVms = value;
                OnPropertyChanged(nameof(InputSegmentVms));
            }
        }

        public List<Guid> FileWriterIds {
            get => _fileWriterIds;
            set {
                _fileWriterIds = value;
                OnPropertyChanged(nameof(FileWriterIds));
                if (value != null) {
                    this.FileWriterVms = AppContext.Instance.FileWriterVms.List.Where(a => value.Contains(a.Id)).ToList();
                }
                else {
                    this.FileWriterVms = new List<FileWriterViewModel>();
                }
            }
        }

        public List<FileWriterViewModel> FileWriterVms {
            get {
                return _fileWriterVms;
            }
            set {
                _fileWriterVms = value;
                OnPropertyChanged(nameof(FileWriterVms));
            }
        }

        public List<Guid> FragmentWriterIds {
            get {
                return _fragmentWriterIds;
            }
            set {
                _fragmentWriterIds = value;
                OnPropertyChanged(nameof(FragmentWriterIds));
                if (value != null) {
                    this.FragmentWriterVms = AppContext.Instance.FragmentWriterVms.List.Where(a => value.Contains(a.Id)).ToList();
                }
                else {
                    this.FragmentWriterVms = new List<FragmentWriterViewModel>();
                }
            }
        }

        public List<FragmentWriterViewModel> FragmentWriterVms {
            get {
                return _fragmentWriterVms;
            }
            set {
                _fragmentWriterVms = value;
                OnPropertyChanged(nameof(FragmentWriterVms));
            }
        }

        public bool IsSupportDualMine {
            get {
                if (!this.Kernel.KernelInputVm.IsSupportDualMine) {
                    return false;
                }
                if (this.DualCoinGroupId != Guid.Empty) {
                    return true;
                }
                return this.Kernel.KernelInputVm.DualCoinGroupId != Guid.Empty;
            }
        }

        public bool IsSupportPool1 {
            get { return _isSupportPool1; }
            set {
                if (_isSupportPool1 != value) {
                    _isSupportPool1 = value;
                    OnPropertyChanged(nameof(IsSupportPool1));
                    AppContext.Instance.MinerProfileVm.OnPropertyChanged(nameof(AppContext.Instance.MinerProfileVm.IsAllMainCoinPoolIsUserMode));
                }
            }
        }

        public SupportedGpu SupportedGpu {
            get => _supportedGpu;
            set {
                if (_supportedGpu != value) {
                    _supportedGpu = value;
                    OnPropertyChanged(nameof(SupportedGpu));
                    OnPropertyChanged(nameof(IsNvidiaIconVisible));
                    OnPropertyChanged(nameof(IsAMDIconVisible));
                    OnPropertyChanged(nameof(IsSupported));
                }
            }
        }

        public bool IsSupported {
            get {
                if (NTMinerRoot.Instance.GpuSet.GpuType == GpuType.Empty) {
                    return true;
                }
                if (this.SupportedGpu == SupportedGpu.Both) {
                    return true;
                }
                if (this.SupportedGpu == SupportedGpu.NVIDIA && NTMinerRoot.Instance.GpuSet.GpuType == GpuType.NVIDIA) {
                    return true;
                }
                if (this.SupportedGpu == SupportedGpu.AMD && NTMinerRoot.Instance.GpuSet.GpuType == GpuType.AMD) {
                    return true;
                }
                return false;
            }
        }

        public Visibility IsNvidiaIconVisible {
            get {
                if (SupportedGpu == SupportedGpu.NVIDIA || SupportedGpu == SupportedGpu.Both) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility IsAMDIconVisible {
            get {
                if (SupportedGpu == SupportedGpu.AMD || SupportedGpu == SupportedGpu.Both) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public EnumItem<SupportedGpu> SupportedGpuEnumItem {
            get {
                return EnumSet.SupportedGpuEnumItems.FirstOrDefault(a => a.Value == SupportedGpu);
            }
            set {
                if (SupportedGpu != value.Value) {
                    SupportedGpu = value.Value;
                    OnPropertyChanged(nameof(SupportedGpuEnumItem));
                }
            }
        }

        public CoinKernelProfileViewModel CoinKernelProfile {
            get {
                return AppContext.Instance.CoinProfileVms.GetOrCreateCoinKernelProfileVm(this.Id);
            }
        }
    }
}
