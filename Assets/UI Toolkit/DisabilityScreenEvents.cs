using UnityEngine;
using UnityEngine.UIElements;

public class DisabilityScreen : MonoBehaviour
{
    private UIDocument uidocument;

    private Button button1;
    //private Button button2;
    //private Button button3;

    private void Awake()
    {
        uidocument = GetComponent<UIDocument>();
        
        button1 = uidocument.rootVisualElement.Q("Button1") as Button;
        button1.RegisterCallback<ClickEvent>(OnButton1);

    }

    private void OnDisable()
    {
        button1.UnregisterCallback<ClickEvent>(OnButton1);        
    }

    private void OnButton1(ClickEvent clk)
    {
        Debug.Log("Button1 was pressed");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
