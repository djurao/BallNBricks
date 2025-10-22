using System;
using System.Collections.Generic;
using CoreMechanism;
using LevelGeneration;
using Misc;
using TMPro;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    [SerializeField]private GridGenerator gridGenerator;
    [SerializeField]private BallController ballController;

    public bool levelInProgress;
    public int currentLevel;
    public GameObject[] attemptUIElements;
    public int attempts;
    public int maxAttempts;
    public List<LevelDefinition> levelDefinitions;
    [Header("UI")]
    public GameObject startLevelButton;
    public TextMeshProUGUI currentLevelLabel;
    public TextMeshProUGUI currentLevelLeaderboardsLabel;
    public GameObject nextLevelButton;
    public GameObject startOverButton;
    public GameObject newBestScorePopup;
    public TextMeshProUGUI newBestScoreLabel;

    [Header("Level Finished")]
    public GameObject levelFinishedPopup;
    public GameObject noEnoughCurrencyPopup;
    public GameObject levelFailedPopup;
    public TextMeshProUGUI scoreLevelFailedLabel;

    public GameObject confirmApplyModifier;
    public GameObject multiplierAlreadyAppliedPopup;

    public TextMeshProUGUI scoreLabelFinishPopup;
    public TextMeshProUGUI oldScoreLabelApplyPopup;
    public TextMeshProUGUI newScoreLabelApplyPopup;
    public int scoreThisLevel;
    public MultiplierDefinition[] multiplierDefinitions;
    public int amountToPay;
    public int multiplierPendingForApply;
    public bool multiplierApplied;
    public bool allBricksDestroyed;
    private void Awake()
    {
        ballController.OnBallsReturnedToBase += SubtractAttempt;
    }

    private void Start()
    {
        PrepareLevel(0);
    }

    void OnDestroy() => ballController.OnBallsReturnedToBase -= SubtractAttempt;

    public void PrepareLevel(int id)
    {
        multiplierApplied = false;
        var levelNormalized = currentLevel + 1;
        currentLevelLabel.text = $"Level {levelNormalized}";
        currentLevelLeaderboardsLabel.text = $"Leaderboards for Level {levelNormalized}";
        maxAttempts = levelDefinitions[currentLevel].allowedAttempts;
        attempts = maxAttempts;
        UpdateAttemptUIState();
        levelFinishedPopup.SetActive(false);
        levelFailedPopup.SetActive(false);
        startLevelButton.SetActive(true);
        Score.Instance.ResetScore();
    }

    public void StartLevel()
    {
        gridGenerator.PrepareLayout(levelDefinitions[currentLevel]);
        ballController.PrepareBatAndBallLogic();
        levelInProgress = true;
    }

    void Update()
    {
        if (!levelInProgress) return;
        allBricksDestroyed = gridGenerator.bricksDestroyedThisLevel == gridGenerator.bricksInstantiated.Count;
        if (allBricksDestroyed)
        {
            LevelFinished();
        }
    }

    private void SubtractAttempt()
    {
        attempts--;
        UpdateAttemptUIState();
        if (attempts <= 0)
            LevelFinished();
    }

    private void LevelFinished()
    {
        levelInProgress = false;
        gridGenerator.CleanUpCurrentLevelRemnants();
        ballController.LockBatInteraction();
        scoreThisLevel = Score.Instance.GetScore();
        PowerUps.Instance.DeactivateAllPowerUps();
        UpdateScoreLabel();

        if (allBricksDestroyed)
        {
            levelFinishedPopup.SetActive(true);
            nextLevelButton.SetActive(currentLevel != levelDefinitions.Count - 1);
            startOverButton.SetActive(currentLevel == levelDefinitions.Count - 1);
            UpdateUserScore();
            ballController.RetractAllBallsToBase();
        }
        else
        {
            scoreLevelFailedLabel.text = $"Score: {scoreThisLevel}";
            levelFailedPopup.SetActive(true);
        }
    }

    public void ShowMultiplierAlreadyAppliedPopup(bool state) => multiplierAlreadyAppliedPopup.SetActive(state);

    public void TryApplyScoreMultiplier(int index)
    {
        if (multiplierApplied)
        {
            ShowMultiplierAlreadyAppliedPopup(true);
            return;
        }
        if (index == 0)
        {
            UpdateScoreLabel();
            scoreLabelFinishPopup.color = Color.white;
            amountToPay = 0;
            multiplierPendingForApply = 0;
            return;
        }

        if (HardCurrency.Instance.amount >= multiplierDefinitions[index].price)
        {
            multiplierPendingForApply = multiplierDefinitions[index].multiplier;
            amountToPay = multiplierDefinitions[index].price;
            oldScoreLabelApplyPopup.text = $"Score: {scoreThisLevel}";
            newScoreLabelApplyPopup.text = $"New Score: {scoreThisLevel*multiplierPendingForApply}";
            scoreLabelFinishPopup.color = Color.green;
            OpenCloseConfirmationPanel(true);
        }
        else
        {
            HardCurrency.Instance.OpenClosePanel(true);
        }
    }
    private void UpdateScoreLabel() => scoreLabelFinishPopup.text = $"Score: {scoreThisLevel}";
    public void ApplyModifier()
    {
        scoreThisLevel *= multiplierPendingForApply;
        UpdateScoreLabel();
        scoreLabelFinishPopup.color = Color.green;
        multiplierApplied = true;
        FinalizeTransaction();
        OpenCloseConfirmationPanel(false);
        UpdateUserScore();
    }

    private void UpdateUserScore()
    {
        if (UserCreation.Instance.user.levelScores[currentLevel].score < scoreThisLevel)
        {
            newBestScorePopup.SetActive(true);
            newBestScoreLabel.text = $"New Best: {scoreThisLevel}!";
            LeaderBoards.Instance.AddUserScore(currentLevel, scoreThisLevel);
        }
        LeaderBoards.Instance.RefreshLeaderBoard(currentLevel, scoreThisLevel);
        UserCreation.Instance.SaveUser();
    }

    private void OpenCloseConfirmationPanel(bool state) => confirmApplyModifier.SetActive(state);

    private void FinalizeTransaction()
    {
        HardCurrency.Instance.DeductCurrency(amountToPay);
        amountToPay = 0;
        multiplierPendingForApply = 0;
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

    public void StartOver()
    {
        currentLevel = 0;
        PrepareLevel(currentLevel);
    }

    public void ProceedToLeaderboard() => LeaderBoards.Instance.OpenLeaderboards(currentLevel, scoreThisLevel);

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
[Serializable]
public class MultiplierDefinition
{
    public int price;
    public int multiplier;
}