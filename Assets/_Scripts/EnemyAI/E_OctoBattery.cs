using UnityEngine;
using System.Collections;

public class E_OctoBattery : Enemy_Basics
{
    octoStruct S;

    public float moveSpeed;
    public bool horizontal;

    public float waitTime = 5f;
    public float eyeTime = 0.2f;

    Vector2 velocity;

    Vector2 boxSize;
    Vector2 boxOff;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();
        S._waitTime = Time.time + waitTime;
        boxSize = GetComponent<BoxCollider2D>().size * 5.5f;
        boxOff = GetComponent<BoxCollider2D>().offset * 6f;
    }

    //public override void OnEnable()
    public override void resetEnemy()
    {
//        base.OnEnable();
        base.resetEnemy();
        S._eyeTime = 0;
        S.eyeCount = 0;
        S.isMoving = false;
        S._waitTime = Time.time + waitTime;
        moveSpeed = Mathf.Abs(moveSpeed);
        mySprite.sprite = mySprites[0];
        if (firstBoot)
            sprTrans.position = startPos;
    }

    public void FixedUpdate()
    {
        if (S.isMoving && !freezeCheck())
        {
            if (horizontal)
                velocity.x = moveSpeed * Time.deltaTime;
            else
                velocity.y = moveSpeed * Time.deltaTime;

            sprTrans.Translate(velocity); //Move the enemy.
        }
    }

    // Update is called once per frame
    public override void E_Update() //An easy place to put all AI unique update scripts without always having to include "base.Update" in each one.
    {
        if (freezeCheck())
        {
            if (S._waitTime > Time.time)
                S._waitTime += Time.deltaTime;
            if (S._eyeTime > Time.time)
                S._eyeTime += Time.deltaTime;
            return;
        }
        if (Time.time > S._waitTime)
        {
            S._eyeTime = Time.time;
            S.isMoving = true;
            S._waitTime = Time.time + 9999; //Just to effectively disable this time check for now.
        }
        if (S.isMoving)
        {
            //-------------------------------------------------------
            // Open the enemies eye if its moving.
            if (S.eyeCount < 2)
            {
                if (Time.time > S._eyeTime)
                {
                    S._eyeTime = Time.time + eyeTime;
                    S.eyeCount += 1;
                    mySprite.sprite = mySprites[S.eyeCount];
                }
            }
            //-------------------------------------------------------
/*
            if (horizontal)
                velocity.x = moveSpeed * Time.fixedDeltaTime;
            else
                velocity.y = moveSpeed * Time.fixedDeltaTime;

            transform.Translate(velocity); //Move the enemy.
*/

            Vector3 boxPos = sprTrans.position + new Vector3(boxOff.x, boxOff.y, 0f);

            //RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance = Mathf.Infinity, int layerMask = DefaultRaycastLayers);
            RaycastHit2D hit = Physics2D.BoxCast(boxPos, boxSize, 0f, Vector2.zero, aa.Micro, aa.collisionMask);

            if (hit)
            {
                velocity = Vector2.zero;
                moveSpeed *= -1;
                BoxCollider2D hitBox = hit.transform.GetComponent<BoxCollider2D>();
                if (horizontal)
                {
                    float touchPointX = hitBox.transform.position.x + ((hitBox.size.x * 6) * Mathf.Sign(moveSpeed));
                    sprTrans.position = new Vector3(touchPointX, sprTrans.position.y, 0);
                }
                else
                {
                    float touchPointY = hitBox.transform.position.y + ((hitBox.size.y * 6) * Mathf.Sign(moveSpeed));
                    sprTrans.position = new Vector3 (sprTrans.position.x, touchPointY, 0);
                }
                //sprTrans.position = new Vector3(sprTrans.position.x, sprTrans.position.y + hit.distance, 0);
                S._waitTime = Time.time + waitTime;
                S._eyeTime = Time.time;
                S.isMoving = false;
            }

        }
        //-------------------------------------------------------
        // Close the enemies eye if it is not moving.
        else if (S.eyeCount > 0)
        {
            if (Time.time > S._eyeTime)
            {
                S._eyeTime = Time.time + eyeTime;
                S.eyeCount -= 1;
                mySprite.sprite = mySprites[S.eyeCount];
            }
        }
        //-------------------------------------------------------
    }

    public struct octoStruct
    {
        public float _waitTime;
        public float _eyeTime;
        public int eyeCount;
        public bool isMoving;
    }
}
