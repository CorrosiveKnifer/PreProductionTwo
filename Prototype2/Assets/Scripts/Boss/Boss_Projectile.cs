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
    private PlayerAdrenalineProvider m_providerInfo;

    public void Start()
    {
        forward = transform.forward;
        m_providerInfo = GetComponent<PlayerAdrenalineProvider>();
    }

    // Update is called once per frame
    void Update()
    {
        Calculate();

        //Tell the player their dodge value.
        if(!m_providerInfo.mutex)
            m_target.GetComponent<PlayerMovement>()?.SetPotentialAdrenaline(m_providerInfo);
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
                m_providerInfo.m_value = 1.0f - ((myDist - radius) / (m_distWindow - radius));
            }
            else
            {
                m_providerInfo.m_value = 0.0f;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerController>().Damage(m_damage);
        }
        else if (other.GetComponent<Rigidbody>() != null)
        {
            other.GetComponent<Rigidbody>().AddExplosionForce(400f, transform.position, 1f);
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Attackable"))
        {
            other.GetComponent<Destructible>().ExplodeObject(transform.position, 400f, 1f);
        }
        
        if(other.tag != "Boss")
        {
            Instantiate(m_impactPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
    private void OnDrawGizmos()
    {
        float radius = transform.localScale.x;
        Gizmos.color = (m_providerInfo.m_value > 0) ? Color.green: Color.red; 
        Gizmos.DrawWireSphere(projPoint, radius);
        Handles.color = (m_providerInfo.m_value > 0) ? Color.green : Color.red;
        Handles.Label(projPoint, $"Dodge:{m_providerInfo.m_value }");
    }
}
