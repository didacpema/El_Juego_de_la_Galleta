using UnityEngine;

public class Cookie : MonoBehaviour
{
    public int clickValue = 1;
    private GameManager gameManager;
    private Vector3 originalScale;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        originalScale = transform.localScale;
    }
    
    void OnMouseDown()
    {
        // This works for in-editor testing
        ClickCookie();
    }
    
    public void ClickCookie()
    {
        gameManager.AddCookies(clickValue);
        StartCoroutine(ClickAnimation());
    }
    
    private System.Collections.IEnumerator ClickAnimation()
    {
        transform.localScale = originalScale * 0.9f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
}