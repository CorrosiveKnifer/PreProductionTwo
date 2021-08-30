using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public float m_health { get; private set; } = 100.0f;
    public float m_stamina { get; private set; } = 100.0f;
    public float m_adrenaline { get; private set; } = 0.0f;

    private PlayerController m_playerController;
    float m_staminaRechargeTimer = 0.0f;
    public float m_staminaRechargeDelay = 0.8f;
    public float m_rechargeRate = 40.0f;
    public float m_adrenalineDecayRate = 3.0f;
    public bool m_dead { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        m_playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_dead)
        {
            if (m_staminaRechargeTimer > 0.0f) // Delay before stamina regens again
            {
                m_staminaRechargeTimer -= Time.deltaTime * m_playerController.m_adrenalineMult;
            }
            else // Regenerate stamina
            {
                ChangeStamina(m_rechargeRate * Time.deltaTime * m_playerController.m_adrenalineMult);
            }
        }
        ChangeAdrenaline(-Time.deltaTime * m_adrenalineDecayRate);
    }

    public void ChangeHealth(float _amount)
    {
        if (_amount > 0) // Gain
        {
            // Remove adrenaline as price for healing
            m_adrenaline -= _amount;

            if (m_adrenaline < 0)
                m_health += _amount + m_adrenaline;
            else
                m_health += _amount;


            m_adrenaline = Mathf.Clamp(m_adrenaline, 0.0f, 100.0f);
        }
        else // Drain
        {
            m_health += _amount;
            if (m_health <= 0.0f && !m_dead)
            {
                // Kill
                m_dead = true;
                m_playerController.KillPlayer();
            }
        }
        m_health = Mathf.Clamp(m_health, 0.0f, 100.0f);
    }

    public void ChangeStamina(float _amount)
    {
        m_stamina += _amount;
        if (_amount > 0) // Gain
        {

        }
        else // Drain
        {
            // If staina goes below 0 make longer delay before recharging.
            m_staminaRechargeTimer = m_staminaRechargeDelay * (m_stamina < 0.0f ? 2 : 1);
        }
        m_stamina = Mathf.Clamp(m_stamina, 0.0f, 100.0f);
    }

    public void ChangeAdrenaline(float _amount)
    {
        m_adrenaline += _amount;
        if (_amount > 0) // Gain
        {

        }
        else // Drain
        {

        }
        m_adrenaline = Mathf.Clamp(m_adrenaline, 0.0f, 100.0f);
    }
}
