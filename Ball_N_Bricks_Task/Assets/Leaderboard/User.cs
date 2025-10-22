using System;
using System.Collections.Generic;

namespace Leaderboard_Module
{
   [Serializable]
   public class User
   {
      public int id;
      public string name;
      public string textureBase64;
      public List<LevelScoreData> levelScores;
      public int hardCurrency;
   }
}
