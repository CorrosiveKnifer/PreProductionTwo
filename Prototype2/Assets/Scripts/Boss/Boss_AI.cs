using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Michael Jordan
public class Boss_AI : MonoBehaviour
{
    public bool m_roarOnAwake = true;
    public string m_behavour;

    enum AI_BEHAVOUR_STATE
    {
        WAITING, //Wait for no reason
        CLOSE_DISTANCE, //Travel towards the player
        MELEE_ATTACK, //Start a melee attack.
        RANGE_ATTACK, //Start a range attack.
    }
    [Header("Load in Stats")]
    [Range(0, 100, order = 0)]
    public int m_startingHealthPercentage = 100;
    
    [Header("Current Stats")]
    public float m_currentHealth;
    public float m_currentPatiences;
    public float m_meleeRange;
    private float m_damageMemory = 0.0f;

    [Header("Externals")]
    [SerializeField] private BossData m_myData;
    [SerializeField] private Transform m_projSpawn;
    [SerializeField] private GameObject m_projPrefab;
    [SerializeField] private GameObject m_aoePrefab;
    [SerializeField] private CapsuleCollider m_kickCollider;
    private AI_BEHAVOUR_STATE m_myCurrentState;
    private GameObject m_player;
    private GameObject m_aoeVFX; 

    //Boss Compoments
    private Boss_Movement m_myMovement;
    private Boss_Camera m_myCamera;
    private Boss_Animator m_myAnimator;
    private Boss_Weapon m_myWeapon;
    private Boss_Kick m_myKick;
    private UI_Bar m_myHealthBar;

    // Start is called before the first frame update
    void Start()
    {
        m_meleeRange = Mathf.Max(m_myData.meleeAttackRange, m_myData.aoeRadius);
        m_currentHealth = m_myData.health * (m_startingHealthPercentage * 0.01f);
        m_currentPatiences = m_myData.patience;
        m_player = GameObject.FindGameObjectWithTag("Player");

        m_myMovement = GetComponentInChildren<Boss_Movement>();

        m_myCamera = GetComponentInChildren<Boss_Camera>();

        m_myAnimator = GetComponentInChildren<Boss_Animator>();

        m_myKick = GetComponentInChildren<Boss_Kick>();

        m_myWeapon = GetComponentInChildren<Boss_Weapon>();
        m_myWeapon.m_weaponDamage = m_myData.weaponDamage;

        m_myHealthBar = HUDManager.instance.GetElement<UI_Bar>("BossHealthBar");

        if (m_roarOnAwake)
        {
            m_behavour = "Waiting";
            m_myCurrentState = AI_BEHAVOUR_STATE.WAITING;
            if (CameraManager.instance != null)
                CameraManager.instance.PlayDirector("BossRoar");
        }
        Physics.IgnoreLayerCollision(2, 10);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(m_myMovement.GetDirection(m_player.transform.position, Space.Self));
        BehavourUpdate();

        if(m_myAnimator != null)
            AnimationUpdate();

        m_myHealthBar.SetValue(m_currentHealth/m_myData.health);
        m_damageMemory = Mathf.Clamp(m_damageMemory - Time.deltaTime, 0.0f, float.MaxValue);

        Debug.DrawRay(transform.position, transform.forward, Color.green);
    }

    public void AnimationUpdate()
    {
        //transform.rotation = Quaternion.LookRotation(transform.position - m_player.transform.position, Vector3.up);
        m_myAnimator.direction = m_myMovement.GetDirection(m_player.transform.position, Space.Self);
        
    }

