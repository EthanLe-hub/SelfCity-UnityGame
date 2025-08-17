using UnityEngine;
using UnityEngine.UI;

namespace LifeCraft.UI
{
    /// <summary>
    /// Debug script to help troubleshoot PlacedItemPrefab issues
    /// </summary>
    public class PlacedItemDebugger : MonoBehaviour
    {
        [ContextMenu("Debug PlacedItem Structure")]
        public void DebugPlacedItemStructure()
        {
            Debug.Log($"=== DEBUGGING {gameObject.name} ===");
            
            // Check root components
            var sr = GetComponent<SpriteRenderer>();
            var img = GetComponent<Image>();
            var placedItemUI = GetComponent<PlacedItemUI>();
            
            Debug.Log($"Root SpriteRenderer: {(sr != null ? $"enabled={sr.enabled}, sprite={sr.sprite?.name ?? "null"}" : "null")}");
            Debug.Log($"Root Image: {(img != null ? $"enabled={img.enabled}, sprite={img.sprite?.name ?? "null"}" : "null")}");
            Debug.Log($"PlacedItemUI: {(placedItemUI != null ? "found" : "null")}");
            
            // Check IconImage child
            var iconImage = transform.Find("IconImage");
            if (iconImage != null)
            {
                var iconImg = iconImage.GetComponent<Image>();
                Debug.Log($"IconImage child: enabled={iconImage.gameObject.activeInHierarchy}, Image component={(iconImg != null ? $"enabled={iconImg.enabled}, sprite={iconImg.sprite?.name ?? "null"}" : "null")}");
            }
            else
            {
                Debug.LogError("IconImage child not found!");
            }
            
            // Check Name child
            var nameChild = transform.Find("Name");
            if (nameChild != null)
            {
                var text = nameChild.GetComponent<TMPro.TextMeshProUGUI>();
                Debug.Log($"Name child: enabled={nameChild.gameObject.activeInHierarchy}, Text component={(text != null ? $"text='{text.text}'" : "null")}");
            }
            
            Debug.Log("=== END DEBUG ===");
        }
        
        [ContextMenu("Force Enable All Components")]
        public void ForceEnableAllComponents()
        {
            var sr = GetComponent<SpriteRenderer>();
            var img = GetComponent<Image>();
            var iconImage = transform.Find("IconImage")?.GetComponent<Image>();
            
            if (sr != null) sr.enabled = true;
            if (img != null) img.enabled = true;
            if (iconImage != null) iconImage.enabled = true;
            
            Debug.Log("Forced all components to enabled state");
        }

        [ContextMenu("Debug Rendering Issues")]
        public void DebugRenderingIssues()
        {
            Debug.LogError($"=== RENDERING DEBUG FOR {gameObject.name} ===");
            
            // Check Canvas and rendering
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.LogError($"Canvas found: renderMode={canvas.renderMode}, camera={canvas.worldCamera != null}");
            }
            else
            {
                Debug.LogError("No Canvas found in parent hierarchy!");
            }

            // Check CanvasRenderer
            var canvasRenderer = GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                Debug.LogError($"CanvasRenderer: cullTransparentMesh={canvasRenderer.cullTransparentMesh}");
            }

            // Check root components in detail
            var sr = GetComponent<SpriteRenderer>();
            var img = GetComponent<Image>();
            
            if (sr != null)
            {
                Debug.LogError($"SpriteRenderer: enabled={sr.enabled}, visible={sr.isVisible}, sprite={sr.sprite?.name ?? "null"}, color={sr.color}, sortingOrder={sr.sortingOrder}");
            }
            
            if (img != null)
            {
                Debug.LogError($"Image: enabled={img.enabled}, sprite={img.sprite?.name ?? "null"}, color={img.color}, raycastTarget={img.raycastTarget}");
            }

            // Check IconImage in detail
            var iconImage = transform.Find("IconImage");
            if (iconImage != null)
            {
                var iconImg = iconImage.GetComponent<Image>();
                var iconCanvasRenderer = iconImage.GetComponent<CanvasRenderer>();
                
                Debug.LogError($"IconImage GameObject: active={iconImage.gameObject.activeInHierarchy}, activeSelf={iconImage.gameObject.activeSelf}");
                if (iconImg != null)
                {
                    Debug.LogError($"IconImage component: enabled={iconImg.enabled}, sprite={iconImg.sprite?.name ?? "null"}, color={iconImg.color}, raycastTarget={iconImg.raycastTarget}");
                }
                if (iconCanvasRenderer != null)
                {
                    Debug.LogError($"IconImage CanvasRenderer: cullTransparentMesh={iconCanvasRenderer.cullTransparentMesh}");
                }
            }

            // Check if any component is actually visible
            bool anyVisible = false;
            if (sr != null && sr.enabled && sr.sprite != null) anyVisible = true;
            if (img != null && img.enabled && img.sprite != null) anyVisible = true;
            if (iconImage != null && iconImage.GetComponent<Image>()?.enabled == true && iconImage.GetComponent<Image>()?.sprite != null) anyVisible = true;
            
            Debug.LogError($"Any component visible: {anyVisible}");
            Debug.LogError("=== END RENDERING DEBUG ===");
        }

        [ContextMenu("Force IconImage Only")]
        public void ForceIconImageOnly()
        {
            var sr = GetComponent<SpriteRenderer>();
            var img = GetComponent<Image>();
            var iconImage = transform.Find("IconImage")?.GetComponent<Image>();
            
            // Disable root components
            if (sr != null) sr.enabled = false;
            if (img != null) img.enabled = false;
            
            // Enable only IconImage
            if (iconImage != null)
            {
                iconImage.enabled = true;
                Debug.LogError($"Forced IconImage only: enabled={iconImage.enabled}, sprite={iconImage.sprite?.name ?? "null"}");
            }
        }
    }
} 