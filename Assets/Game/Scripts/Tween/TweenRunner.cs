//Global runner that ticks active tweens.

using UnityEngine;
using System.Collections.Generic;

public class TweenRunner : MonoBehaviour
{
    private static TweenRunner instance;
    private readonly List<Tween> activeTweens = new();

    public static void AddTween(Tween tween)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("TweenRunner");
            instance = go.AddComponent<TweenRunner>();
            DontDestroyOnLoad(go);
        }

        instance.activeTweens.Add(tween);
    }

    private void Update()
    {
        for (int i = activeTweens.Count - 1; i >= 0; i--)
        {
            if (activeTweens[i].Update(Time.deltaTime))
                activeTweens.RemoveAt(i);
        }
    }
}