using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f; // Tốc độ di chuyển thường
    public float detectionRadius = 5f; // Bán kính phát hiện player
    public float chaseSpeed = 3f; // Tốc độ đuổi theo player
    public LayerMask obstacleLayer; // Layer của chướng ngại vật
    public LayerMask playerLayer; // Layer của player

    [Header("Patrol Settings")]
    public float patrolRadius = 5f; // Bán kính giới hạn di chuyển

    [Header("Random Movement Settings")]
    public float minWanderTime = 1f; // Thời gian di chuyển ngẫu nhiên tối thiểu
    public float maxWanderTime = 3f; // Thời gian di chuyển ngẫu nhiên tối đa
    public float waitBetweenWanderTime = 1f; // Thời gian chờ giữa các lần di chuyển

    [Header("Combat Settings")]
    public int maxHealth = 100; // Máu tối đa
    public int currentHealth; // Máu hiện tại
    public int attackDamage = 10; // Sát thương tấn công
    public float attackRange = 1f; // Khoảng cách tấn công
    public float attackCooldown = 1.5f; // Thời gian chờ giữa các đòn tấn công
    public GameObject deathEffect; // Hiệu ứng khi chết

    [Header("Avoidance Settings")]
    public float maxAvoidanceTime = 5f; // Thời gian tối đa để tránh chướng ngại vật

    [Header("Knockback Settings")]
    [SerializeField] private float KnockbackForce = 0f; // Lực đẩy lùi khi bị đánh
    [SerializeField] private float durPushBack = 0.2f; // Thời gian đẩy lùi
    private Vector2 pushBackVelocity; // Vận tốc đẩy lùi
    private bool isPushBack = false; // Trạng thái đẩy lùi

    private Transform player; // Tham chiếu đến player
    private Vector2 randomDirection; // Hướng di chuyển ngẫu nhiên
    private Vector2 initialPosition; // Vị trí trung tâm vùng giới hạn
    private bool isWandering = false; // Đang di chuyển ngẫu nhiên?
    private bool isChasing = false; // Đang đuổi theo player?
    private bool isAttacking = false; // Đang tấn công?
    private bool isDead = false; // Đã chết?
    private bool isReturningToPatrol = false; // Đang quay lại vùng giới hạn?
    private Rigidbody2D rb; // Thành phần Rigidbody2D
    private SpriteRenderer spriteRenderer; // Thành phần SpriteRenderer
    private Animator animator; // Thành phần Animator
    private float lastAttackTime; // Thời điểm tấn công cuối
    private Color originalColor; // Màu gốc của sprite
    private float avoidanceTimer = 0f; // Bộ đếm thời gian tránh chướng ngại

    private static readonly int IsMoving = Animator.StringToHash("IsMoving"); // ID animation di chuyển
    private static readonly int AttackTrigger = Animator.StringToHash("IsAttacking"); // ID animation tấn công
    private static readonly int HurtTrigger = Animator.StringToHash("IsHurting"); // ID animation bị đau
    private static readonly int DieTrigger = Animator.StringToHash("IsDead"); // ID animation chết

    void Start()
    {
        // Khởi tạo các thành phần và giá trị ban đầu
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // Tìm player
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;
        originalColor = spriteRenderer.color;
        initialPosition = transform.position; // Lưu vị trí ban đầu
        StartCoroutine(RandomMovement()); // Bắt đầu di chuyển ngẫu nhiên
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Nếu đang bị đẩy lùi, áp dụng vận tốc đẩy
        if (isPushBack)
        {
            rb.linearVelocity = pushBackVelocity; // Gán vận tốc đẩy lùi
            return;
        }

        // Cập nhật trạng thái animation di chuyển
        animator?.SetBool(IsMoving, rb.linearVelocity.magnitude > 0.1f);

        // Phát hiện player trong tầm
        if (CanDetectPlayer())
        {
            isChasing = true;
            isWandering = false;
            ChasePlayer(); // Đuổi theo player

            // Tấn công nếu player trong tầm và hết thời gian chờ
            if (Vector2.Distance(transform.position, player.position) <= attackRange &&
                Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer(); // Tấn công
            }
        }
        else
        {
            // Cập nhật vị trí trung tâm khi mất dấu player
            if (isChasing)
            {
                initialPosition = transform.position;
                isChasing = false;
            }

            // Quay lại vùng giới hạn nếu đi quá xa
            if (!isChasing && Vector2.Distance(transform.position, initialPosition) > patrolRadius)
            {
                isWandering = false;
                isReturningToPatrol = true;
                ReturnToPatrolArea();
            }
            else
            {
                isReturningToPatrol = false;
                if (!isWandering && !isAttacking)
                {
                    StartCoroutine(RandomMovement()); // Tiếp tục di chuyển ngẫu nhiên
                }
            }
        }
    }

    bool CanDetectPlayer()
    {
        // Kiểm tra player có trong bán kính phát hiện không
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    void ChasePlayer()
    {
        if (isAttacking) return;

        // Tính hướng trực tiếp đến player
        Vector2 directPath = (player.position - transform.position).normalized;
        // Kiểm tra chướng ngại vật bằng raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directPath, 1f, obstacleLayer);

        if (hit.collider != null)
        {
            // Nếu có chướng ngại, tìm hướng tránh
            avoidanceTimer += Time.deltaTime;
            if (avoidanceTimer >= maxAvoidanceTime)
            {
                isChasing = false; // Ngừng đuổi nếu tránh quá lâu
                avoidanceTimer = 0f;
                return;
            }
            Vector2 avoidanceDirection = FindAvoidanceDirection(directPath);
            rb.linearVelocity = avoidanceDirection * chaseSpeed; // Di chuyển theo hướng tránh
            UpdateFacingDirection(avoidanceDirection);
        }
        else
        {
            avoidanceTimer = 0f;
            rb.linearVelocity = directPath * chaseSpeed; // Di chuyển thẳng đến player
            UpdateFacingDirection(directPath);
        }
    }

    void ReturnToPatrolArea()
    {
        // Di chuyển về vị trí trung tâm
        Vector2 directionToInitial = (initialPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = directionToInitial * moveSpeed; // Gán vận tốc
        UpdateFacingDirection(directionToInitial);
    }

    Vector2 FindAvoidanceDirection(Vector2 directPath)
    {
        // Tìm hướng tránh chướng ngại vật
        Vector2 rightPerpendicular = new Vector2(directPath.y, -directPath.x).normalized;
        Vector2 leftPerpendicular = new Vector2(-directPath.y, directPath.x).normalized;

        RaycastHit2D rightHit = Physics2D.Raycast(transform.position, rightPerpendicular, 1f, obstacleLayer);
        if (rightHit.collider == null)
            return rightPerpendicular; // Ưu tiên hướng phải nếu không có chướng ngại

        RaycastHit2D leftHit = Physics2D.Raycast(transform.position, leftPerpendicular, 1f, obstacleLayer);
        if (leftHit.collider == null)
            return leftPerpendicular; // Chọn hướng trái nếu không có chướng ngại

        // Chọn hướng nào xa chướng ngại hơn
        float rightDistance = rightHit.collider != null ? rightHit.distance : float.MaxValue;
        float leftDistance = leftHit.collider != null ? leftHit.distance : float.MaxValue;
        return rightDistance > leftDistance ? rightPerpendicular : leftPerpendicular;
    }

    void AttackPlayer()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero; // Dừng di chuyển khi tấn công
        animator?.SetTrigger(AttackTrigger); // Kích hoạt animation tấn công

        bool canAttack = player != null && Vector2.Distance(transform.position, player.position) <= attackRange;
        Vector2 knockbackDirection = (player.position - transform.position).normalized;
        StartCoroutine(DealDamageAfterDelay(0.3f, canAttack, knockbackDirection)); // Gây sát thương sau delay
    }

    IEnumerator DealDamageAfterDelay(float delay, bool canAttack, Vector2 knockbackDirection)
    {
        yield return new WaitForSeconds(delay); // Đợi animation tấn công
        if (isDead) yield break;

        // Gây sát thương cho player nếu trong tầm
        if (canAttack)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                
            }
        }
        yield return new WaitForSeconds(0.5f);
        isAttacking = false; // Kết thúc trạng thái tấn công
    }

    IEnumerator RandomMovement()
    {
        isWandering = true;
        while (!isChasing && !isDead && !isReturningToPatrol)
        {
            // Tạo hướng ngẫu nhiên
            randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

            // Kiểm tra giới hạn vùng tuần tra
            Vector2 nextPosition = (Vector2)transform.position + randomDirection * moveSpeed * Time.deltaTime;
            if (Vector2.Distance(nextPosition, initialPosition) > patrolRadius)
            {
                randomDirection = (initialPosition - (Vector2)transform.position).normalized;
            }

            // Kiểm tra chướng ngại vật
            RaycastHit2D initialHit = Physics2D.Raycast(transform.position, randomDirection, 1f, obstacleLayer);
            if (initialHit.collider != null)
            {
                randomDirection = FindAvoidanceDirection(randomDirection);
            }

            float wanderTime = Random.Range(minWanderTime, maxWanderTime);
            float timer = 0;

            while (timer < wanderTime && !isChasing && !isDead && !isReturningToPatrol)
            {
                if (!isAttacking)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, randomDirection, 1f, obstacleLayer);
                    if (hit.collider != null)
                    {
                        randomDirection = FindAvoidanceDirection(randomDirection);
                    }

                    nextPosition = (Vector2)transform.position + randomDirection * moveSpeed * Time.deltaTime;
                    if (Vector2.Distance(nextPosition, initialPosition) > patrolRadius)
                    {
                        randomDirection = (initialPosition - (Vector2)transform.position).normalized;
                    }

                    rb.linearVelocity = randomDirection * moveSpeed; // Di chuyển ngẫu nhiên
                    UpdateFacingDirection(randomDirection);
                }
                timer += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero; // Dừng lại
            yield return new WaitForSeconds(waitBetweenWanderTime); // Chờ trước khi di chuyển tiếp
            if (isChasing || isDead || isReturningToPatrol) break;
        }
        isWandering = false;
    }

    void UpdateFacingDirection(Vector2 direction)
    {
        // Xoay sprite theo hướng di chuyển
        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1); // Quay trái
        else if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1); // Quay phải
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        if (isDead) return;

        currentHealth -= damage; // Giảm máu
        if (currentHealth < 0)
            currentHealth = 0;

        StartCoroutine(FlashEffect()); // Hiệu ứng nhấp nháy
        _PushBack(knockbackDirection); // Áp dụng đẩy lùi
        CameraShake.ins.Shake(1, 20, 0.3f); // Rung camera

        if (currentHealth <= 0)
            Die(); // Chết nếu hết máu
        else
        {
            animator?.SetTrigger(HurtTrigger); // Kích hoạt animation bị đau
            StartCoroutine(PauseAfterHit()); // Tạm dừng sau khi bị đánh
            
        }
    }

    IEnumerator PauseAfterHit()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.3f); // Tạm dừng ngắn
        isAttacking = false;
    }

    IEnumerator FlashEffect()
    {
        spriteRenderer.color = Color.red; // Đổi màu đỏ khi bị đánh
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor; // Trả về màu gốc
    }

    void Die()
    {
        isDead = true;
        animator?.SetTrigger(DieTrigger); // Kích hoạt animation chết
        rb.linearVelocity = Vector2.zero; // Dừng di chuyển

        // Vô hiệu hóa collider
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D c in colliders)
            c.enabled = false;

        // Tạo hiệu ứng chết
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.4f);
        }
        StartCoroutine(DestroyAfterDelay(1.5f)); // Xóa enemy sau delay
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); // Xóa gameObject
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false; // Kết thúc trạng thái tấn công
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ các vùng trong editor để debug
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // Vùng phát hiện
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // Vùng tấn công
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(initialPosition, patrolRadius); // Vùng tuần tra
    }

    private void _PushBack(Vector2 direction)
    {
        // Áp dụng lực đẩy lùi
        pushBackVelocity = direction.normalized * KnockbackForce;
        isPushBack = true;
        StartCoroutine(PushBacking());
    }

    private IEnumerator PushBacking()
    {
        yield return new WaitForSeconds(durPushBack); // Đợi thời gian đẩy lùi
        pushBackVelocity = Vector2.zero; // Xóa vận tốc
        isPushBack = false; // Kết thúc đẩy lùi
    }
}