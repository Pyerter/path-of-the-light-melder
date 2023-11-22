using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Slowable : IIdentifiable
{
    public abstract void OnSlow(float speed);
    public virtual void SubscribeSlow()
    {
        GameManager.Instance.TimeController.SubscribeTargetedSlow(this, OnSlow); 
    }
    public virtual void UnsubscribeSlow()
    {
        GameManager.Instance.TimeController.UnsubscribeTargetedSlow(this);
    }

    public virtual void SubscribeFreeze()
    {
        GameManager.Instance.TimeController.OnFreeze += this.FreezeSlow;
    }
    public virtual void UnsubscribeFreeze()
    {
        GameManager.Instance.TimeController.OnFreeze -= this.FreezeSlow;
    }
    public void FreezeSlow(bool f)
    {
        if (f) 
            OnSlow(0);
        else 
            OnSlow(GameManager.Instance.TimeController.GetTimeScale(this));
    }

    public void SlowAwake()
    {
        SubscribeSlow();
    }
    public bool ShouldNotUpdate()
    {
        return GameManager.Instance.TimeController.Frozen;
    }
}
