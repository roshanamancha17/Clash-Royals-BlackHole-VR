using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed for scene events
using System.Collections;

public class VRScreenFader : MonoBehaviour
{
    public static VRScreenFader Instance;
    public Canvas fadeCanvas;
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- NEW CODE START ---
    private void OnEnable()
    {
        // Subscribe to the event: "When a scene finishes loading, call OnSceneLoaded"
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This runs automatically every time a scene loads (Boot -> Home, Home -> Level1)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FadeIn(); 
    }
    // --- NEW CODE END ---

    public void FadeOut()
    {
        fadeCanvas.enabled = true;
        StopAllCoroutines(); // Stop any existing fades so they don't fight
        StartCoroutine(FadeRoutine(0, 1)); 
    }

    public void FadeIn()
    {
        fadeCanvas.enabled = true;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1, 0)); 
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            // CHANGE THIS: Use unscaledDeltaTime so it works while paused
            timer += Time.unscaledDeltaTime; 
            
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            if(fadeImage != null) 
                fadeImage.color = new Color(0, 0, 0, alpha);
            
            yield return null;
        }

        if(fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, endAlpha);

        if (endAlpha == 0) fadeCanvas.enabled = false;
    }

    void Update()
    {
        if (!fadeCanvas.enabled) return;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            transform.position = mainCam.transform.position;
            transform.rotation = mainCam.transform.rotation;
            transform.Translate(Vector3.forward * 0.5f); // 0.5m in front of face
        }
    }
}