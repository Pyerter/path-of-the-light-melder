using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryExistence : MonoBehaviour
{
    [SerializeField]
    protected float timeDone = 0f;
    public float TimeDone { get { return timeDone; } set { timeDone = value; } }

    [SerializeField]
    protected float duration = 1f;
    public float Duration { get { return duration; } private set { duration = value; } }

    public event System.Action DisappearEvent;

    private void OnEnable()
    {
        timeDone = GameManager.Time + Duration;
        OnAppear();
    }

    private void OnDisable()
    {
        OnDisappear();
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.Paused) 
        {
            OnUpdate();
        }
        if (GameManager.Time > timeDone)
        {
            OnDisappear();
        }
    }

    public float GetPercentTimeLeft()
    {
        return (TimeDone - GameManager.Time) / Duration;
    }

    public virtual void OnAppear()
    {

    }

    public virtual void OnDisappear()
    {
        gameObject.SetActive(false);
        DisappearEvent?.Invoke();
    }

    public virtual void OnUpdate()
    {

    }
}
