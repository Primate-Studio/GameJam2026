using UnityEngine;
using System;

[Serializable]
public struct SoundCategory
{
    [HideInInspector] public string name; // Automatically set in the inspector
    public AudioClip[] clips;
    [Range(0, 1)] public float volume;
}