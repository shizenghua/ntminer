﻿using NTMiner.Core;
using NTMiner.Vms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NTMiner {
    public partial class AppContext {
        public class ColumnsShowViewModels : ViewModelBase {
            public static readonly ColumnsShowViewModels Instance = new ColumnsShowViewModels();

            private readonly Dictionary<Guid, ColumnsShowViewModel> _dicById = new Dictionary<Guid, ColumnsShowViewModel>();

            public ICommand Add { get; private set; }

            private ColumnsShowViewModels() {
#if DEBUG
                VirtualRoot.Stopwatch.Restart();
#endif
                this.Add = new DelegateCommand(() => {
                    new ColumnsShowViewModel(Guid.NewGuid()).Edit.Execute(FormType.Add);
                });
                On<ColumnsShowAddedEvent>("添加了列显后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        if (!_dicById.ContainsKey(message.Source.GetId())) {
                            ColumnsShowViewModel vm = new ColumnsShowViewModel(message.Source);
                            _dicById.Add(message.Source.GetId(), vm);
                            OnPropertyChanged(nameof(List));
                            AppContext.Instance.MinerClientsWindowVm.ColumnsShow = vm;
                        }
                    });
                On<ColumnsShowUpdatedEvent>("更新了列显后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        if (_dicById.ContainsKey(message.Source.GetId())) {
                            ColumnsShowViewModel entity = _dicById[message.Source.GetId()];
                            entity.Update(message.Source);
                        }
                    });
                On<ColumnsShowRemovedEvent>("移除了列显后刷新VM内存", LogEnum.DevConsole,
                    action: message => {
                        AppContext.Instance.MinerClientsWindowVm.ColumnsShow = _dicById.Values.FirstOrDefault();
                        _dicById.Remove(message.Source.GetId());
                        OnPropertyChanged(nameof(List));
                    });
                foreach (var item in NTMinerRoot.Instance.ColumnsShowSet) {
                    _dicById.Add(item.GetId(), new ColumnsShowViewModel(item));
                }
#if DEBUG
                Write.DevWarn($"耗时{VirtualRoot.Stopwatch.ElapsedMilliseconds}毫秒 {this.GetType().Name}.ctor");
#endif
            }

            public List<ColumnsShowViewModel> List {
                get {
                    return _dicById.Values.ToList();
                }
            }
        }
    }
}
