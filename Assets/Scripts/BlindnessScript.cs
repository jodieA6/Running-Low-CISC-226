using System.Collections;
using UnityEngine;

public class BlindnessScript : MonoBehaviour
{
    [SerializeField] private GameObject blindnessMask;
    [SerializeField] private float blindDuration = 0.55f;

    [SerializeField] private float minTimeBetweenBlinds = 3f;
    [SerializeField] private float maxTimeBetweenBlinds = 8f;
    [SerializeField] private float warningDelay = 0.6f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private float audioStartTime = 2f;
    [SerializeField] private float audioFadeDuration = 0.5f;
    [SerializeField] private float playVolume = 1f;

    private bool active;
    private bool isBlind;
    private Coroutine blindnessRoutine;
    private Coroutine fadeRoutine;
    private float defaultVolume;

    private void Awake()
    {
        if (blindnessMask != null){
            blindnessMask.SetActive(false);
        }

        if (audioSource != null){
            defaultVolume = audioSource.volume;
        }
    }

    public void Active(bool change)
    {
        active = change;

        if (active)
        {
            if (blindnessRoutine == null){
                blindnessRoutine = StartCoroutine(BlindnessLoop());
            }
        }
        else
        {
            if (blindnessRoutine != null)
            {
                StopCoroutine(blindnessRoutine);
                blindnessRoutine = null;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            isBlind = false;

            if (blindnessMask != null)
                blindnessMask.SetActive(false);

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.volume = defaultVolume;
            }
        }
    }

    private IEnumerator BlindnessLoop()
    {
        while (active)
        {
            float waitTime = Random.Range(minTimeBetweenBlinds, maxTimeBetweenBlinds);
            yield return new WaitForSeconds(waitTime);

            if (!active){
                yield break;
            }

            PlayWarningAudio();

            yield return new WaitForSeconds(warningDelay);

            if (!active)
                yield break;

            SetBlind(true);
            yield return new WaitForSeconds(blindDuration);
            SetBlind(false);

            if (!active){
                yield break;
            }

            if (fadeRoutine != null){
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeOutAudio(audioFadeDuration));
        }

        blindnessRoutine = null;
    }

    private void PlayWarningAudio()
    {
        if (audioSource == null || warningClip == null){
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        audioSource.Stop();
        audioSource.clip = warningClip;
        audioSource.volume = playVolume;

        float startTime = Mathf.Clamp(audioStartTime, 0f, Mathf.Max(0f, warningClip.length - 0.01f));
        audioSource.time = startTime;
        audioSource.Play();
    }

    private IEnumerator FadeOutAudio(float duration)
    {
        if (audioSource == null || !audioSource.isPlaying){
            yield break;
        }

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.volume = defaultVolume;
        fadeRoutine = null;
    }

    private void SetBlind(bool value)
    {
        isBlind = value;

        if (blindnessMask != null){
            blindnessMask.SetActive(isBlind);
        }
    }
}