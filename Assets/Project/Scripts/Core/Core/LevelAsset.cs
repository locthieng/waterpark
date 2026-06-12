using System;
using UnityEngine;

public enum LevelType
{
    Main,
    Special
    //Story
}

[Serializable]
public class LevelAsset
{
    public int ID;
    public Sprite Avatar;
    public SingleLevelController Prefab;
}