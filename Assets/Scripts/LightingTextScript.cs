using TMPro;
using UnityEngine;
public class LightingTextScript : MonoBehaviour
{
    [SerializeField] public TMP_Text s;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        s.text = STATIC_DATA.ACTIVE_DISABILITIES.ToString();
    }
}
