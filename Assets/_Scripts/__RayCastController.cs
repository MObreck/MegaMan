using UnityEngine;
using System.Collections;

// This is a generic script that handles some basic collision raycast/boxcast/etc coding that is used by many game objects with collision data.

[RequireComponent(typeof(BoxCollider2D))] //Since all this data is built off a BoxCollider2D component, let's force the object to have at least one in its components.
public class __RayCastController : MonoBehaviour
{
    [HideInInspector] public __CachedVault aa;
    [HideInInspector] public const float skinWidth = 0.015f; //**
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    [HideInInspector] public float horizontalRaySpacing;//**
    [HideInInspector] public float verticalRaySpacing;//**

    [HideInInspector] public BoxCollider2D collider;//**
    [HideInInspector] public RaycastOrigins raycastOrigins;//**

    // Use this for initialization
    public virtual void Start()
    {
        aa = GameObject.FindWithTag("GameController").GetComponent<__CachedVault>();

        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

    }

    public void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, 256);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, 256);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

}
