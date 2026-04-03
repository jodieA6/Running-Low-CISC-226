using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D playerRB;
    private Collider2D playerCollider;
    private Animator playerAnimator;
    
    [SerializeField] GameObject grabDetectionPoint;
    public Vector3 grabHoldPosition;
    [SerializeField] PlayerModifiers modifier;
    [SerializeField] TimerScript timer;

    [Header("Buffer timing")]
    public float jumpBufferHeight = 0.2f;
    public float jumpBufferTimerMax = 0.2f;
    public float jumpBufferTimer = 0;
    
    // grounded stuff
    [Header("Grounded stuff")]
    public bool groundedBool = true;
    public float groundedMS = 1.5f; //8
    public float groundedActiveMax;
    // public float groundedMax = 10f; // disable 1
    public float groundedDrag = 50f;
    // public float disabledGroundedMax = 2f;
    float[] groundedMax = {10f, 7f, 4f};

    bool releasedBool = false; // tracks whether the player let go of jump
    bool abilityBool = true; // if the player can use their specials

    [Header("Jump stuff")]
    public float doubleJumpStrength = 16f;
    public float activeJumpStrength;
    // public float jumpStrength = 8f; // disabled 2
    // public float disabledJumpStrength = 2f;
    float[] jumpStrength = {8f, 6f, 4f};

    [Header("Air stuff")]
    public float airMS = 1f;
    public float airMax = 5f;
    public float airDrag = 0.001f;

    [Header("Ability cost")]
    public float doubleJumpCost = 5f;
	[SerializeField]public int doubleJumpCounter = 3;

    // coyote time
    float coyoteTime = 0.2f;
    float coyoteTimer = 0f;
    // stuff for jumping
    float maxJumpWindow = 0.2f; // amount of time after leaving the ground where forces still apply
    float jumpWindow; // something to decrement to track closing jumpWindow
    float tempJumpStrength; // used to track jump strength as it decreases the longer the player is in the air

    // float pushMS = 1f;
    float[] pushMS = {4f, 3f, 2f};

    private InputAction moveDirAction;
    private InputAction jumpAction;

	//To disable when tutorial going on
    public bool acceptingInput = true;
	//you should add something here that tracks states(do it later)

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		if (modifier == null)
			modifier = FindFirstObjectByType<PlayerModifiers>();
		if (timer == null)
			timer = FindFirstObjectByType<TimerScript>();

		moveDirAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        playerRB = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerAnimator = GetComponent<Animator>();

        jumpWindow = maxJumpWindow;
        tempJumpStrength = jumpStrength[modifier.GetJump()];
    }


    void FixedUpdate()
    {
        // maybe remove these
        Vector2 movementInput = moveDirAction.ReadValue<Vector2>();
        float horizontalMovementInput = movementInput.x;

        playerAnimator.SetFloat("playerInput", Mathf.Abs(horizontalMovementInput));
        playerAnimator.SetFloat("playerVel", Mathf.Abs(playerRB.linearVelocityX) * 0.4f);

        groundedBool = IsGrounded();
        // Debug.Log(groundedBool);
        if (!playerAnimator.GetBool("grounded") && groundedBool)
        {
            Debug.Log("land");
            playerAnimator.SetTrigger("land");
        }
        playerAnimator.SetBool("grounded", groundedBool);
        // this should be moved, also the if should be moved to collisionStay
        if (groundedBool){coyoteTimer = coyoteTime;}
        else{coyoteTimer -= Time.deltaTime;}
        
        RaycastHit2D grabCheck = Physics2D.Raycast(grabDetectionPoint.transform.position, Vector2.down, 0.20f);
        // check if the player is close enough to a grabable surface(add a check for holding the up key and how the ledge grab will acc work)
        if ((grabCheck.point != new Vector2(0,0)) && (grabCheck.point.y < grabDetectionPoint.transform.position.y) && modifier.GetClimb() && !grabCheck.collider.CompareTag("Water") && !grabCheck.collider.CompareTag("Gas") && grabCheck.collider.transform.rotation.x == 0 && !groundedBool) //&& (movementInput.y > 0)
        {
            // potentially add something to remember momentum going into the ledge
            if(grabHoldPosition == new Vector3(0, 0, 0))
            {
                Debug.Log("trigger climb");
                grabHoldPosition = this.transform.position;
                playerRB.gravityScale = 0; // this is just to hold the player in place
                playerAnimator.SetTrigger("climb");
                playerAnimator.SetBool("climbing", true);
            }
            playerRB.linearVelocity = new Vector3();
        }
        // thing is grabalbe and player want to grab
        else if (grabCheck.collider && (grabCheck.collider.tag == "Grabable") && (movementInput.y < 0) && groundedBool)
        {
            playerRB.linearVelocityX = pushMS[modifier.GetPush()] * horizontalMovementInput;
            grabCheck.collider.attachedRigidbody.linearVelocityX = pushMS[modifier.GetPush()] * horizontalMovementInput;

            playerAnimator.SetBool("pushing", true);

            // this is mostly just because we want to check if this is negative or positive(I know it's dumb)
            float facingValue = (this.transform.eulerAngles.y * -1 + 1) * horizontalMovementInput;
            if (facingValue > 0){playerAnimator.SetFloat("pushingDirection", 1);}
            else if(facingValue < 0){playerAnimator.SetFloat("pushingDirection", -1);}

        }
        else
        {
            grabHoldPosition = new Vector3();

            playerAnimator.SetBool("pushing", false);

            // change character rotation so they face where they want to go
            // if rotation is 0, player is facing right
            // if rotation is 180, player is facing left
            if (horizontalMovementInput > 0){this.transform.eulerAngles = new Vector3(0, 0, 0);}
            else if (horizontalMovementInput < 0){this.transform.eulerAngles = new Vector3(0, 180, 0);}

            // grounded movement
            if (groundedBool){ApplyMovement(groundedMS, groundedMax[modifier.GetSpeed()], groundedDrag);} // purge
            // airborne movement
            else{ApplyMovement(airMS, airMax, airDrag);}

            if (acceptingInput)
            {
                // jump handeling
                // check for player jump and let player rise a little after jumping
                if (((coyoteTimer > 0) || (jumpWindow > 0)) && jumpAction.IsPressed() || jumpBufferTimer > 0)
                {
                    if (jumpWindow == maxJumpWindow)
                    {
                        Debug.Log("jumped");
                        playerAnimator.SetTrigger("jump");
                        playerAnimator.SetBool("grounded", false);
                    }
                    jumpWindow -= Time.deltaTime;
                    playerRB.AddForce(Vector3.up * tempJumpStrength, ForceMode2D.Impulse);
                    tempJumpStrength *= 0.6f; // gives the start of the jump a nice snap to it without letting them go too high

                    jumpBufferTimer = 0;// prob unessary, stops the possibility of two jumps being buffered
                }
                // stops a weird "double jump" effect
                else if (!(coyoteTimer > 0) && !jumpAction.IsPressed())
                {
                    jumpWindow = 0;
                    releasedBool = true;
                }
                // check if player is close to ground, if they are, buffer a jump instead of double jumping
                RaycastHit2D jumpBufferCheck = Physics2D.Raycast(this.transform.position - new Vector3(0, 0.51f, 0), Vector2.down, jumpBufferHeight);
                if (jumpBufferCheck.collider && releasedBool && jumpAction.IsPressed() && !groundedBool)
                {
                    jumpBufferTimer = jumpBufferTimerMax;
                }
                // double jump
                else if (abilityBool && releasedBool && jumpAction.IsPressed() && !groundedBool && (doubleJumpCounter > 0))
                {
                    Debug.Log("double jumped");
                    playerRB.linearVelocityY = 0;
                    playerRB.AddForce(Vector3.up * doubleJumpStrength, ForceMode2D.Impulse);
                    abilityBool = false;

                    // timer.SubtractTime(doubleJumpCost);
                    doubleJumpCounter -= 1;

                    playerAnimator.SetTrigger("jump");
                }
                if (jumpBufferTimer > 0) { jumpBufferTimer -= Time.deltaTime; }
            }
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position - new Vector3(0, 0.51f, 0), Vector2.down, 0.1f);
        if (groundCheck.collider == collision.collider && !groundCheck.collider.isTrigger)
        {
            jumpWindow = maxJumpWindow;
            tempJumpStrength = jumpStrength[modifier.GetJump()]; // purge
            releasedBool = false;
            abilityBool = true;

            playerAnimator.ResetTrigger("jump");
            // if (!playerAnimator.GetBool("grounded"))
            // {
            //     Debug.Log("land");
            //     playerAnimator.SetTrigger("land");
            // }
            playerAnimator.SetBool("grounded", true);
        }
    }

    bool IsGrounded()
    {
        //NOTE: values should be slightly longer than the player character to their base
        //NOTE: the reason it's like is is to stop the ray from hitting the player, but that can be fixed
        RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position - new Vector3(0, 0.51f, 0), Vector2.down, 0.1f);
        if (groundCheck.collider && !groundCheck.collider.CompareTag("waterfall") && !groundCheck.collider.CompareTag("Gas"))
        {
            return playerCollider.IsTouching(groundCheck.collider);
        }
        return false;
    }

    void ApplyMovement(float movementSpeed, float maxSpeed, float drag)
    {
        Vector2 movementInput = moveDirAction.ReadValue<Vector2>();
        float horizontalMovementInput = movementInput.x;

        playerRB.linearVelocityX = Mathf.MoveTowards(playerRB.linearVelocityX, 0f, drag * Time.deltaTime); // apply drag
        // bellow top speed
        if (Mathf.Abs(playerRB.linearVelocityX) < maxSpeed)
        {
            playerRB.linearVelocityX += horizontalMovementInput * movementSpeed;
        }
        // above top speed
        else
        {
            float wishVel = playerRB.linearVelocityX + horizontalMovementInput * movementSpeed;
            if(Mathf.Abs(wishVel) < Mathf.Abs(playerRB.linearVelocityX)){playerRB.linearVelocityX = wishVel;} // if the player wants to decelerate
        }
    }

    public void FinishClimb()
    {
        this.transform.position = grabDetectionPoint.transform.position;
        playerRB.gravityScale = 3;
    }

    public void ResetPlayer()
    {
        playerRB.linearVelocity = new Vector3();
    }
}