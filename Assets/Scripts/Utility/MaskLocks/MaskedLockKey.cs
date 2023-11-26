using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaskedLocks
{
    [Serializable]
    public struct MaskedLockKey
    {
        [SerializeField] public string key;
        [SerializeField] public int priority;
        [SerializeField] public uint mask;

        public MaskedLockKey(string key, uint mask, int priority = 0)
        {
            this.key = key;
            this.mask = mask;
            this.priority = priority;
        }

        public static uint operator &(uint left, MaskedLockKey right)
        {
            return left & right.mask;
        }

        public static uint operator &(MaskedLockKey left, uint right)
        {
            return left.mask & right;
        }

        public static uint operator |(uint left, MaskedLockKey right)
        {
            return left | right.mask;
        }

        public static uint operator |(MaskedLockKey left, uint right)
        {
            return left.mask | right;
        }

        public static uint operator +(MaskedLockKey left, MaskedLockKey right)
        {
            return left.mask | right.mask;
        }

        public static uint operator -(MaskedLockKey left, MaskedLockKey right)
        {
            return left.mask & (~right.mask);
        }

        public static bool operator ==(MaskedLockKey left, MaskedLockKey right)
        {
            return left.mask == right.mask && left.priority == right.priority;
        }

        public static bool operator !=(MaskedLockKey left, MaskedLockKey right)
        {
            return !(left == right);
        }

        public static bool operator <(MaskedLockKey left, MaskedLockKey right)
        {
            return left.priority < right.priority;
        }

        public static bool operator >(MaskedLockKey left, MaskedLockKey right)
        {
            return left.priority > right.priority;
        }

        public bool MaskEquals(object obj)
        {
            try
            {
                if (obj is MaskedLockKey)
                {
                    MaskedLockKey lockKey = (MaskedLockKey)obj;
                    return this.mask == lockKey.mask;
                }
                return false;
            } catch
            {
                return false;
            }
        }

        public bool PriorityEquals(object obj)
        {
            try
            {
                if (obj is MaskedLockKey)
                {
                    MaskedLockKey lockKey = (MaskedLockKey)obj;
                    return this.priority == lockKey.priority;
                }
                return false;
            } catch
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            try
            {
                if (obj is MaskedLockKey)
                {
                    MaskedLockKey lockKey = (MaskedLockKey)obj;
                    return this.mask == lockKey.mask && key.ToLower().Equals(lockKey.key.ToLower());
                }
                return false;
            } catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}