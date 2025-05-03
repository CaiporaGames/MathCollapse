//Basic data container for a tween operation:

using UnityEngine;
using System;

public class Tween
{
    public RectTransform Target;
    public Vector2 StartPos;
    public Vector2 EndPos;
    public float Duration;
    public float Elapsed;
    public Action OnComplete;

    public Tween(RectTransform target, Vector2 to, float duration, Action onComplete = null)
    {
        Target = target;
        StartPos = target.anchoredPosition;
        EndPos = to;
        Duration = duration;
        Elapsed = 0f;
        OnComplete = onComplete;
    }

    public bool Update(float deltaTime)
    {
        Elapsed += deltaTime;
        float t = Mathf.Clamp01(Elapsed / Duration);
        Vector2 pos = Vector2.Lerp(StartPos, EndPos, t);
        Target.anchoredPosition = pos;

        if (Elapsed >= Duration)
        {
            OnComplete?.Invoke();
            return true; // Done
        }
        return false; // Still running
    }
}