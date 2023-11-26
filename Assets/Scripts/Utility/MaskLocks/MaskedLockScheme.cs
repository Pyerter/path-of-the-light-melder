using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaskedLocks
{
    public class MaskedLockScheme : ScriptableObject
    {
        protected static MaskedLockKey emptyKey = new MaskedLockKey("Empty", 0);
        public static MaskedLockKey EmptyKey { get { return emptyKey; } }

        [SerializeField] protected List<MaskedLockKey> lockKeys = new List<MaskedLockKey>();
        public IReadOnlyList<MaskedLockKey> LockKeys { get { return lockKeys.AsReadOnly(); } }

        [SerializeField] protected Dictionary<string, MaskedLockKey> keyDict;

        public void OnValidate()
        {
            ValidateLockKeys();
        }

        public MaskedLockKey this[string key]
        {
            get
            {
                return GetLockKey(key);
            }
        }

        public void ValidateLockKeys()
        {
            uint o = 1;
            bool updated = false;
            for (int i = 0; i < lockKeys.Count; i++)
            {
                uint current = o << i;
                if (lockKeys[i].mask != current)
                {
                    lockKeys[i] = new MaskedLockKey(lockKeys[i].key, current);
                    updated = true;
                }
            }
            if (updated)
                RebuildKeyDict();
        }

        public void RebuildKeyDict()
        {
            keyDict = new Dictionary<string, MaskedLockKey>();
            foreach (MaskedLockKey key in lockKeys)
            {
                keyDict.TryAdd(key.key, key);
            }
        }

        public uint GetMask(string key)
        {
            return keyDict.TryGetValue(key, out MaskedLockKey mask) ? mask.mask : 0;
        }

        public bool TryGetMask(string key, out uint mask)
        {
            if (keyDict.TryGetValue(key, out MaskedLockKey lockKey))
            {
                mask = lockKey.mask;
                return true;
            }
            mask = 0;
            return false;
        }

        public MaskedLockKey GetLockKey(string key)
        {
            return keyDict.TryGetValue(key, out MaskedLockKey lockKey) ? lockKey : emptyKey;
        }

        public bool TryGetLockKey(string key, out  MaskedLockKey lockKey)
        {
            return keyDict.TryGetValue(key, out lockKey);
        }
    }
}