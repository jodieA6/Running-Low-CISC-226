using UnityEngine;

public class PlayerAudioManagerScript : MonoBehaviour
{
    private AudioSource playerAudioSource;
    public AudioClip jumpSFX;
    public AudioClip landSFX;
    public AudioClip pushSFX;
    public AudioSource pushAudioSource; // I know this is stupid but I'm too sleepy to care
    public AudioClip step1SFX;
    public AudioClip step2SFX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAudioSource = GetComponent<AudioSource>();

        pushAudioSource.clip = pushSFX;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayJump()
    {
        playerAudioSource.PlayOneShot(jumpSFX, 0.5f);
    }

    void PlayLand()
    {
        playerAudioSource.PlayOneShot(landSFX, 0.5f);
    }

    public void PlayPush(bool input)
    {
        Debug.Log("player pushing: " + input);
        if(input && !pushAudioSource.isPlaying)
        {
            pushAudioSource.Play();
        }
        else if (!input)
        {
            pushAudioSource.Stop();
        }
    }

    void PlayFootStep1()
    {
        playerAudioSource.PlayOneShot(step1SFX, 0.1f);
    }

    void PlayFootStep2()
    {
        playerAudioSource.PlayOneShot(step2SFX, 0.1f);
    }
}
