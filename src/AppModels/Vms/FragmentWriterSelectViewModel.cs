﻿using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class FragmentWriterSelectViewModel : ViewModelBase {
        private FragmentWriterViewModel _selectedResult;
        public readonly Action<FragmentWriterViewModel> OnOk;

        public ICommand HideView { get; set; }

        public FragmentWriterSelectViewModel() {
            if (!Design.IsInDesignMode) {
                throw new InvalidProgramException();
            }
        }

        public FragmentWriterSelectViewModel(Action<FragmentWriterViewModel> onOk) {
            OnOk = onOk;
        }

        public FragmentWriterViewModel SelectedResult {
            get => _selectedResult;
            set {
                if (_selectedResult != value) {
                    _selectedResult = value;
                    OnPropertyChanged(nameof(SelectedResult));
                }
            }
        }

        public List<FragmentWriterViewModel> FragmentWriterVms {
            get {
                return AppContext.Instance.FragmentWriterVms.List;
            }
        }
    }
}
