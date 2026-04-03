using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private PlayerModifiers playerMods;
    [SerializeField] private GameObject UI;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject Highlighter;

    public List<(Action action, string title, string desc, int executions)> buttonObjects;
    private List<(Action action, string title, string desc, int executions)> types;

    public List<TMP_Text> titles = new List<TMP_Text>(3);
    public List<TMP_Text> descs = new List<TMP_Text>(3);

    [SerializeField] private int ButtonPresses = 0;
    [SerializeField] private int NumOfDis;

    private void Awake()
    {
        NumOfDis = 0;
        types = new List<(Action action, string title, string desc, int executions)>
        {
            (() => playerMods.SetClimb(false), "Disable Climbing", "Prevents the robot from climbing surfaces. This reduces power usage and extends operating time.", 1),

            (() => playerMods.SetBlind(false), "Optics Reboot", "The visual system periodically restarts, causing brief moments of blindness.", 1),

            (() => Push(), "Pushing Strength Reduction", "Reduces the robot's ability to push objects, preventing mechanical strain.", playerMods.GetNumOfPushes()),

            (() => Jump(),"Jumping Strength Reduction","Reduces the robot's vertical movement capability to conserve power.", playerMods.GetNumOfJumps()),

            (() => Speed(),"Reduced Movement Speed","Lowers the robot's maximum movement speed to conserve power.", playerMods.GetNumOfSpeeds()),

            (() => playerMods.SetWaterProof(false), "Disable Waterproofing","Disables hermetic seals on water sensitive components", 1),

            (() => playerMods.SetGasProof(false), "Disable Gasproofing","Disables gaseous filtering. Do not enter unkowing gas mediums", 1),
        };
        
        if(!STATIC_DATA.Tutorial)
        {
            for (int i = 0; i < types.Count; i++)
            {
                NumOfDis += types[i].executions;
            }
        }
        else
        {
            NumOfDis = 3;
            types = new List<(Action action, string title, string desc, int executions)>
            {
                (() => playerMods.SetWaterProof(false), "Disable Waterproofing","Disables hermetic seals on water sensitive components", 1),

                (() => playerMods.SetBlind(false), "Optics Reboot", "The visual system periodically restarts, causing brief moments of blindness.", 1),

                (() => playerMods.SetGasProof(false), "Disable Gasproofing","Disables gaseous filtering. Do not enter unkowing gas mediums", 1),
            };
        }

        

        
    }

    private void Speed()
    {
        if(playerMods.GetSpeed() != playerMods.GetNumOfSpeeds()){
            playerMods.SetSpeed();
        }
        else
        {
            Debug.Log("ERROR: Attempted to reduce speed below 0. The speed object was not removed from list");
        }
    }

    private void Push()
    {
        if(playerMods.GetPush() != playerMods.GetNumOfPushes()){
            playerMods.SetPush();
        }
        else
        {
            Debug.Log("ERROR: Attempted to reduce push below 0. The push object was not removed from list");
        }
    }

    private void Jump()
    {
        if(playerMods.GetJump() != playerMods.GetNumOfJumps()){
            playerMods.SetJump();
        }
        else
        {
            Debug.Log("ERROR: Attempted to reduce jump below 0. The jump object was not removed from list");
        }
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        if(!STATIC_DATA.Tutorial){
            buttonObjects = GetRandomSublist(types, 3);
        }
        else
        {
            buttonObjects = new List<(Action action, string title, string desc, int executions)>
            {
                types[0],
                types[1],
                types[2]
            };
        }

        RefreshButton(1);
        RefreshButton(2);
        RefreshButton(3);
    
            
    }

    private List<(Action action, string title, string desc, int executions)> GetRandomSublist(List<(Action action, string title, string desc, int executions)> list, int count)
    {
        List<(Action action, string title, string desc, int executions)> sublist = new List<(Action action, string title, string desc, int executions)>();
        List<int> indices = new List<int>();

        while (indices.Count < count)
        {
            int index = UnityEngine.Random.Range(0, list.Count);
            if (!indices.Contains(index))
            {
                indices.Add(index);
                sublist.Add(list[index]);
            }
        }

        return sublist;
    }


    public void ResetButtons()
    {
		ButtonPresses = 0;
        STATIC_DATA.ACTIVE_DISABILITIES = 0;
		foreach (Button button in GetComponentsInChildren<Button>())
		{
			button.interactable = true;
		}
		Awake();
		Start();
	}

    // Update is called once per frame
    void Update()
    {
        if(UI.activeSelf == true)
        {
            playerMods.blindnessScript.Active(false);
        }
    }

    public void ButtonPressed(Button currentButton)
    {
        int id = currentButton.GetComponent<ButtonID>().id;
        print(id);
        buttonObjects[id-1].action.Invoke();

        int index = types.FindIndex(t => t.title == buttonObjects[id - 1].title);

        types[index] = (types[index].action, types[index].title, types[index].desc, types[index].executions -1);

        if(types[index].executions == 0)
        {
            types.Remove(types[index]);
        }

        (Action action, string title, string desc, int executions) temp = GetNextObj(types);
        if(temp == (null, null, null, -2))
        {
            currentButton.interactable = false;
        } else{
            buttonObjects[id-1] = temp;
            RefreshButton(id);
        }
     
        
        Time.timeScale = 1.0f;
        ButtonPresses += 1;
        STATIC_DATA.ACTIVE_DISABILITIES += 1;
        UI.SetActive(false);
        playerMods.blindnessScript.Active(!playerMods.GetBlind());
    }

    private void RefreshButton(int id)
    {
        titles[id-1].text = buttonObjects[id-1].title;
        descs[id-1].text = buttonObjects[id-1].desc;
    }

    private (Action action, string title, string desc, int executions) GetNextObj(List<(Action action, string title, string desc, int executions)> objList)
    {
        if(objList.Count < 3)
        {
            return (null, null, null, -2);
        }
        else
        {
            int breakCount = 0;
            while (true)
            {
                int index = UnityEngine.Random.Range(0, objList.Count);
                if (!buttonObjects.Contains(objList[index]))
                {
                    return objList[index];
                }
                if(breakCount == 100) //Backup to ensure we don't infinitly loop
                {
                    return (null, null, null, -2);
                }
            }
        }
    }

    public int GetNumOfTypes()
    {
        return NumOfDis;
    }

    public void OpenUI(Action action, GameObject lightning = null)
    {
        if(ButtonPresses != NumOfDis && (playerController.groundedBool || STATIC_DATA.Tutorial))
        {
            Destroy(lightning);
            Time.timeScale = 0.0f;
            UI.SetActive(true);
            action.Invoke();
        }
    }
    
    GameObject CurrentHighlighter;
    public void HighlightButton(int buttonID)
    {
        Button button = titles[buttonID].transform.parent.gameObject.GetComponent<Button>();
        CurrentHighlighter = Instantiate(Highlighter, button.transform);
    }

}
