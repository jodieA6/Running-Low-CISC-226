using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.InputSystem;

public class TransitionScript : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private GameObject B3b0TextPrefab;
    [SerializeField] private GameObject Canvas;
    private string nextLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextLevel = STATIC_DATA.NEXT_LEVEL;
        fadeGroup.alpha = 1f;
        StartCoroutine(Fade(1f, 0f));
        fadeGroup.blocksRaycasts = false;
        
        GameObject newText = Instantiate(B3b0TextPrefab, Canvas.transform);
        newText.GetComponent<B3b0TextController>().Initiate("[POWER RESTORED]\n[ALL SYSTEMS HAVE BEEN REBOOTED]\n[CHARGER DEPLETED]\n[FIND A NEW CHARGER TO PROCEED]");
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            FadeToScene(nextLevel);
        }
    }
    private void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        fadeGroup.blocksRaycasts = true;

        yield return StartCoroutine(Fade(fadeGroup.alpha, 1f));

        SceneManager.LoadScene(sceneName);

    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        fadeGroup.alpha = endAlpha;
    }
}
