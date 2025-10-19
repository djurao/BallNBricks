using UnityEngine;

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Level/Level Definition", order = 1)]
public class LevelDefinition : ScriptableObject
{
    public int levelNumber;
    public Texture2D levelTexture;
    public int allowedAttempts = 3;
    public int numberOfCollectables;
}
