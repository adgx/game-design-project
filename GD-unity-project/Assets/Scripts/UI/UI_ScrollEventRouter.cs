using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ScrollEventRouter : MonoBehaviour, IScrollHandler
{
    private ScrollRect scrollRect;

    void Awake()
    {
        // Find the ScrollRect component in the parent at creation time.
        // This way we don’t have to look for it every time.
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    // This method is called by the event system when the mouse wheel scrolls
    // while the cursor is above this GameObject.
    public void OnScroll(PointerEventData eventData)
    {
        // If we found a parent ScrollRect...
        if (scrollRect != null)
        {
            // ...forwards the scrolling event directly to him.
            scrollRect.OnScroll(eventData);
        }
    }
}