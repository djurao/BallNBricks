using System;
using System.Collections.Generic;
using System.IO;
using Leaderboard_Module;
using UnityEngine;

namespace Leaderboard_Module
{
    public static class SaveLoadLeaderboards
    {
            static string FilePath => Path.Combine(Application.persistentDataPath, "users.json");

            public static void SaveUsers(List<UserDto> users)
            {
                var serialList = new SerializableUserList { users = new List<UserDto>() };

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        serialList.users.Add(new UserDto
                        {
                            id = u.id,
                            name = u.name,
                            textureBase64 = u.textureBase64,
                            levelScores = u.levelScores
                        });
                    }
                }

                var json = JsonUtility.ToJson(serialList, prettyPrint: true);
                File.WriteAllText(FilePath, json);
            }

            public static SerializableUserList LoadDummyUsers(string resourceName = "DummyUsers")
            {
                var textAsset = Resources.Load<TextAsset>(resourceName);
                var json = textAsset.text;
                var container = JsonUtility.FromJson<SerializableUserList>(json);
                return container;
            }
    }
}
[Serializable]
public class SerializableUserList
{
    public List<UserDto> users;
}