using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public ControlSchemeManager controlScheme;
    //Creating a speed variable that can change
    float speed;
    Vector2 LookInput;
    public float speedWalking = 2.0f;
    public float speedRunning = 5.0f;
    public float speedJump = 750.0f;
    public float speedClimb = 50.0f;
    public float speedRotation = 2.0f;
    public float speedVerticalRotation = 2.0f;
    public bool jetpackMode = true;
    public bool isClimbingLadder = false;
    public Camera cam;
    public float minPitch = -85f;
    public float maxPitch = 85f;
    private float pitch = 0f;   // Tracks camera X rotation
    Rigidbody playerRigidBody;
    Transform PlayerBase;
    bool canJump = false;
    float floorCheckDistance = 1.0f;
    string movementState = "idle";
    string cameraState = "center";
    string aimState = "basic";
    string stanceState = "standing";
    public bool isMounted = true;

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

        if (isMounted)
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


        if (isMounted)
        {
            //Check to sprint and set movement animation states
            if (Input.GetKey(controlScheme.foreward) || Input.GetKey(controlScheme.backward) || Input.GetKey(controlScheme.left) || Input.GetKey(controlScheme.right))
            {
                if (Input.GetKey(controlScheme.sprint) && stanceState != "crouching")
                {
                    speed = speedRunning * Time.deltaTime;
                    movementState = "sprinting";
                }
                else
                {
                    speed = speedWalking * Time.deltaTime;
                    movementState = "walking";
                }
            }
            else
            {
                speed = 0;
                movementState = "idle";
            }
        }


        //Check Camera animation states
        if (Input.GetKey(controlScheme.leanRight))
        {
            cameraState = "right";
        }
        else if (Input.GetKey(controlScheme.leanLeft))
        {
            cameraState = "left";
        }
        else
        {
            cameraState = "center";
        }

        //Check for AimState
        if (Input.GetKey(controlScheme.weaponAimDownSights))
        {
            aimState = "ads";
        }
        else
        {
            aimState = "basic";
        }

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
        }

        //For camera controls
        //Get the horizontal movement of the mouse to rotate the character from side to side
        LookInput.x = speedRotation * Input.GetAxis("Mouse X");
        //Get the vertical movement of the mouse to rotate the camera up and down
        LookInput.y = speedVerticalRotation * Input.GetAxis("Mouse Y");

        //Set the character to move left and right based off the horizontal variable
        transform.Rotate(0, LookInput.x, 0);

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
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply camera rotation using the clamped pitch
        cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);


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
                if (isMounted)
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
                transform.Translate(0, speedRunning * Time.deltaTime, 0);
            }
            else
            {
                transform.Translate(0, speedWalking * Time.deltaTime, 0);
            }
        }
        //Check for moving down
        if (Input.GetKey(controlScheme.flyingDown))
        {
            if (Input.GetKey(controlScheme.sprint))
            {
                transform.Translate(0, -speedRunning * Time.deltaTime, 0);
            }
            else
            {
                transform.Translate(0, -speedWalking * Time.deltaTime, 0);
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
    public string GetCameraState()
    {
        return cameraState;
    }
    public string GetAimState()
    {
        return aimState;
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
