using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss_Movement : MonoBehaviour
{
    [Range(0, 360, order = 0)]
    public float m_maxStearingAngle = 180;
    public float m_stearDecay = 5.0f;

    public float m_stearModifier = 1.5f;
    private NavMeshAgent m_myAgent;
    private Quaternion m_targetRotation;
    // Start is called before the first frame update
    void Start()
    {
        m_myAgent = GetComponent<NavMeshAgent>();
        m_targetRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        m_stearModifier = Mathf.Clamp(m_stearModifier - m_stearDecay * Time.deltaTime, 4f, 10.0f);
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

    public float GetAngle(Quaternion _rotation)
    {
        return Quaternion.Angle(transform.rotation, _rotation);
    }

    public void RotateTowards(Quaternion _rotation)
    {
        m_targetRotation = Quaternion.RotateTowards(transform.rotation, _rotation, m_maxStearingAngle);
        
    }

    /*
     * GetDirection by Michael Jordan
     * Description:
     *  Returns the direction the boss is moving in relative to either the world or itself.
     *  If the space is relative to the WORLD, it will return the actual direction it is moving in.
     *  If the space is relative to itSELF, it will return the direction which ignores it's current rotation.
     *  Will return a ZERO VECTOR if and only if the boss is NOT CURRENTLY MOVING.
     *
     * Param:
     *  Space - "Which space is it relative to?"
     *
     * Return: 
     *  Vector3 - "Direction the boss is moving in (normalized)."
     */
    public Vector3 GetDirection(Vector3 m_targetPos, Space _relativeTo)
    {
        switch (_relativeTo)
        {
            case Space.World:
                return (m_targetPos - transform.position).normalized;
            case Space.Self: 
                //Removes self rotation
                Vector3 worldDirect = (m_targetPos - transform.position).normalized;
                return (Quaternion.AngleAxis(transform.rotation.eulerAngles.y, -Vector3.up) * worldDirect).normalized;
            default:
                return Vector3.zero;
        }
    }

    public void SetStearModifier(float _val)
    {
        m_stearModifier = Mathf.Clamp(_val, 1.0f, 10.0f);
    }

}
