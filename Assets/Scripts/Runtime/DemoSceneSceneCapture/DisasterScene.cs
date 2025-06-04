using System.Collections;
using System.Collections.Generic;
using PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture;
using UnityEngine;


public abstract class DisasterScene: MonoBehaviour
{
    public abstract SceneType Type { get; }
    public abstract void StartScene(Dictionary<string, List<AnchorInfo>> dict);
    public abstract void UpdateScene(float deltaTime);
    public abstract void EndScene();
}


