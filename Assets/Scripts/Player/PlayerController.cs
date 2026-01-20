using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerController : MonoBehaviour
{

    public ControlSchemeManager controlScheme;
    public GunProfile currentProfile;
    public MultiAimConstraint headAimConstraint;
    //Creating a speed variable that can change
    float speed;
    Vector2 LookInput;
    public float walkingSpeed = 2.5f;
    public float croutchSpeed = 2.0f;
    public float runningSpeed = 5.5f;
    public float speedJump = 750.0f;
    public float speedClimb = 50.0f;
    public float rotationSpeed = 3.0f;
    public float speedVerticalRotation = 2.0f;
    public bool jetpackMode = true;
    public bool isClimbingLadder = false;
    public Camera cam;
    public float pitchRange = 85f;
    public float rollRange = 85f;
    private float pitch = 0f;   // Tracks camera X rotation
    Rigidbody playerRigidBody;
    Transform PlayerBase;
    bool canJump = false;
    public float floorCheckDistance = 1.0f;
    string movementState = "idle";
    string stanceState = "standing";
    public bool isMounted = true;
    public bool canDebugDismount = true;
    private bool resetCameraOffset = false;

    void Start()
    {
        //pulling in needed dependancies
        //cam = this.gameObject.GetComponentInChildren<Camera>();
        playerRigidBody = this.gameObject.GetComponent<Rigidbody>();
        PlayerBase = this.transform.Find("PlayerBase");

        //initialize gravity based on jetpack state
        if (jetpackMode.Equals(true))
        {
            playerRigidBody.useGravity = false;
        }
        else
        {
            playerRigidBody.useGravity = true;
        }

        //Set Cursor to the middle of the game window
        Cursor.lockState = CursorLockMode.Locked;
        //Set Cursor to not be visible
        Cursor.visible = false;

    }
    // Update is called once per frame
    void Update()
    {
        //Delete Me
        if (isMounted && canDebugDismount)
        {
            if (Input.GetKeyDown(controlScheme.interact))
            {
                isMounted = false;
            }
        }

        //Check for jetpack settings
        if (Input.GetKeyDown(controlScheme.flyingMode))
        {
            if (jetpackMode.Equals(false))
            {
                jetpackMode = true;
                playerRigidBody.useGravity = false;
            }
            else
            {
                jetpackMode = false;
                playerRigidBody.useGravity = true;
            }

        }

        if (!isMounted)
        {
            //Check Stance and set stanceState
            if (Input.GetKeyDown(controlScheme.croutch))
            {
                if (stanceState.Equals("standing"))
                {
                    stanceState = "crouching";
                }
                else
                {
                    stanceState = "standing";
                }
            }
        }


        if (!isMounted)
        {
            //Check to sprint and set movement animation states
            if (Input.GetKey(controlScheme.foreward) || Input.GetKey(controlScheme.backward) || Input.GetKey(controlScheme.left) || Input.GetKey(controlScheme.right))
            {
                if (Input.GetKey(controlScheme.sprint) && stanceState != "crouching")
                {
                    speed = runningSpeed * Time.deltaTime;
                    movementState = "sprinting";
                }
                else
                {
                    if (stanceState == "crouching")
                    {
                        speed = croutchSpeed * Time.deltaTime;
                        movementState = "walking";
                    }
                    else
                    {
                        speed = walkingSpeed * Time.deltaTime;
                        movementState = "walking";
                    }
                }
            }
            else
            {
                speed = 0;
                movementState = "idle";
            }
        }

        if (!isMounted)
        {
            RaycastHit hit;
            // This determines if the player is touching the ground or a surface underneath them
            if (Physics.Raycast(PlayerBase.position, PlayerBase.TransformDirection(Vector3.down), out hit, floorCheckDistance))
            {
                Debug.DrawRay(PlayerBase.position, PlayerBase.TransformDirection(Vector3.down) * floorCheckDistance, Color.yellow);
                //Debug.Log("On the ground");
                canJump = true;
            }
            else
            {
                Debug.DrawRay(PlayerBase.position, PlayerBase.TransformDirection(Vector3.down) * floorCheckDistance, Color.white);
                //Debug.Log("Not on the ground");
                canJump = false;
                movementState = "jump";
            }
        }
        

        //For camera controls
        //Get the horizontal movement of the mouse to rotate the character from side to side
        LookInput.x = rotationSpeed * Input.GetAxis("Mouse X");
        //Get the vertical movement of the mouse to rotate the camera up and down
        LookInput.y = speedVerticalRotation * Input.GetAxis("Mouse Y");

        //Set the camera to move up and down based off the vertical variable. (to invert make it positive)
        if (controlScheme.verticalInversion)
        {
            pitch += LookInput.y;
        }
        else
        {
            pitch -= LookInput.y;
        }

        // Clamp pitch to prevent flipping
        pitch = Mathf.Clamp(pitch, -pitchRange, pitchRange);

        if (isMounted)
        {
            // Apply camera rotation using the clamped pitch
            cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            float yVal = headAimConstraint.data.offset.y + LookInput.x;
            float rollVal = Mathf.Clamp(yVal, -rollRange, rollRange);

            headAimConstraint.data.offset = new Vector3(0f, rollVal, 0f);

            if (resetCameraOffset == false)
            {
                resetCameraOffset = true;
            }
        }
        else
        {
            if (resetCameraOffset == true)
            {
                headAimConstraint.data.offset = new Vector3(0f, 0f, 0f);
                resetCameraOffset = false;
            }
            
            // Apply camera rotation using the clamped pitch
            cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            //Set the character to move left and right based off the horizontal variable
            transform.Rotate(0, LookInput.x, 0);
        }



        if (jetpackMode)
        {
            JetpackControls();
        }
        else
        {
            //Check Ladder State
            if (isClimbingLadder.Equals(true))
            {
                playerRigidBody.useGravity = false;
                LadderControls();
            }
            else
            {
                if (!isMounted)
                {
                    playerRigidBody.useGravity = true;
                    playerRigidBody.isKinematic = false;
                    NormalControls();
                }
                else
                {
                    playerRigidBody.useGravity = false;
                    playerRigidBody.isKinematic = true;
                }
            }
        }
    }

    void NormalControls()
    {
        //Check for moving forwards
        if (Input.GetKey(controlScheme.foreward))
        {
            transform.Translate(0, 0, speed);
        }
        //Check for moving backwards
        if (Input.GetKey(controlScheme.backward))
        {
            transform.Translate(0, 0, -speed);
        }
        //Check for moving left
        if (Input.GetKey(controlScheme.left))
        {
            transform.Translate(-speed, 0, 0);
        }
        //Check for moving right
        if (Input.GetKey(controlScheme.right))
        {
            transform.Translate(speed, 0, 0);
        }
        //Check for jumping
        if (canJump.Equals(true) && Input.GetKeyDown(controlScheme.jump))
        {
            playerRigidBody.AddForce(0, speedJump, 0, ForceMode.Impulse);
        }
    }
    void JetpackControls()
    {
        //Check for moving forwards
        if (Input.GetKey(controlScheme.foreward))
        {
            transform.Translate(0, 0, speed);
        }
        //Check for moving backwards
        if (Input.GetKey(controlScheme.backward))
        {
            transform.Translate(0, 0, -speed);
        }
        //Check for moving left
        if (Input.GetKey(controlScheme.left))
        {
            transform.Translate(-speed, 0, 0);
        }
        //Check for moving right
        if (Input.GetKey(controlScheme.right))
        {
            transform.Translate(speed, 0, 0);
        }
        //Check for moving up
        if (Input.GetKey(controlScheme.flyingUp))
        {
            if (Input.GetKey(controlScheme.sprint))
            {
                transform.Translate(0, runningSpeed * Time.deltaTime, 0);
            }
            else
            {
                transform.Translate(0, walkingSpeed * Time.deltaTime, 0);
            }
        }
        //Check for moving down
        if (Input.GetKey(controlScheme.flyingDown))
        {
            if (Input.GetKey(controlScheme.sprint))
            {
                transform.Translate(0, -runningSpeed * Time.deltaTime, 0);
            }
            else
            {
                transform.Translate(0, -walkingSpeed * Time.deltaTime, 0);
            }
        }
    }

    void LadderControls()
    {
        //Check for moving up ladder
        if (Input.GetKey(controlScheme.foreward))
        {
            transform.Translate(0, speedClimb * Time.deltaTime, 0);
            //playerRigidBody.AddForce(0, speedClimb, 0, ForceMode.Force);
        }
        //Check for moving backwards
        if (Input.GetKey(controlScheme.backward))
        {
            isClimbingLadder = false;
        }
        //Check for moving left
        if (Input.GetKey(controlScheme.left))
        {
            isClimbingLadder = false;
        }
        //Check for moving right
        if (Input.GetKey(controlScheme.right))
        {
            isClimbingLadder = false;
        }
        //Check for jumping
        if (canJump.Equals(true) && Input.GetKeyDown(controlScheme.jump))
        {
            playerRigidBody.AddForce(0, speedJump, 0, ForceMode.Impulse);
        }
    }

    public float GetCurrentSpeed()
    {
        return speed;
    }
    public Vector2 GetLookInput()
    {
        return LookInput;
    }
    public bool CanJump()
    {
        return canJump;
    }
    public string GetMovementState()
    {
        return movementState;
    }
    public string GetStanceState()
    {
        return stanceState;
    }
    public void SetMountStatus(bool status)
    {
        isMounted = status;
    }
    public bool GetMountStatus()
    {
        return isMounted;
    }
}
