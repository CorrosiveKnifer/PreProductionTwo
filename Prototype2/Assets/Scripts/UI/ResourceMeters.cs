using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMeters : MonoBehaviour
{
    public PlayerResources m_resources;
    public Image m_healthBar;
    public Image m_staminaBar;
    public Image m_adrenalineBar;
    public TextMeshProUGUI m_adrenalineText;


    // Start is called before the first frame update
    void Start()
    {
        if (m_resources == null)
            m_resources = FindObjectOfType<PlayerResources>();
    }

    // Update is called once per frame
    void Update()
    {
        m_healthBar.fillAmount = m_resources.m_health / 100.0f;
        m_staminaBar.fillAmount = m_resources.m_stamina / 100.0f;
        m_adrenalineBar.fillAmount = m_resources.m_adrenaline / 100.0f;
        m_adrenalineText.alpha = m_resources.m_adrenaline / 100.0f;
        m_adrenalineText.text = Mathf.RoundToInt(m_resources.m_adrenaline).ToString();
    }
}
