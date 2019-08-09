﻿using NTMiner.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.JsonDb {
    public class ServerJsonDb : IServerJsonDb {
        public ServerJsonDb() {
            this.Coins = new CoinData[0];
            this.Groups = new GroupData[0];
            this.CoinGroups = new CoinGroupData[0];
            this.CoinKernels = new List<CoinKernelData>();
            this.FileWriters = new List<FileWriterData>();
            this.FragmentWriters = new List<FragmentWriterData>();
            this.Kernels = new List<KernelData>();
            this.Packages = new List<PackageData>();
            this.KernelInputs = new KernelInputData[0];
            this.KernelOutputs = new KernelOutputData[0];
            this.KernelOutputFilters = new KernelOutputFilterData[0];
            this.KernelOutputTranslaters = new KernelOutputTranslaterData[0];
            this.Pools = new List<PoolData>();
            this.PoolKernels = new List<PoolKernelData>();
            this.SysDics = new SysDicData[0];
            this.SysDicItems = new SysDicItemData[0];
            this.TimeStamp = Timestamp.GetTimestamp();
        }

        public ServerJsonDb(INTMinerRoot root) {
            Coins = root.CoinSet.Cast<CoinData>().ToArray();
            // json向后兼容
            foreach (var coin in Coins) {
                if (root.SysDicItemSet.TryGetDicItem(coin.AlgoId, out ISysDicItem dicItem)) {
                    coin.Algo = dicItem.Value;
                }
            }
            Groups = root.GroupSet.Cast<GroupData>().ToArray();
            CoinGroups = root.CoinGroupSet.Cast<CoinGroupData>().ToArray();
            KernelInputs = root.KernelInputSet.Cast<KernelInputData>().ToArray();
            KernelOutputs = root.KernelOutputSet.Cast<KernelOutputData>().ToArray();
            KernelOutputFilters = root.KernelOutputFilterSet.Cast<KernelOutputFilterData>().ToArray();
            KernelOutputTranslaters = root.KernelOutputTranslaterSet.Cast<KernelOutputTranslaterData>().ToArray();
            Kernels = root.KernelSet.Cast<KernelData>().ToList();
            Packages = root.PackageSet.Cast<PackageData>().ToList();
            CoinKernels = root.CoinKernelSet.Cast<CoinKernelData>().ToList();
            FileWriters = root.FileWriterSet.Cast<FileWriterData>().ToList();
            FragmentWriters = root.FragmentWriterSet.Cast<FragmentWriterData>().ToList();
            PoolKernels = root.PoolKernelSet.Cast<PoolKernelData>().Where(a => !string.IsNullOrEmpty(a.Args)).ToList();
            Pools = root.PoolSet.Cast<PoolData>().ToList();
            SysDicItems = root.SysDicItemSet.Cast<SysDicItemData>().ToArray();
            SysDics = root.SysDicSet.Cast<SysDicData>().ToArray();
            this.TimeStamp = Timestamp.GetTimestamp();
        }

        /// <summary>
        /// 序列化前消减json的体积，但也不比过分担心json的体积，因为会压缩。但是反序列化json时会耗费cpu，或许这个地方应该优化为某种更节省cpu的数据格式。
        /// </summary>
        public void CutJsonSize() {
            foreach (var coinKernel in this.CoinKernels) {
                if (coinKernel.EnvironmentVariables.Count == 0) {
                    coinKernel.EnvironmentVariables = null;
                }
                if (coinKernel.InputSegments.Count == 0) {
                    coinKernel.InputSegments = null;
                }
                if (coinKernel.FileWriterIds.Count == 0) {
                    coinKernel.FileWriterIds = null;
                }
                if (coinKernel.FragmentWriterIds.Count == 0) {
                    coinKernel.FragmentWriterIds = null;
                }
            }
        }

        public void UnCut() {
            foreach (var coinKernel in this.CoinKernels) {
                if (coinKernel.EnvironmentVariables == null) {
                    coinKernel.EnvironmentVariables = new List<EnvironmentVariable>();
                }
                if (coinKernel.InputSegments == null) {
                    coinKernel.InputSegments = new List<InputSegment>();
                }
                if (coinKernel.FileWriterIds == null) {
                    coinKernel.FileWriterIds = new List<Guid>();
                }
                if (coinKernel.FragmentWriterIds == null) {
                    coinKernel.FragmentWriterIds = new List<Guid>();
                }
            }
        }

        public ServerJsonDb(INTMinerRoot root, LocalJsonDb localJsonObj) {
            var minerProfile = root.MinerProfile;
            var mainCoinProfile = minerProfile.GetCoinProfile(minerProfile.CoinId);
            root.CoinKernelSet.TryGetCoinKernel(mainCoinProfile.CoinKernelId, out ICoinKernel coinKernel);
            root.KernelSet.TryGetKernel(coinKernel.KernelId, out IKernel kernel);
            var coins = root.CoinSet.Cast<CoinData>().Where(a => localJsonObj.CoinProfiles.Any(b => b.CoinId == a.Id)).ToArray();
            var coinGroups = root.CoinGroupSet.Cast<CoinGroupData>().Where(a => coins.Any(b => b.Id == a.CoinId)).ToArray();
            var pools = root.PoolSet.Cast<PoolData>().Where(a => localJsonObj.PoolProfiles.Any(b => b.PoolId == a.Id)).ToList();

            Coins = coins;
            // json向后兼容
            foreach (var coin in Coins) {
                if (root.SysDicItemSet.TryGetDicItem(coin.AlgoId, out ISysDicItem dicItem)) {
                    coin.Algo = dicItem.Value;
                }
            }
            CoinGroups = coinGroups;
            Pools = pools;
            Groups = root.GroupSet.Cast<GroupData>().Where(a => coinGroups.Any(b => b.GroupId == a.Id)).ToArray();
            KernelInputs = root.KernelInputSet.Cast<KernelInputData>().Where(a => a.Id == kernel.KernelInputId).ToArray();
            KernelOutputs = root.KernelOutputSet.Cast<KernelOutputData>().Where(a => a.Id == kernel.KernelOutputId).ToArray();
            KernelOutputFilters = root.KernelOutputFilterSet.Cast<KernelOutputFilterData>().Where(a => a.KernelOutputId == kernel.KernelOutputId).ToArray();
            KernelOutputTranslaters = root.KernelOutputTranslaterSet.Cast<KernelOutputTranslaterData>().Where(a => a.KernelOutputId == kernel.KernelOutputId).ToArray();
            Kernels = new List<KernelData> { (KernelData)kernel };
            Packages = root.PackageSet.Cast<PackageData>().Where(a => a.Name == kernel.Package).ToList();
            CoinKernels = root.CoinKernelSet.Cast<CoinKernelData>().Where(a => localJsonObj.CoinKernelProfiles.Any(b => b.CoinKernelId == a.Id)).ToList();
            FileWriters = root.FileWriterSet.Cast<FileWriterData>().ToList();// 这个数据没几条就不精简了
            FragmentWriters = root.FragmentWriterSet.Cast<FragmentWriterData>().ToList();// 这个数据没几条就不精简了
            PoolKernels = root.PoolKernelSet.Cast<PoolKernelData>().Where(a => !string.IsNullOrEmpty(a.Args) && pools.Any(b => b.Id == a.PoolId)).ToList();
            SysDicItems = root.SysDicItemSet.Cast<SysDicItemData>().ToArray();
            SysDics = root.SysDicSet.Cast<SysDicData>().ToArray();
            TimeStamp = NTMinerRoot.ServerJson.TimeStamp;
        }

        public bool Exists<T>(Guid key) where T : IDbEntity<Guid> {
            return GetAll<T>().Any(a => a.GetId() == key);
        }

        public T GetByKey<T>(Guid key) where T : IDbEntity<Guid> {
            return GetAll<T>().FirstOrDefault(a => a.GetId() == key);
        }

        public IEnumerable<T> GetAll<T>() where T : IDbEntity<Guid> {
            string typeName = typeof(T).Name;
            switch (typeName) {
                case nameof(CoinData):
                    return this.Coins.Cast<T>();
                case nameof(GroupData):
                    return this.Groups.Cast<T>();
                case nameof(CoinGroupData):
                    return this.CoinGroups.Cast<T>();
                case nameof(CoinKernelData):
                    return this.CoinKernels.Cast<T>();
                case nameof(FileWriterData):
                    return this.FileWriters.Cast<T>();
                case nameof(FragmentWriterData):
                    return this.FragmentWriters.Cast<T>();
                case nameof(KernelData):
                    return this.Kernels.Cast<T>();
                case nameof(PackageData):
                    return this.Packages.Cast<T>();
                case nameof(KernelInputData):
                    return this.KernelInputs.Cast<T>();
                case nameof(KernelOutputData):
                    return this.KernelOutputs.Cast<T>();
                case nameof(KernelOutputTranslaterData):
                    return this.KernelOutputTranslaters.Cast<T>();
                case nameof(KernelOutputFilterData):
                    return this.KernelOutputFilters.Cast<T>();
                case nameof(PoolData):
                    return this.Pools.Cast<T>();
                case nameof(PoolKernelData):
                    return this.PoolKernels.Cast<T>();
                case nameof(SysDicData):
                    return this.SysDics.Cast<T>();
                case nameof(SysDicItemData):
                    return this.SysDicItems.Cast<T>();
                default:
                    return new List<T>();
            }
        }

        public ulong TimeStamp { get; set; }

        public CoinData[] Coins { get; set; }

        public GroupData[] Groups { get; set; }

        public CoinGroupData[] CoinGroups { get; set; }

        public KernelInputData[] KernelInputs { get; set; }

        public KernelOutputData[] KernelOutputs { get; set; }

        public KernelOutputTranslaterData[] KernelOutputTranslaters { get; set; }

        public KernelOutputFilterData[] KernelOutputFilters { get; set; }

        public List<KernelData> Kernels { get; set; }

        public List<PackageData> Packages { get; set; }

        public List<CoinKernelData> CoinKernels { get; set; }

        public List<FileWriterData> FileWriters { get; set; }

        public List<FragmentWriterData> FragmentWriters { get; set; }

        public List<PoolKernelData> PoolKernels { get; set; }

        public List<PoolData> Pools { get; set; }

        public SysDicData[] SysDics { get; set; }

        public SysDicItemData[] SysDicItems { get; set; }
    }
}
