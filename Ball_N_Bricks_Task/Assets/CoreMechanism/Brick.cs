using UnityEngine;

public class Brick : MonoBehaviour
{
    public int hitsToBreak = 1;
    public GameObject breakEffectPrefab;
    public void OnHit()
    {
        hitsToBreak--;
        if (hitsToBreak <= 0) Break();
    }

    void Break()
    {
        hitsToBreak--;
        //if (breakEffectPrefab != null) Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        if(hitsToBreak <= 0)
            Destroy(gameObject);
    }
}
