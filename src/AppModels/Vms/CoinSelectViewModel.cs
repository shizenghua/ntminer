﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class CoinSelectViewModel : ViewModelBase {
        private string _keyword;
        private CoinViewModel _selectedResult;
        public readonly Action<CoinViewModel> OnOk;
        private readonly IEnumerable<CoinViewModel> _coins;

        public ICommand ClearKeyword { get; private set; }
        public ICommand HideView { get; set; }

        public CoinSelectViewModel() {
            if (!Design.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public CoinSelectViewModel(IEnumerable<CoinViewModel> coins, CoinViewModel selected, Action<CoinViewModel> onOk) {
            _coins = coins;
            _selectedResult = selected;
            OnOk = onOk;
            this.ClearKeyword = new DelegateCommand(() => {
                this.Keyword = string.Empty;
            });
        }

        public string Keyword {
            get => _keyword;
            set {
                if (_keyword != value) {
                    _keyword = value;
                    OnPropertyChanged(nameof(Keyword));
                    OnPropertyChanged(nameof(QueryResults));
                }
            }
        }

        public CoinViewModel SelectedResult {
            get => _selectedResult;
            set {
                if (_selectedResult != value) {
                    _selectedResult = value;
                    OnPropertyChanged(nameof(SelectedResult));
                }
            }
        }

        public List<CoinViewModel> QueryResults {
            get {
                if (!string.IsNullOrEmpty(Keyword)) {
                    return _coins.Where(a => 
                        (a.Code != null && a.Code.IgnoreCaseContains(Keyword)) || 
                        (a.CnName != null && a.CnName.IgnoreCaseContains(Keyword)) || 
                        (a.EnName != null && a.EnName.IgnoreCaseContains(Keyword))).OrderBy(a => a.SortNumber).ToList();
                }
                return _coins.OrderBy(a => a.SortNumber).ToList();
            }
        }
    }
}
