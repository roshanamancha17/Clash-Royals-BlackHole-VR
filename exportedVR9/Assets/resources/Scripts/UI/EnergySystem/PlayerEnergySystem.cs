using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEnergySystem : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 10f;
    public float regenPerTick = 1f;
    public float regenInterval = 1.8f;

    [Header("Runtime")]
    public float currentEnergy;
    private float regenMultiplier = 1f;

    [Header("UI")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    private void Start()
    {
        currentEnergy = 0f;
        UpdateUI();

        InvokeRepeating(nameof(Regenerate), regenInterval, regenInterval);
    }

    private void Regenerate()
    {
        float amount = regenPerTick * regenMultiplier;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
        UpdateUI();
    }

    // Called by GameManagerVR (Overtime)
    public void SetRegenMultiplier(float multiplier)
    {
        regenMultiplier = multiplier;
    }

    public bool TrySpend(float amount)
    {
        if (currentEnergy < amount)
            return false;

        currentEnergy -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (energySlider)
            energySlider.value = currentEnergy / maxEnergy;

        if (energyText)
            energyText.text = $"{Mathf.FloorToInt(currentEnergy)} / {maxEnergy}";
    }
}
