using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

public class StoryScreenController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private GameObject B3b0TextPrefab;
    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject BackgroundImageObject;
    private Image BackgroundImage;
    private GameObject CurrentObject;
    private B3b0TextController CurrentTextController;

    private List<List<Action>> Events;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BackgroundImage = BackgroundImageObject.GetComponent<Image>();
        fadeGroup.blocksRaycasts = false;

        InitiateStoryList();
        Pop();
    }

    private void BackgroundChange(string imageName, Color color = default)
    {
        if(color == default)
        {
            color = new Color(1, 1, 1, 1);
        }

        if(imageName != null)
        {
            BackgroundImage.sprite = Resources.Load<Sprite>(imageName);
            BackgroundImage.color = color;
        }
        // Else do nothing
    }

    private void Pop()
    {
        if (Events.Count > 0)
        {
            foreach (Action action in Events[0])
            {
                action.Invoke();
            }
            Events.RemoveAt(0);
        }
        else
        {
            Debug.LogError("No more events to pop in StoryScreenController!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (CurrentTextController.IsComplete())
            {
                Pop();
            }
            else
            {
                CurrentTextController.JumpToEnd();
            }
        }
    }

    private void InitiateNewText(string text)
    {
        Destroy(CurrentObject);
        GameObject newText = Instantiate(B3b0TextPrefab, Canvas.transform);
        CurrentTextController = newText.GetComponent<B3b0TextController>();
        CurrentTextController.Initiate(text);
        CurrentObject = newText;
    }

    private void InitiateStoryList()
    {
        Events = new List<List<Action>>
        {
            new List<Action>
            {
                () => BackgroundChange("Background", Color.black),
                () => FadeAwake(),
                () => InitiateNewText("................")
            },
            new List<Action>
            {
                () => InitiateNewText(".....     .........   ... .......     ....  ...........")
            },
            new List<Action>
            {
                () => InitiateNewText("[REBOOTING SYSTEMS...]\n[LOADING...]\n[BATTERY LOW...]")
            },
            new List<Action>
            {
                () => BackgroundChange("Earth"),
                () => FadeAwake(),
                () => InitiateNewText("[SYSTEMS ONLINE]\n[LOADING COMPLETE]\n[WELCOME BACK, B3b0]\n[RECHARGE BATTERY IMMEDIATELY]")
            },
            new List<Action>
            {
                () => InitiateNewText("[INTERNAL CLOCK MALFUNCTIONING]\n[ESTIMATED TIME SINCE LAST CHARGE: 254 YEARS]")
            },
            new List<Action>
            {
                () => InitiateNewText("[SCANNING ENVIRONMENT...]\n[NO IMMEDIATE THREATS DETECTED]\n[HUMAN PRESENCE: NULL]\n[ATTEMPING TO CONNECT TO NEAREST NETWORK...]")
            },
            new List<Action>
            {
                () => InitiateNewText("[CONNECTION FAILED]\n[NO NETWORKS WITH 2LY RANGE]\n[RECOMMENDED ACTION: FIND POWER SOURCE AND RECHARGE BATTERY IMMEDIATELY]"),
                () => BackgroundChange("Background", Color.black),
            },
            new List<Action>
            {
                () => InitiateNewText("[REBOOTING OPTICS DRIVE IN 3... 2... 1...]\n[OPTICS DRIVE REBOOTED]")
            },
            new List<Action>
            {
                () => BackgroundChange("Background", Color.black),
                () => FadeToScene("main"),
                () => STATIC_DATA.Tutorial = true
            }
        };
    }


    #region Fading and Scene Transition
    private void FadeAwake()
    {
        StartCoroutine(Fade(1.0f, 0f));
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
    #endregion
}
