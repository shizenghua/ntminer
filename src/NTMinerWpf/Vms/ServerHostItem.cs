﻿namespace NTMiner.Vms {
    public class ServerHostItem {
        public ServerHostItem(string ipOrHost) {
            if (ipOrHost == null) {
                throw new System.ArgumentNullException(nameof(ipOrHost));
            }
            this.IpOrHost = ipOrHost;
            this.IsInnerIp = Ip.Util.IsInnerIp(ipOrHost);
            this.IsLocalHost = Ip.Util.IsLocalHost(ipOrHost);
        }

        public string IpOrHost { get; set; }
        public bool IsInnerIp { get; set; }
        public bool IsLocalHost { get; set; }
    }
}
