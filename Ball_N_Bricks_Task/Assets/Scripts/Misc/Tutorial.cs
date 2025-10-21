using UnityEngine;

namespace Misc
{
    public class Tutorial : MonoBehaviour
    {
        public GameObject tutorialPanel;
        private const string SaveKey = "TuturialShown";

        private void Awake()
        {
            if (PlayerPrefs.HasKey(SaveKey)) return;
            tutorialPanel.SetActive(true);
            PlayerPrefs.SetInt(SaveKey, 1);
        }
    }
}
