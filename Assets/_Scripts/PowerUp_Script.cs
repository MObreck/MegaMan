using UnityEngine;
using System.Collections;

public class PowerUp_Script : MonoBehaviour
{
    [HideInInspector] public __CachedVault aa;
    [HideInInspector] public _AudioMaster bb;
    [HideInInspector] public Transform myTrans; //Store this objects Transform component into a cached variable to make accessing it more streamlined.

    Controller2D controller;
    Vector2 velocity;

    public int type; //The type of item this is.
    // 0 = ammo
    // 1 = health
    // 2 = 1up

    public Vector3 boxOffset;
    public Vector3 boxSize;

    public Vector3 myPos;

    public bool poop;
    float skinWidth = 0.015f;

    public int value = 80;

    bool taken;

    SpriteRenderer mySprite;
    
    // Use this for initialization
    void Start ()
    {
        aa = GameObject.FindWithTag("GameController").GetComponent<__CachedVault>();
        bb = GameObject.FindWithTag("Audio").GetComponent<_AudioMaster>();
        myTrans = GetComponent<Transform>();
        mySprite = GetComponent<SpriteRenderer>();

        controller = GetComponent<Controller2D>();

        boxOffset = controller.collider.offset;
        boxSize = controller.collider.size;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        velocity.y += aa.myP.gravity * Time.deltaTime;

        //Stops the gravity from endlessly building up if the player is on the ground of if they bump a ceiling.
        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0;

        controller.Move(velocity * Time.deltaTime, Vector2.zero);
    }

    void Update()
    {
        if (taken)
            return;
        boxOffset = controller.collider.offset;
        boxSize = controller.collider.size * 6;

        float hitLength = aa.Micro;

        Vector2 myDir = Vector2.up; //velocity.normalized; //"normalize" velocity to get a Vector2 type movement direction for ray cast BoxCast.

        myPos = myTrans.position + boxOffset;

        RaycastHit2D hit = Physics2D.BoxCast(myPos, boxSize, 0f, myDir, hitLength, aa.P_Layer);

        if (hit)
        {
            //aa.myP.HP
            //aa.myP.maxHP
            //myInv
            // value
            if (aa.quickFill)
            {
                gameObject.SetActive(false);
                switch (type)
                {
                    case 1:
                        if (aa.myP.HP < aa.myP.maxHP)
                        {
                            aa.myP.HP += value;
                            aa.myP.myInv.refresh_HealthBar();
                            playSFX(aa.sfx_getItem[0]);
                        }
                        break;
                    default:
                        if (aa.myP.myInv.wepAmmo[aa.myP.myInv.cw] < aa.myP.myInv.maxAmmo)
                        {
                            aa.myP.myInv.wepAmmo[aa.myP.myInv.cw] += value;
                            aa.myP.myInv.refresh_AmmoBar();
                            playSFX(aa.sfx_getItem[0]);
                        }
                        break;
                }
            }
            else
            {
                aa.powerUpFreeze = true;
                mySprite.enabled = false;
                taken = true;
                switch (type)
                {
                    case 1:
                        StartCoroutine(HealthFill(value));
                        break;
                    default:
                        StartCoroutine(AmmoFill(value));
                        break;
                }
            }
        }

        //P_Layer
        //collider
    }

    void playSFX(AudioClip myClip, bool generalChannel = false)
    {
        bb.PlayerGeneral.clip = myClip;
        bb.PlayerGeneral.Play();
    }

    IEnumerator HealthFill(int _value)
    {
        int myValue = _value;
        //aa.fill_time
        while (myValue > 0)
        {
            if (aa.myP.HP >= aa.myP.maxHP)
            {
                myValue = 0;
                aa.myP.HP = aa.myP.maxHP;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            else
            {
                playSFX(aa.sfx_getItem[0]);
                aa.myP.HP += 4;
                aa.myP.myInv.refresh_HealthBar();
                myValue -= 4;
                yield return new WaitForSeconds(aa.fill_time);
            }
        }
        mySprite.enabled = true;
        aa.powerUpFreeze = false;
        taken = false;
        gameObject.SetActive(false);
    }

    IEnumerator AmmoFill(int _value)
    {
        int myValue = _value;
        //aa.fill_time
        while (myValue > 0)
        {
            if (aa.myP.myInv.wepAmmo[aa.myP.myInv.cw] >= aa.myP.myInv.maxAmmo)
            {
                myValue = 0;
                aa.myP.myInv.wepAmmo[aa.myP.myInv.cw] = aa.myP.myInv.maxAmmo;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            else
            {
                playSFX(aa.sfx_getItem[0]);
                aa.myP.myInv.wepAmmo[aa.myP.myInv.cw] += 8;
                aa.myP.myInv.refresh_AmmoBar();
                myValue -= 8;
                yield return new WaitForSeconds(aa.fill_time);
            }
        }
        mySprite.enabled = true;
        aa.powerUpFreeze = false;
        taken = false;
        gameObject.SetActive(false);
    }
    /*
        IEnumerator Esturk_Animation(int _startFrame, int frameCount, float speed, int restartFrame = 0, int oStartFrame = 0, int rev = 1)
    {
        startFrame = _startFrame; //startFrame needs to be stored as a public int as some characters' scripts modify it on occasion (such as the player's walking shoot animation transition).

        animFrameCount = startFrame + oStartFrame;
        baseFrameCount = 0;
        trueFrameCount = 0;

        while (true)
        {
            if (Frozen)
                yield return new WaitForSeconds(Time.deltaTime);
            else
            {
                mySprite.sprite = mySprites[animFrameCount];
                animFrameCount = (animFrameCount >= startFrame + frameCount - 1 || animFrameCount <= startFrame - frameCount - 1) ? startFrame + restartFrame : animFrameCount + 1;
                baseFrameCount = animFrameCount - startFrame;
                trueFrameCount += 1 * rev;
                newFrame = true;
                yield return new WaitForSeconds(speed * Time.fixedDeltaTime);
            }
        }
    }

    */
}
