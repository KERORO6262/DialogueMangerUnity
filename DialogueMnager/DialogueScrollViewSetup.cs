using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueScrollViewSetup : MonoBehaviour
{
    public ScrollRect scrollRect;
    public TMP_Text diaText;

    void Start()
    {
        // Correctly register for text change events if available or ensure the existing method effectively captures all relevant text changes.
        diaText.RegisterDirtyLayoutCallback(UpdateContentSize);
    }

    void OnDisable()
    {
        // Ensure to unregister callbacks to avoid memory leaks or unwanted behavior when the component is disabled.
        diaText.UnregisterDirtyLayoutCallback(UpdateContentSize);
    }

    void UpdateContentSize()
    {
        // Wait for the next frame to ensure all layout calculations are up-to-date
        StartCoroutine(UpdateContentSizeNextFrame());
    }

    IEnumerator UpdateContentSizeNextFrame()
    {
        yield return null; // Wait for the next frame

        Canvas.ForceUpdateCanvases(); // Force a canvas update to ensure all layout information is current

        // Get the updated sizes
        float requiredHeight = diaText.preferredHeight;
        float requiredWidth = diaText.preferredWidth;

        // Update the content size
        RectTransform contentRectTransform = scrollRect.content;
        contentRectTransform.sizeDelta = new Vector2(requiredWidth, requiredHeight);

        // Enable or disable scrolling based on content size
        scrollRect.vertical = requiredHeight > scrollRect.GetComponent<RectTransform>().rect.height;
        scrollRect.horizontal = requiredWidth > scrollRect.GetComponent<RectTransform>().rect.width;
    }
}