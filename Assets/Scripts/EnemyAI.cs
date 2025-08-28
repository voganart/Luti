using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float acceleration = 8f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 1f;
    [SerializeField] private float obstacleCheckDistance = 1f;
    [SerializeField] private float maxObstacleHeight = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Obstacle Check")]
    [SerializeField] private LayerMask obstacleLayer; // выбрать Location

    [Header("Attack")]
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("References")]
    public Transform player;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Animator animator;

    private bool isGrounded;
    private float lastJumpTime = -10f;
    private float attackTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("Grounded", isGrounded);

        if (!isGrounded && agent.enabled)
        {
            Debug.Log("Enemy: Airborne, disabling agent.");
            agent.enabled = false;
        }
        else if (isGrounded && !agent.enabled)
        {
            Debug.Log("Enemy: Landed, enabling agent.");
            agent.enabled = true;
        }
    
        TryJumpIfNeeded();
    }

    
    private void TryJumpIfNeeded()
    {
        Debug.Log("TryJumpIfNeeded called");

        if (!isGrounded)
        {
            Debug.Log("Not grounded, skip jump.");
            return;
        }

        if (Time.time - lastJumpTime < jumpCooldown)
        {
            Debug.Log($"Jump cooldown active: {Time.time - lastJumpTime:0.00}/{jumpCooldown}");
            return;
        }

        if (!IsObstacleInFront())
        {
            Debug.Log("No obstacle detected, skip jump.");
            return;
        }

        Debug.Log("Jump triggered!");
        agent.enabled = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        lastJumpTime = Time.time;
    }


    private bool IsObstacleInFront()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;
        float distance = 0.7f; // Увеличь если нужно

        Debug.DrawRay(origin, direction * distance, Color.red, 0.5f);

        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, distance, obstacleLayer))
        {
            Debug.Log($"Obstacle detected: {hitInfo.collider.name}");
            return true;
        }
        return false;
    }



    private void Update()
    {
        if (player == null) return;
        if (!agent.enabled) return;

        float distance = Vector3.Distance(transform.position, player.position);

        Debug.Log($"Enemy: distance={distance:0.00}, isGrounded={isGrounded}, agent.enabled={agent.enabled}, hasPath={agent.hasPath}");

        if (distance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            float normalizedSpeed = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("MoveSpeed", normalizedSpeed);
        }
        else
        {
            agent.isStopped = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                animator.SetTrigger("Attack");
                attackTimer = attackCooldown;
            }

            animator.SetFloat("MoveSpeed", 0f);
        }
    }
}
