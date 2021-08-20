using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss_Movement : MonoBehaviour
{
    private NavMeshAgent m_myAgent;
    
    // Start is called before the first frame update
    void Start()
    {
        m_myAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Stop()
    {
        m_myAgent.destination = transform.position;
    }

    public void SetTargetLocation(Vector3 _targetPos)
    {
        m_myAgent.destination = _targetPos;  
    }

    public bool IsNearTargetLocation(float distOffset = 1.0f)
    {
        return (transform.position - m_myAgent.destination).magnitude < distOffset;
    }
    public Vector3 GetDirection()
    {
        if (m_myAgent.isStopped)
            return Vector3.zero;

        return (m_myAgent.destination - transform.position).normalized;
    }
}
