using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Target Sequence Data", menuName = "Scriptable Objects/Target Sequence Data")]

public class TargetSequenceData : ScriptableObject
{
    public string description;
    public List<TargetEntry> targets = new List<TargetEntry>();
}

[System.Serializable]
public class TargetEntry
{
    public int number;
    public int count;
}
