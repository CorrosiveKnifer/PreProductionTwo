using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Weapon : MonoBehaviour
{
    [Header("Weapon Materials")]
    public Material m_inactive;
    public Material m_active;
    public SkinnedMeshRenderer m_weaponMesh;

    public bool m_isLive = false;
    public float m_weaponDamage;
    public List<GameObject> m_damaged;
    public float m_modifier = 1.0f;
    public Transform parentTransform;

    protected ParticleSystem particles;
    // Start is called before the first frame update
    void Start()
    {
        m_damaged = new List<GameObject>();
        particles = GetComponentInChildren<ParticleSystem>();
        SetWeaponStatus(false);
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
                Vector3 direction = other.transform.position - parentTransform.position;
                direction.y = 0;
                other.GetComponent<PlayerMovement>().Knockback(direction.normalized, 10.0f);
            }
            else if(other.tag == "Adrenaline Shadow")
            {
                other.GetComponent<AdrenalineProvider>().GiveAdrenaline();
            }

            if(other.gameObject.layer == LayerMask.NameToLayer("Attackable"))
            {
                other.GetComponent<Destructible>().CrackObject();
            }
            m_damaged.Add(other.gameObject);
        }
    }

    public void SetWeaponDisplay(bool status)
    {
        if (status)
            m_weaponMesh.material = m_active;
        else
            m_weaponMesh.material = m_inactive;
    }

    public void SetWeaponStatus(bool status)
    {
        m_isLive = status;

        var emission = particles.emission;
        emission.enabled = status;
    }
}
