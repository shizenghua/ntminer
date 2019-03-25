﻿using System;

namespace NTMiner.Core {
    public interface ICoinGroup : IEntity<Guid> {
        Guid GroupId { get; }
        Guid CoinId { get; }
        int SortNumber { get; }
    }
}
