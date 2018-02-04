using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _MM_PaletteSwapper : MonoBehaviour
{
    [HideInInspector] public __CachedVault aa;

    public SpriteRenderer mySprite;
    public Texture2D myTex;
    public Texture2D trueTex;

    public List <int> C_C;

    public bool debugPalette;

    // Use this for initialization
    void Start ()
    {
        aa = GameObject.FindWithTag("GameController").GetComponent<__CachedVault>();
        
        //Screen.SetResolution(640, 480, true);
        //trueTex = Instantiate(myTex) as Texture2D;
        trueTex = new Texture2D(8, 1);
        trueTex.SetPixel(3, 0, aa.C_[C_C[3]]);
        trueTex.SetPixel(4, 0, aa.C_[C_C[4]]);
        trueTex.SetPixel(5, 0, aa.C_[C_C[5]]);
        trueTex.SetPixel(6, 0, aa.C_[C_C[6]]);
        trueTex.SetPixel(7, 0, aa.C_[C_C[7]]);
        trueTex.Apply();

        setPalette(C_C[0], C_C[1], C_C[2]);

        //^^Goro = Resources.LoadAll<Sprite>(Bobo.name);//(texture.name);

        //**StartCoroutine(AnimationTest(12,4,animSpeed));

        //string spriteSheet = AssetDatabase.GetAssetPath(Bobo);
        //Goro = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
    }

    // Update is called once per frame
    void Update ()
    {
        /*
        if (debugPalette)
        {
            setPalette();
            debugPalette = false;
        }
        */
    }


    public void setPalette (int c_1, int c_2, int c_3)
    {
        trueTex.SetPixel(0, 0, aa.C_[c_1]);
        trueTex.SetPixel(1, 0, aa.C_[c_2]);
        trueTex.SetPixel(2, 0, aa.C_[c_3]);
        trueTex.Apply();
        //C_statics[0] = trueTex.GetPixel(2, 0);
        mySprite.sharedMaterial.SetTexture("_PaletteTex", trueTex); //Uses "sharedMaterial" to change all objects that share the player palette, such as the ammo meter and ammo powerups.
    }

    void OnApplicationQuit()
    {
        //Resets Mega Man's texture back to a default after game is shut down. The sharedMaterial script ends up modding it to a temporary texture.
        mySprite.sharedMaterial.SetTexture("_PaletteTex", aa.mm_tex);
    }
}
