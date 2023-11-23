using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface HotSwapSupplier
{
    // Return false if this supplier should be removed
    // set animation to be the next HotSwapAnimation in the series
    // setting animation to null will result in nothing getting added
    public bool QueryHotSwap(ComplexAnimatorHotSwapper hotSwapper, out HotSwapAnimation animation);
    public virtual string SupplierName { get { return "defaultHotSwapSupplier"; } }
}
