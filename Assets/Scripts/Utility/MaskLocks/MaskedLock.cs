using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaskedLocks
{
    [CreateAssetMenu(menuName = "Masked Locks/Masked Lock")]
    public class MaskedLock : ScriptableObject
    {
        [SerializeField] protected MaskedLockScheme lockScheme;
        [SerializeField] protected string lockName = "MaskedLock";
        [SerializeField] protected int priority = 0;
        [SerializeField][HideInInspector] protected List<MaskedLockKey> keys = new List<MaskedLockKey>();

        public MaskedLockScheme LockScheme { get { return lockScheme; } }
        public string LockName { get { return lockName; } }
        public int Priority { get { return priority; } }
        public int Count { get { return keys.Count; } }
        public IReadOnlyList<MaskedLockKey> Keys { get { return keys.AsReadOnly(); } }

        protected bool compositeKeyUpdated = false;
        protected MaskedLockKey compositeKey;
        public MaskedLockKey CompositeKey
        {
            get
            {
                if (!compositeKeyUpdated)
                {
                    if (Keys.Count == 0)
                    {
                        compositeKey = MaskedLockScheme.EmptyKey;
                        compositeKeyUpdated = true;
                        return compositeKey;
                    }
                    compositeKey = keys[0];
                    compositeKey.key = lockName + "?" + compositeKey.key;
                    compositeKey.priority = priority;
                    for (int i = 1; i < keys.Count; i++)
                    {
                        compositeKey.key = compositeKey.key + "&" + keys[i].key;
                        compositeKey.mask = compositeKey + keys[i];
                    }
                    compositeKeyUpdated = true;
                }
                return compositeKey;
            }
        }

        public void MarkCompositeKeyDirty()
        {
            compositeKeyUpdated = false;
        }

        public bool Equals(MaskedLockKey key)
        {
            return Key.Equals(key);
        }

        public MaskedLockKey Key
        {
            get
            {
                return CompositeKey;
            }
        }

        public void OnValidate()
        {
            ValidateLockKeys();
        }

        public void ValidateLockKeys()
        {
            if (lockScheme == null)
                return;

            bool sorted = true;
            for (int i = 0; i < keys.Count - 1; i++)
            {
                if (keys[i].mask >= keys[i + 1].mask)
                {
                    sorted = false;
                }
            }
            if (!sorted)
            {
                List<MaskedLockKey> sortedKeys = new List<MaskedLockKey>();
                foreach (MaskedLockKey lockKey in keys)
                {
                    ListUtility.TryInsertIntoSortedList(sortedKeys, lockKey, MaskedLockKey.maskComparer);
                }
                keys = sortedKeys;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                MaskedLockKey currentKey = keys[i];
                bool updated = false;
                if (lockScheme.TryGetLockKey(currentKey.key, out MaskedLockKey schemeKey)) {
                    if (currentKey.mask != schemeKey.mask)
                    {
                        currentKey.mask = schemeKey.mask;
                        updated = true;
                    }
                    if (currentKey.priority != priority)
                    {
                        currentKey.priority = priority;
                        updated = true;
                    }
                    if (updated)
                        keys[i] = currentKey;
                } else
                {
                    keys.RemoveAt(i);
                    i--;
                }
            }
        }

        public bool TryAddKey(string key)
        {
            if (lockScheme == null)
                return false;

            if (lockScheme.TryGetLockKey(key, out MaskedLockKey lockKey)) 
            {
                ListUtility.InsertIntoSortedList(keys, lockKey, MaskedLockKey.maskComparer);
                compositeKeyUpdated = false;
                return true;
            }
            return false;
        }

        public bool TryRemoveKey(string key)
        {
            if (lockScheme == null)
                return false;

            if (lockScheme.TryGetLockKey(key, out MaskedLockKey lockKey)
                && ListUtility.TryGetIndexInSortedList(keys, lockKey, MaskedLockKey.maskComparer, out int index))
            {
                keys.RemoveAt(index);
                compositeKeyUpdated = false;
                return true;
            }
            return false;
        }

        public bool ContainsKey(string key)
        {
            if (lockScheme == null)
                return false;

            if (lockScheme.TryGetLockKey(key, out MaskedLockKey lockKey))
            {
                return ListUtility.SortedListContains(keys, lockKey, MaskedLockKey.maskComparer);
            }
            return false;
        }

        public void ClearKeys()
        {
            keys.Clear();
            compositeKeyUpdated = false;
        }
    }
}