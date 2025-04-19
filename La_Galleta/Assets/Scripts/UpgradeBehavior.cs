using UnityEngine;

public class UpgradeBehavior : MonoBehaviour
{
    [Tooltip("Must match an ID in the GameManager's upgrade definitions")]
    public string upgradeId;
    
    // Optional: You can add visual effects, animations, or other behaviors here
    // that are specific to this upgrade type
    
    // For example, a grandfather clock might tick, or a cursor might occasionally animate
    
    // You could also add methods that GameManager can call to trigger special effects
    public void PlaySpecialEffect()
    {
        // Implement special effects for this upgrade
    }
}