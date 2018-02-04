using UnityEngine;
using System.Collections;

public class Controller2D : __RayCastController
{
    const float maxClimbAngle = 65f; //The maximum steepness angle a climbable slope can be.
    const float maxDescendAngle = 65f; //The maximum steepness angle a decendable slope can be (by walking down it :P )

    public CollisionInfo collisions;
    Vector2 playerInput; //Used for the ability to fall through "Through" tagged platforms if the player pressed Down+Jump.

    public bool noHorizontal; //Disables horizontal collision for certain simpler objects like powerups.

    // Use this for initialization
    
	public override void Start ()
    {
        base.Start();
        collisions.faceDir = 1; //Set a starting faceDir.
    }
    

    // Update is called once per frame
    /*
    void Update ()
    {

    }
    */

    public void Move(Vector3 velocity, bool standingOnPlatform) //Overload verision of this command for moving platforms.
    {
        Move(velocity, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisions.Reset(); //Make a blank slate for the collision bool flags each update check.
        playerInput = input; //Used for the ability to fall through "Through" tagged platforms if the player pressed Down+Jump.

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        // Coding for slope climbing.

        collisions.velocityOld = velocity; //Store the previous velocity for certain slope climbing/decending code corrections.

        if (velocity.y < 0)
        {
            DecendSlope(ref velocity);
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        if (velocity.x != 0)
            collisions.faceDir = (int)Mathf.Sign(velocity.x); //If trying to move left or right set the faceDir in that direction.

        if (!noHorizontal)
        {
            HorizontalCollision(ref velocity);
        }

        if (velocity.y != 0)
        {
            VerticalCollision(ref velocity);
        }

        transform.Translate(velocity);

        if (standingOnPlatform)
        {
            collisions.below = true;
        }
    }

    void HorizontalCollision(ref Vector3 velocity)
    {
        float directionX = collisions.faceDir; //Get the face Direction
        float rayLength = Mathf.Abs(velocity.x) + skinWidth; //Set up a length for the casted rays based on the velocity y speed + skinWidth as a modifer.

        if (Mathf.Abs(velocity.x) < skinWidth)
            rayLength = 2 * skinWidth; //If not trying to moving horizontally use this default rayLength.

        //footLevel = false;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, aa.collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.green);

            if (hit)
            {
                if (hit.transform.tag == "Through" || hit.transform.tag == "LadderTop") //Don't do wall collision for solids with this tag set, meaning its a platform the can be stood on, but not solid on the sides and from below.
                    return;
                if (hit.distance == 0)
                {
                    continue; //Fix for if a character is inside a wall or solid moving platform.
                }
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                // Slope detection friendly collision code.
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); //Get the angle of the solid for sloped surfaces.

                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    //A small fix for if the character goes directly from decending a slope to climbing a slope without touching level ground first.
                    if (collisions.decendingSlope)
                    {
                        collisions.decendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    //-------------------
                    //Fixes the character to climbing the slope at its exact level instead of slightly above sometimes.
                    float distanceToSlopeStart = 0; 
                    if (slopeAngle != collisions.slopeAngleOld) //Don't run this if already climbing a slope.
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    //-------------------
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX; //Resets Velocity X once the slope climbing fixing adjustments are finished.
                }

                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope) //fix for if the character bumps a wall while climbing a slope.
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    //If the lowest ray detects a wall set footlevel to true.
                    //Used for things like checking if Mega Man is sliding into a wall
                    //(in that case upper rays aren't needed and can even cause minor control issues for the check)
                    //=============================================================================================
                    if (i == 0)
                        collisions.footLevel = true;
                    //=============================================================================================

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
                // Simplified collision code that doesn't use slope detection.
                /*
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
                */
                //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            }
        }
    }

    void VerticalCollision (ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y); //Get the direction of the y velocity.
        float rayLength = Mathf.Abs(velocity.y) + skinWidth; //Set up a length for the casted rays based on the velocity y speed + skinWidth as a modifer.

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, aa.collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                if (hit.collider.tag == "Through" || hit.transform.tag == "LadderTop") //Don't do ceiling collision for solids with this tag set, meaning its a platform the can be stood on, but not solid on the sides and from below.
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (collisions.fallingThroughPlatform)
                        continue;
                    /*
                    if (playerInput.y == -1) // Input.GetButtonDown("Jump")
                    {
                        collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThrough", 0.4f);
                        continue;
                    }
                    */

                }
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                // Slope detection friendly collision code.
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope) //Fix for bumping head on ceiling while climbing a slope.
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;

                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
                // Simplified collision code that doesn't use slope detection.
                /*
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
                */
                //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            }
        }

        //Fixes a minor issue that happens if a character passes from one slope direction to a slope of a different angle.
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, aa.collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); //Get the angle of the solid for sloped surfaces.
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    }

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    //Calculates most of the settings when climbing up a slope.
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x); //Make an alway positive value verision of the X velocity.
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance; //Use some Trig to alter the Y velocity to climb the slope.

        if (velocity.y <= climbVelocityY) //Make sure the character isn't trying to jump or otherwise being pulled upwards before slope velocity adjustments are applied
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * velocity.x; //Use some Trig to adjust the X velocity.
            collisions.below = true; //Make sure the character is always flagged as on the ground when climbing a slope.
            collisions.climbingSlope = true; //Set the climbing a slope flag to true.
            collisions.slopeAngle = slopeAngle; //Set the slope angle into storage.
        }
        //        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x); //Use some Trig to adjust the X velocity.

    }

    //Calculates most of the settings when climbing down a slope.
    void DecendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, aa.collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); //Get the angle of the solid for sloped surfaces.
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x); //Make an alway positive value verision of the X velocity.
                        float decendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance; //Use some Trig to alter the Y velocity to climb the slope.
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * velocity.x; //Use some Trig to adjust the X velocity.
                        velocity.y -= decendVelocityY;

                        collisions.below = true; //Make sure the character is always flagged as on the ground when climbing a slope.
                        collisions.decendingSlope = true; //Set the decending a slope flag to true.
                        collisions.slopeAngle = slopeAngle; //Set the slope angle into storage.
                    }
                }
            }
        }
    }
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    void ResetFallingThrough()
    {
        collisions.fallingThroughPlatform = false;
    }

    public struct CollisionInfo //Store a bunch of Bool flags used for collision in a struct formation (more memory efficient).
    {
        public bool above, below, left, right;

        public bool climbingSlope, decendingSlope;
        public float slopeAngle, slopeAngleOld; //Make some angle storage floats for smoothing slope climbing.
        public Vector3 velocityOld;
        public int faceDir; //Stores the direction the character is currently facing.

        public bool fallingThroughPlatform;
        public bool footLevel; //Used when a wall is detected at the level of this character's feet (used for things like Mega Man's sliding into a wall).

        public void Reset() //Clear the wall/floor/slope collision flags for each collision check.
        {
            above = below = left = right = climbingSlope = decendingSlope = footLevel = false; //reset all the bool flags in one neat little command.

            slopeAngleOld = slopeAngle; // store the last slopeAngle value
            slopeAngle = 0; //Now that the last slopeAngle value is stored set slopeAngle to 0 for the next collision check.
        }
    }
}
