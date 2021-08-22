using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss_Movement : MonoBehaviour
{
    [Range(0, 360, order = 0)]
    public float m_maxStearingAngle = 180;
    public float m_stearDecay = 5.0f;

    public float m_stearModifier = 1.0f;
    private NavMeshAgent m_myAgent;
    private Quaternion m_targetRotation;
    // Start is called before the first frame update
    void Start()
    {
        m_myAgent = GetComponent<NavMeshAgent>();
        m_targetRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        m_stearModifier = Mathf.Clamp(m_stearModifier - m_stearDecay * Time.deltaTime, 1.0f, 10.0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, m_targetRotation, Time.deltaTime * m_stearModifier);
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

    public void RotateTowards(Quaternion _rotation)
    {
        m_targetRotation = Quaternion.RotateTowards(transform.rotation, _rotation, m_maxStearingAngle);
    }

    public Vector3 GetDirection()
    {
        if (m_myAgent.isStopped)
            return Vector3.zero;

        return (m_myAgent.destination - transform.position).normalized;
    }
    public void SetStearModifier(float _val)
    {
        m_stearModifier = Mathf.Clamp(_val, 1.0f, 10.0f);
    }
}
