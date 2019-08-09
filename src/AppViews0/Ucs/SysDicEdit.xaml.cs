﻿using NTMiner.Vms;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class SysDicEdit : UserControl {
        public static void ShowWindow(FormType formType, SysDicViewModel source) {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "系统字典",
                FormType = formType,
                IsDialogWindow = true,
                CloseVisible = System.Windows.Visibility.Visible,
                IconName = "Icon_SysDic"
            }, ucFactory: (window) => {
                SysDicViewModel vm = new SysDicViewModel(source) {
                    CloseWindow = () => window.Close()
                };
                return new SysDicEdit(vm);
            }, fixedSize: true);
        }

        private SysDicViewModel Vm {
            get {
                return (SysDicViewModel)this.DataContext;
            }
        }

        public SysDicEdit(SysDicViewModel vm) {
            this.DataContext = vm;
            InitializeComponent();
        }
    }
}
