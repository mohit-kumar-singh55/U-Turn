using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] AudioSource vehicleAudioSource;

    [Header("Audio Settings")]
    [SerializeField] float idlePitch = 1f;
    [SerializeField] float throttlingPitch = 2f;
    [SerializeField] float startVolume = 0.5f;

    private bool _isThrottling = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ValidateFields();
    }

    void Start()
    {
        StartCoroutine(FadeVolume(vehicleAudioSource, 0f, startVolume, 0.5f));
    }

    public void StopAllAudios(bool forceStop = false)
    {
        StopAllCoroutines();
        if (!forceStop) StartCoroutine(FadeVolume(vehicleAudioSource, vehicleAudioSource.volume, 0f, 0.5f, true));
        else vehicleAudioSource.Stop();
    }

    public void PlayThrottlingSound(bool play = true)
    {
        if ((play && _isThrottling) || (!play && !_isThrottling)) return;

        StopAllCoroutines();

        _isThrottling = play;
        StartCoroutine(FadePitch(vehicleAudioSource, play ? idlePitch : throttlingPitch, play ? throttlingPitch : idlePitch, 0.3f));

        vehicleAudioSource.Play();
    }

    IEnumerator FadeVolume(AudioSource source, float startVolume, float endVolume, float time, bool stopPlaying = false)
    {
        float t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / time;
            source.volume = Mathf.Lerp(startVolume, endVolume, t);
            yield return null;
        }

        if (stopPlaying) source.Stop();
    }

    IEnumerator FadePitch(AudioSource source, float startPitch, float endPitch, float time)
    {
        float t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / time;
            source.pitch = Mathf.Lerp(startPitch, endPitch, t);
            yield return null;
        }
    }

    void ValidateFields()
    {
        Assert.IsNotNull(vehicleAudioSource, "Vehicle Audio Source cannot be null");
    }
}
