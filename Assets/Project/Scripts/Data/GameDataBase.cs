using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDataBase", menuName = "Data/GameDataBase", order = 1)]
public class GameDataBase : ScriptableObject
{
    public List<Color> Colors = new List<Color>();
}
