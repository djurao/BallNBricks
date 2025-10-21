using System;
using System.Collections.Generic;
using Leaderboard_Module;
using UnityEngine;
using System.Linq;
public class LeaderBoards : MonoBehaviour
{
    public static LeaderBoards Instance;
    public GameObject leaderboardsPanel;    
    public LeaderboardThumb thumbPrefab;
    public List<LeaderboardThumb> entryThumbs;
    public Transform content;
    public SerializableUserList  userList;
    public List<UserDto> sortedUsers;
    public int currentLevel;
    private LeaderboardThumb usersThumb;
    public UserDto user;
    public int lastVisibleThumbEntry = 15;

    private void Awake() => Instance = this;

    private void Start()
    {
        var userName = UserCreation.Instance.UserCreated();
        if (!string.IsNullOrEmpty(userName))
        {
           InitUser(userName); 
        }
    }

    public void InitUser(string userName)
    {
        var newId = userList.users.Count == 0 ? 1 : userList.users.Max(u => u.id) + 1;
        user = new UserDto
        {
            id = newId,
            name = userName,
            textureBase64 = null,
            levelScores = new List<LevelScoreData>
            {
                new LevelScoreData { levelID = 1, score = 0 },
                new LevelScoreData { levelID = 2, score = 0 },
                new LevelScoreData { levelID = 3, score = 0 }
            }
        };
        userList.users.Add(user);
    }
    private void FetchUsers() => userList = SaveLoadLeaderboards.LoadDummyUsers();
    private void SortUsersByScoreForSpecificLevel(int level)
    {
        sortedUsers = userList.users
            .OrderByDescending(u => u.levelScores?.FirstOrDefault(ls => ls.levelID == level)?.score ?? 0)
            .ThenBy(u => u.name)
            .ToList();
    }
    public void OpenLeaderboards(int level, int score)
    {
        currentLevel = level;
        AddUserScore(level, score);
        leaderboardsPanel.SetActive(true);
        SortUsersByScoreForSpecificLevel(currentLevel);
        DisplayUsers();
    }
    private void AddUserScore(int level, int score) => user.levelScores.Find(l => l.levelID == level+1).score = score;
    private void OnEnable()
    {
        FetchUsers();
    }
    private void OnDisable() => CleanUpPreviousEntries();

    private void DisplayUsers()
    {
        for (var i = 0; i < sortedUsers.Count; i++) 
        {
            var isUsersThumb = sortedUsers[i] == user;  
            CreateThumb(sortedUsers[i], i+1, isUsersThumb);
        }

        if (usersThumb.leaderBoardPosition > lastVisibleThumbEntry)
        {
            usersThumb.transform.SetSiblingIndex(lastVisibleThumbEntry);
        }
    }

    private void CreateThumb(UserDto user, int leaderboardPosition, bool isUsersThumb)
    {
        var newThumb = Instantiate(thumbPrefab, content);
        var fromLvlToIndexNormalization = currentLevel - 1;
        newThumb.Set(user, fromLvlToIndexNormalization, leaderboardPosition, isUsersThumb);
        entryThumbs.Add(newThumb);
        if(isUsersThumb)
            usersThumb =  newThumb;
    }

    private void CleanUpPreviousEntries()
    {
        foreach (var thumb in entryThumbs)
        {
            Destroy(thumb.gameObject);
        }
        entryThumbs.Clear();
    }
}
