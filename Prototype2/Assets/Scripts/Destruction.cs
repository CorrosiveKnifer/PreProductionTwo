using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{
    private Rigidbody[] m_myChildren;

    // Start is called before the first frame update
    public void Awake()
    {
        m_myChildren = GetComponentsInChildren<Rigidbody>();
    }

    public void ApplyExplosionForce(Vector3 forceLoc, float forceVal, float maxDist)
    {
        foreach (var bodies in m_myChildren)
        {
            bodies.AddExplosionForce(forceVal, forceLoc, maxDist);
        }
    }
    
}
