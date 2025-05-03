//API to trigger tweens like DOTween-style:

using UnityEngine;

public static class TweenExtensions
{
    public static void DoMove(this RectTransform rect, Vector2 to, float duration, System.Action onComplete = null)
    {
        Tween tween = new Tween(rect, to, duration, onComplete);
        TweenRunner.AddTween(tween);
    }
}