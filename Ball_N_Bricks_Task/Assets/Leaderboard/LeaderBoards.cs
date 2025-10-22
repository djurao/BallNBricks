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
    public List<User> sortedUsers;
    public int currentLevel;
    private LeaderboardThumb usersThumb;
    public int lastVisibleThumbEntry = 15;
    private User user;
    private void Awake() => Instance = this;

    private void Start() => FetchUsers();
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
        CleanUpPreviousEntries();
        currentLevel = level;
        AddUserScore(level, score);
        leaderboardsPanel.SetActive(true);
        SortUsersByScoreForSpecificLevel(currentLevel);
        DisplayUsers();
    }

    public void RefreshLeaderBoard(int level, int score)
    {
        currentLevel = level;
        SortUsersByScoreForSpecificLevel(currentLevel);
        if (!leaderboardsPanel.activeInHierarchy) return;
        CleanUpPreviousEntries();
        DisplayUsers();
    }

    public void AddUserScore(int level, int score)
    {
        user = UserCreation.Instance.user;

        if (user == null) return;
        if(!userList.users.Contains(user))
            userList.users.Add(user);
            
        user.levelScores.Find(l => l.levelID == level).score = score;
    }

    private void OnDisable() => CleanUpPreviousEntries();

    private void DisplayUsers()
    {
        for (var i = 0; i < sortedUsers.Count; i++) 
        {
            var isUsersThumb = sortedUsers[i] == user;  
            CreateThumb(sortedUsers[i], i+1, isUsersThumb);
        }
        Invoke(nameof(RepositionPlayersLeaderboardThumb), 0.1f);
    }

    private void RepositionPlayersLeaderboardThumb()
    {
        if (usersThumb.leaderBoardPosition > lastVisibleThumbEntry)
        {
            usersThumb.transform.SetSiblingIndex(lastVisibleThumbEntry);
        }
    }

    private void CreateThumb(User user, int leaderboardPosition, bool isUsersThumb)
    {
        var newThumb = Instantiate(thumbPrefab, content);
        newThumb.Set(user, currentLevel, leaderboardPosition, isUsersThumb);
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
