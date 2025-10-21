using TMPro;
using UnityEngine;

namespace Misc
{
    public class Score : MonoBehaviour
    {
        public static Score Instance;
        [SerializeField] private int score;
        [SerializeField] private TextMeshProUGUI scoreText;

        void Awake() => Instance = this;
        
        public void IncrementScore(int amountToIncrement)
        {
            score +=  amountToIncrement;
            UpdateScoreUI();
        }

        public void ResetScore()
        {
            score = 0;
            UpdateScoreUI();
        }

        private void UpdateScoreUI() => scoreText.text = $"{score}";

        public int GetScore() => score;
    }
}
