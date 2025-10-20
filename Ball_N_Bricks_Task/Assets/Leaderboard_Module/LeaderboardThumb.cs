using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Leaderboard_Module
{
   public class LeaderboardThumb : MonoBehaviour
   {
      public RawImage icon;
      public TextMeshProUGUI positionLabel;
      public TextMeshProUGUI nameLabel;
      public TextMeshProUGUI scoreLabel;
      public int leaderBoardPosition;
      public void Set(UserDto user,int levelID, int leaderboardPosition,  bool isUsersThumb)
      {
         this.leaderBoardPosition = leaderboardPosition;
         //icon.texture = user.textureBase64;
         positionLabel.text = $"#{leaderboardPosition}"; 
         nameLabel.text = user.name;
         scoreLabel.text = $"{user.levelScores[levelID].score}";

         if (isUsersThumb)
         {
            positionLabel.color = Color.green;
            nameLabel.color = Color.green;
            scoreLabel.color = Color.green;
         }
      }
   }
}
