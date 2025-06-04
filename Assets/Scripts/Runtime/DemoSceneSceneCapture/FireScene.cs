using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

            // 🔥 Start fire only on Sofa
            // if (anchorDict.TryGetValue("Sofa", out var sofaAnchors))
            // {
            //     foreach (var anchor in sofaAnchors)
            //     {
            //         GameObject fire = GameObject.Instantiate(firePrefab, anchor.position, anchor.rotation);
            //         spawnedObjects.Add(fire);
            //         burningAnchors.Add(anchor);
            //     }
            // }

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

                            // Optional: show UI tip here ("🔥 Fire is spreading!")
                        }
                    }
                }
            }

            burningAnchors.AddRange(newFires);
        }

        void TrySpawnOn(string label, GameObject prefab)
        {
            if (!anchorDict.TryGetValue(label, out var anchors)) return;

            foreach (var anchor in anchors)
            {
                GameObject obj;
                if(anchor.label == "Window")
                  obj = GameObject.Instantiate(prefab, anchor.position, anchor.rotation);
  
                else
                 obj = GameObject.Instantiate(prefab, anchor.position,  prefab.transform.rotation);
                spawnedObjects.Add(obj);

                // Optional: scaling animation effect
               // obj.transform.localScale = Vector3.zero;
           //     StartCoroutine(ScaleUp(obj));


                // Optional: outline or glow shader setup
                // Optional: floating text "🪄 Spawned: Water Bucket"
            }
        }

        
        private IEnumerator ScaleUp(GameObject obj)
        {
            float duration = 0.5f;
            float time = 0f;
            Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f);
            obj.transform.localScale = Vector3.zero;

            while (time < duration)
            {
                obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, targetScale, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            obj.transform.localScale = targetScale;
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
