using UnityEngine;
using System.Collections;

public class Inv_PlayerWeapon : MonoBehaviour
{
    public GameObject B_Type;
    public int B_Limit;
    public int AmmoUse = 8; //A setting of 0 or less will give unlimited ammo and make the ammo meter invisible.
    public AudioClip shotSound;
    public bool noShotMove;

    public int anim_type;
}
