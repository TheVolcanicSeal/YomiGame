﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastMoveDirection
{

    private Vector2 raycastDirection;
    private Vector2[] offsetPoints;
    private LayerMask layerMask;
    private float addLenght;

    public RaycastMoveDirection(Vector2 start, Vector2 end, Vector2 dir, LayerMask mask,Vector2 parallelInset, Vector2 perpendicularInset)
    {
        this.raycastDirection = dir;

        this.offsetPoints = new Vector2[]
        {
            start + parallelInset + perpendicularInset,
            end - parallelInset + perpendicularInset,

        };

        this.layerMask = mask;

    }
    

    public float DoRaycast(Vector2 origin, float distance)
    {
        float minDistance = distance;

        foreach (var offset in offsetPoints)
        {
            RaycastHit2D hit = Raycast(origin + offset, raycastDirection, distance + addLenght, layerMask);
               

            if (hit.collider != null)
            {

                minDistance = Mathf.Min(minDistance, hit.distance - addLenght);

            }

        }
        return minDistance;

    }

    private RaycastHit2D Raycast(Vector2 start, Vector2 dir, float len, LayerMask mask)
    {
        //Debug.Log(string.Format("Raycast start {0} is {1} for {2}", start, dir, len));
        Debug.DrawLine(start, start + dir * len, Color.blue);
        return Physics2D.Raycast(start, dir, len, mask);

    }
}
