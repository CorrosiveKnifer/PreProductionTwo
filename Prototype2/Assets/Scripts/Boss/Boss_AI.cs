using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Michael Jordan
public class Boss_AI : MonoBehaviour
{
    public bool m_waitOnAwake = true;
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
    public bool m_isDead = false;
    public float m_cancelAngle = 90.0f;
    public float m_kickCD = 0;
    public float m_aoeCD = 0;
    public float m_tripleCD = 0;
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
    private bool m_canCancel = false;

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
        m_myWeapon.m_modifier = m_myData.weaponAdrenalineModifier;
        m_myHealthBar = HUDManager.instance.GetElement<UI_Bar>("BossHealthBar");

        m_aoeCD = 10f;
        m_tripleCD = 5f;
        if (m_waitOnAwake)
        {
            m_behavour = "Waiting";
            TransitionBehavourTo(AI_BEHAVOUR_STATE.WAITING);
        }
        Physics.IgnoreLayerCollision(2, 10);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(m_myMovement.GetDirection(m_player.transform.position, Space.Self));

        if(!m_isDead)
        {
            BehavourUpdate();

            if (m_myAnimator != null)
                AnimationUpdate();

            m_myHealthBar.SetValue(m_currentHealth / m_myData.health);
            m_damageMemory = Mathf.Clamp(m_damageMemory - Time.deltaTime, 0.0f, float.MaxValue);

            m_aoeCD = Mathf.Clamp(m_aoeCD - Time.deltaTime, 0.0f, m_myData.aoeMaxCooldown);
            m_kickCD = Mathf.Clamp(m_kickCD - Time.deltaTime, 0.0f, m_myData.kickMaxCooldown);
            m_tripleCD = Mathf.Clamp(m_tripleCD - Time.deltaTime, 0.0f, m_myData.kickMaxCooldown);
        }

