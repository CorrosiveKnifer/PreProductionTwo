using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement m_playerMovement;
    private CameraController m_cameraController;
    private bool m_isJumping = false;

    // Start is called before the first frame update
    void Start()
    {
        m_playerMovement = GetComponent<PlayerMovement>();
        m_cameraController = GetComponent<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        m_playerMovement.Move(GetPlayerMovementVector(), InputManager.instance.IsGamepadButtonDown(ButtonType.SOUTH, InputManager.instance.GetAnyGamePad()));
        m_cameraController.MoveCamera(GetCameraMovementVector());
    }
    private Vector2 GetPlayerMovementVector()
    {
        int gamepadID = InputManager.instance.GetAnyGamePad();
        if (gamepadID != -1)
        {
            return InputManager.instance.GetGamepadStick(StickType.LEFT, gamepadID);
        }
        else
        {
            Debug.LogWarning("No controller detected. Try shaking the monitor until it works.");
            return new Vector2(0, 0);
        }
    }
    private Vector2 GetCameraMovementVector()
    {
        int gamepadID = InputManager.instance.GetAnyGamePad();
        if (gamepadID != -1)
        {
            return InputManager.instance.GetGamepadStick(StickType.RIGHT, gamepadID);
        }
        else
        {
            Debug.LogWarning("No controller detected. Try shaking the monitor until it works.");
            return new Vector2(0, 0);
        }
    }
}
