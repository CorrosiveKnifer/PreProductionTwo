using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController m_characterController;
    public Camera m_camera;
    public GameObject m_playerModel;
    public float m_gravityMult = 9.81f;
    public float m_jumpSpeed = 5.0f;

    float m_turnSmoothTime = 0.075f;
    float m_turnSmoothVelocity;

    public float m_moveSpeed = 6.0f;
    private bool m_grounded = true;
    private float m_yVelocity = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Check if character is touching the ground
        if (m_characterController.isGrounded)
        {
            m_grounded = true;

            if (m_yVelocity < 0.0f) // Checking if character is not going upwards
            {
                // Snap character to ground
                m_yVelocity = -0.1f;
            }
        }
        else
        {
            m_grounded = false;

            // If not grounded apply gravity
            m_yVelocity -= m_gravityMult * Time.fixedDeltaTime;
        }
    }

    public void Move(Vector2 _move, bool _jump)
    {
        if (_jump && m_grounded)
        {
            m_yVelocity = m_jumpSpeed;
        }

        // Movement
        Vector3 normalizedMove = new Vector3(0, 0, 0);
        Vector3 cameraRight = m_camera.transform.right;
        cameraRight.y = 0;

        Vector3 cameraForward = m_camera.transform.forward;
        cameraForward.y = 0;

        normalizedMove += _move.x * cameraRight.normalized;
        normalizedMove += _move.y * cameraForward.normalized;

        m_characterController.Move(normalizedMove * m_moveSpeed * Time.deltaTime // Movement
            + transform.up * m_yVelocity * Time.deltaTime); // Jump

        Vector3 direction;
        direction.x = normalizedMove.x;
        direction.y = 0;
        direction.z = normalizedMove.z;

        // Rotate player model
        if (_move.magnitude >= 0.1f && m_playerModel != null)
        {
            float targetAngle = Mathf.Atan2(direction.normalized.x, direction.normalized.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(m_playerModel.transform.eulerAngles.y, targetAngle, ref m_turnSmoothVelocity, m_turnSmoothTime);
            m_playerModel.transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            //direction = direction.normalized;
            //float angle = Vector3.SignedAngle(cameraForward, direction, transform.up);
            ////angle *= Mathf.Sign(direction.x);
            //m_playerModel.transform.rotation = Quaternion.Lerp(m_playerModel.transform.rotation,
            //    Quaternion.Euler(0, angle, 0),
            //    1 - Mathf.Pow(2.0f, -Time.deltaTime * 20.0f));
        }
    }
}
