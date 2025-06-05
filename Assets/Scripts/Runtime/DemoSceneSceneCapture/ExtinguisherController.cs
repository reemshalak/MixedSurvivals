// Attach this to your extinguisher GameObject

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.DemoSceneSceneCapture
{
    public class ExtinguisherController : MonoBehaviour
    {
        public ParticleSystem extinguisherParticles;
        public AudioSource extinguisherSound;
        public float extinguisherStrength = 2f; // Should match FireBehaviour's value

        private void Start()
        {
            var interactable = GetComponent<XRGrabInteractable>();
            interactable.activated.AddListener(StartExtinguishing);
            interactable.deactivated.AddListener(StopExtinguishing);
        }

        private void StartExtinguishing(ActivateEventArgs args)
        {
            extinguisherParticles.Play();
            extinguisherSound.Play();
        }

        private void StopExtinguishing(DeactivateEventArgs args)
        {
            extinguisherParticles.Stop();
            extinguisherSound.Stop();
        }
    }
}