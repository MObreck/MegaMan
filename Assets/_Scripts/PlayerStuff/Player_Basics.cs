using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent (typeof (Controller2D))]
public class Player_Basics : _Generic_Character
{
    public PlayerStuff zz; //Get all the data from the AnimStuff struct on access.

    public int maxHP = 112;
    public int HP;

    public bool stunned; //When set the player is considered stunned from being hurt and cannot move.
    public bool onLadder;
    public float stunTime = 0.2f;
    float trueStunTime;

    public float maxJumpHeight = 4; //The max height of the jump
    public float minJumpHeight = 0.25f;

    public float timeToJumpApex = 0.4f; //0.4f; //The time it takes to reach maximum jump height.
    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;
    public float moveSpeed = 6;
    public float slideSpeed = 8;
    public float ladderSpeed = 4.5f;
    public float stunSpeed = 2; //The speed the player stumbles backwards when stunned.

    [HideInInspector] public float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    public bool debugJumpReset; //A debug flag that will tell the coding to reset the gravity and maxJumpVelocity/minJumpVelocity.

    Vector3 velocity;
    Vector2 input;
    float velocityXSmoothing;

    Controller2D controller;

    public float endShootPause;
    public float shootPause = 0.25f;

    public bool isSliding;

    public bool cutScened; //Non-stun based reasons the player doesn't have control over themselves.
    public bool canWallClimb; //Can this player preform wall climbing to begin with?
    public bool noSlide; //Disables the ability to slide.
    public bool buttonSlide; //Sets the slide command to a single button instead of Down+Jump;

    public float wallSlideSpeedMax = 3; //The maxium decent speed the player can fall at while sliding down a wall. (for wall jumping/clinging only).
    public Vector2 wallJumpClimb; //For "climb" jumping straight up a wall.
    public Vector2 wallDropOff; //Drop straigt off a wall from wall climbing.
    public Vector2 wallLeap; //Spring off a wall type wall jumping.
    bool wallFall; //Set after dropping off a wall to disable wall cling.
    public float wallStickTime = 0.25f;
    float timeToWallUnstick;

    List<int> animIntS = new List<int>(); //An array list of all the animation start points for the various standing animations.
    List<int> animIntJ = new List<int>(); //An array list of all the animation start points for the various jumping animations.
    List<int> animIntW = new List<int>(); //An array list of all the animation start points for the various walking animations.
    List<int> animIntL = new List<int>(); //An array list of all the animation start points for the various ladder animations.

    public int spriteFacing = 1; //Has to be a seperate Int from collisions.faceDir since the latter is effected by the interia of moving platforms/etc.

    public Vector4[] aSet; //A set of animation ID numbers for the currently using standing, walking, etc animations. Changing when shooting and such.
    // X = Standing 0
    // Y = Jumping 4
    // Z = Walking 12
    // W = Ladder 17

    public P_Inventory myInv;

    public bool debugTest;

    public GameObject[] playerDusts;

    public Vector3[] shotOffsets;
    [HideInInspector] public Vector3 wepOffset;
    public AudioClip shotSound;
    public int shotAnim;
    public List<GameObject> shotReserve;
    public Transform weaponCache;
    public bool noShotMove; //This is used for weapons that don't allow ground movement while shooting, such as the Metal Blade or Bass Buster.


    // Use this for initialization
    void Start ()
    {
        HP = maxHP;

        controller = GetComponent<Controller2D>();
        bootAnimInts(); //Load in all the int numbers for swappable animations like standing, walking, etc (the ones that change when shooting and such).
        zz.bootVars();

        changeColliders(0); //Load the default player collision box.

        //Create the player sliding and damaged dust effects and put them in the player cache pool.
        for (int i = 0; i < playerDusts.Length; i++)
        {
            playerDusts[i] = Instantiate(playerDusts[i], myTrans.position, Quaternion.identity) as GameObject;
            playerDusts[i].transform.parent = weaponCache;
            playerDusts[i].transform.localScale = Vector3.one;
            playerDusts[i].SetActive(false);
        }

        resetJumpGravity();
    }

