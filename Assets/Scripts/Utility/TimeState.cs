using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloatState
{
    [SerializeField] public bool active;
    [SerializeField] public float threshold;
    [SerializeField] public System.Func<float> valueProvider;

    public FloatState(float threshold, System.Func<float> valueProvider = null, bool active = false)
    {
        this.threshold = threshold;
        this.active = active;
        if (valueProvider == null)
            this.valueProvider = () => GameManager.Time;
        else
            this.valueProvider = valueProvider;
    }

    public virtual bool AboveThreshold()
    {
        return valueProvider.Invoke() > threshold;
    }

    public virtual bool BelowThreshold()
    {
        return valueProvider.Invoke() > threshold;
    }

    public virtual void Update(bool active)
    {
        this.active = active;
    }

    public virtual bool TryUpdateAbove(bool changeTo, bool remainAt = true)
    {
        bool updated = AboveThreshold();
        if (updated)
            active = changeTo;
        else if (!remainAt)
            active = !changeTo;
        return updated;
    }

    public virtual bool TryUpdateBelow(bool changeTo, bool remainAt = true)
    {
        bool updated = BelowThreshold();
        if (updated)
            active = changeTo;
        else if (!remainAt)
            active = !changeTo;
        return updated;
    }

    public static bool operator true(FloatState state) => state.active;
    public static bool operator false(FloatState state) => !state.active;
    public static implicit operator bool(FloatState state) => state.active;
}

[System.Serializable]
public class TimeState : FloatState
{

    [SerializeField] public float previous;

    public TimeState(float initial, float threshold, System.Func<float> valueProvider = null, bool active = false) : base(threshold, valueProvider, active)
    {
        previous = initial;
    }

    public override bool AboveThreshold()
    {
        return valueProvider.Invoke() > previous + threshold;
    }

    public override bool BelowThreshold()
    {
        return valueProvider.Invoke() < previous + threshold;
    }

    public override void Update(bool active)
    {
        base.Update(active);
        this.previous = valueProvider.Invoke();
    }

    public void Update()
    {
        this.previous = valueProvider.Invoke();
    }

    public override bool TryUpdateAbove(bool changeTo, bool remainAt = true)
    {
        bool updated = base.TryUpdateAbove(changeTo, remainAt);
        if (updated)
            previous = valueProvider.Invoke();
        return updated;
    }

    public override bool TryUpdateBelow(bool changeTo, bool remainAt = true)
    {
        bool updated = base.TryUpdateBelow(changeTo, remainAt);
        if (updated)
            previous = valueProvider.Invoke();
        return updated;
    }
}
