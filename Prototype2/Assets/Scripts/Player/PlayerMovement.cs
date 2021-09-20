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

    public bool m_grounded = true;
    private float m_yVelocity = 0.0f;

    public float m_shadowDuration = 1.0f;
    public GameObject m_adrenShadowPrefab;

    public bool m_stagger { get; private set; } = false;
    public bool m_knockedDown { get; private set; } = false;
    private Vector3 m_knockVelocity = Vector3.zero;

    public bool m_isRolling { get; private set; } = false;
    private Vector3 m_lastMoveDirection;

    private Vector3 m_knockbackSourceDir;

    // Start is called before the first frame update
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_knockVelocity.magnitude > 0.05f)
        {
            m_characterController.Move(m_knockVelocity * Time.deltaTime
                + transform.up * m_yVelocity * Time.deltaTime);
            m_knockVelocity = Vector3.Lerp(m_knockVelocity, Vector3.zero, 5 * Time.deltaTime);
        }
        else if (m_knockedDown)
        {
            m_characterController.Move(transform.up * m_yVelocity * Time.deltaTime);
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

        // Object destruction
        DestroyNearbyObjects();
    }

    private void RollUpdate()
    {
        if (m_isRolling)
        {
            // Move player in stored direction while roll is active
            m_characterController.Move(m_lastMoveDirection.normalized * m_rollSpeed * Time.fixedDeltaTime 
                + transform.up * m_yVelocity * Time.fixedDeltaTime);
            RotateToFaceDirection(new Vector3(m_lastMoveDirection.x, 0, m_lastMoveDirection.z));
            m_playerController.CeaseSwing();
        }
    }
    private void DestroyNearbyObjects()
    {
        if (m_isRolling || m_knockedDown)
        {
            // Find all colliders
            Destructible[] destructibles = FindObjectsOfType<Destructible>();
            foreach (var destruct in destructibles)
            {
                if (Vector3.Distance(destruct.transform.position, transform.position) < 5.0f)
                {
                    if (Vector3.Distance(destruct.GetComponent<Collider>().ClosestPoint(transform.position), transform.position) < 1.0f)
                    {
                        destruct.CrackObject();
                    }
                }
            }
        }
    }
    public void Move(Vector2 _move, bool _jump, bool _roll)
    {
        _jump = false;

        if (m_knockedDown)
            RotateToFaceDirection(new Vector3(m_knockbackSourceDir.x, 0, m_knockbackSourceDir.z));

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
        if (!_jump && _roll && m_playerController.m_playerResources.m_stamina > 0.0f)
        {
            // Subtract stamina
            m_playerController.m_playerResources.ChangeStamina(-30.0f);
            m_isRolling = true;

            //Create Provider
            AdrenalineProvider provider = GameObject.Instantiate(m_adrenShadowPrefab, transform.position, Quaternion.identity).GetComponent<AdrenalineProvider>();
            provider.m_durationInSeconds = m_shadowDuration;
            provider.m_playerRef = this;

            if (normalizedMove.magnitude > 0)
            {
                m_lastMoveDirection = normalizedMove;
            }
            else
            {
                m_lastMoveDirection = m_playerModel.transform.forward;
            }

            // Play animation
            m_playerController.m_animator.SetTrigger("Roll");
            m_playerController.CeaseSwing();
        }
        else
        {
            Vector3 movementVector = normalizedMove * m_moveSpeed * Time.deltaTime // Movement
                + transform.up * m_yVelocity * Time.deltaTime; // Jump
            if (movementVector.magnitude != 0.0f)
                m_characterController.Move(movementVector); 

            // Movement
            Vector3 rotationVector = new Vector3(0, 0, 0);

            rotationVector += normalizedMove.z * m_playerModel.transform.right;
            rotationVector += normalizedMove.x * m_playerModel.transform.forward;

            m_playerController.m_animator.SetFloat("VelocityHorizontal", rotationVector.z);
            m_playerController.m_animator.SetFloat("VelocityVertical", rotationVector.x);
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

        m_playerController.CeaseSwing();
        m_playerController.m_animator.SetTrigger("Knockdown");
        m_knockVelocity = _direction.normalized * _power;
        m_knockbackSourceDir = -_direction.normalized;
        m_knockedDown = true;
        m_stagger = false;
        m_isRolling = false;
    }
    public void Knockback(Vector3 _direction, float _power, bool _ignoreInv = false)
    {
        if (!_ignoreInv && m_isRolling)
            return;

        m_knockVelocity = _direction.normalized * _power;
        m_knockbackSourceDir = -_direction.normalized;
    }
    public void StopKnockdown()
    {
        m_knockedDown = false;
        m_stagger = false;
        m_knockVelocity = Vector3.zero;
    }
    public void Stagger(float _duration)
    {
        if (m_knockedDown)
            return;

        m_playerController.CeaseSwing();
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
    public void GiveAdrenaline(float _val)
    {

        m_playerController.m_playerResources.ChangeAdrenaline(100 * _val);

        GetComponent<Player_AudioAgent>().PlayAdrenalineGain();

        //Slow mo
        GameManager.instance.SlowTime(0.4f, _val);
    }
}
