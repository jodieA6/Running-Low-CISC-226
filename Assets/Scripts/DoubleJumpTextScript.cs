using TMPro;
using UnityEngine;

public class DoubleJumpTextScript : MonoBehaviour
{
    [SerializeField] TMP_Text s;
    [SerializeField] PlayerController pc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        s.text = pc.doubleJumpCounter.ToString();
    }
}
