﻿using System.Linq;
using System.Windows.Media;

namespace NTMiner.Vms {
    public class GpuStatusBarViewModel : ViewModelBase {
        public static readonly GpuStatusBarViewModel Instance = new GpuStatusBarViewModel();

        private GpuStatusBarViewModel() {
#if DEBUG
                VirtualRoot.Stopwatch.Restart();
#endif
            this.GpuAllVm = AppContext.Instance.GpuVms.FirstOrDefault(a => a.Index == NTMinerRoot.GpuAllId);
#if DEBUG
                Write.DevWarn($"耗时{VirtualRoot.Stopwatch.ElapsedMilliseconds}毫秒 {this.GetType().Name}.ctor");
#endif
        }

        public GpuViewModel GpuAllVm {
            get; set;
        }

        private Geometry _icon;
        public Geometry Icon {
            get {
                if (_icon == null) {
                    string iconName;
                    switch (NTMinerRoot.Instance.GpuSet.GpuType) {
                        case GpuType.NVIDIA:
                            iconName = "Icon_Nvidia";
                            break;
                        case GpuType.AMD:
                            iconName = "Icon_AMD";
                            break;
                        default:
                            iconName = "Icon_GpuEmpty";
                            break;
                    }
                    _icon = (Geometry)System.Windows.Application.Current.Resources[iconName];
                }
                return _icon;
            }
        }

        public string IconFill {
            get {
                string iconFill;
                switch (NTMinerRoot.Instance.GpuSet.GpuType) {
                    case GpuType.NVIDIA:
                        iconFill = "Green";
                        break;
                    case GpuType.AMD:
                        iconFill = "Red";
                        break;
                    case GpuType.Empty:
                    default:
                        iconFill = "Gray";
                        break;
                }
                return iconFill;
            }
        }

        public string GpuSetName {
            get {
                return NTMinerRoot.Instance.GpuSet.GpuType.GetDescription();
            }
        }

        public string GpuSetInfo {
            get {
                return NTMinerRoot.Instance.GpuSetInfo;
            }
        }

        public string GpuCountText {
            get {
                return $"x{NTMinerRoot.Instance.GpuSet.Count}";
            }
        }
    }
}
