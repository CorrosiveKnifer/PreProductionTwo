using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private PlayerMovement m_playerMovement;

    public CinemachineFreeLook m_camera;
    private CinemachineTargetGroup m_targetGroup;
    private GameObject m_targetObject; 
    private float m_cameraLockOnLerp = 0.0f;
    public float m_maxLockOnAngle = 60.0f;
    private Camera m_camObject;
    private Vector3 m_pivot;
    public TargetObject m_selectedTarget { get; private set; }
    private Vector3 m_lastKnownPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        m_playerMovement = GetComponent<PlayerMovement>();

        m_pivot = transform.position + transform.up;
        m_targetObject = new GameObject();
        m_targetObject.transform.position = transform.position;

        m_targetGroup = m_camera.LookAt.GetComponent<CinemachineTargetGroup>();
        m_targetGroup.m_Targets[1].target = m_targetObject.transform;
        m_camObject = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        m_cameraLockOnLerp += ((m_selectedTarget == null) ? -1 : 1) * Time.deltaTime * 3.0f;
        m_cameraLockOnLerp = Mathf.Clamp(m_cameraLockOnLerp, 0.0f, 1.0f);

        if (m_selectedTarget != null)
        {
            m_lastKnownPosition = m_selectedTarget.transform.position;
            Vector3 direction = m_lastKnownPosition - transform.position;
            float targetAngle = Mathf.Atan2(direction.normalized.x, direction.normalized.z) * Mathf.Rad2Deg;
            m_camera.m_XAxis.Value = Mathf.LerpAngle(m_camera.m_XAxis.Value, targetAngle, 1 - Mathf.Pow(2.0f, -Time.deltaTime * 6.0f));
            //m_camera.m_YAxis.Value;
        }

        m_targetObject.transform.position = Vector3.Lerp(transform.position, m_lastKnownPosition, m_cameraLockOnLerp);
    }
    public void MoveCamera(Vector2 _move)
    {
        if (m_selectedTarget != null)
            return;
        m_camera.m_XAxis.Value += -GameManager.m_sensitivity.x * _move.x * Time.deltaTime;
        m_camera.m_YAxis.Value += (GameManager.m_sensitivity.y / 100.0f) * _move.y * Time.deltaTime;
    }

    public void ToggleLockOn()
    {
        if (m_selectedTarget != null)
        {
            m_selectedTarget = null;
            //m_targetGroup.m_Targets[1].target = null;
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
            //m_targetGroup.m_Targets[1].target = m_selectedTarget.transform;
            Debug.Log("Target found");
        }

    }
}
