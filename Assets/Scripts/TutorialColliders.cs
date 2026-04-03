using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class TutorialColliders : MonoBehaviour
{
    [SerializeField] private GameObject B3b0TextPrefab;
    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject ButtMan;
    private List<Action> actions;
    private Action Instance;
    [SerializeField] int id = 0;
    [SerializeField] private List<int> additionalInts;
    [SerializeField] private GameObject Collider6;
    private GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        actions = new List<Action>
        {
            null,
            () => CreateText("Looks like I'm on my own\nLet's get out of here\nPress [SPACE] to jump\nYou can use the [A] and [D] keys to move."),
            () => CreateText("Sometimes a ledge can be barly out of grasp\nBut you can still reach it\nI'll grab on to ledges for you, you just go in the direction you want and I'll follow."),
            () => CreateText("I'm running low on power, I need to disable a function to maintain power\nTake a look around and we'll decide which one to disable"),
            () => ButtMan.GetComponent<ButtonManager>().HighlightButton(1),
            () => ButtMan.GetComponent<ButtonManager>().OpenUI(() => { }),
            () => CreateText("Look there! There's Gas and Water up ahead so maybe we should keep those abilities\nDisable my Optic Cooling, we'll pull through"),
            () => Collider6.SetActive(true),
            () => CreateText("Oh... that wasn't supposed to happen\nHow about this: take a look up at the top left\nThat double arrow is your double jump counter, I can only give you a few per charge so make them count!"),
            () => CreateText("Wait! Two other things before we move on, the other counter up there is how many functions you've disabled\nAnd those white blocks. You can push and pull them while holding [S]\nJust so you know....."),            
            () => CreateText("Great, I think you're ready, let's keep moving\nI'll try and warn you if you need to disable another function\nAlso my battery is really low so we might need to do it more often...")
        };
        Instance = actions[id];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player = collision.gameObject;
            Instance.Invoke();
            Instance = () => { };
            for (int i = 0; i < additionalInts.Count; i++)
            {
                actions[additionalInts[i]].Invoke();
                actions[additionalInts[i]] = () => { };
            }
            Destroy(gameObject);
        }
    }

    private GameObject CurrentText;
    private B3b0TextController CurrentTextController;

    public void CreateText(string str = "[{ERORR: NO TEXT WAS PROVIDED}]")
    {
        if (CurrentText != null) Destroy(CurrentText);
        GameObject newText = Instantiate(B3b0TextPrefab, Canvas.transform);
        CurrentTextController = newText.GetComponent<B3b0TextController>();
        if(id != 6)
        {
            CurrentTextController.Initiate(str, true);
        }
        else
        {
            CurrentTextController.Initiate(str, true, true);
        }
        
        CurrentText = newText;
    }
}
