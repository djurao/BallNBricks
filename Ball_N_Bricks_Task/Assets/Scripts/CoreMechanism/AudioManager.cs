using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioListener listener;
    [SerializeField] private AudioSource ballSource;
    [SerializeField] private AudioSource batAudioSource;
    [SerializeField] private AudioSource brickShatteringAudioSource;
    [SerializeField] private AudioSource collectAudioSource;
    [SerializeField] private AudioSource ambienceAudioSource;
    [SerializeField] private AudioClip[] glassHits;
    [SerializeField] private AudioClip[] ballHits;
    [SerializeField] private AudioClip[] glassShattering;
    public GameObject muteBtn;
    public GameObject unMuteBtn;
    private void Awake() => Instance = this;
    void Start() => ambienceAudioSource.Play();
    public void PlayGlassHit(float volume = 0.5f) => PlayRandomFromArray(glassHits, volume, ballSource);
    public void PlayBallHit(float volume = 0.8f) => PlayRandomFromArray(ballHits, volume, batAudioSource);
    public void PlayGlassShattering(float volume = 0.2f) => PlayRandomFromArray(glassShattering, volume, brickShatteringAudioSource);
    private void PlayRandomFromArray(AudioClip[] clips, float volume, AudioSource audioSource)
    {
        var index = Random.Range(0, clips.Length);
        var clip = clips[index];
        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
    public void Collect() => collectAudioSource.Play();

    public void MuteUnmuteAudio(bool state)
    {
        AudioListener.volume = state ? 0f : 1f;
        muteBtn.SetActive(!state);
        unMuteBtn.SetActive(state);
    }
}
