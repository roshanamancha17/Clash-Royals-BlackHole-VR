using UnityEngine;

public class EnemyEnergySystem : MonoBehaviour
{
    [Header("Enemy Energy Settings (Hidden)")]
    public float maxEnergy = 10f;          // Same cap as player
    public float regenPerTick = 1f;        // Same as player
    public float regenInterval = 1.8f;     // Same as player (CR-style)

    [Header("Runtime")]
    public float currentEnergy = 0f;

    private float regenMultiplier = 1f;

    private void Start()
    {
        // ✅ Start from ZERO
        currentEnergy = 0f;

        // Same regen rhythm as player
        InvokeRepeating(nameof(Regenerate), regenInterval, regenInterval);
    }

    private void Regenerate()
    {
        float amount = regenPerTick * regenMultiplier;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
    }

    public bool TrySpend(float amount)
    {
        if (currentEnergy < amount)
            return false;

        currentEnergy -= amount;
        return true;
    }

    // 🔥 Called by GameManagerVR in last 1 minute
    public void SetRegenMultiplier(float multiplier)
    {
        regenMultiplier = multiplier;
    }
}
