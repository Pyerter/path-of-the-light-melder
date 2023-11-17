/*
 * Author: Porter Squires
 */

using System.Collections.Generic;
using UnityEngine;

namespace BufferedInput
{
    [AddComponentMenu("Buffered Input/Control Lock Manager")]
    public class ControlLockManager : MonoBehaviour
    {
        /// <summary>
        /// Used to track the active ControlLockers for this manager.
        /// </summary>
        public List<ControlLocker> activeLockers = new List<ControlLocker>();

        /// <summary>
        /// Used to designate certain controls that ignore locks.
        /// </summary>
        public List<ControlLocker> lockBypassers = new List<ControlLocker>();

        /// <summary>
        /// Used to reference the composite of control locks made by
        /// combining the locks from all active ControlLockers.
        /// </summary>
        public ControlLock CompositeLock
        {
            get
            {
                if (!compositeUpdated)
                {
                    compositeLock = new ControlLock(activeLockers);
                    if (useBypassers)
                        compositeLock.RemoveFromComposite(lockBypassers);
                }
                return compositeLock;
            }
        }
        private bool compositeUpdated = false;
        private ControlLock compositeLock;
        private bool useBypassers = true;
        public bool UseBypassers { get { return useBypassers; } set{ useBypassers = value; compositeUpdated = false; } }

        /// <summary>
        /// Adds a designated ControlLocker to the list of active lockers.
        /// This will add all locks present in the ControlLocker.Lock object
        /// to the CompositeLock of this object.
        /// </summary>
        /// <param name="cl"> the ControlLocker to add </param>
        public void AddLocker(ControlLocker cl)
        {
            if (!activeLockers.Contains(cl))
            {
                activeLockers.Add(cl);
                compositeUpdated = false;
            }
        }

        public void AddBypass(ControlLocker cl)
        {
            if (!lockBypassers.Contains(cl))
            {
                lockBypassers.Add(cl);
                compositeUpdated = false;
            }
        }

        /// <summary>
        /// Removes a designated ControlLocker from the list of active lockers.
        /// This will potentially remove all locks present in the ControlLocker.Lock
        /// object from the CompositeLock of this object. If there are other lockers
        /// that lock certain controls, then those controls will remain locked until
        /// the corresponding lockers are also removed.
        /// </summary>
        /// <param name="cl"> the ControlLocker to remove </param>
        /// <returns> true if the ControlLocker was successfully removed </returns>
        public bool RemoveLocker(ControlLocker cl)
        {
            compositeUpdated = false;
            return activeLockers.Remove(cl);
        }

        public bool RemoveBypasser(ControlLocker cl)
        {
            compositeUpdated = false;
            return lockBypassers.Remove(cl);
        }

        /// <summary>
        /// Used to check if all of the designated controls are compatible with
        /// the currently locked input in the CompositeLock of this object.
        /// </summary>
        /// <param name="controls"> the controls to be checked for compatibility </param>
        /// <returns> 
        /// true if the CompositeLock doesn't block any of the controls passed in 
        /// </returns>
        public bool ControlsAllowed(ActionMask controls)
        {
            return !ControlLock.IsLocking(controls, CompositeLock);
        }

        public bool ControlsLocked(ActionMask controls)
        {
            return ControlLock.IsLocking(controls, CompositeLock);
        }

        public bool LockIsUsed(ControlLocker cl)
        {
            return activeLockers.Contains(cl);
        }
    }
}