using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField] float jumpHeight;      // Affects the max height a player can jump.
    [SerializeField] float movementSpeed;   // Affects horizontal movement speed.
    [SerializeField] float rollDistance;    // Affects how far the player moves when rolling.
    [SerializeField] float rollTime = 1f;   // Controls how long the roll takes. Lower values = faster roll.
    [SerializeField] float rollCooldown = 1f;
    [SerializeField] Transform groundCheck; // Allows a transform object to be placed visually in the editor. Acts as a check for colliding with the ground.
    [SerializeField] float groundRadius;// Used to specify the size of the overlap circle for grounded checks.

    Rigidbody2D rigidBody;
    Animator anim;
    Rigidbody2D pushableObjectRB;
    GameObject pushableObject;
    GameObject npc;
    GameObject item;
    bool grounded = false; // Used to see if player is on the ground.
    bool wallJumpPossible = false; // Used to see if a wall jump is possible.
    bool interactingWithObject = false; // Used to see if player is interacting with an object.
    bool movingObject = false; // Used to see if player is pushing/pulling an object.
    int numberOfJumps = 2; // Keeps track of the number of times the player has jumped for limiting wall jumps to 2 jumps only.
    bool facingRight = true;
    bool rolling = false;
    float nextRollTime = 1f;


    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        anim = GetComponent<Animator> ();
    }
	
	void Update () {
        Jump (); // Jump input carried out in Update due to slugishness of FixedUpdate for jumping. Plus force is added instantly, so update is fine.
        InteractWithObject (); // Needed in update rather than FixedUpdate due to potential of missing prompt for key release. Plus no force is added.
        Attack ();
        // Sets rolling bool to true when the associated key has been pressed, and the rollcooldown time has passed.
        // Does not allow rolling in the air.
        if (Input.GetKeyDown (KeyCode.Alpha2) && Time.time > nextRollTime && rigidBody.velocity.y == 0) {
            rolling = true;
            nextRollTime = Time.time + rollCooldown;
        }
    }

    // Use FixedUpdate for most rigidbody changes.
    void FixedUpdate () {
        IsGrounded ();
        IsWallJumpPossible ();
        Move ();

        // Activates the roll ability when rolling boolean is set to true, and then sets it back to false after roll is done, according to specified time.
        // Can be in fixed update as input is caught in update, and roll takes period over set time, therefore belongs in fixed update.
        if (rolling) {
            Roll ();
            Invoke ("RollReset", rollTime); // Sets rolling to false after roll is over.
        }

    }

    // ---------------
    // PLAYER MOVEMENT FUNCTIONS
    // ---------------
    void Move () {
        // Doesn't move if interacting or rolling.
        if (!interactingWithObject && !rolling) {
            float horizontalMovement = Input.GetAxis ("Horizontal");
            anim.SetFloat ("Speed", Mathf.Abs (horizontalMovement));
            rigidBody.velocity = new Vector2 (horizontalMovement * movementSpeed, rigidBody.velocity.y);

            // Flips the player sprite, except doesnt flip if player is moving an object.
            if (horizontalMovement > 0 && !facingRight && !movingObject) {
                Flip ();
            } else if (horizontalMovement < 0 && facingRight && !movingObject) {
                Flip ();
            }
        }
    }

    void Jump () {
        if (Input.GetButtonDown ("Jump") && (grounded || wallJumpPossible) && numberOfJumps > 1 && !rolling) {
            rigidBody.velocity = Vector2.zero; // Reset force for jump to keep jump power consistent and to prevent super jumps.
            rigidBody.AddForce (Vector2.up * jumpHeight, ForceMode2D.Impulse);
            numberOfJumps--; // Reduces jump count to stop endless wall jumping.
        }
    }

    // Flips the player sprite when changing direction in the x axis.
    void Flip () {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void IsGrounded () {
        // Creates a small circle on groundCheck transform object and constantly updates bool to reflect whether player is grounded or not.
        grounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, GameManager.instance.WhatIsGround);
        // Reset jump count back to 2 on player landing to enable wall jumps again.
        if (grounded) {
            numberOfJumps = 2;
        }
    }

    void IsWallJumpPossible () {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.right;
        float rayDistance = 0.8f;

        if (!facingRight) {
            direction = Vector2.left;
        }

        // Uncomment for visualising the ray.
        //Debug.DrawRay (position, direction * rayDistance, Color.red);
        RaycastHit2D hit = Physics2D.Raycast (position, direction, rayDistance, GameManager.instance.WhatIsWall);

        if (hit.collider != null) {
            wallJumpPossible = true;
        } else {
            wallJumpPossible = false;
        }
    }

    //------------
    // PLAYER ATTACK METHODS
    //------------
    void Attack () {
        if (Input.GetKeyDown (KeyCode.Alpha1)) {
            anim.SetTrigger ("Slash");
            // Stop flipping of sprite during animation. Check ori and hollow knight to see if they can chnage direction during swing.
        }
    }

    void Roll () {
        if (facingRight) {
            rigidBody.velocity = Vector2.zero; // Prevents additive velocity form movement at start of roll, also stops flipping of sprite.
            rigidBody.AddForce (Vector2.right * rollDistance, ForceMode2D.Impulse);
            anim.SetTrigger ("Slash");
        } else if (!facingRight) {
            rigidBody.velocity = Vector2.zero; // Prevents additive velocity form movement at start of roll, also stops flipping of sprite.
            rigidBody.AddForce (Vector2.left * rollDistance, ForceMode2D.Impulse);
            anim.SetTrigger ("Slash");
        }
    }

    void RollReset () {
        rolling = false;
    }

    //------------
    // PLAYER INTERACTION METHODS
    //------------
    void InteractWithObject () {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.right;
        float rayDistance = 2f;

        if (!facingRight) {
            direction = Vector2.left;
        }

        // Uncomment for visualising the ray.
        //Debug.DrawRay (position, direction * rayDistance, Color.green);
        RaycastHit2D hit = Physics2D.Raycast (position, direction, rayDistance, GameManager.instance.WhatIsObject);

        // Stores item or NPC gameobject the player is interacting with, then interacts accordingly.
        if (hit.collider != null && hit.collider.tag == "NPC") {
            npc = hit.collider.gameObject;
            if (npc != null) {
                if (Input.GetButtonDown ("Fire1")) {
                    npc.GetComponent<NPCController> ().Talk (); // Calls the talk script attached to the specific NPC being spoken to.
                    // If the player is talking to a NPC, player movement is locked.
                    if (npc.GetComponent<NPCController> ().IsNpcTalking) {
                        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
                        interactingWithObject = true;
                        anim.SetFloat ("Speed", 0);
                    } else {
                        rigidBody.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
                        interactingWithObject = false;
                    }
                }
            }
        } else if (hit.collider != null && hit.collider.tag == "Item") {
            item = hit.collider.gameObject;
            // If the object is an item carry out the following. ----TODO--- WILL NEED TO SEE IF THIS IS THE BEST WAY ONCE IT COMES TO DISPLAYING THE ITEMS IN THE INVENTORY. pOSSIBLY BETTER TO USE AN ARRAY METHOD INSTEAD AND JUST STORE ITEM NAME????
            if (item != null) {
                if (Input.GetButtonDown ("Fire1")) {
                    // Sets parent of the item to inventory gaemobject under player.
                    item.transform.SetParent (GameObject.Find ("Inventory").transform);
                    // Removes the collider from the item and deactivates. ---TODO--- MAY NEED TO DEACTIVATE EVERYTHING ELSE AS WELL.
                    item.GetComponent<BoxCollider2D> ().enabled = false;
                    item.SetActive (false);
                }
            }
        }

        // If an object is movable carry out the following.
        if (pushableObject != null && hit.collider != null && hit.collider.tag == "PushableObject") {
            // Allows pushing and pulling of an object by toggling fixed joint connection to player on movable object.
            if (Input.GetButton ("Fire2")) {
                movingObject = true;
                pushableObject.GetComponent<FixedJoint2D> ().enabled = true;
                pushableObject.GetComponent<FixedJoint2D> ().connectedBody = rigidBody;
                // Unlocks constraints on movable object.
                pushableObjectRB.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
            } else if (Input.GetButtonUp ("Fire2")) {
                movingObject = false;
                pushableObject.GetComponent<FixedJoint2D> ().connectedBody = null;
                pushableObject.GetComponent<FixedJoint2D> ().enabled = false;
                // Locks constraints on movable object.
                pushableObjectRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    void OnCollisionEnter2D (Collision2D collision) {
        // Allows interaction with interactable objects by storing gameobject details.
        if (collision.gameObject.tag == "PushableObject") {
            pushableObject = collision.gameObject;
            pushableObjectRB = collision.gameObject.GetComponent<Rigidbody2D> ();
        }
    }

    // Removes references to pushable objects on exit of collider.
    void OnCollisionExit2D () {
        pushableObject = null;
        pushableObjectRB = null;
    }
}
