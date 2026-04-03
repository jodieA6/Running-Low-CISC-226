using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerModifiers : MonoBehaviour
{
    private int jump = 0;
    private int speed = 0;
    private int push = 0;
    private bool climb = true;
    private bool blind = true;
    private bool waterproof = true;
    private bool gasproof = true;
    int numOfSpeeds = 2;
    int numOfPushes = 2;
    int numOfJumps = 2;

    [SerializeField] public BlindnessScript blindnessScript;
    [SerializeField] private ButtonManager buttonManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(STATIC_DATA.Tutorial)
        {
            TutorialMods();
        } else
        {
            ResetModification();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetModification()
    {
        numOfJumps = 2;
        numOfPushes = 2;
        numOfSpeeds = 2;
        buttonManager.ResetButtons();
        SetJump(0);
        SetSpeed(0);
        SetClimb(true);
        SetPush(0);
        SetBlind(true);
		blindnessScript.Active(false);
	}

    public void TutorialMods()
    {
        SetWaterProof(true);
        SetGasProof(true);
        SetBlind(true);
        SetClimb(true);
        numOfJumps = 1;
        numOfPushes = 1;
        numOfSpeeds = 1;
        SetJump(0);
        SetPush(0);
        SetSpeed(0);
    }

#region Getters
    

/** Jump is as follows:
 * 0 = normal jump
 * 1 = reduced jump
 * 2 = further reduced jump
 * 3 = close to no vertical movement
*/
    public int GetJump()
    {
        return jump;
    }

/** Push is as follows:
 * 0 = normal push
 * 1 = reduced push
 * 2 = further reduced push
 * 3 = close to no object movement
*/
    public int GetPush()
    {
        return push;
    }

/** Speed is as follows:
 * 0 = normal speed
 * 1 = reduced speed
 * 2 = further reduced speed
 * 3 = close to no movement
*/
    public int GetSpeed()
    {
        return speed;
    }


    public int GetNumOfPushes()
    {
        return numOfPushes;
    }
    
    public int GetNumOfSpeeds()
    {
        return numOfSpeeds;
    }

    public int GetNumOfJumps()
    {
        return numOfJumps;
    }

    public bool GetClimb()
    {
        return climb;
    }

    public bool GetBlind()
    {
        return blind;
    }

/**Returns true if gas proof*/
    public bool GetGasProof()
    {
        return gasproof;
    }
/**Returns true if gas proof*/
    public bool GetWaterProof()
    {
        return waterproof;
    }
#endregion
#region Setters

    public void SetJump(int jump = -1)
    {
        if(jump == -1)
        {
            this.jump += 1;
        }
        else
        {
            this.jump = jump;
        }
    }

    public void SetPush(int push = -1)
    {   
        if(push == -1)
        {
            this.push += 1;
        }
        else
        {
            this.push = push;
        }
    }

    public void SetSpeed(int speed = -1)
    {
        if(speed == -1)
        {
            this.speed += 1;
        }
        else
        {
            this.speed = speed;
        }
    }

    public void SetClimb(bool climb)
    {
        this.climb = climb;
    }

    public void SetBlind(bool blind)
    {
        this.blind = blind;
        blindnessScript.Active(!blind);
    }

    public void SetWaterProof(bool water)
    {
        this.waterproof = water;
    }

    public void SetGasProof(bool gas)
    {
        this.gasproof = gas;
    }
#endregion
}
