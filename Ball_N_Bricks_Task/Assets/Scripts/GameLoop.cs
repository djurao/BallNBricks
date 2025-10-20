using System;
using System.Collections.Generic;
using LevelGeneration;
using TMPro;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    [SerializeField]private GridGenerator gridGenerator;
    [SerializeField]private BallController ballController;

    public int currentLevel;
    public GameObject[] attemptUIElements;
    public int attempts;
    public int maxAttempts;
    public List<LevelDefinition> levelDefinitions;
    public GameObject levelFinishedPopup;
    public GameObject startLevelButton;
    public TextMeshProUGUI currentLevelLabel;
    private void Awake()
    {
        ballController.OnBallsReturnedToBase += SubtractAttempt;
        PrepareLevel(0);
    }
    void OnDestroy() => ballController.OnBallsReturnedToBase -= SubtractAttempt;

    public void PrepareLevel(int id)
    {
        var levelNormalized = currentLevel + 1;
        currentLevelLabel.text = $"Level {levelNormalized}";
        maxAttempts = levelDefinitions[currentLevel].allowedAttempts;
        attempts = maxAttempts;
        UpdateAttemptUIState();
        levelFinishedPopup.SetActive(false);
        startLevelButton.SetActive(true);
    }

    public void StartLevel()
    {
        gridGenerator.PrepareLayout(levelDefinitions[currentLevel]);
        ballController.PrepareBatAndBallLogic();
    }

    private void SubtractAttempt()
    {
        attempts--;
        UpdateAttemptUIState();
        if (attempts <= 0)
            OnAllAttemptsUsed();
    }

    private void OnAllAttemptsUsed()
    {
        levelFinishedPopup.SetActive(true);
        gridGenerator.CleanUpCurrentLevelRemnants();
        ballController.LockBatInteraction();
        // inject score
    }

    public void PlayAgain() => PrepareLevel(currentLevel);
    public void NextLevel()
    {
        if (currentLevel + 1 < levelDefinitions.Count)
        {
            currentLevel++;
        }
        PrepareLevel(currentLevel);
    }

    private void UpdateAttemptUIState()
    {
        if (attemptUIElements == null) return;
        for (var i = 0; i < attemptUIElements.Length; i++)
        {
            var active = i < attempts && i < maxAttempts;
            attemptUIElements[i].SetActive(active);
        }
    }
}