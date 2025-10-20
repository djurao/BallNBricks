using System;
using System.Collections.Generic;

namespace Leaderboard_Module
{
   [Serializable]
   public class UserDto
   {
      public int id;
      public string name;
      public string textureBase64;
      public List<LevelScoreData> levelScores;
   }
}
