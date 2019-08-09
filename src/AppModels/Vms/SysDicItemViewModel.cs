﻿using NTMiner.Core;
using System;
using System.Linq;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class SysDicItemViewModel : ViewModelBase, ISysDicItem, IEditableViewModel {
        public static readonly SysDicItemViewModel PleaseSelect = new SysDicItemViewModel(Guid.Empty) {
            _code = string.Empty,
            _value = "不指定",
            _description = "不指定",
            _sortNumber = -1
        };

        private Guid _id;
        private Guid _dicId;
        private string _code;
        private string _value;
        private string _description;
        private int _sortNumber;
        private DataLevel _dataLevel;
        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand SortUp { get; private set; }
        public ICommand SortDown { get; private set; }
        public ICommand Save { get; private set; }

        public Action CloseWindow { get; set; }

        public Guid GetId() {
            return this.Id;
        }

        public SysDicItemViewModel(ISysDicItem data) : this(data.GetId()) {
            _dicId = data.DicId;
            _code = data.Code;
            _value = data.Value;
            _description = data.Description;
            _sortNumber = data.SortNumber;
        }

        public SysDicItemViewModel(Guid id) {
            _id = id;
            this.Save = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                if (NTMinerRoot.Instance.SysDicItemSet.ContainsKey(this.Id)) {
                    VirtualRoot.Execute(new UpdateSysDicItemCommand(this));
                }
                else {
                    VirtualRoot.Execute(new AddSysDicItemCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand<FormType?>((formType) => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                VirtualRoot.Execute(new SysDicItemEditCommand(formType ?? FormType.Edit, this));
            });
            this.Remove = new DelegateCommand(() => {
                if (this.Id == Guid.Empty) {
                    return;
                }
                this.ShowDialog(message: $"您确定删除{this.Code}系统字典项吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new RemoveSysDicItemCommand(this.Id));
                }, icon: IconConst.IconConfirm);
            });
            this.SortUp = new DelegateCommand(() => {
                SysDicItemViewModel upOne = AppContext.Instance.SysDicItemVms.List.OrderByDescending(a => a.SortNumber).FirstOrDefault(a => a.SortNumber < this.SortNumber);
                if (upOne != null) {
                    int sortNumber = upOne.SortNumber;
                    upOne.SortNumber = this.SortNumber;
                    VirtualRoot.Execute(new UpdateSysDicItemCommand(upOne));
                    this.SortNumber = sortNumber;
                    VirtualRoot.Execute(new UpdateSysDicItemCommand(this));
                    if (AppContext.Instance.SysDicVms.TryGetSysDicVm(this.DicId, out SysDicViewModel sysDicVm)) {
                        sysDicVm.OnPropertyChanged(nameof(sysDicVm.SysDicItems));
                        sysDicVm.OnPropertyChanged(nameof(sysDicVm.SysDicItemsSelect));
                    }
                }
            });
            this.SortDown = new DelegateCommand(() => {
                SysDicItemViewModel nextOne = AppContext.Instance.SysDicItemVms.List.OrderBy(a => a.SortNumber).FirstOrDefault(a => a.SortNumber > this.SortNumber);
                if (nextOne != null) {
                    int sortNumber = nextOne.SortNumber;
                    nextOne.SortNumber = this.SortNumber;
                    VirtualRoot.Execute(new UpdateSysDicItemCommand(nextOne));
                    this.SortNumber = sortNumber;
                    VirtualRoot.Execute(new UpdateSysDicItemCommand(this));
                    if (AppContext.Instance.SysDicVms.TryGetSysDicVm(this.DicId, out SysDicViewModel sysDicVm)) {
                        sysDicVm.OnPropertyChanged(nameof(sysDicVm.SysDicItems));
                        sysDicVm.OnPropertyChanged(nameof(sysDicVm.SysDicItemsSelect));
                    }
                }
            });
        }

        public DataLevel DataLevel {
            get { return _dataLevel; }
            set {
                if (_dataLevel != value) {
                    _dataLevel = value;
                    OnPropertyChanged(nameof(DataLevel));
                    OnPropertyChanged(nameof(DataLevelText));
                    OnPropertyChanged(nameof(IsReadOnly));
                }
            }
        }

        public bool IsReadOnly {
            get {
                if (!DevMode.IsDebugMode && this.DataLevel == DataLevel.Global) {
                    return true;
                }
                return false;
            }
        }

        public string DataLevelText {
            get {
                return this.DataLevel.GetDescription();
            }
        }

        public void SetDataLevel(DataLevel dataLevel) {
            this.DataLevel = dataLevel;
        }

        public Guid Id {
            get => _id;
            private set {
                if (_id != value) {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public Guid DicId {
            get => _dicId;
            set {
                if (_dicId != value) {
                    _dicId = value;
                    OnPropertyChanged(nameof(DicId));
                }
            }
        }

        public string Code {
            get => _code;
            set {
                if (_code != value) {
                    _code = value;
                    OnPropertyChanged(nameof(Code));
                    if (this == PleaseSelect) {
                        return;
                    }
                    if (string.IsNullOrEmpty(value)) {
                        throw new ValidationException("编码是必须的");
                    }
                    if (AppContext.Instance.SysDicItemVms.List.Any(a => a.DicId == this.DicId && a.Code == value && a.Id != this.Id)) {
                        throw new ValidationException("编码重复");
                    }
                }
            }
        }

        public string Value {
            get => _value;
            set {
                if (_value != value) {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public string Description {
            get => _description;
            set {
                if (_description != value) {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public int SortNumber {
            get => _sortNumber;
            set {
                if (_sortNumber != value) {
                    _sortNumber = value;
                    OnPropertyChanged(nameof(SortNumber));
                }
            }
        }

        public SysDicViewModel SysDicVm {
            get {
                AppContext.Instance.SysDicVms.TryGetSysDicVm(this.DicId, out SysDicViewModel sysDicVm);
                return sysDicVm;
            }
        }
    }
}
