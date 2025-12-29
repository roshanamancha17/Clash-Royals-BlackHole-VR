using UnityEngine;
using System.Collections.Generic;

public class PlayerTroopSpawnerVR : MonoBehaviour
{
    [Header("Energy")]
    public PlayerEnergySystem energySystem;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Deck")]
    public List<UnitData> debugDeck;
    private List<UnitData> activeDeck;

    private void Start()
    {
        if (SelectedDeck.deck != null && SelectedDeck.deck.Count > 0)
            activeDeck = SelectedDeck.deck;
        else
            activeDeck = debugDeck;
    }

    // Called by UI buttons
    public void SpawnUnit(int deckIndex)
    {
        if (TroopPoolManager.Instance == null)
        {
            Debug.LogError("TroopPoolManager missing!");
            return;
        }

        if (activeDeck == null || deckIndex >= activeDeck.Count)
            return;

        UnitData unit = activeDeck[deckIndex];

        // Energy check
        if (!energySystem.TrySpend(unit.cost))
            return;

        if (unit.prefab == null)
        {
            Debug.LogError("UnitData prefab missing!");
            return;
        }

        Troop troopPrefab = unit.prefab.GetComponent<Troop>();
        if (troopPrefab == null)
        {
            Debug.LogError("Prefab does not contain Troop component!");
            return;
        }

        Transform spawnPoint = spawnPoints[deckIndex % spawnPoints.Length];

        // ✅ CORRECT POOL SPAWN
        TroopPoolManager.Instance.SpawnTroop(
            troopPrefab.troopType,
            spawnPoint.position,
            spawnPoint.rotation,
            Team.Player
        );
    }
}
