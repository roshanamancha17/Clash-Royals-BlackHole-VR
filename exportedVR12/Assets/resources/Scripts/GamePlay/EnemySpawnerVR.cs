using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnerVR : MonoBehaviour
{
    /* ───────────────────── CONFIG ───────────────────── */

    [Header("Enemy Troop Prefabs")]
    public GameObject archerPrefab;
    public GameObject knightPrefab;
    public GameObject tankPrefab;

    [Header("Spawn Points")]
    public Transform archerSpawnPoint;
    public Transform knightSpawnPoint;
    public Transform tankSpawnPoint;

    [Header("Enemy Energy (Hidden)")]
    public EnemyEnergySystem enemyEnergy;

    [Header("Energy Costs")]
    public float archerCost = 2f;
    public float knightCost = 3f;
    public float tankCost = 5f;

    [Header("Spawn Control")]
    public int maxEnemiesAlive = 3;
    public float decisionInterval = 2.5f;
    public float initialDelay = 5f;

    [Header("Difficulty Tuning")]
    [Range(0.3f, 1f)]
    public float spawnSpeedMultiplier = 0.7f;

    [Header("Aggression Settings")]
    public float aggressiveDecisionMultiplier = 0.6f; // faster decisions
    public int aggressiveExtraUnits = 1;               // pressure spike

    /* ───────────────────── RUNTIME ───────────────────── */

    private float decisionTimer;
    private float elapsedTime;

    private AIAggressionState aggressionState = AIAggressionState.Normal;
    private BaseHealth enemyBase;

    /* ───────────────────── UNITY ───────────────────── */

    private void Start()
    {
        GameObject baseObj = GameObject.FindWithTag("EnemyBase");
        if (baseObj != null)
            enemyBase = baseObj.GetComponent<BaseHealth>();
    }

    private void Update()
    {
        if (enemyEnergy == null || enemyBase == null)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime < initialDelay)
            return;

        UpdateAggressionState();

        int allowedEnemies = maxEnemiesAlive;
        float interval = decisionInterval;

        if (aggressionState == AIAggressionState.Aggressive)
        {
            allowedEnemies += aggressiveExtraUnits;
            interval *= aggressiveDecisionMultiplier;
        }

        if (CountEnemies() >= allowedEnemies)
            return;

        decisionTimer += Time.deltaTime * spawnSpeedMultiplier;

        if (decisionTimer < interval)
            return;

        decisionTimer = 0f;

        AttemptStrategicSpawn();
    }

    /* ───────────────────── AGGRESSION LOGIC ───────────────────── */

    private void UpdateAggressionState()
    {
        float healthRatio = enemyBase.currentHealth / enemyBase.maxHealth;

        // Trigger aggression after 90% damage (≤10% HP)
        if (healthRatio <= 0.1f)
        {
            aggressionState = AIAggressionState.Aggressive;
        }
        else
        {
            aggressionState = AIAggressionState.Normal;
        }
    }

    /* ───────────────────── AI DECISION ───────────────────── */

    private void AttemptStrategicSpawn()
    {
        TroopType choice = DecideNextSpawn();

        switch (choice)
        {
            case TroopType.Tank:
                if (enemyEnergy.TrySpend(tankCost))
                    SpawnEnemy(tankPrefab, tankSpawnPoint);
                break;

            case TroopType.Melee:
                if (enemyEnergy.TrySpend(knightCost))
                    SpawnEnemy(knightPrefab, knightSpawnPoint);
                break;

            case TroopType.Ranged:
                if (enemyEnergy.TrySpend(archerCost))
                    SpawnEnemy(archerPrefab, archerSpawnPoint);
                break;
        }
    }

    private TroopType DecideNextSpawn()
    {
        GetPlayerTroopComposition(out int melee, out int ranged, out int tank);

        // Aggressive mode favors tanks for pressure
        if (aggressionState == AIAggressionState.Aggressive)
        {
            if (enemyEnergy.currentEnergy >= tankCost)
                return TroopType.Tank;
        }

        if (ranged >= melee && ranged >= tank)
            return TroopType.Tank;

        if (tank > melee)
            return TroopType.Melee;

        return TroopType.Ranged;
    }

    /* ───────────────────── BATTLEFIELD ANALYSIS ───────────────────── */

    private void GetPlayerTroopComposition(
        out int melee,
        out int ranged,
        out int tank
    )
    {
        melee = 0;
        ranged = 0;
        tank = 0;

        Troop[] troops = FindObjectsByType<Troop>(FindObjectsSortMode.None);

        foreach (var t in troops)
        {
            if (t == null) continue;

            TeamComponent tc = t.GetComponent<TeamComponent>();
            if (tc == null || tc.team != Team.Player) continue;

            switch (t.troopType)
            {
                case TroopType.Melee: melee++; break;
                case TroopType.Ranged: ranged++; break;
                case TroopType.Tank: tank++; break;
            }
        }
    }

    /* ───────────────────── SPAWN ───────────────────── */

    private void SpawnEnemy(GameObject prefab, Transform spawnPoint)
    {
        if (prefab == null || spawnPoint == null)
            return;

        GameObject troopGO = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        if (NavMesh.SamplePosition(
            troopGO.transform.position,
            out NavMeshHit hit,
            5f,
            NavMesh.AllAreas))
        {
            troopGO.transform.position = hit.position;
        }

        TeamComponent tc = troopGO.GetComponent<TeamComponent>();
        if (tc != null)
            tc.team = Team.Enemy;
    }

    /* ───────────────────── UTIL ───────────────────── */

    private int CountEnemies()
    {
        Troop[] troops = FindObjectsByType<Troop>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var t in troops)
        {
            if (t == null) continue;

            TeamComponent tc = t.GetComponent<TeamComponent>();
            if (tc != null && tc.team == Team.Enemy)
                count++;
        }

        return count;
    }
}
