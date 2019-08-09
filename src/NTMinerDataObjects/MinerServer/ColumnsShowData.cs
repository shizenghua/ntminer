﻿using System;
using System.Text;

namespace NTMiner.MinerServer {
    public class ColumnsShowData : IDbEntity<Guid>, IColumnsShow, IGetSignData {
        public static readonly ColumnsShowData PleaseSelect = new ColumnsShowData {
            ColumnsShowName = "默认",
            Id = Guid.Parse("197f19e8-0c1b-4018-875d-2f5e56a02491"),
            BootTimeSpanText = true,
            ClientName = true,
            DiskSpace = true,
            DualCoinCode = true,
            DualCoinPool = true,
            DualCoinRejectPercentText = true,
            DualCoinSpeedText = true,
            DualCoinWallet = true,
            GpuDriver = true,
            GpuInfo = true,
            GpuTableVm = true,
            GpuType = true,
            IncomeDualCoinPerDayText = true,
            IncomeMainCoinPerDayText = true,
            IsAutoBoot = true,
            IsAutoRestartKernel = true,
            IsAutoStart = true,
            IsNoShareRestartKernel = true,
            IsNoShareRestartComputer = false,
            IsPeriodicRestartComputer = true,
            IsPeriodicRestartKernel = true,
            MinerIp = true,
            Kernel = true,
            KernelCommandLine = true,
            LastActivedOnText = true,
            MainCoinCode = true,
            MainCoinPool = true,
            MainCoinRejectPercentText = true,
            MainCoinSpeedText = true,
            MainCoinWallet = true,
            MaxTempText = true,
            MinerGroup = true,
            MinerName = true,
            MineTimeSpanText = true,
            OSName = true,
            OSVirtualMemoryGbText = true,
            TotalPowerText = true,
            Version = true,
            WindowsLoginNameAndPassword = true,
            WindowsPassword = true,
            Work = true
        };

        public ColumnsShowData() { }

        public Guid GetId() {
            return this.Id;
        }

        public Guid Id { get; set; }

        public string ColumnsShowName { get; set; }

        public bool Work { get; set; }

        public bool MinerName{ get; set; }

        public bool ClientName { get; set; }

        public bool MinerIp{ get; set; }

        public bool MinerGroup{ get; set; }

        public bool MainCoinCode{ get; set; }

        public bool MainCoinSpeedText{ get; set; }

        public bool GpuTableVm { get; set; }

        public bool MainCoinWallet{ get; set; }

        public bool MainCoinPool{ get; set; }

        public bool Kernel{ get; set; }

        public bool DualCoinCode{ get; set; }

        public bool DualCoinSpeedText{ get; set; }

        public bool DualCoinWallet{ get; set; }

        public bool DualCoinPool{ get; set; }

        public bool LastActivedOnText{ get; set; }

        public bool Version{ get; set; }

        public bool WindowsLoginNameAndPassword{ get; set; }

        public bool WindowsPassword{ get; set; }

        public bool GpuInfo{ get; set; }

        public bool MainCoinRejectPercentText { get; set; }

        public bool DualCoinRejectPercentText { get; set; }

        public bool BootTimeSpanText { get; set; }

        public bool MineTimeSpanText { get; set; }

        public bool IncomeMainCoinPerDayText { get; set; }

        public bool IncomeDualCoinPerDayText { get; set; }

        public bool IsAutoBoot { get; set; }

        public bool IsAutoStart { get; set; }

        public bool OSName { get; set; }

        public bool OSVirtualMemoryGbText { get; set; }

        public bool GpuType { get; set; }

        public bool GpuDriver { get; set; }

        public bool TotalPowerText { get; set; }

        public bool MaxTempText { get; set; }

        public bool KernelCommandLine { get; set; }

        public bool DiskSpace { get; set; }

        public bool IsAutoRestartKernel { get; set; }

        public bool IsNoShareRestartKernel { get; set; }

        public bool IsPeriodicRestartKernel { get; set; }

        public bool IsPeriodicRestartComputer { get; set; }

        public bool IsNoShareRestartComputer { get; set; }

        public StringBuilder GetSignData() {
            return this.BuildSign();
        }
    }
}
