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

    private bool matchEnded = false;
    private bool overtimeTriggered = false;

    public bool MatchEnded => matchEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (matchEnded) return;

        matchTime -= Time.deltaTime;
        UpdateTimerUI();

        // 🔥 LAST 60s → DOUBLE ENERGY (ONCE)
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

        if (matchTime <= 0f)
        {
            matchEnded = true;
            ResolveByTime();
        }
    }

    private void UpdateTimerUI()
    {
        int m = Mathf.FloorToInt(matchTime / 60f);
        int s = Mathf.FloorToInt(matchTime % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }

    private void ResolveByTime()
    {
        BaseHealth playerBase = GameObject.FindWithTag("PlayerBase").GetComponent<BaseHealth>();
        BaseHealth enemyBase = GameObject.FindWithTag("EnemyBase").GetComponent<BaseHealth>();

        EndMatch(playerBase.currentHealth > enemyBase.currentHealth);
    }

    public void OnBaseDestroyed(bool isPlayerBase)
    {
        if (matchEnded) return;
        EndMatch(!isPlayerBase);
    }

    private void EndMatch(bool playerWon)
    {
        matchEnded = true;

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.lastMatchWon = playerWon;

        winUI.SetActive(playerWon);
        loseUI.SetActive(!playerWon);

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

    private IEnumerator ShowElixir2xPopup()
    {
        elixir2xPopup.SetActive(true);

        RectTransform rt = elixir2xPopup.GetComponent<RectTransform>();
        CanvasGroup cg = elixir2xPopup.GetComponent<CanvasGroup>();
        if (!cg) cg = elixir2xPopup.AddComponent<CanvasGroup>();

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = t / popDuration;

            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, n);
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
