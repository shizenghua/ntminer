﻿using NTMiner.Vms;
using System.Windows;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class CoinEdit : UserControl {
        public static void ShowWindow(FormType formType, CoinViewModel source) {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "币种",
                FormType = formType,
                IsDialogWindow = true,
                Width = 500,
                CloseVisible = Visibility.Visible,
                IconName = "Icon_Coin",
            }, ucFactory: (window) =>
            {
                CoinViewModel vm = new CoinViewModel(source) {
                    CloseWindow = () => window.Close()
                };
                return new CoinEdit(vm);
            }, fixedSize: true);
        }

        private CoinViewModel Vm {
            get {
                return (CoinViewModel)this.DataContext;
            }
        }
        public CoinEdit(CoinViewModel vm) {
            this.DataContext = vm;
            InitializeComponent();
        }

        private void KbButtonAlgo_Clicked(object sender, RoutedEventArgs e) {
            OpenAlgoPopup();
            e.Handled = true;
        }

        private void OpenAlgoPopup() {
            var popup = PopupAlgo;
            popup.IsOpen = true;
            var selected = Vm.AlgoItem;
            popup.Child = new SysDicItemSelect(
                new SysDicItemSelectViewModel(AppContext.Instance.SysDicItemVms.AlgoItems, selected, onOk: selectedResult => {
                    if (selectedResult != null) {
                        if (Vm.AlgoItem != selectedResult) {
                            Vm.AlgoItem = selectedResult;
                        }
                        popup.IsOpen = false;
                    }
                }) {
                    HideView = new DelegateCommand(() => {
                        popup.IsOpen = false;
                    })
                });
        }
    }
}
