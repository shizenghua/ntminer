﻿using NTMiner.Vms;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class QQGroupQrCode : UserControl {
        public static void ShowWindow() {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                IconImage = "Icon_QQ",
                Width = 280,
                Height = 320,
                IsDialogWindow = true,
                CloseVisible = System.Windows.Visibility.Visible,
            }, ucFactory: (window) => new QQGroupQrCode(), fixedSize: true);
        }

        public QQGroupQrCode() {
            InitializeComponent();
            ResourceDictionarySet.Instance.FillResourceDic(this, this.Resources);
        }
    }
}
