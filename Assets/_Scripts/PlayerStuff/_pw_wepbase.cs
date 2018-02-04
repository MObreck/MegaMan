using UnityEngine;
using System.Collections;
//using System.Collections.Generic;

public class _pw_wepbase : MonoBehaviour
{
    protected Transform myTrans; //Store this objects Transform component into a cached variable to make accessing it more streamlined.

    protected __CachedVault aa;
    protected _AudioMaster bb;

    public SpriteRenderer mySprite;
    public int baseDamage = 4;
    public bool pierceDeadEnemies; //Makes a projectile not be destroyed if it lands a fatal hit on an enemy.

    protected Vector2 trueSpeed;

    public int myFacing = 1;
    public Vector2 boxSize;
    protected Vector2 myDir; //Creates a direction for the raycast hitbox to move in based on the projectiles velocity (mySpeed after being modified).

    public Enemy_Basics hitEnemy;

    protected bool firstBoot; //Stops some errors caused by the OnEnable event insisted on being processed when the object is first created.

    public float Test;
//    public List <Transform> Bobo;

    // Use this for initialization
    public virtual void Start ()
    {
        aa = GameObject.FindWithTag("GameController").GetComponent<__CachedVault>();
        bb = GameObject.FindWithTag("Audio").GetComponent<_AudioMaster>();

        myTrans = GetComponent<Transform>();
        mySprite = GetComponent<SpriteRenderer>();

        boxSize = GetComponent<BoxCollider2D>().size * 6;

        firstBoot = true; //Now OnEnable is ok to run its scripts.
    }

    public virtual void OnEnable()
    {
        transform.tag = "Untagged"; //Reset any tags set.

        if (firstBoot)
        {
            myFacing = aa.myP.spriteFacing;
            mySprite.flipX = (myFacing == -1) ? true : false;

            myTrans.position = aa.myP.myTrans.position + (aa.myP.wepOffset);
        }
    }

    public virtual void CameraBounds ()
    {
        //-----------------------------------------------------------------------------------
        // Camera bournaries for this projectile.
        Vector3 viewPos = Camera.main.WorldToViewportPoint(myTrans.position);

        if (viewPos.y > 1.01f || viewPos.y < -0.01f || viewPos.x > 1.01f || viewPos.x < -0.01f)
        {
            DisableObject();
        }
        //-----------------------------------------------------------------------------------
    }

    public virtual void DisableObject()
    {
        //myTrans.position = aa.noWhere; //Make sure this projectile is totally place off screen.
        gameObject.SetActive(false);
    }

    public virtual void detectEnemy() //Runs the hitbox checks for enemy contact, so the weapon shot can damage or be deflected the enemy.
    {
        float hitLength = aa.Micro; //Set up a length for the casted box colliders.

        //transform.Translate(velocity);

        if (transform.tag == "Dead") //If this weapon is labeled as dead it cannot make contact with enemies anymore.
            return;

        myDir = Vector2.zero; //"normalize" velocity to get a Vector2 type movement direction for ray cast BoxCast.
        Vector2 boxOff = GetComponent<BoxCollider2D>().offset * 6;
        Vector3 boxPos = myTrans.position + new Vector3(boxOff.x, boxOff.y, 0f);

        //RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance = Mathf.Infinity, int layerMask = DefaultRaycastLayers);
        RaycastHit2D hit = Physics2D.BoxCast(boxPos, boxSize, 0f, myDir, hitLength, aa.E_Layer);
        //RaycastHit2D[] hit = Physics2D.BoxCastAll(boxPos, boxSize, 0f, myDir, hitLength, aa.E_Layer);

        //Bobo.Clear();
        //        for (int i = 0; i < hit.Length; i++)
        //            if (hit[i] && hit[i].transform.tag != "Dead")
        if (hit && hit.transform.tag != "Dead")
        {
            /*
            if (!Bobo.Contains(hit[i].transform))
            Bobo.Add(hit[i].transform);
            */
            
            strikeEnemy(hit);
        }
    }

    public virtual void strikeEnemy (RaycastHit2D hit)
    {
        if (hit.transform.tag == "Invincible") //(hitEnemy.invincible)
        {
            deflectEffect();
        }
        else //If the enemy isn't invincible to this weapon then let's damage it.
        {
            hitEnemy = hit.transform.parent.GetComponent<Enemy_Basics>();
            playSFX(aa.sfx_HitSound);
            hitEnemy.damageEnemy(baseDamage);
            if (!pierceDeadEnemies || hitEnemy.HP > 0)
                DisableObject();
        }
    }

    public virtual void deflectEffect()
    {
        playSFX(aa.sfx_DeflectSound);
        transform.tag = "Dead"; //Set this bullet so it no longer can collide with enemies now that it is dead.
        trueSpeed.y = trueSpeed.x; //Mathf.Abs(velocity.x);
        trueSpeed.x *= -1;
        mySprite.flipX = !mySprite.flipX;
    }

    public virtual void playSFX(AudioClip mySnd) //bool generalChannel = false)
    {
        /*
        if (generalChannel)
        {
            bb.PlayerGeneral.clip = bb.TestSounds[2];
            bb.PlayerGeneral.Play();
        }
        else
        */
        {
            bb.PlayerGeneral.clip = mySnd;
            bb.PlayerGeneral.Play();
            //            bb.PlayerAttacks.clip = aa.sfx_HitSound;
            //            bb.PlayerAttacks.Play();
        }
    }

    protected bool freezeCheck()
    {
        if (aa.powerUpFreeze)
            return true;
        return false;
    }
}