    void bootAnimInts() //Load in all the int numbers for swappable animations like standing, walking, etc (the ones that change when shooting and such).
    {
        animIntS.Clear(); //An array list of all the animation start points for the various standing animations.
        animIntJ.Clear(); //An array list of all the animation start points for the various jumping animations.
        animIntW.Clear(); //An array list of all the animation start points for the various walking animations.
        animIntL.Clear(); //An array list of all the animation start points for the various ladder animations.

        animIntS.Add(0); //normal [0]
        animIntS.Add(25); //shooting [1] 
        animIntS.Add(37); //throwing [2]
        animIntS.Add(49); //knuckle shot part 1 [3]
        animIntS.Add(61); //knuckle shot part 2 [4]
        animIntS.Add(72); //Holding Gutsblock [5]

        animIntJ.Add(4); //normal [0]
        animIntJ.Add(24); //shooting [1] 
        animIntJ.Add(36); //throwing [2]
        animIntJ.Add(48); //knuckle shot part 1 [3]
        animIntJ.Add(60); //knuckle shot part 2 [4]
        animIntJ.Add(75); //Holding Gutsblock [5]

        //NOTE: YES THE WALKING ANIMATIONS DO START WITH THE STANDING POSE ON BUT THE NORMAL AND GUTS ANIMATION [0 & 5] SEQUENCES. THE STAND POSE REPLACES THE INCHING FORWARD FRAME IN ALL THE OTHER SETS
        animIntW.Add(12); //normal [0]
        animIntW.Add(25); //shooting [1] 
        animIntW.Add(37); //throwing [2]
        animIntW.Add(49); //knuckle shot part 1 [3]
        animIntW.Add(61); //knuckle shot part 2 [4]
        animIntW.Add(76); //Holding Gutsblock [5]

        animIntL.Add(17); //normal [0]
        animIntL.Add(30); //shooting [1] 
        animIntL.Add(42); //throwing [2]
        animIntL.Add(54); //knuckle shot part 1 [3]
        animIntL.Add(66); //knuckle shot part 2 [4]
        animIntL.Add(30); //Holding Gutsblock [5] (Doesn't actaully have ladder frames so just reuse the default frame start.
    }

    void resetJumpGravity()
    {
//        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2); //Calculate the gravity using some physics stuff.
        gravity = -(1.5f * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 1.5f); //Calculate the gravity using some physics stuff.
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        debugJumpReset = false;
    }
	
    void FixedUpdate ()
    {
        if (!onLadder && !freezeCheck())
            velocity.y += gravity * Time.fixedDeltaTime;

        if (!freezeCheck())
            controller.Move(velocity * Time.deltaTime, input);

        //Stops the gravity from endlessly building up if the player is on the ground of if they bump a ceiling.
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }

