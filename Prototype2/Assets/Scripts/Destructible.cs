﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public GameObject crackedObject;

    public void CrackObject()
    {
        Instantiate(crackedObject, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}