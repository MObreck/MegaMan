using UnityEngine;
using System.Collections;

public class _Animated_Simple : MonoBehaviour
{
    protected __CachedVault aa;

    public SpriteRenderer mySprite;
    public Sprite[] mySprites;

    public float animSpeed = 5f;
    public bool disableOnEnd;

    public int poop;

    // Use this for initialization
    void Awake ()
    {
        aa = GameObject.FindWithTag("GameController").GetComponent<__CachedVault>();
        mySprite = GetComponent<SpriteRenderer>();
	}

    void OnEnable()
    {
        mySprite.sprite = mySprites[0];
        StartCoroutine(Esturk_Animation_Simple());
    }

    // Update is called once per frame
    IEnumerator Esturk_Animation_Simple ()
    {
        int counter = 0;
        poop = 1;
        //speed = animSpeed * Time.fixedDeltaTime

        while (counter < mySprites.Length || !disableOnEnd)
        {
            if (freezeCheck())
                yield return new WaitForSeconds(Time.deltaTime);
            else
            {
                yield return new WaitForSeconds(animSpeed * Time.fixedDeltaTime);
                counter += 1;
                poop = counter;
                if (counter >= mySprites.Length)
                {
                    counter = 0;
                    if (disableOnEnd)
                        gameObject.SetActive(false);
                }
                mySprite.sprite = mySprites[counter];
            }
        }
        gameObject.SetActive(false);
    }

    protected bool freezeCheck()
    {
        if (aa.powerUpFreeze)
            return true;
        return false;
    }
}
