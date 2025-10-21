using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource ballSource;
    [SerializeField] private AudioSource batAudioSource;
    [SerializeField] private AudioSource brickShatteringAudioSource;

    [SerializeField] private AudioClip[] glassHits;
    [SerializeField] private AudioClip[] ballHits;
    [SerializeField] private AudioClip[] glassShattering;
    private void Awake() => Instance = this;
    
    public void PlayGlassHit(float volume = 1f) => PlayRandomFromArray(glassHits, volume, ballSource);
    public void PlayBallHit(float volume = 1f) => PlayRandomFromArray(ballHits, volume, batAudioSource);
    public void PlayGlassShattering(float volume = 1f) => PlayRandomFromArray(glassShattering, volume, brickShatteringAudioSource);
    private void PlayRandomFromArray(AudioClip[] clips, float volume, AudioSource audioSource)
    {
        var index = Random.Range(0, clips.Length);
        var clip = clips[index];
        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void Collect()
    {
        
    }
}
