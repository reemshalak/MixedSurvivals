using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture;
using UnityEngine.UI;


public class SceneManagerXR : MonoBehaviour
{
    public GameObject firePrefab, waterPrefab, gasPrefab;
    
    [Header("UI")]
    public Slider timerSlider; 
    public TextMeshPro timeText;
    
    [Header("Fire Scene Prefabs")]
    public GameObject extinguisherPrefab;
    public GameObject rugPrefab;
    public GameObject curtainPrefab;
    public GameObject fanPrefab;
    public GameObject waterBucketPrefab;
    public GameObject alcoholPrefab;
    
    public float sceneDuration = 60f;
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

    void Start()
    {
        debugText.text = "Waiting for anchors...";
        // Do not call PickAndStartScene here — wait for SetAnchors instead
    }

    void Update()
    {
        if (sceneActive)
        {
            timeText.text = timer.ToString("F2") + " (s)";
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
        
        StringBuilder sb = new StringBuilder();

        foreach (var anchorList in anchorDict.Values)
        {
            foreach (var anchor in anchorList)
            {
                sb.AppendLine($"Label: {anchor.label}");
                sb.AppendLine($"Pos: {anchor.position}");
                sb.AppendLine($"Rot: {anchor.rotation.eulerAngles}");
            }
        }

        debugText.text = sb.ToString();
   
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

    void EndScene()
    {
        currentDisaster?.EndScene();
        sceneActive = false;
    }
}

