using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private PlayerController m_playerController;

    public Volume m_postProcessingVolume;
    public CinemachineFreeLook m_camera;
    private Camera m_camObject;

    // Target lockon
    public TargetObject m_selectedTarget { get; private set; }
    public float m_maxLockOnAngle = 60.0f;
    private GameObject m_targetObject; 
    private CinemachineTargetGroup m_targetGroup;
    private float m_cameraLockOnLerp = 0.0f;
    private Vector3 m_lastKnownPosition = Vector3.zero;

    [Header("Screen Shake")] // Screen shake
    private CinemachineBasicMultiChannelPerlin[] m_cameraChannels = new CinemachineBasicMultiChannelPerlin[3];

    // Start is called before the first frame update
    void Start()
    {
        m_playerController = GetComponent<PlayerController>();

        m_targetObject = new GameObject();
        m_targetObject.transform.position = transform.position;

        m_targetGroup = m_camera.LookAt.GetComponent<CinemachineTargetGroup>();
        m_targetGroup.m_Targets[1].target = m_targetObject.transform;
        m_camObject = Camera.main;

        // Get array of camera noise channels
        for (int i = 0; i < 3; i++)
        {
            CinemachineVirtualCamera rig = m_camera.GetRig(i);
            m_cameraChannels[i] = rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Change post processing weight based on adrenaline levels
        m_postProcessingVolume.weight = m_playerController.m_effectsPercentage;

        // Change camera lock on lerp depending on whether or not a target is selected
        m_cameraLockOnLerp += ((m_selectedTarget == null) ? -1 : 1) * Time.deltaTime * 3.0f;
        m_cameraLockOnLerp = Mathf.Clamp(m_cameraLockOnLerp, 0.0f, 1.0f);

        if (m_selectedTarget != null)
        {
            // Store the last known position of the selected object
            m_lastKnownPosition = m_selectedTarget.transform.position;

            // Make camera have both target and player in frame
            Vector3 direction = m_lastKnownPosition - transform.position;
            float targetAngle = Mathf.Atan2(direction.normalized.x, direction.normalized.z) * Mathf.Rad2Deg;
            m_camera.m_XAxis.Value = Mathf.LerpAngle(m_camera.m_XAxis.Value, targetAngle, 1 - Mathf.Pow(2.0f, -Time.deltaTime * 6.0f));
            m_camera.m_YAxis.Value = Mathf.LerpAngle(m_camera.m_YAxis.Value, 0.667f, 1 - Mathf.Pow(2.0f, -Time.deltaTime * 6.0f));
        }

        // Set the target's position
        m_targetObject.transform.position = Vector3.Lerp(transform.position, m_lastKnownPosition, m_cameraLockOnLerp);
    }
    public void MoveCamera(Vector2 _move)
    {
        if (m_selectedTarget != null) // Don't move if selected target exists.
            return;
        m_camera.m_XAxis.Value += -GameManager.m_sensitivity.x * _move.x * Time.deltaTime;
        m_camera.m_YAxis.Value += (GameManager.m_sensitivity.y / 100.0f) * _move.y * Time.deltaTime;
    }

    public void ToggleLockOn()
    {
        // Deselect current target.
        if (m_selectedTarget != null)
        {
            m_selectedTarget = null;
            return;
        }

        // Find all targets
        TargetObject[] targets = FindObjectsOfType<TargetObject>();
        TargetObject currentTarget = null;

        foreach (var target in targets)
        {
            // If no current target exists check if it meets requirements.
            if (currentTarget == null)
            {
                if (Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position) < m_maxLockOnAngle) // Check if target within angle
                {
                    Debug.Log(Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position));
                    currentTarget = target;
                }
                continue;
            }

            if (Vector3.Distance(transform.position, target.transform.position) < Vector3.Distance(transform.position, currentTarget.transform.position)) // Check if target is closer than current.
            {
                if (Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position) < m_maxLockOnAngle) // Check if target within angle
                {
                    Debug.Log(Vector3.Angle(m_camObject.transform.forward, target.transform.position - transform.position));
                    currentTarget = target;
                }
            }
        }

        m_selectedTarget = currentTarget;
        if (m_selectedTarget != null)
        {
            Debug.Log("Target found");
        }

    }

    public void ScreenShake(float _duration, float _amplitude, float _frequency)
    {
        StartCoroutine(ShakeUpdate(_duration, _amplitude, _frequency));
    }
    IEnumerator ShakeUpdate(float _duration, float _amplitude, float _frequency)
    {
        // Apply amplitude and frequency
        for (int i = 0; i < 3; i++)
        {
            m_cameraChannels[i].m_AmplitudeGain = _amplitude;
            m_cameraChannels[i].m_FrequencyGain = _frequency;
        }
        yield return new WaitForSecondsRealtime(_duration); // Wait for duration
        // Revert changes
        for (int i = 0; i < 3; i++)
        {
            m_cameraChannels[i].m_AmplitudeGain = 0.0f;
            m_cameraChannels[i].m_FrequencyGain = 0.0f;
        }
    }
}