        Debug.DrawRay(transform.position, transform.forward, Color.green);
    }
    public void WakeUp()
    {
        if(m_waitOnAwake)
        {
            TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
            m_waitOnAwake = false;
            m_aoeCD = 10f;
            m_tripleCD = 5f;
        }
    }

    public void AnimationUpdate()
    {
        //transform.rotation = Quaternion.LookRotation(transform.position - m_player.transform.position, Vector3.up);
        if(m_myCurrentState == AI_BEHAVOUR_STATE.WAITING)
        {
            m_myAnimator.direction = Vector3.zero;
            return;
        }
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
                break;
            case AI_BEHAVOUR_STATE.CLOSE_DISTANCE:
                MoveState();
                
                if (m_myAnimator.AnimMutex)
                {
                    return;
                }
                if (Vector3.Distance(m_player.transform.position, transform.position) < m_meleeRange)
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.MELEE_ATTACK);
                }
                else
                {
                    if (Vector3.Distance(m_player.transform.position, transform.position) < m_meleeRange * 1.5f)
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
                else
                {
                    //Quaternion target = Quaternion.LookRotation(m_myMovement.GetDirection(m_player.transform.position, Space.World));
                    //float angle = m_myMovement.GetAngle(target);
                    //if (m_canCancel && Mathf.Abs(angle) > Mathf.Abs(m_cancelAngle))
                    //{
                    //    m_myAnimator.CancelAnimation();
                    //}
                }
                break;
            case AI_BEHAVOUR_STATE.RANGE_ATTACK:
                if (!m_myAnimator.AnimMutex)
                {
                    Quaternion target = Quaternion.LookRotation(m_myMovement.GetDirection(m_player.transform.position, Space.World));
                    float angle = m_myMovement.GetAngle(target);
                    if (angle <= 65 || angle >= -65)
                    {
                        m_myAnimator.IsRanged = true;
                        m_canCancel = true;
                    }
                    else
                    {
                        m_myMovement.SetStearModifier(5.0f);
                    }
                }
                else
                {
                    //Quaternion target = Quaternion.LookRotation(m_myMovement.GetDirection(m_player.transform.position, Space.World));
                    //float angle = m_myMovement.GetAngle(target);
                    //if (m_myAnimator.CanIRotateTheCharacter() && m_canCancel && (angle > m_cancelAngle || angle < -m_cancelAngle))
                    //{
                    //    m_myAnimator.CancelAnimation();
                    //}
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
        Quaternion target = Quaternion.LookRotation(m_myMovement.GetDirection(m_player.transform.position, Space.World));
        float angle = m_myMovement.GetAngle(target);

        if (m_myAnimator.AnimMutex || m_myAnimator.IsTurn)
        {
            m_myMovement.Stop();
            //if (m_myAnimator.CanIRotateTheCharacter() && m_canCancel && (angle > m_cancelAngle || angle < -m_cancelAngle))
            //{
            //    m_myAnimator.CancelAnimation();
            //}
            return;
        }
        
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
        Quaternion target = Quaternion.LookRotation(m_myMovement.GetDirection(m_player.transform.position, Space.World));
        float angle = m_myMovement.GetAngle(target);
        float distance = Vector3.Distance(m_player.transform.position, transform.position);
        bool isWithinMeleeRange = distance < m_meleeRange; //With AOE
        bool isWithinSlashRange = distance < m_myData.meleeAttackRange; //Also within melee slash range.
        bool isWithinKickCollider = m_myKick.isPlayerWithin;
        bool isBehindTheBoss = angle > 90 || angle < -90;
        bool isAbleToKick = m_kickCD <= 0;
        bool isAbleToAOE = m_aoeCD <= 0;
        bool isAbleToTriple = m_tripleCD <= 0;

        if(isWithinMeleeRange)
        {
            if(isWithinKickCollider && isAbleToTriple && !CheckForwardForEnvironment())
            {
                m_myMovement.Stop();
                m_canCancel = true;
                m_tripleCD += m_myData.tripleMaxCooldown;
                m_myAnimator.IsMeleeTriple = true;
                TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                return;
            }
            else if(isWithinKickCollider && isAbleToKick)
            {
                m_myMovement.Stop();
                m_canCancel = true;
                m_kickCD += m_myData.kickMaxCooldown;
                m_myAnimator.IsKick = true;
                TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                return;
            }
            else if(isWithinSlashRange && !isBehindTheBoss)
            {
                m_myMovement.Stop();
                m_canCancel = true;
                m_myAnimator.IsMelee = true;
                m_player.GetComponent<PlayerMovement>().SetPotentialAdrenaline(m_myWeapon);
                TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                return;
            }
            else if(isAbleToAOE)
            {
                m_myMovement.Stop();
                m_aoeCD += m_myData.aoeMaxCooldown;
                m_canCancel = false;
                m_myAnimator.IsAOE = true;
                TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                return;
            }
            else
            {
                m_myMovement.SetStearModifier(1.5f);
                MoveState();
                return;
            }
        }
        //Else just move
        TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
    }

    public void CreateProjectile()
    {
        Vector3 forward = (m_player.transform.position - m_projSpawn.position).normalized;
        Rigidbody proj = GameObject.Instantiate(m_projPrefab, m_projSpawn.position, Quaternion.LookRotation(forward, Vector3.up)).GetComponent<Rigidbody>();
        proj.AddForce(forward * m_myData.projectileForce, ForceMode.Impulse);
        Physics.IgnoreCollision(proj.GetComponent<Collider>(), GetComponent<Collider>());
        Boss_Projectile boss_proj = proj.GetComponent<Boss_Projectile>();
        boss_proj.m_sender = transform;
        boss_proj.m_target = m_player;
        proj.gameObject.SetActive(true);
    }

    public void CreateAOEPrefab()
    {
        m_aoeVFX = GameObject.Instantiate(m_aoePrefab, transform);
        AOEAdrenalineProvider provider = m_aoeVFX.GetComponent<AOEAdrenalineProvider>();
        provider.m_player = m_player;
        provider.m_modifier = m_myData.aoeModifier;
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
        if (m_isDead)
            return;

        float damageMod = 1.0f - Mathf.Log(m_myData.resistance)/2 * m_myData.resistance/100.0f;
        damageMod = Mathf.Clamp(damageMod, 0.0f, 1.0f);
        m_currentHealth -= damage * damageMod;

        //Deal with death
        if(m_currentHealth <= 0)
        {
            m_isDead = true;
            m_myAnimator.IsDead = true;
            GetComponent<Collider>().enabled = false;
        }
        m_damageMemory += 3.0f;
        m_myMovement.SetStearModifier(5.0f);
    }
    public void StartAOEWindow(float window)
    {
        m_aoeVFX.GetComponent<AOEAdrenalineProvider>().StartAdrenalineWindow(window);
    }
    public void ApplyAOE()
    {
        if(Vector3.Distance(m_player.transform.position, transform.position) < m_myData.aoeRadius * 1.5f)
        {
            Vector3 direction = (m_player.transform.position - transform.position);
            direction.y = 0;

            //AOE damage
            m_player.GetComponent<PlayerMovement>().Knockdown(direction.normalized, m_myData.aoeForce);
            m_player.GetComponent<PlayerController>().Damage(m_myData.aoeDamage);
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
            m_player.GetComponent<PlayerMovement>().Knockdown(direction.normalized, m_myData.kickForce);
            m_player.GetComponent<PlayerController>().Damage(m_myData.kickDamage);
        }
    }

    public void ShakePlayerCam(float _intensity)
    {
        m_player.GetComponent<PlayerController>().m_cameraController.ScreenShake(0.5f, _intensity, 1.0f);
    }

    public bool CheckForwardForEnvironment()
    {
        Vector3 start = transform.position + Vector3.up * 1.6f;
        Debug.DrawRay(start, transform.forward * 5.0f, Color.red);
        RaycastHit[] hits = Physics.SphereCastAll(start, 1.25f, transform.forward, 6.0f);

        foreach (var hit in hits)
        {
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, m_myData.meleeAttackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_myData.aoeRadius);
    }
}
