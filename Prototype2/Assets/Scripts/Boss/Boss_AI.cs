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

    [Header("Current Stats")]
    public float m_currentHealth;
    public float m_currentPatiences;

    [Header("Externals")]
    [SerializeField] private BossData m_myData;
    [SerializeField] private Transform m_projSpawn;
    [SerializeField] private GameObject m_projPrefab;

    private AI_BEHAVOUR_STATE m_myCurrentState;
    private GameObject m_player;
    private Boss_Movement m_myMovement;

    private UI_Bar m_myHealthBar;

    // Start is called before the first frame update
    void Start()
    {
        m_currentHealth = m_myData.health * 0.5f;
        m_currentPatiences = m_myData.patience;
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_myMovement = GetComponentInChildren<Boss_Movement>();

        m_myHealthBar = HUDManager.instance.GetElement<UI_Bar>("BossHealthBar");

        if (m_roarOnAwake)
        {
            m_behavour = "Waiting";
            m_myCurrentState = AI_BEHAVOUR_STATE.WAITING;
            CameraManager.instance.PlayDirector("BossRoar");
            //Play animation
        }
    }

    // Update is called once per frame
    void Update()
    {
        BehavourUpdate();
        m_myHealthBar.SetValue(m_currentHealth/m_myData.health);
    }

    public void BehavourUpdate()
    {
        switch (m_myCurrentState)
        {
            case AI_BEHAVOUR_STATE.WAITING:
                if (!CameraManager.instance.IsDirectorPlaying("BossRoar"))
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                }
                break;
            case AI_BEHAVOUR_STATE.CLOSE_DISTANCE:

                m_myMovement.SetTargetLocation(m_player.transform.position);

                if (m_myMovement.IsNearTargetLocation(m_myData.meleeAttackRange))
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.MELEE_ATTACK);
                }
                else
                {
                    if (m_myMovement.IsNearTargetLocation(m_myData.meleeAttackRange * 2.0f))
                    {
                        m_currentPatiences -= Time.deltaTime * 0.5f;
                    }
                    else
                    {
                        m_currentPatiences -= Time.deltaTime;
                    }

                    if(m_currentPatiences <= 0)
                    {
                        TransitionBehavourTo(AI_BEHAVOUR_STATE.RANGE_ATTACK);
                    }
                }
                break;
            case AI_BEHAVOUR_STATE.MELEE_ATTACK:
                if (m_myMovement.IsNearTargetLocation(m_myData.meleeAttackRange))
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);
                }
                break;
            case AI_BEHAVOUR_STATE.RANGE_ATTACK:
                CreateProjectile((m_player.transform.position - m_projSpawn.position).normalized);
                TransitionBehavourTo(AI_BEHAVOUR_STATE.CLOSE_DISTANCE);

                break;
            default:
                break;
        }
    }
    private void CreateProjectile(Vector3 forward)
    {
        Rigidbody proj = GameObject.Instantiate(m_projPrefab, m_projSpawn.position, Quaternion.LookRotation(forward, Vector3.up)).GetComponent<Rigidbody>();
        proj.AddForce(forward * 50.0f, ForceMode.Impulse);
        Physics.IgnoreCollision(proj.GetComponent<Collider>(), GetComponent<Collider>());
        Boss_Projectile boss_proj = proj.GetComponent<Boss_Projectile>();
        boss_proj.m_sender = transform;
        boss_proj.m_target = m_player;
        proj.gameObject.SetActive(true);
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
                break;
            case AI_BEHAVOUR_STATE.RANGE_ATTACK:
                m_behavour = "Attacking (Range)";
                m_myMovement.Stop();
                m_currentPatiences = m_myData.patience;
                break;
            case AI_BEHAVOUR_STATE.MELEE_ATTACK:
                m_behavour = "Attacking (Melee)";
                m_currentPatiences = m_myData.patience;
                break;
            default:
                Debug.LogError($"State is not supported {nextState}.");
                break;
        }

        m_myCurrentState = nextState;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, m_myData.meleeAttackRange);
    }
}
