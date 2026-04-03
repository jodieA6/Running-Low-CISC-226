using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class FadeOutController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup fadeGroup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void FadeIn()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    public void ReturnToMainMenu()
    {
        FadeToScene("MainMenu");
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        fadeGroup.blocksRaycasts = true;

        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Load next scene
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1f;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            Debug.Log("Fading: " + fadeGroup.alpha);
            yield return null;
        }

        fadeGroup.alpha = endAlpha;
    }
}
