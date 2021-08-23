using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController m_playerController;
    private CharacterController m_characterController;

    public Camera m_camera;
    public GameObject m_playerModel;

    [Header("Movement")]
    public float m_gravityMult = 9.81f;
    public float m_jumpSpeed = 5.0f;
    public float m_moveSpeed = 6.0f;
    public float m_rollSpeed = 12.0f;

    float m_turnSmoothTime = 0.075f;
    float m_turnSmoothVelocity;

    private bool m_grounded = true;
    private float m_yVelocity = 0.0f;

    public bool m_stagger { get; private set; } = false;
    public bool m_knockedDown { get; private set; } = false;
    private Vector3 m_knockVelocity = Vector3.zero;

    public bool m_isRolling { get; private set; } = false;
    private Vector3 m_lastMoveDirection;

    //External Link to adrenaline giver
    private PlayerAdrenalineProvider o_adrenalineProvider;

    // Start is called before the first frame update
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_knockedDown)
        {
            m_characterController.Move(m_knockVelocity * Time.deltaTime);
            m_knockVelocity = Vector3.Lerp(m_knockVelocity, Vector3.zero, 5 * Time.deltaTime);
        }
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

        // Rolling process
        RollUpdate();
    }

    private void RollUpdate()
    {
        if (m_isRolling)
        {
            // Move player in stored direction while roll is active
            m_characterController.Move(m_lastMoveDirection.normalized * m_rollSpeed * Time.fixedDeltaTime 
                + transform.up * m_yVelocity * Time.fixedDeltaTime);
            RotateToFaceDirection(new Vector3(m_lastMoveDirection.x, 0, m_lastMoveDirection.z));
        }
    }

    public void Move(Vector2 _move, bool _jump, bool _roll)
    {
        if (m_isRolling || m_knockedDown || m_stagger)
            return;

        // Jump
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

        // If player is trying to roll and can
        if (!_jump && m_grounded && _roll && normalizedMove.magnitude >= 0.1f && m_playerController.m_playerResources.m_stamina > 0.0f)
        {
            // Subtract stamina
            m_playerController.m_playerResources.ChangeStamina(-30.0f);
            m_isRolling = true;

            // Apply adrenaline if roll timed correctly
            if(o_adrenalineProvider != null)
            {
                m_playerController.m_playerResources.ChangeAdrenaline(100 * o_adrenalineProvider.m_value);

                // Slow motion calculation (Pretty terrible honestly, would not recommend)
                //GameManager.instance.SlowTime(0.75f * o_adrenalineProvider.m_value);

                o_adrenalineProvider = null;
            }

            m_lastMoveDirection = normalizedMove;
            // Play animation
            m_playerController.m_animator.SetTrigger("Roll");
        }
        else
        {
            m_characterController.Move(normalizedMove * m_moveSpeed * Time.deltaTime // Movement
                + transform.up * m_yVelocity * Time.deltaTime); // Jump
        }

        if (m_playerController.m_cameraController.m_selectedTarget == null) // If no target, rotate in moving direction
        {
            RotateToFaceDirection(new Vector3(normalizedMove.x, 0, normalizedMove.z));
        }
        else // If has target, rotate in direction of target.
        {
            Vector3 direction = m_playerController.m_cameraController.m_selectedTarget.transform.position - transform.position;
            RotateToFaceDirection(new Vector3(direction.x, 0, direction.z));
        }
    }

    private void RotateToFaceDirection(Vector3 _direction)
    {
        // Rotate player model
        if (_direction.magnitude >= 0.1f && m_playerModel != null)
        {
            float targetAngle = Mathf.Atan2(_direction.normalized.x, _direction.normalized.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(m_playerModel.transform.eulerAngles.y, targetAngle, ref m_turnSmoothVelocity, m_turnSmoothTime);
            m_playerModel.transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }
    }

    public void StopRoll()
    {
        m_isRolling = false;
    }
    public void Knockdown(Vector3 _direction, float _power, bool _ignoreInv = false)
    {
        if (!_ignoreInv && m_isRolling)
            return;

        m_playerController.m_animator.SetTrigger("Knockdown");
        m_knockVelocity = _direction.normalized * _power;
        m_knockedDown = true;
        m_stagger = false;
        m_isRolling = false;
    }
    public void StopKnockdown()
    {
        m_knockedDown = false;
        m_stagger = false;
        m_knockVelocity = Vector3.zero;
    }
    public void Stagger(float _duration)
    {
        m_playerController.m_animator.SetFloat("StaggerDuration", 1.0f/ _duration);
        m_playerController.m_animator.SetTrigger("Stagger");
        m_stagger = true;
        m_knockedDown = false;
        m_isRolling = false;
    }
    public void StopStagger()
    {
        m_stagger = false;
        m_knockedDown = false;
    }
    private void StopAllStuns()
    {
        m_isRolling = false;
        m_stagger = false;
        m_knockedDown = false;
    }
    public void SetPotentialAdrenaline(PlayerAdrenalineProvider _provider)
    {
        if(o_adrenalineProvider == _provider)
        {
            return;
        }

        //if either is null, then they will represent the maximum value of a float
        float currentDist = (o_adrenalineProvider != null) ? Vector3.Distance(transform.position, o_adrenalineProvider.transform.position) : float.MaxValue; 
        float newDist = (_provider != null) ? Vector3.Distance(transform.position, _provider.transform.position) : float.MaxValue;

        if(newDist < currentDist) 
        {
            //New provider! (Used to determine who has priority when the player is dodging).
            o_adrenalineProvider = _provider;
        }
    }
}
