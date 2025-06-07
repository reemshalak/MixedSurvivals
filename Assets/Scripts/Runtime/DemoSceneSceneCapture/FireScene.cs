using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class FireScene : DisasterScene
    {
        public GameObject firePrefab;
        public GameObject extinguisherPrefab;
        public GameObject rugPrefab;
        public GameObject curtainPrefab;
        public GameObject waterBucketPrefab;
        public GameObject alcoholBottlePrefab;
        public GameObject fanPrefab;

        private List<GameObject> spawnedObjects = new();
        private List<AnchorInfo> burningAnchors = new();
        private Dictionary<string, List<AnchorInfo>> anchorDict;

        private float spreadInterval = 5f;
        private float spreadTimer = 0f;

        public override SceneType Type => SceneType.Fire;
    public void Start()
    {
        
        }
        public override void StartScene(Dictionary<string, List<AnchorInfo>> anchorDict)
        {
            this.anchorDict = anchorDict;
            burningAnchors.Clear();

            //🔥 Start fire only on Sofa
            if (anchorDict.TryGetValue("Sofa", out var sofaAnchors))
            {
                foreach (var anchor in sofaAnchors)
                {
                    GameObject fire = GameObject.Instantiate(firePrefab, anchor.position, anchor.rotation);
                    spawnedObjects.Add(fire);
                    burningAnchors.Add(anchor);
                }
            }

            // 🎯 Spawn interactive objects on appropriate anchors
            TrySpawnOn("Floor", rugPrefab);
            TrySpawnOn("Window", curtainPrefab);
            TrySpawnOn("Floor", waterBucketPrefab);
            TrySpawnOn("Table", waterBucketPrefab);
            TrySpawnOn("Wall", extinguisherPrefab);
            TrySpawnOn("Table", alcoholBottlePrefab);
            TrySpawnOn("Wall", alcoholBottlePrefab);
           TrySpawnOn("Floor", fanPrefab);
        }

        public override void UpdateScene(float deltaTime)
        {
            spreadTimer -= deltaTime;
            if (spreadTimer <= 0f)
            {
                TrySpreadFire();
                spreadTimer = spreadInterval;
            }
        }

        void TrySpreadFire()
        {
            List<AnchorInfo> newFires = new();

            foreach (var burning in burningAnchors)
            {
                foreach (var list in anchorDict.Values)
                {
                    foreach (var anchor in list)
                    {
                        if (burningAnchors.Contains(anchor)) continue;

                        if (Vector3.Distance(burning.position, anchor.position) < 2f &&
                            (anchor.label == "Sofa" || anchor.label == "Table" || anchor.label == "Wall"))
                        {
                            GameObject fire = GameObject.Instantiate(firePrefab, anchor.position, anchor.rotation);
                            spawnedObjects.Add(fire);
                            newFires.Add(anchor);

                        }
                    }
                }
            }

            burningAnchors.AddRange(newFires);
        }

        // Add this to your TrySpawnOn method
        void TrySpawnOn(string label, GameObject prefab)
        {
            if (!anchorDict.TryGetValue(label, out var anchors)) return;

            foreach (var anchor in anchors)
            {
                // Instantiate with original rotation and scale
                GameObject obj = Instantiate(prefab, anchor.position, prefab.transform.rotation);
                obj.transform.localScale = prefab.transform.localScale; // Ensure original scale
                spawnedObjects.Add(obj);
        
                // Add pulse effect
                var pulse = obj.AddComponent<PulseHighlight>();
                pulse.highlightColor = GetColorForPrefab(prefab);
                pulse.StartPulsing();
        
                // Optional: Auto-stop after delay
                StartCoroutine(StopPulsingAfterTime(pulse, 3f));
            }
        }

        private Color GetColorForPrefab(GameObject prefab)
        {
            if (prefab == waterBucketPrefab) return Color.blue;
            if (prefab == extinguisherPrefab) return Color.red;
            if (prefab == alcoholBottlePrefab) return Color.yellow;
            return Color.green; // Default
        }

        private IEnumerator StopPulsingAfterTime(PulseHighlight pulse, float delay)
        {
            yield return new WaitForSeconds(delay);
            pulse.StopPulsing();
        }

        private IEnumerator FadeOutline(Outline outline, float duration = 2f)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                outline.effectColor = new Color(outline.effectColor.r,
                    outline.effectColor.g,
                    outline.effectColor.b,
                    Mathf.Lerp(1f, 0f, elapsed/duration));
                yield return null;
            }
            Destroy(outline);
        }

 
        
        public void ForceSpread(Vector3 position)
        {
            List<AnchorInfo> newFires = new();

            foreach (var kvp in anchorDict)
            {
                foreach (var anchor in kvp.Value)
                {
                    // Skip if already burning or too far
                    if (burningAnchors.Contains(anchor) || 
                        Vector3.Distance(position, anchor.position) > 2f)
                        continue;

                    // Only spread to flammable objects
                    if (IsFlammable(anchor.label))
                    {
                        GameObject fire = Instantiate(
                            firePrefab, 
                            anchor.position, 
                            Quaternion.identity, // Use identity rotation unless needed
                            transform // Parent to the scene for organization
                        );
                
                        spawnedObjects.Add(fire);
                        newFires.Add(anchor);
                
                        // Optional visual feedback
                        Debug.Log($"🔥 Fire spread to {anchor.label} at {anchor.position}");
                    }
                }
            }

            burningAnchors.AddRange(newFires);
        }

        private bool IsFlammable(string label)
        {
            // Define what objects can catch fire
            return label == "Sofa" || 
                   label == "Table" || 
                   label == "Wall" || 
                   label == "Curtain" || 
                   label == "Rug";
        }

        
        public override void EndScene()
        {
            foreach (var obj in spawnedObjects)
            {
                GameObject.Destroy(obj);
            }

            burningAnchors.Clear();
        }
    }
}
