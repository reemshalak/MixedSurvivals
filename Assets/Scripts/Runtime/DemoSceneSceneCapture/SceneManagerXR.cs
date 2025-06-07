using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class SceneManagerXR : MonoBehaviour
    {
        [Header("Scene Selection")]
        public SceneType[] availableScenes; // Set this in Inspector

        public GameObject sceneCanvas;
        public Image sceneIcon; // UI Image for scene icon
        public TMP_Text sceneNameText; // UI Text for scene name
        public Sprite[] sceneIcons; // Match order with availableScenes
        public string[] sceneNames; // Match order with availableScenes
        public AudioClip shuffleAndSelectClip; // Your 2-second audio with tick at end
        public AudioSource audioSource;

        [Header("Existing References")]
        public GameObject firePrefab, waterPrefab, gasPrefab;
        public Slider timerSlider; 
        public TextMeshPro timeText;
        [SerializeField] private GameObject endUI;
        [SerializeField] private TextMeshProUGUI scoreText;
        public TextMeshProUGUI debugText;

        [Header("Fire Scene Prefabs")]
        public GameObject extinguisherPrefab;
        public GameObject rugPrefab;
        public GameObject curtainPrefab;
        public GameObject fanPrefab;
        public GameObject waterBucketPrefab;
        public GameObject alcoholPrefab;

        public float sceneDuration = 60f;

        private int mistakes = 0;
        private float healthLeft = 100f;
        private float survivalTime = 0f;
        public static SceneManagerXR Instance { get; private set; }

        private SceneType currentScene;
        private float timer;
        private bool sceneActive = false;
        private Dictionary<string, List<AnchorInfo>> anchorDict = new();
        private DisasterScene currentDisaster;


        void Awake()
        {
            Instance = this;
        }

           void Start()
        {
            if (PlayerStats.Instance == null)
            {
                GameObject playerStatsObj = new GameObject("PlayerStats");
                playerStatsObj.AddComponent<PlayerStats>();
            }
            
            debugText.text = "Waiting for anchors...";
        }

        public void SetAnchors(Dictionary<string, List<AnchorInfo>> dict)
        {
            anchorDict = dict;
            debugText.text = "Anchors received.";
            StartCoroutine(ShuffleAndStartScene());
        }

        private IEnumerator ShuffleAndStartScene()
        {
            PlayShuffleSound();

            float shuffleTime = 2f;
            float updateInterval = 0.1f;
            float elapsedTime = 0f;
    
            int lastIndex = -1;

            while (elapsedTime < shuffleTime)
            {
                if (availableScenes == null || availableScenes.Length == 0 ||
                    sceneIcons == null || sceneIcons.Length == 0 ||
                    sceneNames == null || sceneNames.Length == 0)
                {
                    Debug.LogError("Scene data arrays (availableScenes, sceneIcons, or sceneNames) are empty or not assigned. Cannot shuffle scenes.");
                    yield break;
                }

                int randomIndex;
                do {
                    randomIndex = Random.Range(0, availableScenes.Length);
                } while (randomIndex == lastIndex && availableScenes.Length > 1);

                if (sceneIcon != null && randomIndex < sceneIcons.Length &&
                    sceneNameText != null && randomIndex < sceneNames.Length)
                {
                    sceneIcon.sprite = sceneIcons[randomIndex];
                    sceneNameText.text = sceneNames[randomIndex];
                    lastIndex = randomIndex;
                }
                else
                {
                    Debug.LogWarning($"Shuffle animation: UI references or arrays are not properly set up for index {randomIndex}. Skipping UI update this frame.");
                }

                elapsedTime += updateInterval;
                yield return new WaitForSeconds(updateInterval);
            }

            if (availableScenes != null && availableScenes.Length > 0)
            {
                currentScene = availableScenes[Random.Range(0, availableScenes.Length)];
                UpdateSceneUI(currentScene);
            }
            else
            {
                Debug.LogError("No available scenes to select for the final scene. Check 'availableScenes' array.");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
            sceneCanvas.SetActive(false);
            StartSelectedScene();
        }

        private void PlayShuffleSound()
        {
            // Play the entire clip (2 seconds with ending tick)
            audioSource.PlayOneShot(shuffleAndSelectClip); 
        }
        private void UpdateSceneUI(SceneType scene)
        {
            int index = System.Array.IndexOf(availableScenes, scene);
            sceneIcon.sprite = sceneIcons[index];
            sceneNameText.text = sceneNames[index];
        }

        private void StartSelectedScene()
        {
            timer = sceneDuration;
            sceneActive = true;

            if (timerSlider != null)
            {
                timerSlider.maxValue = 8f;
                timerSlider.minValue = 0f;
                timerSlider.value = 8f;
            }

            debugText.text = "Starting " + currentScene.ToString() + " scene";

            switch (currentScene)
            {
                case SceneType.Fire:
                    StartFireScene();
                    break;
                case SceneType.Water:
               //     StartWaterScene();
                    break;
                case SceneType.Gas:
                 //   StartGasScene();
                    break;
            }
        }

        private void StartFireScene()
        {
            GameObject fireSceneGO = new GameObject("FireScene");
            FireScene fireScene = fireSceneGO.AddComponent<FireScene>();

            fireScene.firePrefab = firePrefab;
            fireScene.extinguisherPrefab = extinguisherPrefab;
            fireScene.rugPrefab = rugPrefab;
            fireScene.curtainPrefab = curtainPrefab;
            fireScene.fanPrefab = fanPrefab;
            fireScene.waterBucketPrefab = waterBucketPrefab;
            fireScene.alcoholBottlePrefab = alcoholPrefab;

            currentDisaster = fireScene;
            fireScene.StartScene(anchorDict);
        }

        GameObject GetPrefabForScene(SceneType type)
        {
            return type switch
            {
                SceneType.Fire => firePrefab,
                SceneType.Water => waterPrefab,
                SceneType.Gas => gasPrefab,
                _ => null,
            };
        }

        public void ShowTip(string message)
        {
            // Optional: Floating text system
            Debug.Log("TIP: " + message);
            debugText.text = message;
        }

        public FireScene GetCurrentFireScene()
        {
            return currentDisaster as FireScene;
        }
    
        public void RegisterMistake() => mistakes++;


       public  void EndScene()
        {
            currentDisaster?.EndScene();
            sceneActive = false;
    
            // Get stats directly from PlayerStats
            survivalTime = sceneDuration - timer;
            healthLeft = PlayerStats.Instance.currentHealth;
            mistakes = PlayerStats.Instance.mistakes; // You'll need to add mistakes to PlayerStats

            int score = Mathf.RoundToInt(healthLeft + survivalTime - mistakes * 10);
            scoreText.text = $"🔥 Time Survived: {survivalTime:F1}s\n❤️ Health Left: {healthLeft:F0}\n🚨 Mistakes: {mistakes}\n🧠 Score: {score}";
            endUI.SetActive(true);
        }
        
        public void RestartScene()
        {
            PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
            PlayerStats.Instance.mistakes = 0;
            // ... other reset logic
        }
    }
}

