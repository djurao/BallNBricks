using System;
using System.Collections.Generic;
using Leaderboard_Module;
using UnityEngine;
using System.Linq;
public class LeaderBoards : MonoBehaviour
{
    public LeaderboardThumb thumbPrefab;
    public List<LeaderboardThumb> entryThumbs;
    public Transform content;
    public SerializableUserList  userList;
    public List<UserDto> sortedUsers;
    public int levelID;
    private LeaderboardThumb usersThumb;
    public UserDto user;
    public int lastVisibleThumbEntry = 15;
    private void FetchUsers() => userList = SaveLoadLeaderboards.LoadDummyUsers();
    private void SortUsersByScoreForSpecificLevel()
    {
        sortedUsers = userList.users
            .OrderByDescending(u => u.levelScores?.FirstOrDefault(ls => ls.levelID == levelID)?.score ?? 0)
            .ThenBy(u => u.name)
            .ToList();
    }

    private void AddUserScore()
    {
        var newId = userList.users.Count == 0 ? 1 : userList.users.Max(u => u.id) + 1;
        // create user
        user = new UserDto
        {
            id = newId,
            name = "DJURO",
            textureBase64 = null,
            levelScores = new List<LevelScoreData>
            {
                new LevelScoreData { levelID = 1, score = 100 },
                new LevelScoreData { levelID = 2, score = 2500 },
                new LevelScoreData { levelID = 3, score = 10000 }
            }
        };

        userList.users.Add(user);
    }

    private void OnEnable()
    {
        FetchUsers();
        AddUserScore();
        SortUsersByScoreForSpecificLevel();
        DisplayUsers();
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
        var fromLvlToIndexNormalization = levelID - 1;
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
