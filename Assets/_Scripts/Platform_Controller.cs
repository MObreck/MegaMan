using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Platform_Controller : __RayCastController
{
    Transform myTrans; //Store this objects Transform component into a cached variable to make accessing it more streamlined.

    public LayerMask passengerMask;
    //public Vector3 move;

    public float speed;
    public bool cyclic; //If true the platform will directly return to the first waypoint after reaching the last instead of going in reverse order.
    public float waitTime; //An optional waittime for each time the platform reaches a waypoint.
    [Range(0,2)] public float easeAmount; //Allows a slowing down effort as the platform reaches each waypoint. A setting of 0 = no slowdown.

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime; //Used for the waitTime effect.

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    List<PassengerMovement> passengerMovement; //Store all the characters on platform movement data into a list array.
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>(); //Reduce the number of GetCompnent calls with a dictionary array.

    // Use this for initialization
    public override void Start ()
    {
        myTrans = GetComponent<Transform>(); //Put the objects transform component into its cached variable.
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + myTrans.position;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = CaculatePlatformMovement(); //move * Time.fixedDeltaTime;
        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    float Ease(float x) //Allowing moving platforms to come to a smooth, slowing stop at each waypoint if desired.
    {
        float a = easeAmount + 1; //Modify the easeAmount so 0 = no slowing instead of 1.
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1-x, a));
    }

    Vector3 CaculatePlatformMovement()
    {
        if (Time.time < nextMoveTime) //If there is still waiting time don't move at all.
        {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length; //Reset the waypoint counter if reached the end.
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;

        //get the distance to the next waypoint in a variable for consistant movement speed.
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);

        percentBetweenWaypoints += Time.fixedDeltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01 (percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints); //Add in the slowing effect.

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        //if we've reached are current target waypoint's position...
        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic) //Use waypoint reversal style waypoint restart if cyclic isn't set to true.
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1) //If we've reached the last waypoint start over in reverse order.
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime; //Add any waiting time before making the next move.
        }

        return newPos - transform.position;
    }

    void MovePassengers(bool beforeMovePlatform)
    {
        if (!collider.enabled) //If the collider component is disabled, don't do character collision checks.
            return;

        foreach (PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }

            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity)
    {
        if (!collider.enabled) //If the collider component is disabled, don't do character collision checks.
            return;

        HashSet<Transform> movedPassengers = new HashSet<Transform>(); //Store all the characters on this platform for this frame.
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth; //Set up a length for the casted rays based on the velocity y speed + skinWidth as a modifer.

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.yellow);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform)) //If the hashset doesn't currently contain the character being detected by the current raycast.
                    {
                        movedPassengers.Add(hit.transform); //Add this character to the hashset list.

                        float pushX = (directionY == 1) ? velocity.x : 0; //If the platform is moving up push on the X axis a bit.
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY; //Close any Y axis distance between the character and the moving platform

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
//                        hit.transform.Translate(new Vector3(pushX, pushY)); //Move the character on the platform.
                    }
                }
            }
        }

        //***********************************************************************************
        //Horizontally Moving platform
        if (velocity.x != 0 && myTrans.tag != "Through") //Only do this check is moving horizontally and considered solid on the sides.
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth; //Set up a length for the casted rays based on the velocity y speed + skinWidth as a modifer.

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.blue);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform)) //If the hashset doesn't currently contain the character being detected by the current raycast.
                    {
                        movedPassengers.Add(hit.transform); //Add this character to the hashset list.

                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX; //Close any X axis distance between the character and the moving platform
                        float pushY = -skinWidth; //Set just a small enough downward force to ensure the character continue's doing floor cllision checks.

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                        //hit.transform.Translate(new Vector3(pushX, pushY)); //Move the character on the platform.
                    }
                }
            }
        }

        //***********************************************************************************
        //If a character is on top of a horizontally or downward Moving platform
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2; //Set up a length for the casted rays based on the velocity y speed + skinWidth as a modifer.

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.cyan);

                if (hit && hit.distance != 0)
                {
//                    if (hit.transform.position.y < myTrans.position.y - 0.24f) //Don't process this collision if the hit character is more than 4 pixels below the moving platform.
//                        return;
                    if (!movedPassengers.Contains(hit.transform)) //If the hashset doesn't currently contain the character being detected by the current raycast.
                    {
                        movedPassengers.Add(hit.transform); //Add this character to the hashset list.

                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                        //hit.transform.Translate(new Vector3(pushX, pushY)); //Move the character on the platform.
                    }
                }
            }
        }
    }

    struct PassengerMovement //Store a bunch of data for characters on the moving platform into a struct
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement (Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = 0.3f;

            if (myTrans == null)
                myTrans = GetComponent<Transform>(); //Put the objects transform component into its cached variable.

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + myTrans.position;
                //Draw in crosshairs to show the waypoint positions.
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size); //vertical line of the cross.
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size); //horizontal line of the cross.
            }
        }
    }
}
