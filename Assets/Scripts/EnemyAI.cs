using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    [Header("References")]
    [SerializeField] private Transform player;
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;

    [Header("Combat")]
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float attackTimer;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private float jumpCooldown = 1f;
    [SerializeField] private float obstacleCheckDistance = 1f;
    [SerializeField] private LayerMask obstacleLayer;
    private float lastJumpTime = -10f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1f; // Дистанция остановки для атаки

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (!animator || !agent || !rb || !groundCheck || !player)
        {
            Debug.LogError($"Missing required component or player reference on {gameObject.name}!");
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance; // Устанавливаем дистанцию остановки
        attackTimer = attackCooldown;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Замораживаем вращение
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("Grounded", isGrounded);

        if (isGrounded && !agent.enabled)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
            animator.ResetTrigger("Jump");
            animator.SetFloat("MoveSpeed", 0f);
        }
        else if (!isGrounded && agent.enabled)
        {
            agent.enabled = false;
        }

        TryJumpIfNeeded();
    }

    private void TryJumpIfNeeded()
    {
        if (!isGrounded || Time.time - lastJumpTime < jumpCooldown || !player) return;

        bool needsJump = NeedsToJumpToReachPlayer() || IsObstacleInPath();
        if (needsJump)
        {
            agent.enabled = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            lastJumpTime = Time.time;
            animator.SetTrigger("Jump");
        }
    }

    private bool NeedsToJumpToReachPlayer()
    {
        return player.position.y > transform.position.y + 0.5f;
    }

    private bool IsObstacleInPath()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, out hit, obstacleCheckDistance, obstacleLayer))
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, directionToPlayer * obstacleCheckDistance, Color.red, 0.1f);
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (!player || !agent.enabled) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log($"Enemy: distance={distance}, agent.isStopped={agent.isStopped}, hasPath={agent.hasPath}, attackTimer={attackTimer}");

        if (distance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetFloat("MoveSpeed", agent.velocity.magnitude / agent.speed);
            animator.ResetTrigger("Attack"); // Сбрасываем триггер атаки
        }
        else
        {
            agent.isStopped = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f && isGrounded) // Атака только на земле
            {
                animator.SetTrigger("Attack");
                attackTimer = attackCooldown;
            }

            animator.SetFloat("MoveSpeed", 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Уменьшаем физическое отталкивание
        if (collision.gameObject.layer == obstacleLayer)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y, rb.linearVelocity.z * 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}