    void Update ()
    {
        if (debugJumpReset)
            resetJumpGravity();

        if (Input.GetButtonDown("DebugButton"))
            Frozen = !Frozen;

        if (freezeCheck())
            FrozenTimers();

        bool wallSliding = false; //Reset the wall sliding bool flag.

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //stunned; //When set the player is considered stunned from being hurt and cannot move.
        if (!stunned && !cutScened && zz._autoSlideTime < Time.time)
        {
            //Get the directional controller input.

            if (onLadder)
            {
                if (!zz.isShooting)
                {
                    //If on a ladder and not shooting input y moves vertically.
                    float targetVelocityY = input.y * ladderSpeed;
                    velocity.y = targetVelocityY; //Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
                }
                else velocity.y = 0;
            }
            else
            {
                //Move horizontally with some smoothing
                float targetVelocityX = input.x * moveSpeed;
                velocity.x = targetVelocityX; //Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
            }

            // This void handles all the player's attack routines
            playerShoot(ref input);

        }
        else
            input = Vector2.zero;

        // This void handles all the player's slide routines
        playerSlide(ref input);

        //
        //Walljumping supporting jumping controls.
        //*****************************************************************************

        if (controller.collisions.below)
            wallFall = false;
        int wallDirX = (controller.collisions.left) ? -1 : 1; //Get the direction a wall the player is sliding off is in.
        // && input.x != 0
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0 && input.x != 0 && canWallClimb && !wallFall) //Check if player is pushing against a wall and airborne and able to wall climb.
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
                velocity.y = -wallSlideSpeedMax;
            /*
            if (timeToWallUnstick > 0)
            {
                velocity.x = 0;
                velocityXSmoothing = 0;

                if (input.x != wallDirX && input.x != 0)
                {
                    timeToWallUnstick -= Time.fixedDeltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
                timeToWallUnstick = wallStickTime;
                */
        }

        //debugTest = zz.isShooting;

        //********************************************************************************************************************************
        //Jumping Code
        if (!stunned && !cutScened && !onLadder && zz._autoSlideTime < Time.time && !zz.noJumping) //Can't use jump controls if any of those coditions are active or the player is locked into a sliding.
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (wallSliding)
                {
                    if (input.y > 0)
                    {
                        velocity.x = -wallDirX * wallJumpClimb.x;
                        velocity.y = wallJumpClimb.y;
                    }
                    else if (input.y < 0)
                    {
                        velocity.x = -wallDirX * wallDropOff.x;
                        velocity.y = wallDropOff.y;
                        wallFall = true;
                    }
                    else
                    {
                        velocity.x = -wallDirX * wallLeap.x;
                        velocity.y = wallLeap.y;
                    }
                }
                if (controller.collisions.below && (noSlide || input.y >= 0 || buttonSlide))
                {
                    velocity.y = maxJumpVelocity;
                    zz.didJump = true;
                }
            }
            //*****************************************************************************

            //Non-walljumping supporting jumping controls.
            //*****************************************************************************
            /*
            if (Input.GetButtonDown("Jump") && controller.collisions.below)
            {
                velocity.y = maxJumpVelocity;
            }
            */
            //*****************************************************************************
            if (Input.GetButtonUp("Jump") && !controller.collisions.below && zz.didJump) //If you release the jump button early stop going up.
            {
                if (velocity.y > minJumpVelocity)
                {
                    velocity.y = minJumpVelocity;
                }
            }

        }
        //********************************************************************************************************************************

        //For some reason in Classic Megaman if the player is standing still on the ground and shooting they can't move, despite being able to shoot while already walking.
        //...or if the player is using a weapon that prohibits ground movement while shooting such as the Metal Blade or Bass Buster.
        if (zz.isShooting && (zz.isStanding || (noShotMove && controller.collisions.below)))
        {
            input.x = 0;
            velocity.x = 0;
        }

        if (velocity.y <= 0)
            zz.didJump = false;
        if (!isSliding)
            zz.noJumping = false;
        //&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&
        /*
                if (!onLadder)
                    velocity.y += gravity * Time.fixedDeltaTime;
        */
        //&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&
        AnimatePlayer(input);

        //===============================================
        // LADDER DETECTION;
        //===============================================
        detectLadder(ref velocity, input);

        //Void that checks to see if the player is being damaged by an enemy.
        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
        detectEnemyContact(ref velocity);
        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
        //&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&
        /*
        controller.Move(velocity * Time.fixedDeltaTime, input);

        //Stops the gravity from endlessly building up if the player is on the ground of if they bump a ceiling.
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
        */
        //&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&&^%&^%&^%&^%&^%&%^&
    }

    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
    IEnumerator Stunned_Sequence(float animSpeed)
    {
        int stunCount = 0;
        int flashCount = 0; //The number of times the player blinks after recovering from being hit stunned. They are still immune to damage at this time.

        while (stunCount < 5) //repeats the stun animation frames 5 times before letting the player recover.
        {
            if (freezeCheck())
                yield return new WaitForSeconds(Time.deltaTime);
            else
            {
                mySprite.sprite = mySprites[5];
                yield return new WaitForSeconds(animSpeed);
                mySprite.sprite = mySprites[3];
                yield return new WaitForSeconds(animSpeed);
                stunCount += 1;
            }
        }
        stunned = false; //Player recovers from stun.

        while (flashCount < 8) //Gives 8 frame blinks of damage immunity. The player can control themself again during this time but is also invincible.
        {
            if (freezeCheck())
                yield return new WaitForSeconds(Time.deltaTime);
            else
            {
                mySprite.enabled = false;
                yield return new WaitForSeconds(animSpeed);
                mySprite.enabled = true;
                yield return new WaitForSeconds(animSpeed);
                flashCount += 1;
            }
        }

        mySprite.enabled = true;
        mySprite.transform.tag = "Untagged"; //Setting this once again makes the player able to be damaged.
    }

    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // This script checks for player contact with enemies (thus gets hurt if not already stunned/etc)

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void detectEnemyContact(ref Vector3 velocity)
    {
        // 5 and 8
        if (stunned)
        {
            velocity.x = stunSpeed * -spriteFacing;
        }
        //5
        if (mySprite.transform.tag != "Untagged") //If the player's sprite object has certain tags set they cannot be damaged.
            return;

        float hitLength = aa.Micro;

        Vector2 myDir = Vector2.right; //velocity.normalized; //"normalize" velocity to get a Vector2 type movement direction for ray cast BoxCast.
        Vector2 boxSize = controller.collider.size * 6;

        Vector3 boxPos = myTrans.position + new Vector3(controller.collider.offset.x * 6, controller.collider.offset.y * 6, 0f);

        RaycastHit2D hit = Physics2D.BoxCast(boxPos, boxSize, 0f, myDir, hitLength, aa.E_Layer);

        if (hit && hit.transform.tag != "Dead")
        {
            if (AnimCoroutine != null)
                StopCoroutine(AnimCoroutine);
            onLadder = false; //The player gets knocked off ladders when hit by an enemy.
            zz._slideTime = 0; //Set sliding to reset the moment the player isn't in an area to short to stand up in.
            mySprite.sprite = mySprites[5]; //Set the sprite to the player's pain sprite. Mainly used for if the player is struck while stunned.
            StartCoroutine(Stunned_Sequence(aa.p_flicker_time));
            stunned = true;
            playSFX(bb.TestSounds[4], true);
            mySprite.transform.tag = "Damaged"; //Setting this tag makes the player immune to damage.
            spriteFacing = (hit.transform.position.x < myTrans.position.x) ? -1 : 1;
            velocity = Vector3.zero;

            trueStunTime = Time.time + stunTime;

            Enemy_Basics myHit = hit.transform.parent.GetComponent<Enemy_Basics>();
            HP -= myHit.damage;
            playerDusts[0].SetActive(true);
            playerDusts[0].transform.position = myTrans.position;
            myInv.refresh_HealthBar();
            //testy.damageEnemy(baseDamage);
            //gameObject.SetActive(false);
        }
    }
    //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // This script checks for player contact with ladders as well as setting up climbing on them.

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void detectLadder(ref Vector3 velocity, Vector2 input)
    {
        // 5 and 8
        if (stunned || cutScened || zz.isShooting || zz.noLad > Time.time) //Ladder detection doesn't run when stunned/etc.
            return;

        float hitLength = aa.Micro;

        Vector2 myDir = Vector2.up;
        Vector2 boxSize = controller.collider.size * 5.5f;
        boxSize.x *= 0.15f; //Make the boxcast's X size narrower so ladders have to be close the player's center to be detected.

        Vector3 boxPos = myTrans.position + new Vector3(controller.collider.offset.x * 6, controller.collider.offset.y * 6, 0f);

        RaycastHit2D hit = Physics2D.BoxCast(boxPos, boxSize, 0f, myDir, hitLength, aa.LadderMask);

        if (onLadder)
        {
            //ALSO CHECK THE ANIMATE PLAYER VOID FOR LADDER TOP TYPE DETECTION
            if ((input.y < 0 && controller.collisions.below))
            {
                onLadder = false;
                zz.isAirBorne = false;
                velocity.y = 0;
                controller.collisions.below = true;
            }
            else if (!hit || Input.GetButtonDown("Jump"))
            {
                onLadder = false;
            }
        }
        else
        {
            float rayLength = 0.48f;
            Vector3 lowPosition = myTrans.position;
            lowPosition.y -= rayLength;
            RaycastHit2D hitD = Physics2D.Raycast(lowPosition, -Vector2.up, rayLength, aa.collisionMask);

            if (hitD && hitD.transform.tag == "LadderTop" && input.y < 0)
            {
                velocity = Vector3.zero;
                myTrans.position = new Vector3(hitD.transform.position.x, hitD.transform.position.y + rayLength, myTrans.position.z);
                NewAnimation(animIntL[0], 1, 6, 1, 1);
                zz.AnimReset();
                onLadder = true;
                return;
            }
            //            if ((hit && input.y > 0) || (hitD && input.y < 0 && controller.collisions.below))
            if (hit && input.y > 0)
            {
                velocity = Vector3.zero;
                myTrans.position = new Vector3(hit.transform.position.x, myTrans.position.y, myTrans.position.z);
                NewAnimation(animIntL[0], 1, 6, 1, 1);
                zz.AnimReset();
                onLadder = true;
            }
        }
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // SHOOT SCRIPT

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void playerShoot(ref Vector2 input)
    {
        if (isSliding || freezeCheck())
            return;

        if (zz.isShooting && Time.time > endShootPause) //If there is still waiting time don't move at all.
        {
            zz.isShooting = false; //Must be above AnimReset
            zz.AnimReset();
            if (zz.isWalking)
            {
                startFrame = animIntW[0];
                animFrameCount = animIntW[0] + baseFrameCount;
                //mySprite.sprite = mySprites[animIntW[0] + baseFrameCount + 1];
            }
        }


        if (Input.GetButtonDown("Shoot") && myInv.wepAmmo[myInv.cw] > 0)
        {
            for (int i = 0; i < shotReserve.Count; i++)
            {
                if (!shotReserve[i].activeSelf)
                {
                    if (input.x != 0)
                        spriteFacing = (input.x < 0) ? -1 : 1;

                    zz.isShooting = true; //Must be above AnimReset
                    zz.AnimReset();
                    if (zz.isWalking)
                    {
                        startFrame = animIntW[shotAnim];
                        animFrameCount = animIntW[shotAnim] + baseFrameCount;
                        //mySprite.sprite = mySprites[animIntW[1] + baseFrameCount + 1];
                    }

                    int pickOffset = (zz.isStanding) ? 1 : 0;
                    wepOffset = new Vector3(shotOffsets[pickOffset].x * spriteFacing, shotOffsets[pickOffset].y, 0);

                    //shotReserve[i].transform.position = myTrans.position + (myOffset);
                    shotReserve[i].SetActive(true);
                    playSFX(shotSound, false);
                    endShootPause = Time.time + shootPause;
                    myInv.wepAmmo[myInv.cw] -= myInv.wepScript[myInv.cw].AmmoUse;
                    myInv.refresh_AmmoBar();
                    break;
                }
            }
            //shotOffsets
        }
    }
    
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // SLIDE SCRIPT

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void playerSlide(ref Vector2 input)
    {
        //didJump;
        //noJumping;
        if (noSlide)
            return;
        if (isSliding)
        {
            if (onLadder) //Shut down sliding if the player grabs a ladder.
            {
                changeColliders(0);
                isSliding = false;
                velocity = Vector3.zero;
                return;
            }

            bool ceilingDetected = false;
            zz.noJumping = false;

            //if (Time.time > zz._autoSlideTime)
            {
                controller.UpdateRaycastOrigins();

                float rayLength = 0.72f; //Set up a length for the casted rays based on the velocity y speed + skinWidth as a modifer.

                for (int i = 0; i < controller.verticalRayCount; i++)
                {
                    Vector2 rayOrigin = controller.raycastOrigins.topLeft;
                    rayOrigin.y -= 0.24f;
                    rayOrigin += Vector2.right * (controller.verticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, aa.collisionMask);

                    Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.yellow);

                    if (hit)
                    {
                        zz.noJumping = true;
                        ceilingDetected = true;
                    }
                }
            }

            //If the player isn't on the floor or sliding time is over or tries to move after the forced autoslide time is past run the slide ender script.
            if (!controller.collisions.below || Time.time > zz._slideTime || (input.x != 0 && Time.time > zz._autoSlideTime) || controller.collisions.footLevel) // (controller.collisions.right && spriteFacing > 0) || (controller.collisions.left && spriteFacing < 0))
            {
                if (!ceilingDetected)
                {
                    changeColliders(0);
                    isSliding = false;
                }
            }

            if (Time.time < zz._autoSlideTime)
            {
                input.x = 0; // Remove any X input while the player is stuck in the forced sliding timeframe (first 50% of the slide time).
            }
            velocity.x = spriteFacing * slideSpeed;
        }
        else
        {
            if (onLadder || stunned || cutScened || !controller.collisions.below || controller.collisions.footLevel) // || (controller.collisions.right && spriteFacing > 0) || (controller.collisions.left && spriteFacing < 0))
                return;

            //If player is standing on top of a ladder, no sliding option.
            //----------------------------------------------------------------
            //----------------------------------------------------------------
            float rayLength = 0.48f;
            Vector3 lowPosition = myTrans.position;
            lowPosition.y -= rayLength;
            RaycastHit2D hitD = Physics2D.Raycast(lowPosition, -Vector2.up, rayLength, aa.collisionMask);

            if (hitD && hitD.transform.tag == "LadderTop")
                return;
            //----------------------------------------------------------------
            //----------------------------------------------------------------

            if ((Input.GetButtonDown("Jump") && input.y < 0 && !buttonSlide) || (Input.GetButtonDown("Slide") && buttonSlide))
            {
                changeColliders(1);
                zz._slideTime = Time.time + zz.slideTime;
                zz._autoSlideTime = Time.time + (zz.slideTime * 0.5f); //Makes the first 50% of the player's sliding time forced, after which they regain control and can choose to cancel out with other inputs.

                Vector2 dustSpot = (spriteFacing == -1) ? controller.raycastOrigins.bottomLeft : controller.raycastOrigins.bottomRight;
                dustSpot.x -= 0.54f * spriteFacing;
                playerDusts[1].SetActive(false);
                playerDusts[1].SetActive(true);
                playerDusts[1].transform.position = dustSpot;

                isSliding = true;
            }
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // ANIMATION SCRIPT

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void AnimatePlayer(Vector2 input) //The sprite animation coding for this player.
    {
        int currentSet = (zz.isShooting) ? shotAnim : 0; //The current 
        if (input.x != 0 && !freezeCheck())
            spriteFacing = (input.x < 0) ? -1 : 1;

        if (onLadder)
        {
            if (zz.isShooting)
            {
                mySprite.flipX = (spriteFacing < 0) ? true : false; //Make the player's sprite mirrioring based on the faceDir.
                zz.ladFrame = (spriteFacing < 0) ? 17 : 18;
                NewAnimation(animIntL[currentSet], 1, 6, 1, 1);
            }
            else
            {
                mySprite.flipX = false; //No sprite flipping when the playing is using normal ladder animations.
                if (AnimCoroutine != null)
                    StopCoroutine(AnimCoroutine);
                if (Time.time > zz._ladCount && input.y != 0 && !freezeCheck())
                {
                    zz.ladFrame = (zz.ladFrame != 17) ? 17 : 18;
                    zz._ladCount = Time.time + zz.ladCount;
                }
                //********************************************************************************************************************************************
                //These lines of code are what case the player to enter their "hunched over" animation when they are close to the top of the ladder.
                float rayLength = 1.44f;
                Vector3 checkPos = myTrans.position;
                checkPos.y -= 0.48f;

                Debug.DrawRay(checkPos, Vector2.up * rayLength, Color.red);
                //Check for a "laddertop" object that would be spawned by any ladder segments that have empty space above them.
                RaycastHit2D hitU = Physics2D.Raycast(checkPos, Vector2.up, rayLength, aa.collisionMask);

                if (hitU && hitU.transform.tag == "LadderTop")
                {
                    float getDist = myTrans.position.y - hitU.transform.position.y;

                    if (getDist >= 0.7f && input.y > 0) //Leave a ladder if you reach its top
                    {
                        myTrans.position = new Vector3(myTrans.position.x, hitU.transform.position.y + 1.26f, myTrans.position.z);
                        zz.isAirBorne = false;
                        velocity.y = 0;
                        controller.collisions.below = true;
                        zz.noLad = Time.time + 0.5f;
                        onLadder = false;
                    }
                    else if (getDist >= 0.55f) //Enter "hunched over" animation if close enough to the top.
                    {
                        zz.ladFrame = 19;
                        zz._ladCount = Time.time;
                    }
                    /*

                    */
                }
                //********************************************************************************************************************************************

                mySprite.sprite = mySprites[zz.ladFrame];
            }
            return;
        }

        mySprite.flipX = (spriteFacing < 0) ? true : false; //Make the player's sprite mirrioring based on the faceDir.
        if (stunned)
        {

            //Check "IEnumerator Stunned_Sequence" for actual stun animation.
            zz.isAirBorne = false;
            zz.isShooting = false;
            zz.isWalking = false;
            zz.isStanding = false;
            return;
        }

        if (zz.isAirBorne) //All the animations for if the player is airborne (not standing on a platform/solid object
        {
            zz.isStanding = false;
            zz.isWalking = false;
            if (controller.collisions.below)
            {
                if (input.x != 0)
                {
                    zz.isWalking = true;
                    NewAnimation(animIntW[currentSet], 5, 6, 1, 1);
                }
                else
                    NewAnimation(animIntS[currentSet], 1, 8);
                playSFX(bb.TestSounds[2],true);
                zz.isAirBorne = false;
            }

        }
        else //All the animations if the player is on the ground/standing.
        {
            if (!controller.collisions.below)
            {
                NewAnimation(animIntJ[currentSet], 1, 8);
                zz.isAirBorne = true;
                zz.isWalking = false;
                return; //Cut off any remaining code below this line.
            }

            if (isSliding)
            {
                NewAnimation(6, 1, 8);
                zz.isStanding = false;
                zz.isWalking = false;
                return;
            }

            if (!zz.isWalking && input.x != 0)
            {
                NewAnimation(animIntW[currentSet], 5, 6, 1);
                zz.isStanding = false;
                zz.isWalking = true;
            }
            if (!zz.isStanding && input.x == 0)
            {
                NewAnimation(animIntS[currentSet], 1, 8);
                zz.isStanding = true;
                zz.isWalking = false;
            }
        }
    }

    void playSFX(AudioClip myClip, bool generalChannel = false)
    {
        if (generalChannel)
        {
            bb.PlayerGeneral.clip = myClip;
            bb.PlayerGeneral.Play();
        }
        else
        {
            bb.PlayerAttacks.clip = myClip;
            bb.PlayerAttacks.Play();
        }
    }

    void changeColliders (int whichOne)
    {
        switch (whichOne)
        {
            case 1:
                controller.collider.offset = zz.duckC_Offset;
                controller.collider.size = zz.duckC_Size;
                break;

            default:
                controller.collider.offset = zz.standC_Offset;
                controller.collider.size = zz.standC_Size;
                break;
        }
        controller.CalculateRaySpacing();
    }

    public void FrozenTimers()
    {
        /*
        endShootPause
        public float _slideTime;
        public float _autoSlideTime;
        */
        if (endShootPause > Time.time)
            endShootPause += Time.deltaTime;
        if (zz._slideTime > Time.time)
            zz._slideTime += Time.deltaTime;
        if (zz._autoSlideTime > Time.time)
            zz._autoSlideTime += Time.deltaTime;
        if (zz._ladCount > Time.time)
            zz._ladCount += Time.deltaTime;
    }

    protected override bool freezeCheck()
    {
        if (aa.powerUpFreeze || Frozen)
            return true;
        return false;
    }

    public struct PlayerStuff
    {
        public bool isStanding;
        public bool isWalking;
        public bool isAirBorne;

        public bool isShooting;

        public float ladCount;
        public float _ladCount;
        public int ladFrame;
        public float noLad;

        public float slideTime;
        public float _slideTime;
        public float _autoSlideTime;

        public Vector2 standC_Offset;
        public Vector2 standC_Size;

        public Vector2 duckC_Offset;
        public Vector2 duckC_Size;

        public bool didJump;
        public bool noJumping;

        public void bootVars()
        {
            ladCount = 0.25f;
            _ladCount = 0;
            ladFrame = 17;
            noLad = 0;

            slideTime = 0.4f;

            standC_Offset = new Vector2(0,-0.015f);
            standC_Size = new Vector2(0.16f, 0.23f);

            duckC_Offset = new Vector2(0, -0.055f);
            duckC_Size = new Vector2(0.16f, 0.15f);
        }

        public void AnimReset()
        {
            isStanding = isAirBorne = false;
        }
    }
}
