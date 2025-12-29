using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRButtonTroop : MonoBehaviour
{
    public PlayerTroopSpawnerVR spawner;

    [Header("Deck Index (0 = Top, 1 = Middle, 2 = Bottom)")]
    public int index; 

    public void PressButton()
    {
        // Instead of checking type (Knight/Archer), we just send the index.
        // The Spawner will look at the Deck to decide what unit that is.
        if (spawner != null)
        {
            spawner.SpawnUnit(index);
        }
    }
}