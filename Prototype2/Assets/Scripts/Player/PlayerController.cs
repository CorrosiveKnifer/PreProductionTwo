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

    // Start is called before the first frame update
    void Start()
    {
        m_playerResources = GetComponent<PlayerResources>();
        m_playerMovement = GetComponent<PlayerMovement>();
        m_cameraController = GetComponent<CameraController>();
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get movement inputs and apply
        int gamepadID = InputManager.instance.GetAnyGamePad();
        m_playerMovement.Move(GetPlayerMovementVector(), // Run
            InputManager.instance.IsGamepadButtonDown(ButtonType.SOUTH, gamepadID), // Jump
            InputManager.instance.IsGamepadButtonDown(ButtonType.EAST, gamepadID)); // Roll

        // Get camera inputs and apply
        m_cameraController.MoveCamera(GetCameraMovementVector());

        // Roll
        if (InputManager.instance.IsGamepadButtonDown(ButtonType.RB, gamepadID))
        {
            if (m_playerResources.m_stamina > 0.0f)
            {
                SwingSword();
                m_playerResources.ChangeStamina(-30.0f);
            }
        }

        // Lock on
        if (InputManager.instance.IsGamepadButtonDown(ButtonType.LB, gamepadID))
        {
            m_cameraController.ToggleLockOn();
        }

        // Debug inputs
        if (InputManager.instance.IsKeyDown(KeyType.H))
        {
            m_playerResources.ChangeHealth(20.0f);
        }
        if (InputManager.instance.IsKeyDown(KeyType.D))
        {
            m_playerResources.ChangeHealth(-20.0f);
        }
        if (InputManager.instance.IsKeyDown(KeyType.A))
        {
            m_playerResources.ChangeAdrenaline(20.0f);
        }

        CalculateAdrenalineBoost();
    }

    private void FixedUpdate()
    {
        if (m_damageActive) // Check if swing is active
            DamageDetection();
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
        int gamepadID = InputManager.instance.GetAnyGamePad();
        return InputManager.instance.GetGamepadStick(StickType.LEFT, gamepadID);
    }
    private Vector2 GetCameraMovementVector()
    {
        int gamepadID = InputManager.instance.GetAnyGamePad();
        return InputManager.instance.GetGamepadStick(StickType.RIGHT, gamepadID);
    }

    private void SwingSword()
    {
        m_animator.SetTrigger("Swing");
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
                    }
                }
            }
        }

        // If any target was hit, apply screen shake 
        if (foundTarget)
            m_cameraController.ScreenShake(0.15f, 1.0f * m_effectsPercentage, 5.0f);
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
