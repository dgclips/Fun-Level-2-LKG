using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fades each item in a scrolling grid/list toward transparent as it nears
/// or crosses the viewport's top/bottom edge, instead of overlaying a
/// tinted gradient panel on top of the scroll view. Since nothing is drawn
/// over the content, there is no color-matching problem and no visible
/// "band"/seam - items simply dissolve into whatever background is
/// actually behind them.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollEdgeFade : MonoBehaviour
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [Tooltip("Distance (in pixels) from each edge over which an item fades from fully visible to fully transparent.")]
    [SerializeField] private float fadeDistance = 140f;

    [Tooltip("Max alpha change per second when easing toward the target fade. Keeps the fade feeling gradual even during a fast flick, where the target itself can jump a lot in one frame.")]
    [SerializeField] private float maxAlphaChangePerSecond = 4f;

    private ScrollRect _scrollRect;
    private readonly Vector3[] _viewportCorners = new Vector3[4];
    private readonly Vector3[] _childCorners = new Vector3[4];

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();

        if (viewport == null)
            viewport = _scrollRect.viewport;

        if (content == null)
            content = _scrollRect.content;
    }

    private void OnEnable()
    {
        if (_scrollRect != null)
            _scrollRect.onValueChanged.AddListener(OnScroll);

        UpdateFades();
    }

    private void OnDisable()
    {
        if (_scrollRect != null)
            _scrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    private void OnScroll(Vector2 _)
    {
        UpdateFades();
    }

    private void LateUpdate()
    {
        // Content can change (buttons spawned/resized) without a scroll event firing.
        UpdateFades();
    }

    private void UpdateFades()
    {
        if (viewport == null || content == null || fadeDistance <= 0f)
            return;

        viewport.GetWorldCorners(_viewportCorners);
        float viewportBottom = _viewportCorners[0].y;
        float viewportTop = _viewportCorners[1].y;

        // NOTE: this relies on the grid's top padding being larger than
        // fadeDistance, so the first row rests comfortably inside the "fully
        // opaque" zone instead of starting right at the edge of it (which
        // caused fading to snap in abruptly the instant scrolling began,
        // instead of easing in only once a row actually nears the edge).

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;
            if (child == null)
                continue;

            child.GetWorldCorners(_childCorners);
            float childBottom = _childCorners[0].y;
            float childTop = _childCorners[1].y;

            float alpha = 1f;

            // Use each item's OWN near edge (not its center) so alpha reaches 0
            // exactly as that edge reaches the viewport boundary - i.e. before
            // the Mask component would start physically clipping it. Fading
            // based on the center let a large item's edge get hard-clipped by
            // the mask while it was still only partially transparent, which
            // read as an abrupt cutoff instead of a smooth dissolve.
            float distFromTop = viewportTop - childTop;
            if (distFromTop < fadeDistance)
            {
                alpha = Mathf.Min(alpha, Ease01(Mathf.Clamp01(distFromTop / fadeDistance)));
            }

            float distFromBottom = childBottom - viewportBottom;
            if (distFromBottom < fadeDistance)
            {
                alpha = Mathf.Min(alpha, Ease01(Mathf.Clamp01(distFromBottom / fadeDistance)));
            }

            CanvasGroup group = child.GetComponent<CanvasGroup>();
            if (group == null)
                group = child.gameObject.AddComponent<CanvasGroup>();

            // Ease the actual displayed alpha toward the target over real time,
            // instead of snapping to it every frame. During a fast flick the
            // target can jump most of the way to 0 in a single frame (since the
            // fade zone is only a small fraction of the scroll distance covered
            // per frame at speed) - smoothing this keeps the dissolve looking
            // gradual regardless of how fast the user scrolls.
            group.alpha = Mathf.MoveTowards(group.alpha, alpha, maxAlphaChangePerSecond * Time.unscaledDeltaTime);

            // Avoid letting a nearly-invisible item still eat clicks/drags.
            group.blocksRaycasts = group.alpha > 0.05f;
        }
    }

    private static float Ease01(float t)
    {
        // Smoothstep for a gentler fade than a straight linear ramp.
        return t * t * (3f - 2f * t);
    }
}
