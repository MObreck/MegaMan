using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class P_Inventory : MonoBehaviour
{
    __CachedVault aa;

    public SpriteRenderer h_meter; //The health meter sprite
    public SpriteRenderer w_meter; //The weapon meter sprite

    public int maxAmmo = 224; //The default maxium Robot Master/Utility ammo.
    public List <int> wepAmmo;

    public int cw; //The current assigned weapon.
    public Vector2[] p_palettes;
    public bool[] wepDisabled;
    public bool altSwapMode; //When set instead of using an unique button weapons are cycled backwards with down+cycleFoward.

    public Sprite[] meter_Units;

    int maxMeter = 28; //The maximum this meter can be.
    //Player_Basics myP;
    _MM_PaletteSwapper pal;

    public List<Transform> wepPool;
    public List<Inv_PlayerWeapon> wepScript;

    // Use this for initialization
    void Start ()
    {
        aa = GameObject.FindWithTag("GameController").GetComponent<__CachedVault>();

        //myP = transform.parent.GetComponent<Player_Basics>();

        pal = transform.parent.GetComponent<_MM_PaletteSwapper>();
        //Let's set player palette 0 to the default palette of the currently used character skin.
        p_palettes[0] = new Vector2(pal.C_C[1], pal.C_C[2]);

        //We don't need to show the ammo meter at the start of a stage because the player's default weapon is equipped.
        w_meter.enabled = false;

        refresh_HealthBar(true);
        refresh_AmmoBar(true);


        //GameObject[] getPool = GameObject.FindGameObjectsWithTag("Weapon_Pool");
        wepPool.Clear();
        wepAmmo.Clear();

        int w_NO = 1;
        Transform wepCache = GameObject.FindWithTag("Weapon_Pool").transform;
        for (int i = 0; i < wepScript.Count; i++)
        {
            GameObject newObj = new GameObject("wepPool_" + w_NO.ToString());
            newObj.transform.parent = wepCache;
            newObj.transform.localScale = Vector3.one;
            wepPool.Add(newObj.transform);
            w_NO += 1;

            //----------------------------------------------------------------------------------------------------
            //----------------------------------------------------------------------------------------------------
            for (int j = 0; j < wepScript[i].B_Limit; j++)
            {
                GameObject makeBullet = Instantiate(wepScript[i].B_Type, transform.position, Quaternion.identity) as GameObject;
                makeBullet.transform.parent = wepPool[i];
                makeBullet.transform.localScale = Vector3.one;
            }

            wepAmmo.Add(maxAmmo);
        }

        Set_Weapon ();
        /*
            wepPool.Add(getPool[i].transform);
            */
    }

    void Set_Weapon()
    {
        aa.myP.shotReserve.Clear();
        // wepPool[cw]
        for (int i = 0; i < wepPool[cw].childCount; i++)
        {
            aa.myP.shotReserve.Add(wepPool[cw].GetChild(i).gameObject);
        }
        aa.myP.shotAnim = wepScript[cw].anim_type + 1;
        aa.myP.shotSound = wepScript[cw].shotSound;
        aa.myP.noShotMove = wepScript[cw].noShotMove;
    }

    public void refresh_HealthBar(bool bootUP = false)
    {
        aa.myP.HP = Mathf.Clamp(aa.myP.HP, 0, maxMeter * aa.meter_mult);
        if (bootUP) //Set the meter to full at the beginning of the game or if the player's health somehow is calulated over the legal limit.
        {
            h_meter.sprite = meter_Units[28];
            return;
        }
        if (aa.myP.HP <= 0) //Default to showing an empty bar if this meter's stat is at 0 or below.
        {
            h_meter.sprite = meter_Units[0];
        }
        else //Overwise recalculate how full the meter is.
        {
            h_meter.sprite = meter_Units[(int)Mathf.Ceil(aa.myP.HP / aa.meter_mult)];
        }
    }

    public void refresh_AmmoBar(bool bootUP = false)
    {
        if (!w_meter.enabled)
            return;
        wepAmmo[cw] = Mathf.Clamp(wepAmmo[cw],0, maxAmmo);
        if (bootUP) //Set the meter to full at the beginning of the game or if the player's ammo somehow is calulated over the legal limit.
        {
            w_meter.sprite = meter_Units[28];
            return;
        }
        if (wepAmmo[cw] <= 0) //Default to showing an empty bar if this meter's stat is at 0 or below.
        {
            w_meter.sprite = meter_Units[0];
        }
        else //Overwise recalculate how full the meter is.
        {
            w_meter.sprite = meter_Units[(int)Mathf.Ceil(wepAmmo[cw] / aa.ammo_meter_mult)];
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (aa.myP.cutScened)
            return;

        if ((Input.GetButtonDown("WepBack") && !altSwapMode) || (Input.GetButtonDown("WepFwd") && Input.GetAxisRaw("Vertical") < 0 && altSwapMode))
        {

            cw = (cw <= 0) ? p_palettes.Length - 1 : cw - 1;

            //Skip over weapons that haven't been unlocked yet / are disabled.
            while (cw > 0 && wepDisabled[cw])
                cw = (cw <= 0) ? p_palettes.Length - 1 : cw - 1;

            changeWeapon();
        }
        else if (Input.GetButtonDown("WepFwd"))
        {
            cw = (cw >= p_palettes.Length - 1) ? 0 : cw + 1;

            //Skip over weapons that haven't been unlocked yet / are disabled.
            while (wepDisabled[cw])
                cw = (cw >= p_palettes.Length - 1) ? 0 : cw + 1;

            changeWeapon();
        }
    }

    void changeWeapon ()
    {
        pal.setPalette(49, (int)p_palettes[cw].x, (int)p_palettes[cw].y);

        if (wepScript[cw].AmmoUse < 1) //cw == 0) //Don't show the ammo meter if the player's default weapon is set.
            w_meter.enabled = false;
        else
        {
            w_meter.enabled = true;
            /*
            Texture2D wepTex = new Texture2D(8, 1);
            wepTex.SetPixel(0, 0, aa.C_[49]);
            wepTex.SetPixel(1, 0, aa.C_[(int)p_palettes[cw].x]);
            wepTex.SetPixel(2, 0, aa.C_[(int)p_palettes[cw].y]);
            wepTex.SetPixel(3, 0, aa.C_[49]);
            wepTex.Apply();

            w_meter.material.SetTexture("_PaletteTex", wepTex);
            */
        }

        Set_Weapon();
        refresh_AmmoBar();
    }
}
