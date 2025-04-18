using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections;

public class CookieController : MonoBehaviour
{
    public float cookieCount = 0;
    public TextMeshProUGUI counterText;
    public AudioSource clickSound;
    public GameObject clickParticleEffect;
    
    [Header("Click Animation")]
    [Range(0.5f, 0.95f)]
    public float clickShrinkSize = 0.8f;  // How small the cookie gets when clicked
    [Range(0.05f, 0.5f)]
    public float clickAnimDuration = 0.15f;  // Total animation duration
    
    private Vector3 originalScale;
    private Coroutine scaleAnimation;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnCookieClicked(SelectEnterEventArgs args)
    {
        cookieCount++;
        UpdateCounterDisplay();
        
        // Play feedback
        if (clickSound) clickSound.Play();
        if (clickParticleEffect) Instantiate(clickParticleEffect, transform.position, Quaternion.identity);
        
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
    
    public void UpdateCounterDisplay()
    {
        if (counterText != null)
        {
            counterText.text = cookieCount.ToString("N0");
        }
    }
}