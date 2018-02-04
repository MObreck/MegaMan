using UnityEngine;
using System.Collections;

public class pw_MetalBlade : _pw_wepbase
{
    public float mySpeed;
    Vector2 velocity;

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        gameObject.SetActive(false); //Disable this by default until its ready for use.
    }

    public override void OnEnable()
    {
        //trueSpeed = mySpeed;

        transform.tag = "Untagged"; //Reset any tags set.


        if (firstBoot)
        {
            myFacing = aa.myP.spriteFacing;
            mySprite.flipX = (myFacing == -1) ? true : false;
            trueSpeed = Vector2.zero;

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (input == Vector2.zero)
            {
                trueSpeed.x = mySpeed * myFacing;
            }
            else
            {
                if (input.x != 0)
                    trueSpeed.x = mySpeed * myFacing;
                if (input.y != 0)
                    trueSpeed.y = mySpeed * input.y;
            }

            myTrans.position = aa.myP.myTrans.position;
        }
    }

    void FixedUpdate()
    {
        if (!freezeCheck())
            myTrans.Translate(velocity * Time.deltaTime);
        //myTrans.RotateAround(Vector3.zero, Vector3.forward, 20 * Time.deltaTime);
        //myTrans.Rotate(Vector3.forward * Time.deltaTime * 50, Space.World);
    }

    public override void DisableObject()
    {
        velocity = Vector2.zero;
        trueSpeed = Vector2.zero;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CameraBounds();

        velocity = trueSpeed;
        //velocity.x *= myFacing;

        detectEnemy(); //Check hitbox for enemies, check the parent script for actual scripting.
    }

    public override void deflectEffect()
    {
        playSFX(aa.sfx_DeflectSound);
        transform.tag = "Dead"; //Set this bullet so it no longer can collide with enemies now that it is dead.
        trueSpeed.y = mySpeed; //Mathf.Abs(velocity.x);
        trueSpeed.x = (mySpeed / 4) * -myFacing;
        mySprite.flipX = !mySprite.flipX;
    }

}
