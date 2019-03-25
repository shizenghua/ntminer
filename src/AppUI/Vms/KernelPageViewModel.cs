﻿using NTMiner.Core.Kernels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace NTMiner.Vms {
    public class KernelPageViewModel : ViewModelBase {
        public static readonly KernelPageViewModel Current = new KernelPageViewModel();

        private string _keyword;
        private List<KernelMenu> _kernelMenus = new List<KernelMenu>();
        private KernelMenu _currentKernelMenu;
        private Visibility _kernelDownloadingVisible = Visibility.Collapsed;
        private CoinViewModel _selectedCoinVm = CoinViewModel.PleaseSelect;
        private int _pageIndex;
        private int _pageSize = 15;
        private ObservableCollection<int> _pageNumbers;

        public ICommand Home { get; private set; }
        public ICommand ChangeCurrentKernelMenu { get; private set; }

        public ICommand Add { get; private set; }
        public ICommand ClearKeyword { get; private set; }

        public ICommand PageSub { get; private set; }
        public ICommand PageAdd { get; private set; }

        public ICommand Search { get; private set; }

        private readonly KernelMenu _repositoryKernelMenu = new KernelMenu("宝库", "Icon_Kernel");
        private readonly KernelMenu _uninstallKernelMenu = new KernelMenu("卸载", "Icon_Delete");

        private KernelPageViewModel() {
            this.ChangeCurrentKernelMenu = new DelegateCommand<KernelMenu>((kernelMenu) => {
                SetCurrentKernelMenu(kernelMenu);
                this.PageNumber = 1;
                KernelDownloadingVisible = Visibility.Collapsed;
            });
            this.Home = new DelegateCommand(() => {
                ChangeCurrentKernelMenu.Execute(_repositoryKernelMenu);
            });

            this._kernelMenus.Add(_repositoryKernelMenu);
            this._kernelMenus.Add(_uninstallKernelMenu);
            this.Add = new DelegateCommand(() => {
                new KernelViewModel(Guid.NewGuid()).Edit.Execute(FormType.Add);
            });
            this.Search = new DelegateCommand(() => {
                this.PageNumber = 1;
            });
            this.ClearKeyword = new DelegateCommand(() => {
                Keyword = string.Empty;
            });
            this.PageSub = new DelegateCommand(() => {
                this.PageNumber = this.PageNumber - 1;
            });
            this.PageAdd = new DelegateCommand(() => {
                this.PageNumber = this.PageNumber + 1;
            });
            this.Home.Execute(null);
        }

        public void Download(Guid kernelId, Action<bool, string> downloadComplete) {
            KernelDownloadingVisible = Visibility.Visible;
            KernelViewModel kernelVm;
            if (KernelViewModels.Current.TryGetKernelVm(kernelId, out kernelVm)) {
                kernelVm.KernelProfileVm.Download(downloadComplete);
            }
        }

        private SysDicItemViewModel _brandItem = SysDicItemViewModel.PleaseSelect;
        public SysDicItemViewModel BrandItem {
            get {
                return _brandItem;
            }
            set {
                if (value == null) {
                    value = SysDicItemViewModel.PleaseSelect;
                }
                _brandItem = value;
                OnPropertyChanged(nameof(BrandItem));
                this.PageNumber = 1;
            }
        }

        public SysDicItemViewModels SysDicItemVms {
            get {
                return SysDicItemViewModels.Current;
            }
        }

        public Visibility IsBrandVisible {
            get {
                if (NTMinerRoot.KernelBrandId != Guid.Empty) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public bool CanPageSub {
            get {
                return PageNumber != 1;
            }
        }

        public bool CanPageAdd {
            get {
                return PageNumbers.Count > PageNumber;
            }
        }

        public CoinViewModels CoinVms {
            get {
                return CoinViewModels.Current;
            }
        }

        public MinerProfileViewModel MinerProfile {
            get {
                return MinerProfileViewModel.Current;
            }
        }

        public CoinViewModel SelectedCoinVm {
            get {
                return _selectedCoinVm;
            }
            set {
                if (_selectedCoinVm != value) {
                    _selectedCoinVm = value;
                    OnPropertyChanged(nameof(SelectedCoinVm));
                    this.PageNumber = 1;
                }
            }
        }

        public Visibility KernelDownloadingVisible {
            get => _kernelDownloadingVisible;
            set {
                if (_kernelDownloadingVisible != value) {
                    _kernelDownloadingVisible = value;
                    OnPropertyChanged(nameof(KernelDownloadingVisible));
                }
            }
        }

        public string Keyword {
            get => _keyword;
            set {
                if (_keyword != value) {
                    _keyword = value;
                    OnPropertyChanged(nameof(Keyword));
                }
            }
        }

        public int PageIndex {
            get => _pageIndex;
            set {
                if (_pageIndex != value) {
                    _pageIndex = value;
                    OnPropertyChanged(nameof(PageIndex));
                }
            }
        }

        public int PageNumber {
            get {
                return PageIndex + 1;
            }
            set {
                PageIndex = value - 1;
                OnPropertyChanged(nameof(PageNumber));
                OnPropertyChanged(nameof(QueryResults));
            }
        }

        public int PageSize {
            get => _pageSize;
            set {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
            }
        }

        public ObservableCollection<int> PageNumbers {
            get => _pageNumbers;
            set {
                _pageNumbers = value;
                OnPropertyChanged(nameof(PageNumbers));
            }
        }

        public bool IsPageNumbersEmpty {
            get {
                return PageNumbers.Count == 0;
            }
        }

        public List<KernelViewModel> QueryResults {
            get {
                IQueryable<KernelViewModel> query = KernelViewModels.Current.AllKernels.AsQueryable();
                if (!AppStatic.IsDebugMode) {
                    query = query.Where(a => a.PublishState == PublishStatus.Published);
                }
                if (!string.IsNullOrEmpty(Keyword)) {
                    string keyword = this.Keyword.ToLower();
                    query = query.
                        Where(a => (!string.IsNullOrEmpty(a.Code) && a.Code.ToLower().Contains(keyword))
                            || (!string.IsNullOrEmpty(a.Version) && a.Version.ToLower().Contains(keyword))
                            || (!string.IsNullOrEmpty(a.Notice) && a.Notice.ToLower().Contains(keyword)));
                }
                if (SelectedCoinVm != null && SelectedCoinVm != CoinViewModel.PleaseSelect) {
                    query = query.Where(a => a.SupportedCoinVms.Contains(SelectedCoinVm));
                }
                if (BrandItem != null && BrandItem != SysDicItemViewModel.PleaseSelect) {
                    query = query.Where(a => a.BrandItem == BrandItem);
                }
                if (CurrentKernelMenu == _uninstallKernelMenu) {
                    query = query.Where(a => a.KernelProfileVm.InstallStatus == InstallStatus.Installed);
                }
                int total = query.Count();
                int pages = (int)Math.Ceiling((double)total / PageSize);
                if (PageNumbers == null) {
                    List<int> pageNumbers = new List<int>();
                    for (int i = 1; i <= pages; i++) {
                        pageNumbers.Add(i);
                    }
                    PageNumbers = new ObservableCollection<int>(pageNumbers);
                }
                else {
                    int count = PageNumbers.Count;
                    if (pages < count) {
                        for (int n = pages + 1; n <= count; n++) {
                            PageNumbers.Remove(n);
                        }
                    }
                    else {
                        for (int n = count + 1; n <= pages; n++) {
                            PageNumbers.Add(n);
                        }
                    }
                }
                OnPropertyChanged(nameof(CanPageSub));
                OnPropertyChanged(nameof(CanPageAdd));
                OnPropertyChanged(nameof(IsPageNumbersEmpty));

                List<KernelViewModel> orderedList = new List<KernelViewModel>();
                var groups = query.GroupBy(a => a.Code);
                foreach (var g in groups.OrderBy(a => a.Key)) {
                    foreach (var item in g.OrderByDescending(a => a.Version)) {
                        orderedList.Add(item);
                    }
                }

                return orderedList.Skip(PageIndex * PageSize).Take(PageSize).ToList();
            }
        }

        public List<KernelViewModel> DownloadingVms {
            get {
                return KernelViewModels.Current.AllKernels.Where(a => a.KernelProfileVm.IsDownloading).OrderBy(a => a.Code + a.Version).ToList();
            }
        }

        public List<KernelMenu> KernelMenus {
            get { return _kernelMenus; }
            set {
                if (_kernelMenus != value) {
                    _kernelMenus = value;
                    OnPropertyChanged(nameof(KernelMenus));
                }
            }
        }

        public void SetCurrentKernelMenu(KernelMenu kernelMenu) {
            if (this.CurrentKernelMenu != null) {
                this.CurrentKernelMenu.SetDefaultBackground();
            }
            this.CurrentKernelMenu = kernelMenu;
            if (kernelMenu != null) {
                this.CurrentKernelMenu.SetSelectedBackground();
            }
        }

        public KernelMenu CurrentKernelMenu {
            get => _currentKernelMenu;
            set {
                if (_currentKernelMenu != value) {
                    _currentKernelMenu = value;
                    OnPropertyChanged(nameof(CurrentKernelMenu));
                }
            }
        }
    }
}
