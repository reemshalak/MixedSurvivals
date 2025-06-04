using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class CoroutineRunner : MonoBehaviour
    {
        public static CoroutineRunner Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}