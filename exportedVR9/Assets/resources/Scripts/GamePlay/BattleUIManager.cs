using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    [Header("Managers")]
    public PlayerTroopSpawnerVR spawner;       // To know what cards we have
    public PlayerEnergySystem energySystem;    // To show elixir

    [Header("Deck UI Buttons")]
    // Drag your 3 Button Objects here
    public Button cardButton1; 
    public Button cardButton2;
    public Button cardButton3;

    [Header("Elixir UI")]
    public Slider elixirSlider;
    public TextMeshProUGUI elixirText;

    private void Start()
    {
        // Wait a tiny bit for Spawner to initialize the deck
        Invoke(nameof(SetupDeckUI), 0.1f);
    }

    void SetupDeckUI()
    {
        // Get the list of cards from the spawner
        // (The spawner has already grabbed them from SelectedDeck)
        List<UnitData> deck = spawner.debugDeck; // Default fallback
        
        // We need to access the private 'activeDeck' from Spawner. 
        // For now, let's assume the Spawner handled the logic. 
        // Actually, let's just read SelectedDeck directly here for safety.
        if (SelectedDeck.deck != null && SelectedDeck.deck.Count > 0)
        {
            deck = SelectedDeck.deck;
        }

        // Setup Button 1
        if (deck.Count > 0) UpdateButtonVisuals(cardButton1, deck[0]);
        
        // Setup Button 2
        if (deck.Count > 1) UpdateButtonVisuals(cardButton2, deck[1]);

        // Setup Button 3
        if (deck.Count > 2) UpdateButtonVisuals(cardButton3, deck[2]);
    }

    void UpdateButtonVisuals(Button btn, UnitData data)
    {
        // 1. Find the Icon Image (Assuming it's a child named "Icon")
        Transform iconTrans = btn.transform.Find("Icon");
        if (iconTrans) iconTrans.GetComponent<Image>().sprite = data.icon;

        // 2. Find the Cost Text (Assuming it's a child named "CostText")
        Transform costTrans = btn.transform.Find("CostText");
        if (costTrans) costTrans.GetComponent<TextMeshProUGUI>().text = data.cost.ToString();
    }

    void Update()
    {
        if (energySystem != null)
    {
        // Update the slider bar
        if (elixirSlider) 
            elixirSlider.value = energySystem.currentEnergy / energySystem.maxEnergy;
            
        // Update the text to show "Current / Max"
        if (elixirText) 
            elixirText.text = $"{Mathf.FloorToInt(energySystem.currentEnergy)} / {energySystem.maxEnergy}";
    }
    }
    
    public void OnClickQuitToMenu()
    {
        Time.timeScale = 1f; // Unpause if paused
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home"); // Make sure "Home" is your menu scene name
    }
}