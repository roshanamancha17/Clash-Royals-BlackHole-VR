using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TeamComponent))]
public class Troop : MonoBehaviour
{
    /* ───────────────── CONFIG ───────────────── */

    [Header("Troop Data")]
    public TroopType troopType;                  // Melee, Ranged, Tank
    public float maxHealth = 50f;
    public float attackDamage = 10f;
    public float attackRange = 2.5f;
    public float attackCooldown = 1f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;

    [Header("Projectile")]
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 15f;

    [Header("Tank Settings")]
    public float tankFireRange = 10f;

    [Header("Targeting")]
    public float scanInterval = 0.5f;
    public float baseAttackDistance = 2.5f;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;

    /* ───────────────── RUNTIME ───────────────── */

    private float currentHealth;
    private float attackTimer;
    private float scanTimer;

    private NavMeshAgent agent;
    private TeamComponent teamComponent;

    private BaseHealth targetBase;
    private Transform attackPoint;
    private Troop currentEnemyTarget;
    private HealthBar healthBar;

    /* ───────────────── UNITY ───────────────── */

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        teamComponent = GetComponent<TeamComponent>();

        agent.speed = moveSpeed;
        agent.acceleration = 12f;
        agent.angularSpeed = 720f;
        agent.autoBraking = true;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        AssignTargetBase();
        SetDestinationToBase();
        SpawnHealthBar();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        scanTimer -= Time.deltaTime;

        if (!agent.isOnNavMesh)
            return;

        // ───────── Tank logic ─────────
        if (troopType == TroopType.Tank)
        {
            HandleTankBehaviour();
            return;
        }

        // ───────── Melee / Ranged logic ─────────
        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;
            FindClosestEnemyTroop();
        }

        if (currentEnemyTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentEnemyTarget.transform.position);
            if (dist <= attackRange)
            {
                agent.isStopped = true;
                transform.LookAt(currentEnemyTarget.transform);
                AttackTroop();
                return;
            }
        }

        agent.isStopped = false;
        MoveTowardsBase();
    }

    /* ───────────────── TARGETING ───────────────── */

    private void AssignTargetBase()
    {
        if (teamComponent.team == Team.Player)
        {
            targetBase = GameObject.FindWithTag("EnemyBase")?.GetComponent<BaseHealth>();
            attackPoint = GameObject.FindWithTag("EnemyAttackPoint")?.transform;
        }
        else
        {
            targetBase = GameObject.FindWithTag("PlayerBase")?.GetComponent<BaseHealth>();
            attackPoint = GameObject.FindWithTag("PlayerAttackPoint")?.transform;
        }
    }

    private void FindClosestEnemyTroop()
    {
        Troop[] troops = FindObjectsByType<Troop>(FindObjectsSortMode.None);

        float closest = Mathf.Infinity;
        currentEnemyTarget = null;

        foreach (var t in troops)
        {
            if (t == null || t == this) continue;
            if (t.teamComponent.team == teamComponent.team) continue;

            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < closest)
            {
                closest = dist;
                currentEnemyTarget = t;
            }
        }
    }

    /* ───────────────── MOVEMENT ───────────────── */

    private void SetDestinationToBase()
    {
        if (attackPoint == null || !agent.isOnNavMesh) return;
        agent.SetDestination(attackPoint.position);
    }

    private void MoveTowardsBase()
    {
        if (attackPoint == null || targetBase == null) return;

        float dist = Vector3.Distance(transform.position, attackPoint.position);

        if (dist <= baseAttackDistance)
        {
            agent.isStopped = true;
            transform.LookAt(targetBase.transform);
            AttackBase();
        }
        else if (!agent.hasPath)
        {
            SetDestinationToBase();
        }
    }

    private void HandleTankBehaviour()
    {
        if (attackPoint == null || targetBase == null) return;

        float dist = Vector3.Distance(transform.position, attackPoint.position);

        if (dist <= tankFireRange)
        {
            agent.isStopped = true;
            transform.LookAt(targetBase.transform);
            AttackBase();
        }
        else if (!agent.hasPath)
        {
            SetDestinationToBase();
        }
    }

    /* ───────────────── COMBAT ───────────────── */

    private void AttackTroop()
    {
        if (attackTimer > 0f || currentEnemyTarget == null) return;

        attackTimer = attackCooldown;

        if (troopType == TroopType.Melee)
            currentEnemyTarget.TakeDamage(attackDamage);
        else
            FireProjectile(currentEnemyTarget.transform);
    }

    private void AttackBase()
    {
        if (attackTimer > 0f || targetBase == null) return;

        attackTimer = attackCooldown;

        if (troopType == TroopType.Melee)
            targetBase.TakeDamage(attackDamage);
        else
            FireProjectile(targetBase.transform);
    }

    private void FireProjectile(Transform target)
    {
        if (ProjectilePoolManager.Instance == null || projectileSpawnPoint == null)
            return;

        Projectile p = ProjectilePoolManager.Instance.GetProjectile(
            troopType == TroopType.Tank ? ProjectileType.Tank : ProjectileType.Archer,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation
        );

        p?.Initialize(target, attackDamage, projectileSpeed);
    }

    /* ───────────────── HEALTH ───────────────── */

    private void SpawnHealthBar()
    {
        GameObject canvas = GameObject.FindWithTag("WorldCanvas");
        if (!canvas || !healthBarPrefab) return;

        GameObject hb = Instantiate(healthBarPrefab, canvas.transform);
        healthBar = hb.GetComponent<HealthBar>();
        healthBar.target = transform;
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthBar?.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (healthBar != null)
            Destroy(healthBar.gameObject);

        Destroy(gameObject);
    }
}
