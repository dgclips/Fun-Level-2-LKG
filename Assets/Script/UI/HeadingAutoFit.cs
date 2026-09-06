using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grows the heading pill/banner (this Image's RectTransform) wide enough to
/// fit its label text with breathing room on both sides, so a longer heading
/// - more characters, or a bigger font size - doesn't spill past the rounded
/// ends. Never shrinks below the banner's original/minimum width, so a short
/// heading still keeps the normal pill size.
///
/// Requires the banner's sprite to use Image Type = Sliced with a left/right
/// border wide enough to cover the rounded end-caps (see the heading
/// sprite's import settings) - otherwise stretching the width would squash
/// the rounded ends and the star artwork into ovals instead of just
/// widening the flat middle section.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class HeadingAutoFit : MonoBehaviour
{
    [SerializeField] private Text headingText;

    [Tooltip("Extra width added on each side of the text so it never touches the rounded ends/stars.")]
    [SerializeField] private float horizontalPadding = 90f;

    [Tooltip("The banner never shrinks below this width. Defaults to whatever width it was authored with (captured the first time this runs).")]
    [SerializeField] private float minWidth = -1f;

    [Tooltip("0 = unclamped. Otherwise the banner stops growing here and the text wraps/overflows instead.")]
    [SerializeField] private float maxWidth = 0f;

    private RectTransform _rect;
    private string _lastText;
    private float _lastFontSize = -1f;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();

        if (headingText == null)
            headingText = GetComponentInChildren<Text>();

        if (minWidth < 0f)
            minWidth = _rect.sizeDelta.x;
    }

    private void OnEnable()
    {
        Refresh(force: true);
    }

    private void Update()
    {
        // Cheap change check every frame so this keeps up whether the
        // heading text is edited live in the Inspector, changed by another
        // script at runtime, or its font size is tweaked.
        Refresh(force: false);
    }

    /// <summary>
    /// Recomputes the banner width from the heading text's natural
    /// (unwrapped) width. Pass force:true to skip the "did anything change"
    /// check (e.g. right after enabling).
    /// </summary>
    public void Refresh(bool force = true)
    {
        if (headingText == null)
            return;

        if (_rect == null)
            _rect = GetComponent<RectTransform>();

        if (minWidth < 0f)
            minWidth = _rect.sizeDelta.x;

        if (!force && headingText.text == _lastText && Mathf.Approximately(headingText.fontSize, _lastFontSize))
            return;

        _lastText = headingText.text;
        _lastFontSize = headingText.fontSize;

        float textWidth = headingText.preferredWidth;
        float targetWidth = Mathf.Max(minWidth, textWidth + horizontalPadding * 2f);

        if (maxWidth > 0f)
            targetWidth = Mathf.Min(targetWidth, maxWidth);

        Vector2 size = _rect.sizeDelta;

        if (!Mathf.Approximately(size.x, targetWidth))
        {
            size.x = targetWidth;
            _rect.sizeDelta = size;
        }
    }
}
