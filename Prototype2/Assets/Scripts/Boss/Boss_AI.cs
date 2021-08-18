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
    private float m_currentHealth;
    private float m_currentPatiences;

    [SerializeField] private BossData m_myData;
    private AI_BEHAVOUR_STATE m_myCurrentState;
    private GameObject m_player;
    private Boss_Movement m_myMovement;

    // Start is called before the first frame update
    void Start()
    {
        m_currentHealth = m_myData.health;
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_myMovement = GetComponentInChildren<Boss_Movement>();
        if (m_roarOnAwake)
        {
            CameraManager.instance.PlayDirector("BossRoar");
            //Play animation
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!CameraManager.instance.IsDirectorPlaying("BossRoar"))
        {
            m_myMovement.SetTargetLocation(m_player.transform.position);
        }
    }

    public void BehavourUpdate()
    {
        switch (m_myCurrentState)
        {
            case AI_BEHAVOUR_STATE.WAITING:

                break;
            case AI_BEHAVOUR_STATE.CLOSE_DISTANCE:
                if(m_myMovement.IsNearTargetLocation(m_player.transform.position, m_myData.meleeAttackRange))
                {
                    TransitionBehavourTo(AI_BEHAVOUR_STATE.MELEE_ATTACK);
                }
                break;
            case AI_BEHAVOUR_STATE.MELEE_ATTACK:

                break;
            case AI_BEHAVOUR_STATE.RANGE_ATTACK:

                break;
            default:
                break;
        }
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
        Gizmos.DrawWireSphere(transform.position, m_myData.m_meleeAttackRange);
    }
}
