using System.Collections.Generic;
using LevelGeneration;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    [SerializeField]private GridGenerator gridGenerator;
    [SerializeField]private BallController ballController;

    public int currentLevel;
    public List<LevelDefinition> levelDefinitions;
    
    
    public void StartLevel()
    {
        gridGenerator.PrepareLayout(levelDefinitions[currentLevel]);
        ballController.PrepareBatAndBallLogic();
    }
    
    
}