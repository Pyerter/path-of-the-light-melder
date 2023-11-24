/*
 * Author: Porter Squires
 */

using UnityEngine;

namespace BufferedInput
{
    [CreateAssetMenu(menuName = "Buffered Input/Control Locks/Standard Locker")]
    public class StandardControlLocker : ScriptableObject, ControlLocker
    {
        [SerializeField] private ControlLockScheme controlLockScheme;
        public ControlLockScheme LockScheme { get { return controlLockScheme; } }
        [SerializeField] private ControlLock controlLock;
        public ControlLock Lock { get { return controlLock; } }
    }
}