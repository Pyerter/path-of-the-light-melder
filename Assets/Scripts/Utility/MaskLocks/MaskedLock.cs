using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaskedLocks
{
    public class MaskedLock : MonoBehaviour
    {
        [SerializeField] protected MaskedLockKey key;
        public MaskedLockKey Key { get { return key; } }
    }
}