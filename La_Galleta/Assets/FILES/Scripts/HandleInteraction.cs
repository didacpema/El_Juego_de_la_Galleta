using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class HandInteraction : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public Camera arCamera;
    
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
            
            // Raycast against the cookie
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Cookie cookie = hit.collider.GetComponent<Cookie>();
                if (cookie != null)
                {
                    cookie.ClickCookie();
                }
            }
            
            // Alternative: Raycast against AR planes (simulating hand position)
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                // You could use this to detect when hand is "close enough" to cookie
                // For simplicity, we're just using direct touch in this version
            }
        }
    }
}