using System.Collections.Generic;
using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class AnchorRegistry : MonoBehaviour
    {
        public Dictionary<string, List<AnchorInfo>> anchorDict = new();

        public List<AnchorInfo> GetAnchorsByLabel(string label)
        {
            return anchorDict.TryGetValue(label, out var list) ? list : new List<AnchorInfo>();
        }
    }

}