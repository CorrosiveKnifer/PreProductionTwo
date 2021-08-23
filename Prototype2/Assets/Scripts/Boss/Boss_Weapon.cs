using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Weapon : MonoBehaviour
{
    public bool m_isLive = false;
    public float m_weaponDamage;
    public List<GameObject> m_damaged;

    // Start is called before the first frame update
    void Start()
    {
        m_damaged = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isLive)
            m_damaged.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(m_isLive)
        {
            //Deal damage
            if(other.tag == "Player")
            {
                other.GetComponent<PlayerController>().Damage(m_weaponDamage);
            }
            m_damaged.Add(other.gameObject);
        }
    }

    public void SetWeaponStatus(bool status)
    {
        m_isLive = status;
    }
}
