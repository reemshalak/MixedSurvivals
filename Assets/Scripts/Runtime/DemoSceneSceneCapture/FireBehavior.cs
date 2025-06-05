using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class FireBehaviour : MonoBehaviour
    {
        [Header("Fire Settings")]
        public float damageRadius = 2f;
        public float healthDrainPerSecond = 5f;
        public float extinguisherExtinguishSpeed = 2f;
        public float waterExtinguishSpeed = 0.5f;
        public GameObject smokeVFX;
        
        [Header("Particle Settings")]
        [SerializeField] private float minEmissionRate = 5f;
        
        private ParticleSystem fireParticles;
        private bool isExtinguishing = false;
        private float currentExtinguishRate = 0f;
        private float initialEmissionRate;
        private float fireHealth = 100f;

        private void Start()
        {
            fireParticles = GetComponentInChildren<ParticleSystem>();
            if (fireParticles != null)
            {
                var emission = fireParticles.emission;
                initialEmissionRate = emission.rateOverTime.constant;
                fireHealth = initialEmissionRate;
            }
        }

        private void Update()
        {
            // Damage player when nearby
            if (Vector3.Distance(Camera.main.transform.position, transform.position) < damageRadius)
            {
                PlayerStats.Instance?.DamageOverTime(healthDrainPerSecond * Time.deltaTime);
            }

            // Handle extinguishing process
            if (isExtinguishing && fireParticles != null)
            {
                ReduceFireIntensity();
            }
        }

        private void ReduceFireIntensity()
        {
            var emission = fireParticles.emission;
            fireHealth -= currentExtinguishRate * Time.deltaTime;
            float healthPercentage = fireHealth / initialEmissionRate;
            
            // Update particle emission
            emission.rateOverTime = Mathf.Lerp(minEmissionRate, initialEmissionRate, healthPercentage);
            
            // Visual feedback
            if (healthPercentage < 0.3f)
            {
                var main = fireParticles.main;
                main.startColor = Color.Lerp(Color.red, Color.yellow, healthPercentage * 3f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Alcohol"))
            {
                SceneManagerXR.Instance.ShowTip("💥 Alcohol spread the fire!");
                FireScene scene = SceneManagerXR.Instance.GetCurrentFireScene();
                scene?.ForceSpread(transform.position);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                StartExtinguishing(waterExtinguishSpeed);
                if (!isExtinguishing)
                {
                    SceneManagerXR.Instance.ShowTip("💧 Water is putting out the fire!");
                }
            } 
            if (other.CompareTag("Extinguisher"))
            {
                StartExtinguishing(extinguisherExtinguishSpeed);
                if (!isExtinguishing)
                {
                    SceneManagerXR.Instance.ShowTip("🧯 Extinguisher is putting out the fire quickly!");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water") || other.CompareTag("Extinguisher"))
            {
                StopExtinguishing();
            }
        }

        private void StartExtinguishing(float rate)
        {
            if (!isExtinguishing)
            {
                isExtinguishing = true;
                currentExtinguishRate = rate;
                SpawnSmokeEffect();
            }
            else if (rate > currentExtinguishRate)
            {
                currentExtinguishRate = rate;
            }
        }

        private void SpawnSmokeEffect()
        {
            if (smokeVFX == null) return;
            
            GameObject smokeInstance = Instantiate(smokeVFX, transform.position, Quaternion.identity);
            ParticleSystem ps = smokeInstance.GetComponent<ParticleSystem>();
            if (ps == null) return;

            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f) },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = grad;
        }

        private void StopExtinguishing()
        {
            isExtinguishing = false;
            currentExtinguishRate = 0f;
        }

        private void DestroyFire()
        {
            if (fireParticles != null)
            {
                fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            
            SceneManagerXR.Instance.ShowTip("✅ Fire extinguished!");
            Destroy(gameObject, 1f);
            
            // Optional: Add score or analytics here
           // GameManager.Instance?.AddScore(100);
        }
    }
}