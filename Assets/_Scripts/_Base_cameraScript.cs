using UnityEngine;
using System.Collections;

public class _Base_cameraScript : MonoBehaviour
{
    public Transform myTarget; //The transform component of the object the camera will be following. Typically this would be the player.
    Transform myTrans; //Store this objects Transform component into a cached variable to make accessing it more streamlined.

    public Vector2 aspectRatio = new Vector2 (5.0f, 4.25f);
    public bool resetRatio;

    public Transform leftBorderRoom; //The left most screen of the current "Room" the player is in. Used for giving the camera a left border.
    public Transform rightBorderRoom; //The left most screen of the current "Room" the player is in. Used for giving the camera a left border.

    public Vector2 storeBorders = new Vector2 (0f,2.56f); //Store the border setting. X = left border, Y = right border

    void Awake()
    {
        myTrans = GetComponent<Transform>();
        ResetRatio();

        storeBorders.x = leftBorderRoom.position.x;
        storeBorders.y = rightBorderRoom.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (resetRatio)
        {
            ResetRatio();
            resetRatio = false;
        }

        float modXPos = myTarget.position.x; //Store the X position of the camera's target, which by default will also be the Camera's X position. Unless....
        //... You've reached a corner.
        if (modXPos <= storeBorders.x)
            modXPos = storeBorders.x;
        if (modXPos >= storeBorders.y)
            modXPos = storeBorders.y;

        myTrans.position = new Vector3(modXPos, myTrans.position.y, myTrans.position.z);
    }

    void ResetRatio()
    {
        // set the desired aspect ratio (the values in this example are
        // hard-coded for 16:9, but you could make them into public
        // variables instead so you can set them at design time)
        float targetaspect = aspectRatio.x / aspectRatio.y;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;

        // obtain camera component so we can modify its viewport
        Camera camera = GetComponent<Camera>();

        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            camera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }
}
