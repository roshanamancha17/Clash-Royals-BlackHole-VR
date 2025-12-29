using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MatchSessionManager : MonoBehaviour
{
    [Header("UI Groups")]
    public CanvasGroup gameplayUI;      // Drag 'Gameplay_Group' here!
    
    [Header("Phase UI")]
    public GameObject introPanel;       
    public GameObject dangerVignette;   
    public TextMeshProUGUI timerText;   

    [Header("Game References")]
    public BaseHealth playerBase;       
    public PlayerTroopSpawnerVR spawner;      

    [Header("Settings")]
    public float tensionThreshold = 0.3f; 

    private bool isTensionActive = false;

    private float matchTime = 0f;

    void Start()
    {
        StartCoroutine(PlayMatchIntro());
    }

    void Update()
    {
        matchTime += Time.deltaTime;
        if (timerText) timerText.text = FormatTime(matchTime);


        MonitorTension();
    }

    IEnumerator PlayMatchIntro()
    {
        // 1. SETUP: Lock inputs and Hide Gameplay UI
        if (spawner != null) spawner.enabled = false;
        if (dangerVignette) dangerVignette.SetActive(false);
        
        // Hide the deck/health bars instantly
        if (gameplayUI) gameplayUI.alpha = 0; 

        // 2. SHOW INTRO
        if (introPanel) introPanel.SetActive(true);

        // 3. WAIT (The Text is visible, the game is black)
        yield return new WaitForSeconds(2.0f);

        // 4. START BATTLE
        if (introPanel) introPanel.SetActive(false);
        if (spawner != null) spawner.enabled = true; 
        
        // 5. FADE IN THE GAMEPLAY UI
        if (gameplayUI)
        {
            float duration = 1.0f; // 1 second fade
            float time = 0;
            while (time < duration)
            {
                gameplayUI.alpha = Mathf.Lerp(0, 1, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            gameplayUI.alpha = 1; // Ensure it ends fully visible
        }

        Debug.Log("Match Started!");
    }

    

    // ... (Keep MonitorTension, ActivateTensionMode, FormatTime same as before) ...
    void MonitorTension()
    {
        if (playerBase == null || isTensionActive) return;
        float hpPercent = playerBase.currentHealth / playerBase.maxHealth;
        if (hpPercent <= tensionThreshold) ActivateTensionMode();
    }

    void ActivateTensionMode()
    {
        isTensionActive = true;
        if (dangerVignette) dangerVignette.SetActive(true);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }
}