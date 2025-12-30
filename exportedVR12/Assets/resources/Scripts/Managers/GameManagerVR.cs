using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManagerVR : MonoBehaviour
{
    public static GameManagerVR Instance;

    [Header("Scene Management")]
    public string resultSceneName = "Result";

    [Header("Timer")]
    public float matchTime = 180f;
    public TextMeshProUGUI timerText;

    [Header("UI")]
    public GameObject winUI;
    public GameObject loseUI;

    [Header("Energy Systems")]
    public PlayerEnergySystem playerEnergy;
    public EnemyEnergySystem enemyEnergy;

    [Header("Overtime UI")]
    public GameObject elixir2xPopup;
    public float popDuration = 0.3f;

    // ───────── Internal State ─────────
    private bool matchEnded = false;
    private bool overtimeTriggered = false;

    private BaseHealth playerBase;
    private BaseHealth enemyBase;

    public bool MatchEnded => matchEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Cache base references once
        playerBase = GameObject.FindWithTag("PlayerBase")?.GetComponent<BaseHealth>();
        enemyBase = GameObject.FindWithTag("EnemyBase")?.GetComponent<BaseHealth>();

        if (playerBase == null || enemyBase == null)
        {
            Debug.LogError("GameManagerVR: BaseHealth references missing!");
        }
    }

    private void Update()
    {
        if (matchEnded) return;

        matchTime -= Time.deltaTime;
        UpdateTimerUI();

        // 🔥 LAST 60 SECONDS → DOUBLE ENERGY (ONLY ONCE)
        if (matchTime <= 60f && !overtimeTriggered)
        {
            overtimeTriggered = true;

            if (playerEnergy != null)
                playerEnergy.SetRegenMultiplier(2f);

            if (enemyEnergy != null)
                enemyEnergy.SetRegenMultiplier(2f);

            if (elixir2xPopup != null)
                StartCoroutine(ShowElixir2xPopup());
        }

        // ⏱ TIME OVER → COMPARE BASE HEALTH
        if (matchTime <= 0f)
        {
            ResolveByTime();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(matchTime / 60f);
        int seconds = Mathf.FloorToInt(matchTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // ───────────────── MATCH RESOLUTION ─────────────────

    private void ResolveByTime()
    {
        if (matchEnded) return;
        matchEnded = true;

        float playerHP = playerBase != null ? playerBase.currentHealth : 0f;
        float enemyHP = enemyBase != null ? enemyBase.currentHealth : 0f;

        Debug.Log($"TIME OVER | Player HP: {playerHP} | Enemy HP: {enemyHP}");

        if (playerHP > enemyHP)
        {
            EndMatch(true);
        }
        else if (enemyHP > playerHP)
        {
            EndMatch(false);
        }
        else
        {
            // TIE BREAK RULE (choose one)
            // Option A: Player wins on tie
            EndMatch(true);

            // Option B (alternative): Enemy wins
            // EndMatch(false);

            // Option C (later): Sudden death
        }
    }

    // Called immediately when a base is destroyed
    public void OnBaseDestroyed(bool isPlayerBase)
    {
        if (matchEnded) return;

        // If player base destroyed → player lost
        EndMatch(!isPlayerBase);
    }

    private void EndMatch(bool playerWon)
    {
        matchEnded = true;

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.lastMatchWon = playerWon;

        if (winUI != null) winUI.SetActive(playerWon);
        if (loseUI != null) loseUI.SetActive(!playerWon);

        Time.timeScale = 0f;
        StartCoroutine(LoadResultSceneDelay());
    }

    private IEnumerator LoadResultSceneDelay()
    {
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.LoadScene(resultSceneName);
        else
            SceneManager.LoadScene(resultSceneName);
    }

    // ───────────────── UI EFFECTS ─────────────────

    private IEnumerator ShowElixir2xPopup()
    {
        elixir2xPopup.SetActive(true);

        RectTransform rt = elixir2xPopup.GetComponent<RectTransform>();
        CanvasGroup cg = elixir2xPopup.GetComponent<CanvasGroup>();
        if (cg == null) cg = elixir2xPopup.AddComponent<CanvasGroup>();

        float t = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * 1.2f;

        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = t / popDuration;

            rt.localScale = Vector3.Lerp(startScale, targetScale, n);
            cg.alpha = n;

            yield return null;
        }

        rt.localScale = Vector3.one;
        cg.alpha = 1f;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
