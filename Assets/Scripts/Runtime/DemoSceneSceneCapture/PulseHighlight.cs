using UnityEngine;

public class PulseHighlight : MonoBehaviour
{
    [Header("Settings")]
    public float pulseSpeed = 2f;
    public float sizeOffset = 0.1f; // How much to grow/shrink
    public Color highlightColor = Color.yellow;

    [Header("References")]
    public Renderer targetRenderer;
    
    private Vector3 originalScale;
    private Material[] originalMaterials;
    private Material[] highlightMaterials;
    private bool isPulsing;

    void Awake()
    {
        // Store original scale FIRST
        originalScale = transform.localScale;
        
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        // Cache original materials
        originalMaterials = targetRenderer.materials;
        
        // Create highlight materials
        highlightMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            highlightMaterials[i] = new Material(originalMaterials[i]);
            highlightMaterials[i].color = highlightColor;
        }
    }

    void Update()
    {
        if (!isPulsing) return;
        
        // Pulse scale while preserving original proportions
        float pulseAmount = Mathf.Sin(Time.time * pulseSpeed) * sizeOffset;
        transform.localScale = originalScale * (1f + pulseAmount);
    }

    public void StartPulsing()
    {
        isPulsing = true;
        targetRenderer.materials = highlightMaterials;
    }

    public void StopPulsing()
    {
        isPulsing = false;
        transform.localScale = originalScale; // Reset to exact original scale
        targetRenderer.materials = originalMaterials;
    }

    void OnDestroy()
    {
        // Clean up materials
        if (highlightMaterials != null)
        {
            foreach (var mat in highlightMaterials)
            {
                if (mat != null) Destroy(mat);
            }
        }
    }
}