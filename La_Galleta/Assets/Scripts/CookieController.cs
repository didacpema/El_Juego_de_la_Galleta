using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class CookieController : MonoBehaviour
{
    public AudioSource clickSound;
    public GameObject clickParticleEffect;
    
    [Header("Click Animation")]
    [Range(0.5f, 0.95f)]
    public float clickShrinkSize = 0.8f;  // How small the cookie gets when clicked
    [Range(0.05f, 0.5f)]
    public float clickAnimDuration = 0.15f;  // Total animation duration
    
    // Used by GameManager to track cookies
    [HideInInspector]
    public float cookieCount = 0;
    
    private Vector3 originalScale;
    private Coroutine scaleAnimation;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnCookieClicked(SelectEnterEventArgs args)
    {
        // Add cookies to global counter instead of local counter
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCookies(1);
        }
        else
        {
            // Fallback if GameManager isn't available
            cookieCount++;
        }
        
        // Play feedback
        if (clickSound) clickSound.Play();
        
        // Handle particle effect with auto-cleanup
        if (clickParticleEffect) 
        {
            GameObject particleInstance = Instantiate(clickParticleEffect, transform.position, Quaternion.identity);
            
            // Get the ParticleSystem component to determine its duration
            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // Calculate total duration (including stop time)
                float totalDuration = particleSystem.main.duration;
                if (particleSystem.main.loop)
                {
                    totalDuration = particleSystem.main.startLifetime.constant;
                }
                
                // Add a small buffer to ensure all particles are finished
                Destroy(particleInstance, totalDuration + 0.5f);
            }
            else
            {
                // If no particle system found, destroy after a default time
                Destroy(particleInstance, 2.0f);
            }
        }
        
        // Cookie animation
        if (scaleAnimation != null)
            StopCoroutine(scaleAnimation);
            
        scaleAnimation = StartCoroutine(AnimateCookieClick());
    }
    
    IEnumerator AnimateCookieClick()
    {
        float elapsedTime = 0;
        
        // Shrink phase
        while (elapsedTime < clickAnimDuration/2)
        {
            float t = elapsedTime / (clickAnimDuration/2);
            float scale = Mathf.Lerp(1f, clickShrinkSize, t);
            transform.localScale = originalScale * scale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Expand phase
        elapsedTime = 0;
        while (elapsedTime < clickAnimDuration/2)
        {
            float t = elapsedTime / (clickAnimDuration/2);
            float scale = Mathf.Lerp(clickShrinkSize, 1f, t);
            transform.localScale = originalScale * scale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at exactly the original scale
        transform.localScale = originalScale;
        scaleAnimation = null;
    }
}