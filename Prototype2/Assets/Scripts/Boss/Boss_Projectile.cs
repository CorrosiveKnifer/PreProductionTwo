using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Projectile : MonoBehaviour
{
    public Transform m_sender;
    public float m_damage;
    public float m_maxDistance = 100.0f;
    public GameObject m_target;
    public float m_distWindow = 20.0f;
    private Vector3 forward;

    private float dodgeVal;
    private Vector3 projPoint;
    public void Start()
    {
        forward = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        Calculate();
        if (Vector3.Distance(Vector3.zero, transform.position) > m_maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void Calculate()
    {
        float radius = transform.localScale.x;
        //Points: A = ball origin, B = origin + forward, P = target origin
        Vector3 AP = m_target.transform.position - transform.position;
        Vector3 AB = forward;

        //Formula: A + dot(AP, AB) / dot(AB, AB) * AB
        float t = (Vector3.Dot(AP, AB) / Vector3.Dot(AB, AB));
        projPoint = transform.position + t * AB;

        float myDist = Vector3.Distance(transform.position, projPoint);
        if(myDist < m_distWindow)
        {
            float theirDist = Vector3.Distance(m_target.transform.position, projPoint);
            if(theirDist < radius && t >= 0)
            {
                //Danger!
                dodgeVal = 1.0f - ((myDist - radius) / (m_distWindow - radius));
            }
            else
            {
                dodgeVal = 0.0f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag != "Boss")
        {
            Destroy(gameObject);
        } 
    }
    private void OnDrawGizmos()
    {
        float radius = transform.localScale.x;
        Gizmos.color = (dodgeVal > 0) ? Color.green: Color.red; 
        Gizmos.DrawWireSphere(projPoint, radius);
    }
}
