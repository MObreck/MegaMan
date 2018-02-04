using UnityEngine;
using System.Collections;

public class Enemy_Basics : _Generic_Character
{
    public int pallette = 1;  //FROM SetSpritePallitNumber

    //Player_Basics myP;

    public int MaxHP = 20;
    public int HP;
    public int damage = 12;

    float flicker_time;
    protected bool firstBoot; //Stops some errors caused by the OnEnable event insisted on being processed when the object is first created.

    protected Transform sprTrans;
    protected Vector3 startPos;

    public bool respawns = true; //This enemy respawns if this is set.
    public bool reseted; //This is set when an a dead enemy is ready to be respawned.
    protected GameObject sprObj;
    protected string baseTag;

    //NOTE: Setting an enemy's sprite object tag to "invincible" will make them immune to all player weapons.

    // Use this for initialization
    public virtual void Start ()
    {
        //SetColor();//FROM SetSpritePallitNumber
        //myP = aa.myP;

        firstBoot = true;

        sprTrans = mySprite.transform;
        sprObj = sprTrans.gameObject;

        startPos = myTrans.position;

        HP = MaxHP;

        baseTag = transform.tag;
    }

    //public virtual void OnEnable()
    public virtual void resetEnemy()
    {
        sprObj.SetActive(true);
        HP = MaxHP;
        mySprite.enabled = true;
        flicker_time = 0;
        Frozen = false;
        if (firstBoot)
        {
            sprTrans.tag = baseTag;
        }
    }
    /*
    //FROM SetSpritePallitNumber
    [ContextMenu("Set Pallette Number")]
    void SetColor()
    {
        //        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        mySprite.color = new Color32((byte)pallette, 0, 0, 0);
    }
    //FROM SetSpritePallitNumber
    */
    public void damageEnemy (int myDamage)
    {
        HP -= myDamage;
        if (HP <= 0)
        {
            sprTrans.tag = "Dead";
            for (int i = 0; i < aa.DeathSparks.Count; i++)
            {
                if (!aa.DeathSparks[i].activeSelf)
                {
                    aa.DeathSparks[i].transform.position = sprTrans.position;
                    aa.DeathSparks[i].SetActive(true);
                }
            }
            //gameObject.SetActive(false);
            sprObj.SetActive(false);
        }
        mySprite.enabled = false;
        flicker_time = Time.time + aa.flicker_time;
    }

    // Update is called once per frame
    void Update ()
    {
	    if (!mySprite.enabled && Time.time > flicker_time)
            mySprite.enabled = true;

        cameraZone();

        E_Update(); //An easy place to put all AI unique update scripts without always having to include "base.Update" in each one.
    }

    public virtual void E_Update() //An easy place to put all AI unique update scripts without always having to include "base.Update" in each one.
    {
    }

    public void cameraZone()
    {
        //-----------------------------------------------------------------------------------
        // Camera bournaries for this projectile.
        Vector3 viewPos = Camera.main.WorldToViewportPoint(myTrans.position);

        if (viewPos.y > 1.2f || viewPos.y < -0.2f || viewPos.x > 1.2f || viewPos.x < -0.2f)
        {
            if (!sprObj.activeSelf)
                reseted = true;
            //DisableObject();
        }

        if (reseted && respawns)
        {
            if (viewPos.y < 1 && viewPos.y > 0f && viewPos.x < 1f && viewPos.x > 0f)
            {
                resetEnemy();
                reseted = false;
            }
        }

        Vector3 sprViewPos = Camera.main.WorldToViewportPoint(sprTrans.position);

        if (sprViewPos.y > 1.2f || sprViewPos.y < -0.2f || sprViewPos.x > 1.2f || sprViewPos.x < -0.2f)
        {
            if (sprObj.activeSelf)
                sprObj.SetActive(false);
        }
        //-----------------------------------------------------------------------------------
    }
}
