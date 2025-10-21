using System;
using UnityEngine;
using TMPro;
public class UserCreation : MonoBehaviour
{
    public static UserCreation Instance;
    public GameObject userCreationPanel;
    public TMP_InputField userNameField;
    public GameObject notEnoughCharacterInName;
    private const string userSaveKey = "User";
    private void Awake()
    {
        Instance = this;
        userCreationPanel.SetActive(string.IsNullOrEmpty(UserCreated()));
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
        LeaderBoards.Instance.InitUser(userNameField.text);
        userCreationPanel.SetActive(false);
    }

    public string UserCreated() => PlayerPrefs.GetString(userSaveKey);
}
