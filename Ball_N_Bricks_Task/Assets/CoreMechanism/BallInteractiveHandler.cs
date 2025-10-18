using System;
using System.Collections.Generic;
using UnityEngine;

public class BallInteractiveHandler : MonoBehaviour
{
    public BallController ballController;
    public List<Animator> brickHitEffectObjects;
    public GameObject effect;
    private void HandleBrickCollision()
    {
        print("times hit");
        effect.SetActive(true);
        /*for (int i = 0; i < brickHitEffectObjects.Count; i++) {
            brickHitEffectObjects[i].gameObject.SetActive(true);
            brickHitEffectObjects[i].SetTrigger("TriggerEffect");
        }*/

    }
    private void OnEnable() => ballController.OnCollidedWithBrick += HandleBrickCollision;
    private void OnDisable() => ballController.OnCollidedWithBrick -= HandleBrickCollision;
}
