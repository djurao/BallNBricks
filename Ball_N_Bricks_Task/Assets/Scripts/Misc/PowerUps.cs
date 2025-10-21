using System;
using System.Collections.Generic;
using UnityEngine;

namespace Misc
{
    public enum PowerUpType
    {
        ChromeBall = 0
    }

    public class PowerUps : MonoBehaviour
    {
        public static PowerUps Instance;
        public List<PowerUpsDefinition> powerUps;
        public bool chromeBallActive;
        public SpriteRenderer[] visualBallsSR;
        public Sprite normalBall;
        public Sprite chromeBall;
        private void Awake() => Instance = this;

        public void ReFillPowerUp(PowerUpType  powerUpType, int amountToFill)
        {
            powerUps[(int)powerUpType].Refill(amountToFill);
        }

        public void ActivatePowerUp(int i)
        {
            var powerUp = powerUps[i];
            if (powerUp.amount <= 0) return;
            powerUp.amount--;
            if (powerUp.type == PowerUpType.ChromeBall)
            {
                chromeBallActive = true;
                powerUp.iconInThumb.SetActive(false);
                UpdateBallGraphics(chromeBall);
            }
        }
        
        public void DeactivatePowerUp(int i)
        {
            var powerUp = powerUps[i];
            if (powerUp.type == PowerUpType.ChromeBall)
            {
                chromeBallActive = false;
                UpdateBallGraphics(normalBall);
            }
        }

        private void UpdateBallGraphics(Sprite sprite)
        {
            foreach (var ball in visualBallsSR)
            {
                ball.sprite = sprite;
            }
        }

        public void DeactivateAllPowerUps()
        {
            chromeBallActive = false;
            UpdateBallGraphics(normalBall);
        }
    }
    [Serializable]
    public class PowerUpsDefinition
    {
        public PowerUpType type;
        public int amount;
        public GameObject iconInThumb;
        
        public void Refill(int amountToAdd)
        {
            amount +=  amountToAdd;
            iconInThumb.SetActive(true);
        }
    }
}