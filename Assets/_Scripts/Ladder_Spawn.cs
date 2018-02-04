using UnityEngine;
using System.Collections;

public class Ladder_Spawn : MonoBehaviour
{
    public LayerMask checkMask;
    public GameObject topPlatform;

    public Transform test;

    // Use this for initialization
    void Start()
    {
        float rayLength = 0.96f;
        Vector2 rayOrigin = transform.position;
        rayOrigin.y += rayLength;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, checkMask);

        if (!hit)
        {
            GameObject tp = Instantiate(topPlatform, transform.position, Quaternion.identity) as GameObject;
            tp.transform.parent = transform;
            tp.transform.localScale = Vector3.one;
        }
        else
            test = hit.transform;

        this.enabled = false;
    }
}
