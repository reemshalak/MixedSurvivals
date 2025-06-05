using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;
    
        [Header("UI Reference")]
        public ProgressBar.Script.ProgressBar healthBar;

        public static PlayerStats Instance { get; private set; }
        public int mistakes;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject); // Optional: if you want stats to persist
            }
        }

        void Start()
        {
            // Safety check - helps debug missing references
            if (healthBar == null)
            {
                Debug.LogError("ProgressBar reference not set in PlayerStats!", this);
            }
            currentHealth = maxHealth;
            UpdateHealthUI();
        }

    

        public void RegisterMistake() 
        {
            mistakes++;
            // Optional: visual/audio feedback
        }
        public void DamageOverTime(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            UpdateHealthUI();
            
            if (currentHealth <= 0) Die();
        }

        private void UpdateHealthUI()
        {
            float healthPercentage = (currentHealth / maxHealth) * 100f;
            healthBar.BarValue = Mathf.FloorToInt(healthPercentage);
         //   healthBar.Title = $"HEALTH: {Mathf.RoundToInt(currentHealth)}";
        }

        private void Die()
        {
            Debug.Log("Player died!");
            SceneManagerXR.Instance.EndScene(); // Trigger end game
        }
    }
}