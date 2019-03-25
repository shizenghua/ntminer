﻿namespace NTMiner.Data {
    public class ClientCount {
        public void Update(int onlineCount, int miningCount) {
            this.OnlineCount = onlineCount;
            this.MiningCount = miningCount;
        }
        public int OnlineCount { get; private set; }
        public int MiningCount { get; private set; }
    }
}
