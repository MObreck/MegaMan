using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class __CachedVault : MonoBehaviour
{
    [HideInInspector] public float D_Width = 0.015f; //**
    [HideInInspector] public float Micro = 0.0001f; //A really, really tiny number used for certain measurement collision effects where we really don't want much measurement movement (like boxcasts).

    public float flicker_time = 0.025f; //The brief time an enemy flashes invisible after being struck by a player attack.
    public float p_flicker_time = 0.05f; //The brief time an enemy flashes invisible after being struck by a player attack.
    public float fill_time = 0.3f; //The pause between each unit of ammo/health as it gets filled from a powerup or boss intro.

    public LayerMask collisionMask;
    public LayerMask LadderMask;
    public LayerMask P_Layer; //The layer mask enemies use to attack the player
    public LayerMask E_Layer; //The layer mask the player uses to attack the enemies

    public AudioClip sfx_HitSound; //That classic default sound effect when an enemy in Megaman is struck by a player attack ;)
    public AudioClip sfx_DeflectSound; //That classic default sound effect for a weapon deflecting off an immune enemy.
    public AudioClip[] sfx_getItem; //An array of typical sounds effects used for getting items.

    public List<Color> C_; //The database of all the entire used palette in this game. All 64 colors :P

    [HideInInspector] public Player_Basics myP;

    public GameObject DeathSpark;
    public List<GameObject> DeathSparks;

    public Sprite Hurt_Spark;

    public Transform G_Cache;

    public Texture2D mm_tex; //This is used to restore Megaman's material to its default setting when the game is shut down, since it is subjected to SharedMaterial scripts.

    [HideInInspector] public Vector3 noWhere = new Vector3(-999, -999, 999); //This insane Vector3 is used to make sure an object is no longer and screen: AT ALL. Used for some failsafe effects.

    //public GameObject bigTurd;
    //public GameObject oldTurd;
    //public bool turdTest;

    [HideInInspector] public int meter_mult = 4; //The multipler used for the health meters, since technically the meter health is 4 times what is shown to allow 1/2 and 1/4 type changes.
    [HideInInspector] public int ammo_meter_mult = 8; //The multipler used for the ammo meters, since technically the meter ammo is 8 times what is shown to allow 1/2, 1/4, 1/8, etc type changes.

    [HideInInspector] public bool powerUpFreeze; //Set while the player's meter is being filled by a powerup.
    public bool quickFill; //When set ammo and health powerups fill instantly instead of the traditional "one unit at a time" style used in Megaman.

    // Use this for initialization
    void Awake ()
    {
        Application.targetFrameRate = 60;

        myP = GameObject.FindWithTag("Player").GetComponent<Player_Basics>();
        G_Cache = GameObject.FindWithTag("G_Cache").transform;
        for (int i = 0; i < 12; i++)
        {
            DeathSparks.Add(Instantiate(DeathSpark, Vector3.zero, Quaternion.identity) as GameObject);
            DeathSparks[i].transform.parent = G_Cache;
            DeathSparks[i].transform.localScale = Vector3.one;
        }

        if (C_.Count < 64) //Oops! The original swap palette must have been deleted! Let's restore it.
            bootColors();
    }

    void Update()
    {
        //Alpha save map code.
        /*
        if (Input.GetButtonDown("TestThis"))
        {
            turdTest = true;
            //GameObject monkey = PrefabUtility.CreatePrefab("Assests/ButtNuggets", bigTurd);
            EditorUtility.ReplacePrefab(bigTurd, oldTurd);
            AssetDatabase.Refresh();
        }
        */
        //END save map code

        print("FPS: " + (1.0f / Time.deltaTime).ToString());
    }

    //If by some blunder the game palette swap palettes are lost this script will at least restore the originals.
    void bootColors()
    {
        C_.Clear();
        C_.Add(new Color32 (0,0,0,0));
        C_.Add(new Color32(36, 24, 140, 255));
        C_.Add(new Color32(0, 112, 232, 255));
        C_.Add(new Color32(60, 188, 252, 255));
        C_.Add(new Color32(168, 224, 248, 255));
        C_.Add(new Color32(0, 0, 168, 255));
        C_.Add(new Color32(32, 56, 232, 255));
        C_.Add(new Color32(92, 148, 252, 255));
        C_.Add(new Color32(196, 212, 252, 255));
        C_.Add(new Color32(68, 0, 156, 255));
        C_.Add(new Color32(128, 0, 240, 255));
        C_.Add(new Color32(204, 136, 252, 255));
        C_.Add(new Color32(208, 200, 248, 255));
        C_.Add(new Color32(140, 0, 116, 255));
        C_.Add(new Color32(184, 0, 184, 255));
        C_.Add(new Color32(244, 120, 252, 255));
        C_.Add(new Color32(252, 196, 252, 255));
        C_.Add(new Color32(168, 0, 16, 255));
        C_.Add(new Color32(228, 0, 88, 255));
        C_.Add(new Color32(248, 112, 176, 255));
        C_.Add(new Color32(252, 196, 216, 255));
        C_.Add(new Color32(160, 132, 248, 255));
        C_.Add(new Color32(216, 40, 0, 255));
        C_.Add(new Color32(252, 116, 96, 255));
        C_.Add(new Color32(248, 184, 176, 255));
        C_.Add(new Color32(124, 8, 0, 255));
        C_.Add(new Color32(200, 76, 12, 255));
        C_.Add(new Color32(252, 152, 56, 255));
        C_.Add(new Color32(252, 216, 168, 255));
        C_.Add(new Color32(64, 44, 0, 255));
        C_.Add(new Color32(136, 112, 0, 255));
        C_.Add(new Color32(240, 184, 56, 255));
        C_.Add(new Color32(252, 228, 160, 255));
        C_.Add(new Color32(0, 68, 0, 255));
        C_.Add(new Color32(0, 148, 0, 255));
        C_.Add(new Color32(128, 208, 16, 255));
        C_.Add(new Color32(224, 252, 160, 255));
        C_.Add(new Color32(0, 80, 0, 255));
        C_.Add(new Color32(0, 168, 0, 255));
        C_.Add(new Color32(76, 220, 72, 255));
        C_.Add(new Color32(168, 240, 188, 255));
        C_.Add(new Color32(0, 60, 20, 255));
        C_.Add(new Color32(0, 144, 56, 255));
        C_.Add(new Color32(88, 248, 152, 255));
        C_.Add(new Color32(176, 252, 204, 255));
        C_.Add(new Color32(24, 60, 92, 255));
        C_.Add(new Color32(0, 128, 136, 255));
        C_.Add(new Color32(0, 232, 216, 255));
        C_.Add(new Color32(152, 248, 240, 255));
        C_.Add(new Color32(5, 5, 5, 255));
        C_.Add(new Color32(29, 29, 29, 255));
        C_.Add(new Color32(58, 58, 58, 255));
        C_.Add(new Color32(98, 98, 98, 255));
        C_.Add(new Color32(118, 118, 118, 255));
        C_.Add(new Color32(192, 192, 192, 255));
        C_.Add(new Color32(252, 252, 252, 255));
        C_.Add(new Color32(252, 0, 0, 255)); //Last offical color for now.
        C_.Add(new Color32(0, 255, 255, 255));
        C_.Add(new Color32(0, 255, 255, 255));
        C_.Add(new Color32(0, 255, 255, 255));
        C_.Add(new Color32(0, 255, 255, 255));
        C_.Add(new Color32(0, 255, 255, 255));
        C_.Add(new Color32(0, 255, 255, 255));
        C_.Add(new Color32(0, 255, 255, 255));
    }
}
