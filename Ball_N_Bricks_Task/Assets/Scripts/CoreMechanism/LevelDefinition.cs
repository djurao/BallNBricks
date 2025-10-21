using UnityEngine;
using System;
[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Level/Level Definition", order = 1)]
public class LevelDefinition : ScriptableObject
{
    public int levelNumber;
    public Texture2D levelTexture;
    public int allowedAttempts = 3;
    public PowerUpConfiguration[] powerUps;
    
}

[Serializable]
public class PowerUpConfiguration
{
    public int amount;
    public GameObject prefab;
}
