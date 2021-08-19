using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Collider m_weaponCollider;
    private PlayerMovement m_playerMovement;
    private PlayerResources m_playerResources;
    private CameraController m_cameraController;
    public Animator m_animator { get; private set; }
    private bool m_damageActive = false;

    public float m_adrenalineMult { get; private set; } = 1.0f;

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
        int gamepadID = InputManager.instance.GetAnyGamePad();
        m_playerMovement.Move(GetPlayerMovementVector(), // Run
            InputManager.instance.IsGamepadButtonDown(ButtonType.SOUTH, gamepadID), // Jump
            InputManager.instance.IsGamepadButtonDown(ButtonType.EAST, gamepadID)); // Roll

        m_cameraController.MoveCamera(GetCameraMovementVector());

        if (InputManager.instance.IsGamepadButtonDown(ButtonType.RB, gamepadID))
        {
            if (m_playerResources.m_stamina > 0.0f)
            {
                SwingSword();
                m_playerResources.ChangeStamina(-30.0f);
            }
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
        if (m_damageActive)
            DamageDetection();
    }
    
    private void CalculateAdrenalineBoost()
    {
        if (m_playerResources.m_adrenaline > 0.0f)
        {
            m_adrenalineMult = 1.0f + m_playerResources.m_adrenaline / 100.0f;
        }
        else
        {
            m_adrenalineMult = 1.0f;
        }
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
        Collider[] colliders = FindObjectsOfType<Collider>();

        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == 9 )
            {
                if (m_weaponCollider.GetComponent<Collider>().bounds.Intersects(collider.bounds))
                {
                    if (!m_hitList.Contains(collider))
                    {
                        Debug.Log("Bonk");
                        m_hitList.Add(collider);

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
