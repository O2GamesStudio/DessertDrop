using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("AudioSource Pool Settings")]
    [SerializeField] int initialPoolSize = 10;
    [SerializeField] int maxPoolSize = 20;

    [Header("Game SFX Clips")]
    [SerializeField] AudioClip gameOverClip;
    [SerializeField] AudioClip uiClickClip;

    [Header("BGM Clips")]
    [SerializeField] AudioClip gameBGM;

    [Header("Sound Settings")]
    private bool isSoundEnabled = true;
    private bool isMusicEnabled = true;

    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private AudioSource bgmSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void InitializeSoundManager()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            audioSourcePool.Enqueue(CreateNewAudioSource());
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = 1f;
        bgmSource.playOnAwake = false;

        isSoundEnabled = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        isMusicEnabled = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;

        PlayBGM(gameBGM);
    }

    AudioSource CreateNewAudioSource()
    {
        GameObject audioObj = new GameObject("PooledAudioSource");
        audioObj.transform.SetParent(transform);
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    AudioSource GetAudioSource()
    {
        AudioSource audioSource;

        while (audioSourcePool.Count > 0)
        {
            audioSource = audioSourcePool.Dequeue();
            if (audioSource != null && !audioSource.isPlaying)
            {
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
        }

        if (activeAudioSources.Count < maxPoolSize)
        {
            audioSource = CreateNewAudioSource();
            audioSourcePool.Dequeue();
            activeAudioSources.Add(audioSource);
            return audioSource;
        }

        audioSource = activeAudioSources[0];
        audioSource.Stop();
        return audioSource;
    }

    void ReturnAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        audioSource.clip = null;
        audioSource.Stop();
        activeAudioSources.Remove(audioSource);

        if (!audioSourcePool.Contains(audioSource))
        {
            audioSourcePool.Enqueue(audioSource);
        }
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;
        PlayerPrefs.SetInt("IsSoundOn", enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled)
        {
            StopAllSFX();
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        PlayerPrefs.SetInt("IsMusicOn", enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (bgmSource != null)
        {
            if (enabled)
            {
                if (!bgmSource.isPlaying && bgmSource.clip != null)
                    bgmSource.Play();
            }
            else
            {
                bgmSource.Pause();
            }
        }
    }

    public bool IsSoundEnabled() => isSoundEnabled;
    public bool IsMusicEnabled() => isMusicEnabled;

    public void PlayGameOverSFX()
    {
        PlaySFX(gameOverClip);
    }

    public void PlayUIClickSFX()
    {
        PlaySFX(uiClickClip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!isSoundEnabled) return;

        if (clip == null)
        {
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = clip;
        audioSource.volume = 1f;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, clip.length));
    }

    public void PlayOneShotSFX(AudioClip clip)
    {
        if (!isSoundEnabled) return;
        if (clip == null) return;

        AudioSource audioSource = GetAudioSource();
        audioSource.PlayOneShot(clip, 1f);

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, clip.length));
    }

    IEnumerator ReturnToPoolAfterPlay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        ReturnAudioSource(audioSource);
    }

    public void PlayGameBGM()
    {
        PlayBGM(gameBGM);
    }

    void PlayBGM(AudioClip clip)
    {
        if (!isMusicEnabled) return;
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = 0.5f;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmSource != null)
            bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource != null && isMusicEnabled)
            bgmSource.UnPause();
    }

    public void FadeBGM(float targetVolume, float duration)
    {
        StartCoroutine(FadeBGMCoroutine(targetVolume, duration));
    }

    IEnumerator FadeBGMCoroutine(float targetVolume, float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    public void StopAllSFX()
    {
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        while (activeAudioSources.Count > 0)
        {
            ReturnAudioSource(activeAudioSources[0]);
        }
    }

    public void PrintPoolStatus()
    {
        Debug.Log($"AudioSource 풀 상태 - 사용 가능: {audioSourcePool.Count}, 활성: {activeAudioSources.Count}");
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}