using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaskedLocks
{
    public interface MaskedLockProvider
    {
        public MaskedLock MaskedLock { get; }
    }
}