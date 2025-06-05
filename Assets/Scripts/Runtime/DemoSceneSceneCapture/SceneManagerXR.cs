using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class SceneManagerXR : MonoBehaviour
    {
        public GameObject firePrefab, waterPrefab, gasPrefab;
    
        [Header("UI")]
        public Slider timerSlider; 
        public TextMeshPro timeText;
        [SerializeField] private GameObject endUI;
        [SerializeField] private TextMeshProUGUI scoreText;
    
    
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
        public TextMeshProUGUI debugText;

        private Dictionary<string, List<AnchorInfo>> anchorDict = new(); // no AnchorRegistry

        private DisasterScene currentDisaster;

        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Initialize player stats if they don't exist
            if (PlayerStats.Instance == null)
            {
                GameObject playerStatsObj = new GameObject("PlayerStats");
                playerStatsObj.AddComponent<PlayerStats>();
            }
            // Auto-find the health bar if not set (optional)
            if (PlayerStats.Instance.healthBar == null)
            {
                var progressBarObj = GameObject.Find("UI ProgressBar"); // Name your ProgressBar GameObject
                if (progressBarObj != null)
                {
                    PlayerStats.Instance.healthBar = progressBarObj.GetComponent<ProgressBar.Script.ProgressBar>();
                }
            }
    
            debugText.text = "Waiting for anchors...";
        }

        void Update()
        {
            if (sceneActive)
            {
                timeText.text = ((Mathf.FloorToInt(timer)).ToString("F2") + " (s)");
                timer -= Time.deltaTime;
                currentDisaster?.UpdateScene(Time.deltaTime);
                if (timerSlider != null)
                {
                    float normalizedTime = Mathf.Clamp01((sceneDuration - timer) / sceneDuration);
                    timerSlider.value = Mathf.Lerp(8f, 0f, normalizedTime);
                }

                if (timer <= 0f)
                {
                    EndScene();
                }
            }
        }
    
        public void SetAnchors(Dictionary<string, List<AnchorInfo>> dict)
        {
            anchorDict = dict;
            debugText.text = "Anchors received.";
            PickAndStartScene(); // now safe to start scene
        }

        public void PickAndStartScene()
        {
            timer = sceneDuration;
            sceneActive = true;

            if (timerSlider != null)
            {
                timerSlider.maxValue = 8f;
                timerSlider.minValue = 0f;
                timerSlider.value = 8f;
            }

            Debug.Log("Picking Start Scene");
            debugText.text = "Picking scene";
            currentScene = SceneType.Fire; // or randomize if needed

            // StringBuilder sb = new StringBuilder();
            //
            // foreach (var anchorList in anchorDict.Values)
            // {
            //     foreach (var anchor in anchorList)
            //     {
            //         sb.AppendLine($"Label: {anchor.label}");
            //         sb.AppendLine($"Pos: {anchor.position}");
            //         sb.AppendLine($"Rot: {anchor.rotation.eulerAngles}");
            //     }
            // }
            //
            // debugText.text = sb.ToString();
   
            // foreach (var anchorList in anchorDict.Values)
            // {
            //     foreach (var anchor in anchorList)
            //     {
            //         if (anchor.label != "Floor" && anchor.label != "Sofa") continue; // <--
            //         Debug.Log($"Spawning at anchor: {anchor.label}");
            //         GameObject prefabToSpawn = GetPrefabForScene(currentScene);
            //         Instantiate(prefabToSpawn, anchor.position, anchor.rotation);
            //     }
            // }
        
            if (currentScene == SceneType.Fire)
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

