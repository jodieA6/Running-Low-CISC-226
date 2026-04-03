using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class LightningBoltScript : MonoBehaviour
{
    [SerializeField] Image backing;
    [SerializeField] Image bolt;
    [SerializeField] GameObject obj;
    private float duration = 8f;
    private float elapsed = 0f;
    private float scale = -.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private bool negative = false;
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        backing.fillAmount = 1 - (elapsed / duration);
        scale += (negative ? -1 : 1) * Time.deltaTime * 1.2f;

        if(scale >= .2f)
        {
            scale = .2f;
            negative = true;
        } else if (scale <= -.2f)
        {
            scale = -.2f;
            negative = false;
        }
        obj.transform.localScale = new Vector3(1 + scale, 1 + scale, 1);
    }
    public void RemoveTime(float timeToRemove)
    {
        elapsed += timeToRemove;
    }
}
