﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    GameObject player;
    public float offsetY;

    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player");

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 targetPos = new Vector3(player.transform.position.x, player.transform.position.y + offsetY, -1);

        transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);

    }
}
