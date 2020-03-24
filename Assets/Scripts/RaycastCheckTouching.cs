using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCheckTouching
{
    private Vector2 raycastDirection;
    private Vector2[] offsetPoints;
    private LayerMask layerMask;
    private float raycastLen;

    public RaycastCheckTouching(Vector2 start, Vector2 end, Vector2 dir, LayerMask mask, Vector2 parallelInset, Vector2 perpendicularInset, float checkLength)
    {
        this.raycastDirection = dir;

        this.offsetPoints = new Vector2[]
        {
            start + parallelInset + perpendicularInset,
            end - parallelInset + perpendicularInset,

        };

        this.raycastLen = perpendicularInset.magnitude + checkLength;

        this.layerMask = mask;

    }


    public bool DoRaycast(Vector2 origin)
    { 
        foreach (var offset in offsetPoints)
        {
            RaycastHit2D hit = Raycast(origin + offset, raycastDirection, raycastLen, layerMask);


            if (hit.collider != null)
            {

                return true;

            }

        }
        return false;

    }

    public bool RaycastCheckClimbable(Vector2 origin)
    {
        foreach (var offset in offsetPoints)
        {

            RaycastHit2D hit = Raycast(origin + offset, raycastDirection, raycastLen, layerMask);


            if (hit.collider != null && hit.collider.gameObject.tag == "Climbable")
            {

                return true;

            }


        }

        return false;

    }

    private RaycastHit2D Raycast(Vector2 start, Vector2 dir, float len, LayerMask mask)
    {
        //Debug.Log(string.Format("Raycast start {0} is {1} for {2}", start, dir, len));
        //Debug.DrawLine(start, start + dir * len, Color.red);
        return Physics2D.Raycast(start, dir, len, mask);

    }
}