    public void BehavourUpdate()
    {
        switch (m_myCurrentState)
        {
            case AI_BEHAVOUR_STATE.WAITING:
                if(CameraManager.instance == null)
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                }
                else if (!CameraManager.instance.IsDirectorPlaying("BossRoar"))
                {
                    m_myMovement.Stop();
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                }
                break;
            case AI_BEHAVOUR_STATE.CLOSE_DISTANCE:
                MoveState();
                
                if (m_myAnimator.AnimMutex)
                {
                    return;
                }
                if (m_myMovement.IsNearTargetLocation(m_meleeRange))
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.MELEE_ATTACK);
                }
                else
                {
                    if (m_myMovement.IsNearTargetLocation(m_meleeRange * 1.5f))
                    {
                        m_currentPatiences -= Time.deltaTime * 0.75f;
                    }
                    else
                    {
                        m_currentPatiences -= Time.deltaTime;
                    }
                }
                if (m_currentPatiences <= 0)
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.RANGE_ATTACK);
                }
                break;
            case AI_BEHAVOUR_STATE.MELEE_ATTACK:
                if (!m_myAnimator.AnimMutex)
                {
                    MeleeState();
                }   
                break;
            case AI_BEHAVOUR_STATE.RANGE_ATTACK:
                if (!m_myAnimator.AnimMutex)
                {
                    m_myAnimator.IsRanged = true;
                }
                TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                break;
            default:
                break;
        }
    }

    //Function to move the boss
    public void MoveState()
    {
        if (m_myAnimator.AnimMutex || m_myAnimator.IsTurn)
        {
            m_myMovement.Stop();
            return;
        }

        Quaternion target = Quaternion.LookRotation(m_myMovement.GetDirection(m_player.transform.position, Space.World));
        float angle = m_myMovement.GetAngle(target);
        if (angle > 130 || angle < -130)
        {
            m_myMovement.Stop();
            m_myAnimator.IsTurn = true;
        }
        else
        {
            m_myMovement.RotateTowards(target);
            m_myMovement.SetTargetLocation(m_player.transform.position);
        }
    }

    //Function to handle if there is melee action
    public void MeleeState()
    {
        if (Vector3.Distance(m_player.transform.position, transform.position) < m_myData.aoeRadius * 0.95f)
        {
            //m_myMovement.RotateTowards(Quaternion.LookRotation(m_myMovement.GetDirection(m_player.transform.position, Space.World)));

            //If the player is infront of the boss? 
            if (m_myMovement.GetDirection(m_player.transform.position, Space.Self).z >= 0 && m_myMovement.IsNearTargetLocation(m_myData.meleeAttackRange))
            {
                m_myMovement.Stop();
                if (m_myMovement.IsNearTargetLocation(m_myData.meleeAttackRange) || m_myKick.isPlayerWithin)
                {
                    if (m_myKick.isPlayerWithin && !(Vector3.Distance(m_player.transform.position, transform.position) <= m_myData.meleeAttackRange))
                    {
                        //Perform kick
                        if(Random.Range(0, 1000) <= 300)
                        {
                            m_myAnimator.IsKick = true;
                            TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                        }
                        else if(Vector3.Distance(m_player.transform.position, transform.position) <= m_myData.meleeAttackRange)
                        {
                            m_myAnimator.IsMelee = true;
                            TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                        }
                        else
                        {
                            MoveState();
                        }
                    }
                    else if(m_myMovement.IsNearTargetLocation(m_myData.meleeAttackRange))
                    {
                        m_myAnimator.IsMelee = true;
                        TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                    }
                    else
                    {
                        MoveState();
                    }
                }
            }
            else if(m_myMovement.GetDirection(m_player.transform.position, Space.Self).z < 0)
            {
                m_myMovement.Stop();
                m_myAnimator.IsAOE = true;
                TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
            }
            else
            {
                MoveState();
            }
        }
        else if (m_myMovement.IsNearTargetLocation(m_meleeRange))
        {
            MoveState();
        }
        else
        {
            TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
        }
    }

    public void CreateProjectile()
    {
        Vector3 forward = (m_player.transform.position - m_projSpawn.position).normalized;
        Rigidbody proj = GameObject.Instantiate(m_projPrefab, m_projSpawn.position, Quaternion.LookRotation(forward, Vector3.up)).GetComponent<Rigidbody>();
        proj.AddForce(forward * 50.0f, ForceMode.Impulse);
        Physics.IgnoreCollision(proj.GetComponent<Collider>(), GetComponent<Collider>());
        Boss_Projectile boss_proj = proj.GetComponent<Boss_Projectile>();
        boss_proj.m_sender = transform;
        boss_proj.m_target = m_player;
        proj.gameObject.SetActive(true);
    }

    public void CreateAOEPrefab()
    {
        m_aoeVFX = GameObject.Instantiate(m_aoePrefab, transform);
    }

    private void TransitionBehavourTo(AI_BEHAVOUR_STATE nextState)
    {
        if (m_myCurrentState == nextState)
            return;

        switch (nextState)
        {
            case AI_BEHAVOUR_STATE.WAITING:
                m_behavour = "Waiting";
                break;
            case AI_BEHAVOUR_STATE.CLOSE_DISTANCE:
                m_behavour = "Closing Distance";
                m_currentPatiences = m_myData.patience;
                break;
            case AI_BEHAVOUR_STATE.RANGE_ATTACK:
                m_behavour = "Attacking (Range)";
                m_myMovement.Stop();
                break;
            case AI_BEHAVOUR_STATE.MELEE_ATTACK:
                m_behavour = "Attacking (Melee)";
                break;
            default:
                Debug.LogError($"State is not supported {nextState}.");
                break;
        }

        m_myCurrentState = nextState;
    }

    public void DealDamage(float damage)
    {
        float damageMod = 1.0f - Mathf.Log(m_myData.resistance)/2 * m_myData.resistance/100.0f;
        damageMod = Mathf.Clamp(damageMod, 0.0f, 1.0f);
        m_currentHealth -= damage * damageMod;

        //Deal with death

        m_damageMemory += 3.0f;
        m_myMovement.SetStearModifier(5.0f);
    }

    public void ApplyAOE()
    {
        if(Vector3.Distance(m_player.transform.position, transform.position) < m_myData.aoeRadius)
        {
            Vector3 direction = (m_player.transform.position - transform.position);
            direction.y = 0;

            //AOE damage
            m_player.GetComponent<PlayerController>().Damage(m_myData.aoeDamage);
            m_player.GetComponent<PlayerMovement>().Knockdown(direction.normalized, 50.0f);
            m_aoeVFX.transform.parent = null;
        }
    }
    public void ApplyKickAction()
    {
        Vector3 direction = transform.forward;
        direction.y = 0;

        //Kick damage
        if(m_myKick.isPlayerWithin)
        {
            m_player.GetComponent<PlayerController>().Damage(m_myData.kickDamage);
            m_player.GetComponent<PlayerMovement>().Knockdown(direction.normalized, 60.0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, m_myData.meleeAttackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_myData.aoeRadius);
    }
}
