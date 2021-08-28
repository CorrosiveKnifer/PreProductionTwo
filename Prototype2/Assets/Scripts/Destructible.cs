using System.Collections;
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
    public void ExplodeObject(Vector3 forceLoc, float forceVal, float maxDist)
    {
        Destruction destructObject = Instantiate(crackedObject, transform.position, transform.rotation).GetComponent<Destruction>();
        //destructObject.ApplyExplosionForce(forceLoc, forceVal, maxDist);
        Destroy(gameObject);
    }
}
