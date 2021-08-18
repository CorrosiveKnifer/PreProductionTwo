using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Collider m_weaponCollider;
    private PlayerMovement m_playerMovement;
    private CameraController m_cameraController;
    private Animator m_animator;
    private bool m_damageActive = false;

    List<Collider> m_hitList = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        m_playerMovement = GetComponent<PlayerMovement>();
        m_cameraController = GetComponent<CameraController>();
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        m_playerMovement.Move(GetPlayerMovementVector(), InputManager.instance.IsGamepadButtonDown(ButtonType.SOUTH, InputManager.instance.GetAnyGamePad()));
        m_cameraController.MoveCamera(GetCameraMovementVector());

        if (InputManager.instance.IsGamepadButtonDown(ButtonType.RB, InputManager.instance.GetAnyGamePad()))
        {
            SwingSword();
        }

    }

    private void FixedUpdate()
    {
        if (m_damageActive)
            DamageDetection();
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
