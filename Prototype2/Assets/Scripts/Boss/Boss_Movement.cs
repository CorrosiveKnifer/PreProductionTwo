using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss_Movement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent m_myAgent;
    
    // Start is called before the first frame update
    void Start()
    {
        if(m_myAgent == null)
            m_myAgent = GetComponentInChildren<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTargetLocation(Vector3 _targetPos)
    {
        m_myAgent.destination = _targetPos;
    }

    public bool IsNearTargetLocation(Vector3 _targetPos, float distOffset = 1.0f)
    {
        return (m_myAgent.destination - _targetPos).magnitude < distOffset;
    }
}
