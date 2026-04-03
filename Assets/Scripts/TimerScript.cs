using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading;

public class TimerScript : MonoBehaviour
{
    [SerializeField] TMP_Text TimerText;
    [SerializeField] private Image TimerImage;
    [SerializeField] private Image TimerFilling;
    [SerializeField] private GameObject UI;
    [SerializeField] private ButtonManager buttMan;
    [SerializeField] private GameManager gameMan;
    [SerializeField] private GameObject LightningPrefab;
    private GameObject lightningObj;
    private float interval;
    private float UIAvaibleDelay = 0f;
    [SerializeField] private float startTime;
    private float UITimer;
    private float timeLeft;
    private float DeltaColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (STATIC_DATA.Tutorial)
        {
            startTime = 600;
        }
        timeLeft = startTime;


        interval = (startTime-1)/(buttMan.GetNumOfTypes() / 2f);

        UITimer = interval;

        TimerFilling.color = new Color(0f, 1f, 0f);
    }

    
    // Update is called once per frame
    void Update()
    {
        
        if(timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            if (!STATIC_DATA.Tutorial)
            {
                UIEnabler();
                UITimer -= Time.deltaTime;
                UIAvaibleDelay += Time.deltaTime;
            }
            
            TimerImage.fillAmount = timeLeft / startTime;
            
            DeltaColor = timeLeft / startTime;

            if (DeltaColor > 0.5f)
            {
                float red = 2f * (1f - DeltaColor);
                TimerFilling.color = new Color(red, 1f, 0f);
            }
            else
            {
                float green = 2f * DeltaColor;
                TimerFilling.color = new Color(1f, green, 0f);
            }

            if(timeLeft > 60)
            {
                FormatMinToSec();
            }
            else
            {   
                TimerText.text = "Time Remaining: " + timeLeft.ToString("0.00");    
            }
        }
        else
        {
			timeLeft = startTime; // prevents calling ResetGame every frame
            Start();
			gameMan.ResetGame();
		}
    }

    void FormatMinToSec()
    {
        float mins = Mathf.FloorToInt(timeLeft / 60);
        float secs = Mathf.FloorToInt(timeLeft % 60);

        TimerText.text = "Time Remaining: " + string.Format("{0:00}:{1:00}", mins, secs);
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    private void UIEnabler()
    {
        if(UITimer <= 8f && lightningObj == null)
        {
            lightningObj = Instantiate(LightningPrefab, UI.transform.GetComponentInParent<Canvas>().transform);
            lightningObj.GetComponent<LightningBoltScript>().SetDuration(UITimer);
        }
        if (UITimer < 0 && UIAvaibleDelay > 2.0f)
        {
            buttMan.OpenUI(() => Continue(), lightningObj);
        }
    }

    public void ResetTimer()
    {
		Debug.Log("ResetTimer called, timeLeft before: " + timeLeft);
        Start();
		Debug.Log("ResetTimer done, timeLeft after: " + timeLeft);
	}

    // subtracts time from the time left, this would be a good place to add the animation
    public void SubtractTime(float delta)
    {
        UITimer -= delta;
        timeLeft -= delta;
        if(lightningObj != null)
        {
            lightningObj.GetComponent<LightningBoltScript>().RemoveTime(delta);
        }
    }

    private void Continue()
    {
        UIAvaibleDelay = 0.0f;
        UITimer = interval;
        lightningObj = null;
    }

}
