using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Collider m_weaponCollider;
    public PlayerMovement m_playerMovement { get; private set; }
    public PlayerResources m_playerResources { get; private set; }
    public CameraController m_cameraController { get; private set; }
    public Animator m_animator { get; private set; }
    private bool m_damageActive = false;

    public float m_adrenalineMult { get; private set; } = 1.0f;
    public float m_effectsPercentage { get; private set; } = 0.0f;

    List<Collider> m_hitList = new List<Collider>();
    
    public Vector3 m_lastWeaponPosition;

    public bool m_functionalityEnabled = true;

    public bool m_swinging = false;
    public int m_nextSwing = 0;
    private float m_resetSwingDelay = 0.1f;
    private float m_resetSwingTimer = 0.0f;
    private float m_damage = 100.0f;

    [Header("Keyboard Things")]
    private Vector3 m_currentVelocity = Vector3.zero;
    private Vector3 m_movementVelocity = Vector3.zero;

    [Header("VFX")]
    public ParticleSystem m_runningDust;
    public GameObject sparkPrefab;
    public ParticleSystem m_swordTrail;
    public ParticleSystem[] m_eyeTrail;
    public LayerMask m_vfxDetectionLayers;

    private void Awake()
    {
        Physics.IgnoreLayerCollision(8, 13);
        Physics.IgnoreLayerCollision(13, 13);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_playerResources = GetComponent<PlayerResources>();
        m_playerMovement = GetComponent<PlayerMovement>();
        m_cameraController = GetComponent<CameraController>();
        m_animator = GetComponentInChildren<Animator>();
        m_lastWeaponPosition = m_weaponCollider.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        int gamepadID = InputManager.instance.GetAnyGamePad();
        if (!m_playerResources.m_dead && m_functionalityEnabled)
        {
            if (m_animator.GetInteger("NextSwing") == 0)
            {
                Vector2 movement = GetPlayerMovementVector();

                // Get movement inputs and apply
                m_playerMovement.Move(movement, // Run
                    InputManager.instance.IsGamepadButtonDown(ButtonType.SOUTH, gamepadID), // Jump
                    InputManager.instance.IsGamepadButtonDown(ButtonType.EAST, gamepadID) || InputManager.instance.IsKeyDown(KeyType.SPACE)); // Roll

                var em = m_runningDust.emission;
                em.enabled = (m_animator.GetFloat("VelocityVertical") > 0.5f);
            }
            // Roll
            if ((InputManager.instance.IsGamepadButtonDown(ButtonType.RB, gamepadID) || InputManager.instance.GetMouseDown(MouseButton.LEFT) 
                                                                                    || InputManager.instance.GetMouseDown(MouseButton.RIGHT))
                && !m_playerMovement.m_knockedDown 
                && !m_playerMovement.m_stagger 
                && !m_playerMovement.m_isRolling)
            {
                if (m_playerResources.m_stamina > 0.0f)
                {
                    SwingSword();
                    m_playerResources.ChangeStamina(-30.0f);
                }
            }
            if (InputManager.instance.IsKeyDown(KeyType.L) && !GameManager.instance.enableTimer)
            {
                m_playerResources.ChangeAdrenaline(100.0f);
            }
            if (InputManager.instance.IsKeyDown(KeyType.K) && !GameManager.instance.enableTimer)
            {
                Damage(100.0f);
            }
        }
        else
        {
            m_animator.SetFloat("VelocityHorizontal", 0.0f);
            m_animator.SetFloat("VelocityVertical", 0.0f);
            var em1 = m_runningDust.emission;
            em1.enabled = false;
        }


        var em2 = m_swordTrail.emission;
        em2.enabled = m_animator.GetBool("TrailActive");

        foreach (var trail in m_eyeTrail)
        {
            var trailMain = trail.main;
            Color newColor = trailMain.startColor.color;
            newColor.a = m_effectsPercentage;

            trailMain.startColor = new ParticleSystem.MinMaxGradient(newColor); 
        }


        // Get camera inputs and apply
        m_cameraController.MoveCamera(GetCameraMovementVector());

        // Lock on
        if (InputManager.instance.IsGamepadButtonDown(ButtonType.RS, gamepadID) || InputManager.instance.IsKeyDown(KeyType.Q))
        {
            m_cameraController.ToggleLockOn();
        }

        //// Debug inputs

        CalculateAdrenalineBoost();

    }
    public void SetDamageByAttackID(int _id)
    {
        switch (_id)
        {
            case 1:
                m_damage = 100.0f;
                break;
            case 2:
                m_damage = 110.0f;
                break;
            case 3:
                m_damage = 150.0f;
                break;
            default:
                m_damage = 100.0f;
                break;
        }
    }
    private void NextSwing()
    {
        m_animator.SetInteger("NextSwing", m_animator.GetInteger("NextSwing") + 1);
    }
    public void ResetSwings()
    {
        m_animator.SetInteger("NextSwing", m_nextSwing);
    }
    public void SetSwinging(bool _active)
    {
        m_swinging = _active;
    }

    public void CeaseSwing()
    {
        m_swinging = false;
        m_nextSwing = 0;
        m_resetSwingTimer = 0;
    }
    public void Damage(float _damage, bool _ignoreInv = false)
    {
        if (!_ignoreInv && m_playerMovement.m_isRolling)
            return;

        //if (!m_playerMovement.m_stagger)
        {
            m_playerResources.ChangeHealth(-_damage);
            m_playerMovement.Stagger(0.5f);
        }
    }

    public void Heal(float _heal)
    {
        if (m_playerResources.m_health < 100.0f)
        {
            m_playerResources.ChangeHealth(_heal);
        }
    }
    private void FixedUpdate()
    {
        if (m_damageActive) // Check if swing is active
            DamageDetection();
    }

    public void KillPlayer()
    {
        m_animator.SetTrigger("Die");
        m_animator.SetBool("IsDead", true);
        GetComponent<Player_AudioAgent>().PlayDeath();
        LevelLoader.instance.LoadNewLevel("MainGame", LevelLoader.Transition.YOUDIED);
    }

    private void CalculateAdrenalineBoost()
    {
        if (m_playerResources.m_adrenaline > 0.0f) // Check if player has adrenaline
        {
            m_adrenalineMult = 1.0f + m_playerResources.m_adrenaline / 100.0f;
            m_effectsPercentage = m_playerResources.m_adrenaline / 100.0f;
        }
        else
        {
            // Defaults
            m_adrenalineMult = 1.0f;
            m_effectsPercentage = 0.0f;
        }
        m_animator.SetFloat("AttackSpeed", m_adrenalineMult); // Set animation speed
    }

    private Vector2 GetPlayerMovementVector()
    {
        if (GameManager.instance.useGamepad)
        {
            int gamepadID = InputManager.instance.GetAnyGamePad();
            return InputManager.instance.GetGamepadStick(StickType.LEFT, gamepadID);
        }
        else
        {
            Vector2 movement = Vector2.zero;
            movement.x += (InputManager.instance.IsKeyPressed(KeyType.D) ? 1.0f : 0.0f);
            movement.x -= (InputManager.instance.IsKeyPressed(KeyType.A) ? 1.0f : 0.0f);
            movement.y += (InputManager.instance.IsKeyPressed(KeyType.W) ? 1.0f : 0.0f);
            movement.y -= (InputManager.instance.IsKeyPressed(KeyType.S) ? 1.0f : 0.0f);
            m_currentVelocity = Vector3.SmoothDamp(m_currentVelocity, movement, ref m_movementVelocity, 0.1f);
            return m_currentVelocity;
        }
    }
    private Vector2 GetCameraMovementVector()
    {
        if (GameManager.instance.useGamepad)
        {
            int gamepadID = InputManager.instance.GetAnyGamePad();
            return InputManager.instance.GetGamepadStick(StickType.RIGHT, gamepadID);
        }
        else
        {
            return InputManager.instance.GetMouseDelta() * -GameManager.m_sensitivity * 0.0001f;
        }
    }

    private void SwingSword()
    {
        Vector3 localPos = m_weaponCollider.transform.position - m_playerMovement.m_playerModel.transform.position;
        m_lastWeaponPosition = localPos;
        m_animator.SetTrigger("Swing");
        NextSwing();
    }

    private void DamageDetection()
    {
        // Find all colliders
        Collider[] colliders = FindObjectsOfType<Collider>();

        bool foundTarget = false;

        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == 9) // Check that it is attackable
            {
                if (m_weaponCollider.GetComponent<Collider>().bounds.Intersects(collider.bounds)) // If intersects with sword
                {
                    if (sparkPrefab != null)
                    {
                        Vector3 collisionPoint = collider.ClosestPointOnBounds(m_weaponCollider.transform.position);
                        Instantiate(sparkPrefab, collisionPoint, Quaternion.Euler(0, 0, 0));
                    }
                    if (!m_hitList.Contains(collider)) // If not already hit this attack
                    {
                        // Action here
                        Debug.Log("Bonk");
                        m_hitList.Add(collider);
                        foundTarget = true;
                        if (collider.GetComponent<Rigidbody>())
                        {
                            collider.GetComponent<Rigidbody>().AddForce(
                                (collider.transform.position - m_weaponCollider.transform.position).normalized * 10.0f, 
                                ForceMode.Impulse);
                        }
                        if (collider.GetComponent<Boss_AI>())
                        {
                            collider.GetComponent<Boss_AI>().DealDamage(m_damage * m_adrenalineMult);

                            GetComponent<Player_AudioAgent>().PlaySwordHit();
                            Heal(20.0f);
                        }
                        if (collider.GetComponent<Destructible>())
                        {
                            collider.GetComponent<Destructible>().CrackObject();
                        }
                    }
                }
            }
        }

        // Apply forces to nearby rigidbodies.
        Vector3 localPos = m_weaponCollider.transform.position - m_playerMovement.m_playerModel.transform.position;

        // If any target was hit, apply screen shake 
        if (foundTarget)
        {
            Vector3 direction = localPos - m_lastWeaponPosition;
            direction.y = 0.5f;
            if (m_effectsPercentage >= 0.3f)
            {
                // Find all rigid bodies
                Rigidbody[] rigidbodies = FindObjectsOfType<Rigidbody>();

                foreach (var item in rigidbodies)
                {
                    float distance = Vector3.Distance(m_weaponCollider.transform.position, item.transform.position);
                    if (distance < 5.0f)
                    {
                        float scale = 1.0f - (distance / 5.0f);
                        item.AddForce(direction.normalized * m_effectsPercentage * 6.0f * scale, ForceMode.Impulse);
                    }
                }
            }
            m_cameraController.ScreenShake(0.25f, 1.0f * m_effectsPercentage, 5.0f);
        }

        m_lastWeaponPosition = localPos;
    }

    public void ActivateDamage(bool _active)
    {
        m_damageActive = _active;
        if (!m_damageActive)
        {
            RefreshHitlist();
        }
    }

    public void RefreshHitlist()
    {
        m_hitList.Clear();
    }
}
