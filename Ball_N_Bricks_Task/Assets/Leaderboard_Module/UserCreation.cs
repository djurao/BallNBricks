using System;
using System.Collections.Generic;
using Leaderboard_Module;
using UnityEngine;
using TMPro;
public class UserCreation : MonoBehaviour
{
    public static UserCreation Instance;
    public GameObject userCreationPanel;
    public TMP_InputField userNameField;
    public GameObject notEnoughCharacterInName;
    private const string userSaveKey = "User";
    public UserDto user;

    private void Awake()
    {
        Instance = this;
        // Try load existing user
        if (LoadUser()) {
            userCreationPanel.SetActive(false);
        } else {
            userCreationPanel.SetActive(true);
        }
    }

    public void CreateUser()
    {
        if (userNameField.text.Length < 3)
        {
            userCreationPanel.SetActive(false);
            notEnoughCharacterInName.SetActive(true);
            return;
        }
        PlayerPrefs.SetString(userSaveKey, userNameField.text);
        InitUser(userNameField.text);
        userCreationPanel.SetActive(false);
        SaveUser();
    }
    private void InitUser(string userName)
    {
        user = new UserDto
        {
            id = 664221,
            name = userName,
            textureBase64 = null,
            levelScores = new List<LevelScoreData>
            {
                new LevelScoreData { levelID = 0, score = 0 },
                new LevelScoreData { levelID = 1, score = 0 },
                new LevelScoreData { levelID = 2, score = 0 }
            }
        };
    }
    public void SaveUser()
    {
        if (user == null) return;
        var json = JsonUtility.ToJson(user);
        PlayerPrefs.SetString(userSaveKey, json);
        PlayerPrefs.Save();
    }

    private bool LoadUser()
    {
        if (!PlayerPrefs.HasKey(userSaveKey)) return false;
        var json = PlayerPrefs.GetString(userSaveKey);
        if (string.IsNullOrEmpty(json)) return false;

        try
        {
            user = JsonUtility.FromJson<UserDto>(json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to parse saved user JSON: " + e.Message);
            return false;
        }
    }
}