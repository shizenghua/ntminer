﻿using NTMiner.Vms;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class SysDicPage : UserControl {
        public static void ShowWindow() {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "系统字典",
                IconName = "Icon_SysDic",
                CloseVisible = System.Windows.Visibility.Visible,
                FooterVisible = System.Windows.Visibility.Collapsed,
                Width = 860,
                Height = 520
            }, ucFactory: (window) => new SysDicPage(), fixedSize: false);
        }

        private SysDicPageViewModel Vm {
            get {
                return (SysDicPageViewModel)this.DataContext;
            }
        }

        public SysDicPage() {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Wpf.Util.DataGrid_MouseDoubleClick<SysDicViewModel>(sender, e);
        }

        private void SysDicItemDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Wpf.Util.DataGrid_MouseDoubleClick<SysDicItemViewModel>(sender, e);
        }
    }
}
