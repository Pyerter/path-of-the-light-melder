/*
 * Author: Porter Squires
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BufferedInput
{
    [Serializable]
    public class ControlLock
    {
        /// <summary>
        /// Priority property is used to track the priority of this lock. This becomes relevant
        /// when multiple locks are going into affect that conflict with each other.
        /// </summary>
        [SerializeField] private int priority;
        public int Priority { get { return priority; } private set { priority = value; } }

        /// <summary>
        /// Composite is used to represent the combined mask of all locks.
        /// </summary>
        [SerializeField] private ActionMask locks;
        public ActionMask Locks { get { return locks; } }

        public ControlLock()
        {
            SetComposite(new List<ActionMask>());
        }

        public ControlLock(List<ActionMask> locks)
        {
            SetComposite(locks);
        }

        public ControlLock(List<ControlLocker> locks)
        {
            SetComposite(locks);
        }

        public void SetComposite(List<ControlLocker> locks)
        {
            long bitMask = 0L;
            foreach (ControlLocker locker in locks)
            {
                bitMask |= locker.Lock.Locks.Mask;
            }
            this.locks = new ActionMask(bitMask, "Composite");
        }

        public void SetComposite(List<ActionMask> locks)
        {
            long bitMask = 0L;
            foreach (ActionMask mask in locks)
            {
                bitMask |= mask.Mask;
            }
            this.locks = new ActionMask(bitMask, "Composite");
        }

        public void RemoveFromComposite(List<ControlLocker> locks)
        {
            long bitMask = this.locks.Mask;
            foreach (ControlLocker locker in locks)
            {
                bitMask &= ~locker.Lock.Locks.Mask;
            }
            this.locks = new ActionMask(bitMask, "Composite");
        }

        public void RemoveFromComposite(List<ActionMask> locks)
        {
            long bitMask = this.locks.Mask;
            foreach (ActionMask mask in locks)
            {
                bitMask &= ~mask.Mask;
            }
            this.locks = new ActionMask(bitMask, "Composite");
        }

        /// <summary>
        /// Checks if any of the target controls would be locked by 
        /// this object's locks.
        /// </summary>
        /// <param name="target"> the target controls that might be locked </param>
        /// <returns> true only if any of the target controls are affected </returns>
        public bool IsLocking(ActionMask target)
        {
            return (target.Mask & locks.Mask) > 0;
        }

        /// <summary>
        /// Checks if any of the target controls would be locked by 
        /// the designated locker.
        /// </summary>
        /// <param name="target"> the target controls that might be locked </param>
        /// <param name="locker"> the target locker which will be used as a reference </param>
        /// <returns> true only if any of the target controls are affected </returns>
        public static bool IsLocking(ActionMask target, ActionMask locker)
        {
            return (target.Mask & locker.Mask) > 0;
        }

        /// <summary>
        /// Check if all of the target controls would be locked by
        /// this object's locks.
        /// </summary>
        /// <param name="target"> the target controls that might be locked </param>
        /// <returns> true only if this object's locks affect all of the controls in target </returns>
        public bool IsLockingAll(ActionMask target)
        {
            return (target.Mask & locks.Mask) == target.Mask;
        }

        /// <summary>
        /// Check if all of the target controls would be locked by
        /// thie designated locker.
        /// </summary>
        /// <param name="target"> the target controls that might be locked </param>
        /// <param name="locker"> the target locker which will be used as a reference </param>
        /// <returns> true only if this object's locks affect all of the controls in target </returns>
        public static bool IsLockingAll(ActionMask target, ActionMask locker)
        {
            return (target.Mask & locker.Mask) == target.Mask;
        }

        public static bool IsLocking(ActionMask target, ControlLock locker)
        {
            return IsLocking(target, locker.locks);
        }

        public static bool IsLockingAll(ActionMask target, ControlLock locker)
        {
            return IsLockingAll(target, locker.locks);
        }
    }
}