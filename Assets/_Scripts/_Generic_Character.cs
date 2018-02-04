using UnityEngine;
using System.Collections;
using System.Linq;

//A base script that contains common commands/variables used by players, enemies, etc.

public class _Generic_Character : MonoBehaviour
{
    [HideInInspector] public __CachedVault aa;
    [HideInInspector] public _AudioMaster bb;

    [HideInInspector] public Transform myTrans; //Store this objects Transform component into a cached variable to make accessing it more streamlined.

    public bool debugGetSprites; //Set this to true while an object is selected to get all its sprites from a sprite texture.

    public SpriteRenderer mySprite;

    public float animSpeed;

    public int animFrameCount;
    public int baseFrameCount;
    public int trueFrameCount;
    public bool newFrame; //Set to true each time a sprite animation frame is changed. Used to detect frame specific actions.

    public int startFrame;
    public int Count;

    public Sprite[] mySprites;
    public Texture BaseTexure;

    public Coroutine AnimCoroutine;

    public bool Frozen; //When set this character is "frozen"

    // Use this for initialization
    public virtual void Awake()
    {
        aa = GameObject.FindWithTag("GameController").GetComponent<__CachedVault>();
        bb = GameObject.FindWithTag("Audio").GetComponent<_AudioMaster>();

        myTrans = GetComponent<Transform>();
        //^^Goro = Resources.LoadAll<Sprite>(Bobo.name);//(texture.name);

        //NewAnimation();
    }

    IEnumerator Esturk_Animation(int _startFrame, int frameCount, float speed, int restartFrame = 0, int oStartFrame = 0, int rev = 1)
    {
        startFrame = _startFrame; //startFrame needs to be stored as a public int as some characters' scripts modify it on occasion (such as the player's walking shoot animation transition).

        animFrameCount = startFrame + oStartFrame;
        baseFrameCount = 0;
        trueFrameCount = 0;

        while (true)
        {
            if (freezeCheck())
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

    // Update is called once per frame
    public void NewAnimation(int startFrame, int frameCount, float speed, int restartFrame = 0, int oStartFrame = 0)
    {
        if (AnimCoroutine != null)
            StopCoroutine(AnimCoroutine);
        AnimCoroutine = StartCoroutine(Esturk_Animation(startFrame, frameCount, speed, restartFrame, oStartFrame));
    }

    void OnDrawGizmosSelected()
    {
        if (debugGetSprites)
        {
            mySprites = Resources.LoadAll<Sprite>(BaseTexure.name);//(texture.name);
            debugGetSprites = false;
        }
    }

    protected virtual bool freezeCheck()
    {
        if (aa.powerUpFreeze || Frozen)
            return true;
        return false;
    }
}
