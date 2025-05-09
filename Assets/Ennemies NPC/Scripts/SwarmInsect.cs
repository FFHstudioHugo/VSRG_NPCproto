using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class SwarmInsect : MonoBehaviour
{
    public enum InsectState { Circling, Attacking, Retreating }

    public float attackRange = 2f;
    public float attackCooldown = 3f;
    public float attackDuration = 1.2f;

    public SwarmManager swarmManager;
    public int index;
    public InsectState currentState = InsectState.Circling;


    private float stateStartTime;

    [SerializeField]
    private Animator animator;
    [SerializeField]
    private NavMeshAgent agent;

    private float cooldownTimer = 0f;
    private float attackTimer = 0f;
    private Vector3 retreatTarget;

    private Transform target;

    private int orbitDirection = 1;
    public int OrbitDirection => orbitDirection;
    public float orbitRadius = 10f;
    public float orbitSpeed = 10f;

    private float directionChangeTimer = 0f;
    private float nextDirectionChangeTime = 0f;
    private Vector3 CircleTargetPosition;
    private float circleDistanceRandom;
    private Vector3 pos;

    //[SerializeField]
    //private LookAt LookAt;
    //[SerializeField]
    //[Range(0f, 3f)]
    //private float WaitDelay = 1f;

    private Vector2 Velocity;
    private Vector2 SmoothDeltaPosition;

    private bool isFlying;
    private float nextFlyingTimerRange;
    private float flyingTimer;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (agent) agent.avoidancePriority = Random.Range(30, 70);
        if (!animator) animator = GetComponent<Animator>();
        if (!swarmManager) swarmManager = GetComponentInParent<SwarmManager>();
        if (!target) target = swarmManager?.player;

        agent.updatePosition = false;
        agent.updateRotation = true;
    }

    private void OnAnimatorMove()
    {
        Vector3 rootPosition = animator.rootPosition;
        rootPosition.y = Mathf.Lerp(rootPosition.y, agent.nextPosition.y, Time.deltaTime * 20f);
        transform.position = rootPosition;
        agent.nextPosition = rootPosition;
    }


    void ScheduleNextDirectionChange()
    {
        // seconds until next change
        nextDirectionChangeTime = Random.Range(0.1f, 2f);
        orbitDirection *= Mathf.RoundToInt((Random.Range(0, 1) - 0.5f) * 2);
        circleDistanceRandom = orbitRadius + Random.Range(-1f, 5f);
        directionChangeTimer = 0f;
    }

    void ScheduleFlyingState()
    {
        nextFlyingTimerRange = Random.Range(1f, 3f);
        int flyStateTriggerChance = Random.Range(-3, 1);
        if (flyStateTriggerChance >= 0)
        {
            isFlying = true;
        }
        else
        {
            isFlying = false;
        }

        animator.SetBool("Flying", isFlying);
        flyingTimer = 0f;
    }
    public void AssignManager(SwarmManager mgr, int idx)
    {
        swarmManager = mgr;
        index = idx - 1;
    }

    void Update()
    {
        flyingTimer += Time.deltaTime;
        if (flyingTimer >= nextFlyingTimerRange)
        {
            ScheduleFlyingState();
        }

        switch (currentState)
        {
            case InsectState.Circling:
                UpdateCircling();
                break;
            case InsectState.Attacking:
                UpdateAttacking();
                break;
            case InsectState.Retreating:
                UpdateRetreating();
                break;
        }

        SynchronizeAnimatorAndAgent();
    }

    public Vector3 GetOrbitPosition()
    {

        Vector3 offset = transform.position - target.position;
        float angle = orbitSpeed * orbitDirection * Time.time; // use orbitDirection
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        Vector3 rotatedOffset = rotation * offset.normalized * circleDistanceRandom;
        return target.transform.position + rotatedOffset;
    }

    void UpdateCircling()
    {
        directionChangeTimer += Time.deltaTime;

        if (directionChangeTimer >= nextDirectionChangeTime)
        {

            ScheduleNextDirectionChange();
            pos = GetOrbitPosition();
            agent.SetDestination(pos);
        }

        float distance = Vector3.Distance(target.transform.position, agent.transform.position);
        if (distance < circleDistanceRandom / 2)
        {
            currentState = InsectState.Retreating;
        }

        if (swarmManager.TryRequestAttack(this))
        {
            stateStartTime = Time.time;
            currentState = InsectState.Attacking;
            agent.SetDestination(target.position);
            Debug.Log($"{name} started ATTACKING.");
        }

    }

    void UpdateAttacking()
    {
        agent.SetDestination(target.transform.position);

        float distance = Vector3.Distance(target.transform.position, agent.transform.position);
        if (distance < 2.5f)
        {
            swarmManager.NotifyAttackEnded(this);
            currentState = InsectState.Retreating;
        }
    }

    void UpdateRetreating()
    {
        Vector3 direction = agent.transform.position - target.transform.position;

        float distance = Vector3.Distance(target.transform.position, agent.transform.position);
        if (distance < circleDistanceRandom)
        {
            agent.SetDestination(transform.position + direction);
        }
        else
        {
            currentState = InsectState.Circling;
        }
    }

    private void SynchronizeAnimatorAndAgent()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        worldDeltaPosition.y = 0;
        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
        SmoothDeltaPosition = Vector2.Lerp(SmoothDeltaPosition, deltaPosition, smooth);

        Velocity = SmoothDeltaPosition / Time.deltaTime;
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Velocity = Vector2.Lerp(Vector2.zero, Velocity, agent.remainingDistance);
        }

        bool shouldMove = Velocity.magnitude > 0.5f && agent.remainingDistance > agent.stoppingDistance;

        if (shouldMove)
        {
            animator.SetFloat("MoveSpeed", Velocity.normalized.magnitude * agent.speed);
        }
        else
        {
            animator.SetFloat("MoveSpeed", 0);
        }

        animator.SetFloat("LeftRight", Velocity.x);
        animator.SetFloat("ForwardBack", Velocity.y);

        //LookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;

        //float deltaMagnitude = worldDeltaPosition.magnitude;
        //if (deltaMagnitude > Agent.radius / 2)
        //{
        //    transform.position = Vector3.Lerp(Animator.rootPosition, Agent.nextPosition, smooth);
        //}
    }


    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Color gizmoColor;
        switch (currentState)
        {
            case InsectState.Circling:
                gizmoColor = Color.yellow;
                gizmoColor.a = 0.5f;
                break;
            case InsectState.Attacking:
                gizmoColor = Color.red;
                gizmoColor.a = 0.5f;
                break;
            case InsectState.Retreating:
                gizmoColor = Color.cyan;
                gizmoColor.a = 0.5f;
                break;
            default:
                gizmoColor = Color.white;
                gizmoColor.a = 0.5f;
                break;
        }

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.2f, 1);

        // Forward direction line
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, transform.position + transform.forward * 0.5f + Vector3.up * 0.2f);
    }
}
