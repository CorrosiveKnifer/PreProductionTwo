using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boss_Projectile : MonoBehaviour
{
    public Transform m_sender;
    public float m_damage;
    public float m_maxDistance = 100.0f;
    public GameObject m_target;
    public float m_distWindow = 20.0f;

    public GameObject m_impactPrefab;
    private Vector3 forward;

    private Vector3 projPoint;

    public void Start()
    {
        forward = transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerController>().Damage(m_damage, true);
            Destroy(gameObject);
            return;
        }

        if(other.tag == "Adrenaline Shadow")
        {
            if(!CanStillHitThePlayer())
            {
                other.GetComponent<AdrenalineProvider>().GiveAdrenaline();
                Destroy(gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }
            return;
        }

        if(other.tag != "Boss")
        {
            Instantiate(m_impactPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
    private bool CanStillHitThePlayer()
    {
        float radius = transform.localScale.x;
        //Points: A = ball origin, B = origin + forward, P = target origin
        Vector3 AP = m_target.transform.position - transform.position;
        Vector3 AB = forward;

        //Formula: A + dot(AP, AB) / dot(AB, AB) * AB
        float t = (Vector3.Dot(AP, AB) / Vector3.Dot(AB, AB));
        projPoint = transform.position + t * AB;

        float myDist = Vector3.Distance(transform.position, projPoint);
        if (myDist < m_distWindow)
        {
            float theirDist = Vector3.Distance(m_target.transform.position, projPoint);
            return (theirDist < radius && t >= 0);
        }
        return true;
    }
}
