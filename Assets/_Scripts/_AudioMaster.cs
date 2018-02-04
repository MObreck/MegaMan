using UnityEngine;
using System.Collections;

public class _AudioMaster : MonoBehaviour
{
    public AudioSource musicPlayer;
    public AudioSource PlayerAttacks;
    public AudioSource PlayerGeneral;
    public AudioSource EnemyAttacks;
    public AudioSource EnemyGeneral;

    public AudioClip[] TestSounds;

	// Use this for initialization
	void Start ()
    {
        AudioSource[] CrazyRazy = GetComponents<AudioSource>();

        musicPlayer = CrazyRazy[0];
        PlayerAttacks = CrazyRazy[1];
        PlayerGeneral = CrazyRazy[2];
        EnemyAttacks = CrazyRazy[3];
        EnemyGeneral = CrazyRazy[4];

        musicPlayer.clip = TestSounds[0];
        musicPlayer.Play();
        //PlayerGeneral.clip = TestSound;
        //PlayerGeneral.Play();
    }

    /*
	// Update is called once per frame
	void Update () {
	
	}
    */
}
