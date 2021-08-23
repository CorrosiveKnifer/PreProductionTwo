using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Kick : MonoBehaviour
{
    public bool isPlayerWithin = false;
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            isPlayerWithin = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isPlayerWithin = false;
        }
    }
}
