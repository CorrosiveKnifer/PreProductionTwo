using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineFreeLook m_camera;
    private Vector3 m_pivot;

    // Start is called before the first frame update
    void Start()
    {
        m_pivot = transform.position + transform.up;
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
}
