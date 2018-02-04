using UnityEngine;

public class SetSpritePallitNumber : MonoBehaviour
{
    //    /*
    //%%public int pallit = 1;
    public int pallette = 1;  //FROM SetSpritePallitNumber
    public SpriteRenderer mySprite;

    // Use this for initialization
    void Start ()
	{
	    SetColor();
	}

    //%%[ContextMenu("Set Pallit Number")]
    [ContextMenu("Set Pallette Number")]
    void SetColor()
    {
        //SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        //%%spriteRenderer.color = new Color32((byte)pallit, 0, 0, 0);
        mySprite.color = new Color32((byte)pallette, 0, 0, 0);
    }

//    */
    //********************************************************************************************************************************************
    //********************************************************************************************************************************************
    //********************************************************************************************************************************************
    //********************************************************************************************************************************************
    //********************************************************************************************************************************************
  /*
    public int pallette = 1;  //FROM SetSpritePallitNumber

    public SpriteRenderer mySprite;
    public Texture2D myTex;
    public Texture2D trueTex;

    public Color C_outline;
    public Color C_main;
    public Color C_secondary;

    public Color[] C_statics;

    public bool debugPalette;

    // Use this for initialization
    void Start()
    {
        SetColor();//FROM SetSpritePallitNumber
        //Screen.SetResolution(640, 480, true);
        //trueTex = Instantiate(myTex) as Texture2D;
        trueTex = new Texture2D(8, 1);
        trueTex.SetPixel(3, 0, C_statics[0]);
        trueTex.SetPixel(4, 0, C_statics[1]);
        trueTex.SetPixel(5, 0, C_statics[2]);
        trueTex.SetPixel(6, 0, C_statics[3]);
        trueTex.SetPixel(7, 0, C_statics[4]);
        trueTex.Apply();
        setPalette();

        //^^Goro = Resources.LoadAll<Sprite>(Bobo.name);//(texture.name);

        //**StartCoroutine(AnimationTest(12,4,animSpeed));

        //string spriteSheet = AssetDatabase.GetAssetPath(Bobo);
        //Goro = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (debugPalette)
        {
            setPalette();
            debugPalette = false;
        }
    }

    //FROM SetSpritePallitNumber
    [ContextMenu("Set Pallette Number")]
    void SetColor()
    {
        //        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        mySprite.color = new Color32((byte)pallette, 0, 0, 0);
    }
    //FROM SetSpritePallitNumber

    void setPalette()
    {
        trueTex.SetPixel(0, 0, C_outline);
        trueTex.SetPixel(1, 0, C_main);
        trueTex.SetPixel(2, 0, C_secondary);
        trueTex.Apply();
        //C_statics[0] = trueTex.GetPixel(2, 0);
        mySprite.material.SetTexture("_PaletteTex", trueTex);
    }
    */
}
