﻿using NTMiner.Vms;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class MinerGroupEdit : UserControl {
        public static void ShowWindow(FormType formType, MinerGroupViewModel source) {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "矿工分组",
                FormType = formType,
                IsDialogWindow = true,
                CloseVisible = System.Windows.Visibility.Visible,
                IconName = "Icon_MinerGroup"
            }, ucFactory: (window) => {
                MinerGroupViewModel vm = new MinerGroupViewModel(source) {
                    CloseWindow = () => window.Close()
                };
                return new MinerGroupEdit(vm);
            }, fixedSize: true);
        }

        private MinerGroupViewModel Vm {
            get {
                return (MinerGroupViewModel)this.DataContext;
            }
        }
        public MinerGroupEdit(MinerGroupViewModel vm) {
            this.DataContext = vm;
            InitializeComponent();
        }
    }
}
