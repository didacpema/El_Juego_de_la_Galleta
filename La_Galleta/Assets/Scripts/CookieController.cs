using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CookieController : MonoBehaviour
{
    public long cookieCount = 0;
    public TMPro.TextMeshProUGUI counterText;
    
    public void OnCookieClicked(SelectEnterEventArgs args)
    {
        cookieCount++;
        UpdateCounterDisplay();
        
        // Play particle effect, sound, or other feedback
        transform.localScale = Vector3.one * 0.9f;
        Invoke("ResetScale", 0.1f);
    }
    
    private void ResetScale()
    {
        transform.localScale = Vector3.one;
    }
    
    private void UpdateCounterDisplay()
    {
        if (counterText != null)
            counterText.text = cookieCount.ToString();
    }
}