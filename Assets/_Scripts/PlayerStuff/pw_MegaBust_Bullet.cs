using UnityEngine;
using System.Collections;

public class pw_MegaBust_Bullet : _pw_wepbase
{
    public Vector2 mySpeed;
    Vector2 velocity;


    // Use this for initialization
    public override void Start()
    {
        base.Start();
        gameObject.SetActive(false); //Disable this by default until its ready for use.
    }

    void FixedUpdate ()
    {
        if (!freezeCheck())
            myTrans.Translate(velocity * Time.deltaTime);
        //myTrans.RotateAround(Vector3.zero, Vector3.forward, 20 * Time.deltaTime);
        //myTrans.Rotate(Vector3.forward * Time.deltaTime * 50, Space.World);
    }

    public override void OnEnable()
    {
        trueSpeed = mySpeed;
        base.OnEnable();
    }

    public override void DisableObject()
    {
        velocity = Vector2.zero;
        trueSpeed = mySpeed;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CameraBounds();

        velocity = trueSpeed;
        velocity.x *= myFacing;

        detectEnemy(); //Check hitbox for enemies, check the parent script for actual scripting.
    }
}
