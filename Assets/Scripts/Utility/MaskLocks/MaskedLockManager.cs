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
        public int Length { get { return MaskedLockKey.MASK_LENGTH; } }
        public uint CurrentMask { get { return IndexesToMask(); } }

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

        public uint IndexesToMask(uint checkMask = uint.MaxValue)
        {
            uint mask = 0u;
            for (int i = 0; i < Length; i++)
            {
                uint current = 1u << i;
                if (((current & checkMask) != 0) && keys[i].mask != 0)
                    mask += current;
            }
            return mask;
        }

        public static uint IndexesToMask(List<int> indexes)
        {
            uint mask = 0u;
            foreach (int index in indexes)
            {
                mask += 1u << index;
            }
            return mask;
        }

        public static List<int> MaskToIndexes(uint mask)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < MaskedLockKey.MASK_LENGTH; i++)
            {
                if ((1 & (mask >> i)) != 0)
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }

        public List<MaskedLockKey> GetOverlappingLocks(uint mask)
        {
            uint overlap = CurrentMask & mask;
            List<int> indexes = MaskToIndexes(overlap);
            List<MaskedLockKey> lockKeys = new List<MaskedLockKey>(indexes.Count);
            foreach (int index in indexes)
            {
                ListUtility.TryInsertIntoSortedList(lockKeys, keys[index], MaskedLockKey.maskComparer);
            }
            return lockKeys;
        }

        public bool PriorityOverrides(List<MaskedLockKey> lockKeys, int priority)
        {
            bool overrides = true;
            foreach (MaskedLockKey key in lockKeys)
            {
                if (key.priority >= priority)
                {
                    overrides = false;
                    break;
                }
            }
            return overrides;
        }

        public bool CanAddLock(MaskedLockKey key)
        {
            List<MaskedLockKey> overlaps = GetOverlappingLocks(key.mask);
            return PriorityOverrides(overlaps, key.priority);
        }

        public bool TryAddLock(MaskedLockKey key)
        {
            return TryAddLock(key, out List<MaskedLockKey> overlaps);
        }

        public bool TryAddLock(MaskedLockKey key, out List<MaskedLockKey> overlaps)
        {
            overlaps = GetOverlappingLocks(key.mask);
            if (PriorityOverrides(overlaps, key.priority))
            {
                foreach (MaskedLockKey lockKey in overlaps)
                {
                    TryRemoveLock(lockKey);
                }
                List<int> keyIndexes = MaskToIndexes(key.mask);
                foreach (int index in keyIndexes)
                {
                    keys[index] = key;
                }
                return true;
            }
            return false;
        }

        public bool TryRemoveLock(MaskedLockKey key)
        {
            List<int> indexes = MaskToIndexes(key.mask);
            bool success = true;
            foreach (int index in indexes)
            {
                if (keys[index] != key)
                    success = false;
            }
            if (success)
            {
                foreach (int index in indexes)
                {
                    keys[index] = MaskedLockScheme.EmptyKey;
                }
            }
            return success;
        }

        public bool LockIsOpen(MaskedLockKey key)
        {
            uint overlap = CurrentMask & key.mask;
            return overlap == 0;
        }

        public bool ContainsLock(MaskedLockKey key)
        {
            List<MaskedLockKey> lockKeys = GetOverlappingLocks(key.mask);
            foreach (MaskedLockKey lockKey in lockKeys)
            {
                if (lockKey.Equals(key))
                    return true;
            }
            return false;
        }

        public bool ContainsLockMask(MaskedLockKey key)
        {
            return ContainsLockMask(key.mask);
        }

        public bool ContainsLockMask(uint mask)
        {
            List<MaskedLockKey> lockKeys = GetOverlappingLocks(mask);
            foreach (MaskedLockKey lockKey in lockKeys)
            {
                if (lockKey == mask)
                    return true;
            }
            return false;
        }
    }
}