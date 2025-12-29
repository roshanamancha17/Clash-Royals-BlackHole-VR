using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnerVR : MonoBehaviour
{
    [Header("Enemy Troop Prefabs")]
    public GameObject archerPrefab;
    public GameObject knightPrefab;
    public GameObject tankPrefab;

    [Header("Spawn Points")]
    public Transform archerSpawnPoint;
    public Transform knightSpawnPoint;
    public Transform tankSpawnPoint;

    [Header("Cooldowns")]
    public float archerCooldown = 5f;
    public float knightCooldown = 7f;
    public float tankCooldown = 10f;

    [Header("Difficulty Tuning")]
    [Range(0.3f, 1f)]
    public float spawnSpeedMultiplier = 0.7f;

    [Header("Spawn Delay")]
    public float initialDelay = 5f;

    private float archerTimer;
    private float knightTimer;
    private float tankTimer;
    private float elapsedTime;

    [Header("Enemy Energy (Hidden)")]
    public EnemyEnergySystem enemyEnergy;

    [Header("Energy Costs")]
    public float archerCost = 2f;
    public float knightCost = 3f;
    public float tankCost = 5f;

    [Header("Troop Control")]
    public int maxEnemiesAlive = 3;

    private void Update()
    {
        if (enemyEnergy == null) return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime < initialDelay) return;

        if (CountEnemies() >= maxEnemiesAlive)
            return;

        float delta = Time.deltaTime * spawnSpeedMultiplier;

        archerTimer += delta;
        knightTimer += delta;
        tankTimer += delta;

        // Priority: Tank → Knight → Archer
        TrySpawnTank();
        TrySpawnKnight();
        TrySpawnArcher();
    }

    void TrySpawnArcher()
    {
        if (archerTimer < archerCooldown) return;
        if (!enemyEnergy.TrySpend(archerCost)) return;

        SpawnEnemy(archerPrefab, archerSpawnPoint);
        archerTimer = 0f;
    }

    void TrySpawnKnight()
    {
        if (knightTimer < knightCooldown) return;
        if (!enemyEnergy.TrySpend(knightCost)) return;

        SpawnEnemy(knightPrefab, knightSpawnPoint);
        knightTimer = 0f;
    }

    void TrySpawnTank()
    {
        if (tankTimer < tankCooldown) return;
        if (!enemyEnergy.TrySpend(tankCost)) return;

        SpawnEnemy(tankPrefab, tankSpawnPoint);
        tankTimer = 0f;
    }

    void SpawnEnemy(GameObject prefab, Transform spawnPoint)
    {
        if (!prefab || !spawnPoint) return;

        GameObject troop = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        if (NavMesh.SamplePosition(troop.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            troop.transform.position = hit.position;

        TeamComponent tc = troop.GetComponent<TeamComponent>();
        if (tc != null)
            tc.team = Team.Enemy;
    }

    int CountEnemies()
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
