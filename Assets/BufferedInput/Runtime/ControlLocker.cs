/*
 * Author: Porter Squires
 */

namespace BufferedInput
{
    public interface ControlLocker
    {
        public ControlLock Lock { get; }
    }
}