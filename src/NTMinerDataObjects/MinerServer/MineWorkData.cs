﻿using System;
using System.Text;

namespace NTMiner.MinerServer {
    public class MineWorkData : IMineWork, IDbEntity<Guid>, ITimestampEntity<Guid>, IGetSignData {
        public MineWorkData() {
            this.CreatedOn = DateTime.Now;
        }

        public MineWorkData(IMineWork data) {
            this.Id = data.GetId();
            this.Name = data.Name;
            this.Description = data.Description;
            this.ServerJsonSha1 = data.ServerJsonSha1;
        }

        public Guid GetId() {
            return this.Id;
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public string ServerJsonSha1 { get; set; }

        public StringBuilder GetSignData() {
            return this.BuildSign();
        }
    }
}
