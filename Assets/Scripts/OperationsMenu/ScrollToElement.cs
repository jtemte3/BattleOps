using UnityEngine;
using UnityEngine.UI;

public class ScrollToElement : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform targetElement; // The element to scroll to

    public void CenterOnItem()
    {
        if (scrollRect == null || targetElement == null || scrollRect.content == null || scrollRect.viewport == null)
        {
            Debug.LogError("ScrollRect, target element, content, or viewport not assigned.");
            return;
        }

        // Calculate the target element's position relative to the content's top
        float elementXInContent = targetElement.anchoredPosition.x;

        // Calculate the center of the viewport
        float viewportCenterX = scrollRect.viewport.rect.width / 2f;

        // Calculate the new content Y position to center the target element
        // This moves the content so the element's center aligns with the viewport's center
        float newContentX = -elementXInContent + viewportCenterX - (targetElement.rect.width / 2f);

        // Apply the new position to the content's anchored position
        // Ensure to clamp the value to prevent scrolling beyond content boundaries
        Vector2 newAnchoredPosition = scrollRect.content.anchoredPosition;
        newAnchoredPosition.x = newContentX;
        scrollRect.content.anchoredPosition = newAnchoredPosition;
    }
}