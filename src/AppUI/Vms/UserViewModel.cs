﻿using NTMiner.User;
using NTMiner.Views;
using NTMiner.Views.Ucs;
using System;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class UserViewModel : ViewModelBase, IUser, IEditableViewModel {
        private string _loginName;
        private string _password;
        private bool _isEnabled;
        private string _description;

        public ICommand Remove { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand Save { get; private set; }
        public ICommand Enable { get; private set; }
        public ICommand Disable { get; private set; }

        public Action CloseWindow { get; set; }

        public UserViewModel(string loginName) : this() {
            _loginName = loginName;
        }

        public UserViewModel() {
            this.Save = new DelegateCommand(() => {
                if (!VirtualRoot.IsMinerStudio) {
                    return;
                }
                if (string.IsNullOrEmpty(this.LoginName)) {
                    return;
                }
                IUser user = NTMinerRoot.Current.UserSet.GetUser(this.LoginName);
                if (user != null) {
                    VirtualRoot.Execute(new UpdateUserCommand(this));
                }
                else {
                    VirtualRoot.Execute(new AddUserCommand(this));
                }
                CloseWindow?.Invoke();
            });
            this.Edit = new DelegateCommand<FormType?>((formType) => {
                UserEdit.ShowWindow(formType ?? FormType.Edit, this);
            });
            this.Remove = new DelegateCommand(() => {
                if (!VirtualRoot.IsMinerStudio) {
                    return;
                }
                if (string.IsNullOrEmpty(this.LoginName)) {
                    return;
                }
                if (VirtualRoot.IsMinerStudio && this.LoginName == SingleUser.LoginName) {
                    throw new ValidationException("不能删除自己");
                }
                DialogWindow.ShowDialog(message: $"您确定删除{this.LoginName}吗？", title: "确认", onYes: () => {
                    VirtualRoot.Execute(new RemoveUserCommand(this.LoginName));
                }, icon: IconConst.IconConfirm);
            });
            this.Enable = new DelegateCommand(() => {
                if (!VirtualRoot.IsMinerStudio) {
                    return;
                }
                if (this.IsEnabled) {
                    return;
                }
                DialogWindow.ShowDialog(message: $"您确定启用{this.LoginName}吗？", title: "确认", onYes: () => {
                    this.IsEnabled = true;
                    VirtualRoot.Execute(new UpdateUserCommand(this));
                }, icon: IconConst.IconConfirm);
            });
            this.Disable = new DelegateCommand(() => {
                if (!VirtualRoot.IsMinerStudio) {
                    return;
                }
                if (!this.IsEnabled) {
                    return;
                }
                DialogWindow.ShowDialog(message: $"您确定禁用{this.LoginName}吗？", title: "确认", onYes: () => {
                    this.IsEnabled = false;
                    VirtualRoot.Execute(new UpdateUserCommand(this));
                }, icon: IconConst.IconConfirm);
            });
        }

        public UserViewModel(IUser data) : this(data.LoginName) {
            _password = data.Password;
            _isEnabled = data.IsEnabled;
            _description = data.Description;
        }

        public void Update(IUser data) {
            this.Password = data.Password;
            this.IsEnabled = data.IsEnabled;
            this.Description = data.Description;
        }

        public string LoginName {
            get => _loginName;
            set {
                _loginName = value;
                OnPropertyChanged(nameof(LoginName));
                if (string.IsNullOrEmpty(value)) {
                    throw new ValidationException("登录名不能为空");
                }
            }
        }

        public bool IsReadOnly {
            get {
                return !string.IsNullOrEmpty(this.LoginName);
            }
        }

        public bool IsMinerStudio {
            get {
                return VirtualRoot.IsMinerStudio;
            }
        }

        public string Password {
            get => _password;
            set {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private const string _stars = "●●●●●●●●●●";
        private string _passwordStar;
        public string PasswordStar {
            get {
                if (string.IsNullOrEmpty(this.Password)) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(_passwordStar)) {
                    return _stars;
                }
                return _passwordStar;
            }
            set {
                if (_passwordStar != value) {
                    _passwordStar = value;
                    OnPropertyChanged(nameof(PasswordStar));
                    if (VirtualRoot.IsMinerStudio) {
                        this.Password = HashUtil.Sha1(value);
                    }
                    else {
                        this.Password = HashUtil.Sha1($"{HashUtil.Sha1(HashUtil.Sha1(value))}{ClientId.Id}");
                    }
                }
            }
        }

        public bool IsEnabled {
            get { return _isEnabled; }
            set {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(IsEnabledText));
            }
        }

        public string IsEnabledText {
            get {
                return this.IsEnabled ? "已启用" : "已禁用";
            }
        }

        public string Description {
            get => _description;
            set {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }
}
