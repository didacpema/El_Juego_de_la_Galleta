using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public int totalCookies = 0;
    public int cookiesPerClick = 1;
    public int autoClickers = 0;
    public float autoClickInterval = 1f;
    
    public Text cookieText;
    public GameObject cookiePrefab;
    public GameObject grandmaPrefab;
    
    private ARTrackedImageManager trackedImageManager;
    private float autoClickTimer = 0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    
    void Update()
    {
        if (autoClickers > 0)
        {
            autoClickTimer += Time.deltaTime;
            if (autoClickTimer >= autoClickInterval)
            {
                AddCookies(autoClickers);
                autoClickTimer = 0f;
            }
        }
    }
    
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (trackedImage.referenceImage.name == "YourCookieImageName")
            {
                SpawnCookie(trackedImage.transform);
            }
        }
    }
    
    void SpawnCookie(Transform imageTransform)
    {
        // Clear existing cookies
        foreach (Transform child in imageTransform)
        {
            Destroy(child.gameObject);
        }
        
        // Spawn new cookie
        GameObject cookie = Instantiate(cookiePrefab, imageTransform);
        cookie.transform.localPosition = Vector3.zero;
        cookie.transform.localRotation = Quaternion.identity;
    }
    
    public void AddCookies(int amount)
    {
        totalCookies += amount;
        UpdateUI();
    }
    
    public void BuyUpgrade(int cost, int newClickValue)
    {
        if (totalCookies >= cost)
        {
            totalCookies -= cost;
            cookiesPerClick = newClickValue;
            UpdateUI();
        }
    }
    
    public void BuyAutoClicker(int cost)
    {
        if (totalCookies >= cost)
        {
            totalCookies -= cost;
            autoClickers++;
            UpdateUI();
            
            // Spawn a grandma
            if (grandmaPrefab)
            {
                Instantiate(grandmaPrefab, 
                    Random.insideUnitSphere * 0.5f + Vector3.up * 0.5f, 
                    Quaternion.identity);
            }
        }
    }
    
    void UpdateUI()
    {
        if (cookieText)
        {
            cookieText.text = "Cookies: " + totalCookies;
        }
    }
}