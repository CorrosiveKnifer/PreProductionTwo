using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineFreeLook m_camera;
    private CinemachineTargetGroup m_targetGroup;
    public float m_maxLockOnAngle = 60.0f;
    private Camera m_camObject;
    private Vector3 m_pivot;
    public TargetObject m_selectedTarget { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        m_pivot = transform.position + transform.up;
        m_targetGroup = m_camera.LookAt.GetComponent<CinemachineTargetGroup>();
        m_camObject = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void MoveCamera(Vector2 _move)
    {
        m_camera.m_XAxis.Value += -GameManager.m_sensitivity.x * _move.x * Time.deltaTime;
        m_camera.m_YAxis.Value += (GameManager.m_sensitivity.y / 100.0f) * _move.y * Time.deltaTime;
    }

    public void ToggleLockOn()
    {
        if (m_selectedTarget != null)
        {
            m_selectedTarget = null;
            m_targetGroup.m_Targets[1].target = null;
            return;
        }

        TargetObject[] targets = FindObjectsOfType<TargetObject>();

        TargetObject currentTarget = null;

        foreach (var target in targets)
        {
            if (currentTarget == null)
            {
                if (Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position) < m_maxLockOnAngle)
                {
                    Debug.Log(Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position));
                    currentTarget = target;
                }
                continue;
            }

            if (Vector3.Distance(transform.position, target.transform.position) < Vector3.Distance(transform.position, currentTarget.transform.position))
            {
                if (Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position) < m_maxLockOnAngle)
                {
                    Debug.Log(Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position));
                    currentTarget = target;
                }
            }
        }

        m_selectedTarget = currentTarget;
        if (m_selectedTarget != null)
        {
            m_targetGroup.m_Targets[1].target = m_selectedTarget.transform;
            Debug.Log("Target found");
        }

    }
}
