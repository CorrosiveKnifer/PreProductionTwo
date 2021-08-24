using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Animator : MonoBehaviour
{
    public Vector3 direction;
    public bool IsMelee;
    public bool IsAOE;
    public bool IsRanged;
    public bool IsKick;
    public bool IsTurn { get { return m_animator.GetBool("TurnAround"); } set { m_animator.SetBool("TurnAround", value); } }
    public bool IsDead;

    private Animator m_animator;
    public bool AnimMutex
    {
        get
        {
            return m_animator.GetBool("Mutex");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsTurn)
        {
            transform.localRotation = Quaternion.identity;
        }
        m_animator.SetFloat("VelocityVertical", direction.z);
        m_animator.SetFloat("VelocityHorizontal", -direction.x);

        if(IsMelee)
        {
            IsMelee = false;
            m_animator.SetTrigger("MeleeAttack");
        }

        if (IsKick)
        {
            IsKick = false;
            m_animator.SetTrigger("KickAttack");
        }

        if (IsDead)
        {
            IsDead = false;
            m_animator.SetTrigger("IsDead");
        }

        if (IsAOE)
        {
            IsAOE = false;
            m_animator.SetTrigger("AOEAttack");
        }

        if (IsRanged)
        {
            IsRanged = false;
            m_animator.SetTrigger("RangeAttack");
        }
    }
}
