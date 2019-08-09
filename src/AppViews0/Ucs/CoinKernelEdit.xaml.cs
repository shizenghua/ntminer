﻿using NTMiner.Core;
using NTMiner.Vms;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class CoinKernelEdit : UserControl {
        public static void ShowWindow(FormType formType, CoinKernelViewModel source) {
            ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                Title = "币种级参数",
                FormType = formType,
                IsDialogWindow = true,
                IconName = "Icon_Kernel",
                CloseVisible = System.Windows.Visibility.Visible
            }, ucFactory: (window) =>
            {
                CoinKernelViewModel vm = new CoinKernelViewModel(source) {
                    CloseWindow = () => window.Close()
                };
                return new CoinKernelEdit(vm);
            }, fixedSize: true);
        }

        private CoinKernelViewModel Vm {
            get {
                return (CoinKernelViewModel)this.DataContext;
            }
        }

        public CoinKernelEdit(CoinKernelViewModel vm) {
            this.DataContext = vm;
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DataGrid dg = (DataGrid)sender;
            Point p = e.GetPosition(dg);
            if (p.Y < dg.ColumnHeaderHeight) {
                return;
            }
            if (dg.SelectedItem != null) {
                Vm.EditEnvironmentVariable.Execute((EnvironmentVariable)dg.SelectedItem);
            }
        }

        private void DataGridSegments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DataGrid dg = (DataGrid)sender;
            Point p = e.GetPosition(dg);
            if (p.Y < 30) {
                return;
            }
            if (dg.SelectedItem != null) {
                Vm.EditSegment.Execute((InputSegment)dg.SelectedItem);
            }
        }

        private void ButtonAddFileWriter_Click(object sender, RoutedEventArgs e) {
            PopupFileWriter.Child = new FileWriterSelect(
                new FileWriterSelectViewModel( onOk: selectedResult => {
                    if (selectedResult != null) {
                        var fileWriterIds = new List<Guid>(this.Vm.FileWriterIds) {
                            selectedResult.Id
                        };
                        this.Vm.FileWriterIds = fileWriterIds;
                        PopupFileWriter.IsOpen = false;
                    }
                }) {
                    HideView = new DelegateCommand(() => {
                        PopupFileWriter.IsOpen = false;
                    })
                });
            PopupFileWriter.IsOpen = true;
        }

        private void ButtonAddFragmentWriter_Click(object sender, RoutedEventArgs e) {
            PopupFragmentWriter.Child = new FragmentWriterSelect(
                new FragmentWriterSelectViewModel(onOk: selectedResult => {
                    if (selectedResult != null) {
                        var fileWriterIds = new List<Guid>(this.Vm.FragmentWriterIds) {
                            selectedResult.Id
                        };
                        this.Vm.FragmentWriterIds = fileWriterIds;
                        PopupFragmentWriter.IsOpen = false;
                    }
                }) {
                    HideView = new DelegateCommand(() => {
                        PopupFragmentWriter.IsOpen = false;
                    })
                });
            PopupFragmentWriter.IsOpen = true;
        }
    }
}
