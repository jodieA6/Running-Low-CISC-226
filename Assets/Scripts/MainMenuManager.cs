using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private GameObject Canvas;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private SaveFileManager saveFileManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeGroup.alpha = 1f;
        StartCoroutine(Fade(1f, 0f));
        fadeGroup.blocksRaycasts = false;
        if(!saveFileManager.SaveFileExists())
        {
            ContinueButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            ContinueButton.GetComponent<Button>().interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ContinueButton.SetActive(saveFileManager.SaveFileExists());
    }

    public void Play()
    {
        STATIC_DATA.NEXT_LEVEL = "Level1";
        FadeToScene("StoryScene");
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void Load()
    {
        string savedLevel = saveFileManager.GetSaveLevel();
        STATIC_DATA.Tutorial = (savedLevel == "main");
        if(savedLevel == "MainMenu")
        {
            FadeToScene("StoryScene");
        }
        else
        {
            FadeToScene(savedLevel);
        }
        
    }


    private void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        fadeGroup.blocksRaycasts = true;

        yield return StartCoroutine(Fade(0f, 1f));

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
