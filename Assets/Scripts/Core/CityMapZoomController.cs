using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LifeCraft.Core
{
    public class CityMapZoomController : MonoBehaviour
    {
        [Header("Assign the RectTransform of your city map (e.g., CityScrollView)")]
        public RectTransform cityMapRect;

        [Header("Zoom/Pan Settings")]
        public float zoomDuration = 0.5f;

        // Define target positions and scales for each region (customize as needed)
        [System.Serializable]
        public struct RegionZoomData
        {
            public string regionName;
            public Vector2 anchoredPosition;
            public float scale;
        }

        public RegionZoomData[] regions;

        // Optional: Button references for convenience
        public Button healthHarborButton;
        public Button mindPalaceButton;
        public Button creativeCommonsButton;
        public Button socialSquareButton;

        public GameObject regionButtonGroup; // Group for "Back to Overview" and "Edit Region" buttons; assigned in the Inspector. 

        private Coroutine zoomCoroutine;

        void Start()
        {
            // Hook up buttons if assigned
            if (healthHarborButton != null)
                healthHarborButton.onClick.AddListener(() => ZoomToRegion("Health Harbor"));
            if (mindPalaceButton != null)
                mindPalaceButton.onClick.AddListener(() => ZoomToRegion("Mind Palace"));
            if (creativeCommonsButton != null)
                creativeCommonsButton.onClick.AddListener(() => ZoomToRegion("Creative Commons"));
            if (socialSquareButton != null)
                socialSquareButton.onClick.AddListener(() => ZoomToRegion("Social Square"));
        }

        public void ZoomToRegion(string regionName)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                if (regions[i].regionName == regionName)
                {
                    ZoomTo(regions[i].anchoredPosition, regions[i].scale);

                    // Show the region button group (e.g., "Back to Overview" and "Edit Region" buttons) only if not in Overview:
                    if (regionName == "Overview")
                    {
                        regionButtonGroup.SetActive(false); // Hide the button group when zoomed out to overview. 
                    }

                    else
                    {
                        regionButtonGroup.SetActive(true); // Show the button group when zoomed into a specific region. 
                    }
                    
                    return;
                }
            }
            Debug.LogWarning("Region not found: " + regionName);
        }

        public void ZoomTo(Vector2 targetPosition, float targetScale)
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(AnimateZoom(targetPosition, targetScale));
        }

        public void ZoomToOverview()
        {
            // "Overview" region in the CityScrollView GameObject (under "City Map Zoom Controller" script component) is the default position and scale. 
            ZoomToRegion("Overview"); 
        }

        private IEnumerator AnimateZoom(Vector2 targetPosition, float targetScale)
        {
            Vector2 startPos = cityMapRect.anchoredPosition;
            float startScale = cityMapRect.localScale.x;
            float elapsed = 0f;

            while (elapsed < zoomDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / zoomDuration);
                cityMapRect.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
                float scale = Mathf.Lerp(startScale, targetScale, t);
                cityMapRect.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            cityMapRect.anchoredPosition = targetPosition;
            cityMapRect.localScale = new Vector3(targetScale, targetScale, 1f);
        }
    }
}