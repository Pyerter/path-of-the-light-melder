using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaskedLocks
{
    public class MaskedLockManager : MonoBehaviour
    {
        [SerializeField] protected MaskedLockScheme lockScheme;
        public MaskedLockScheme LockScheme {  get { return lockScheme; } }

        protected MaskedLockKey[] keys;
        protected uint currentLockMask = 0;
        protected bool initialized = false;
        public bool Initialized { get { return initialized; } }
        public int Length { get { return 32; } }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (initialized)
                return;

            currentLockMask = 0;
            keys = new MaskedLockKey[Length];
            for (int i = 0; i < Length; i++)
                keys[i] = MaskedLockScheme.EmptyKey;
            initialized = true;
        }

        // TODO: Add code to add locks only if currentLockMask & newLock == 0
        // and check if a lock currently exists in this set of locks
        public bool TryAddLock(MaskedLockKey key)
        {
            return false;
        }
    }
}