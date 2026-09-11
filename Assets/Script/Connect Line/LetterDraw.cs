using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Join-the-dots tracing (PO101 / PO104).
///
/// The dots are joined one pair at a time. A stroke that reaches the next dot
/// in the sequence is kept on screen, and the next stroke carries on from where
/// that one stopped - so the player can let go at every dot instead of having
/// to trace the whole page in a single unbroken drag. A stroke that reaches the
/// wrong dot, or never reaches one at all, leaves nothing behind.
///
/// Only one checkpoint on a page carries the ordered list in the inspector
/// (touchPoints + endPoints); that one owns the path, the progress and the one
/// LineRenderer that is actually set up to draw. Every other dot forwards its
/// pointer events to it, which is what lets a stroke start from the dot the
/// player stopped on last time.
/// </summary>
public class LetterDraw : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _detectionLayer;
    [SerializeField] private RectTransform drawingAreaImage;
    [SerializeField] private Image drawingUIImage; // The Image component
    private Texture2D drawingTexture;

    // The stroke the finger is laying down right now - dropped unless it lands
    // on the next dot.
    private List<Vector3> _drawnPoints = new List<Vector3>();
    // Every stroke that did land on the next dot, kept on screen.
    private List<Vector3> _linePoints = new List<Vector3>();
    private bool _isDrawing = false;
    [SerializeField] private float _lineWidth = 0.1f;
    public LetterController letterController;
    public LetterDraw nextPoint;
    public GameObject[] endPoints;
    public List<GameObject> touchPoints;

    // this dot, then every touch point, then the end point(s)
    private readonly List<GameObject> _path = new List<GameObject>();
    private readonly List<LetterDraw> _pathDraws = new List<LetterDraw>();
    private int _reached;              // index in _path the drawn line currently ends on
    private LetterDraw _manager;

    /// <summary>True on the one checkpoint that carries the ordered dot list.</summary>
    public bool HasPath { get { return endPoints != null && endPoints.Length > 0; } }

    private bool IsComplete { get { return _path.Count > 0 && _reached >= _path.Count - 1; } }

    private LetterDraw Manager
    {
        get
        {
            if (_manager == null)
                _manager = HasPath ? this
                                   : (letterController != null ? letterController.GetManager() : null);
            return _manager;
        }
    }

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (drawingUIImage != null && drawingUIImage.sprite != null)
        {
            drawingTexture = drawingUIImage.sprite.texture;
        }

        BuildPath();
    }

    private void Start()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.numCapVertices = 10;
            _lineRenderer.numCornerVertices = 5;
        }
    }

    private void BuildPath()
    {
        _path.Clear();
        _pathDraws.Clear();

        if (!HasPath)
            return;

        AddPathPoint(gameObject);

        if (touchPoints != null)
        {
            foreach (GameObject touchPoint in touchPoints)
                AddPathPoint(touchPoint);
        }

        foreach (GameObject endPoint in endPoints)
            AddPathPoint(endPoint);
    }

    private void AddPathPoint(GameObject point)
    {
        if (point == null || _path.Contains(point))
            return;

        _path.Add(point);
        _pathDraws.Add(point.GetComponent<LetterDraw>());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        LetterDraw manager = Manager;
        if (manager != null)
            manager.BeginStroke(gameObject, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        LetterDraw manager = Manager;
        if (manager != null)
            manager.ContinueStroke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        LetterDraw manager = Manager;
        if (manager != null)
            manager.EndStroke();
    }

    /// <summary>
    /// Starts a stroke, but only from the dot the drawn line currently ends on -
    /// pressing any other dot does nothing.
    /// </summary>
    private void BeginStroke(GameObject from, PointerEventData eventData)
    {
        if (_isDrawing || IsComplete || _path.Count == 0 || _path[_reached] != from)
            return;

        _drawnPoints.Clear();

        // Carry on from the exact point the last kept stroke stopped at so the
        // joined-up line stays unbroken.
        if (_linePoints.Count > 0)
            _drawnPoints.Add(_linePoints[_linePoints.Count - 1]);

        _isDrawing = true;
        AddPoint(eventData.position);
        RenderLine();
    }

    private void ContinueStroke(PointerEventData eventData)
    {
        if (!_isDrawing)
            return;

        AddPoint(eventData.position);
        RenderLine();
    }

    private void EndStroke()
    {
        if (!_isDrawing)
            return;

        _isDrawing = false;
        Evaluate(true);
    }

    private void AddPoint(Vector2 screenPos)
    {
        // Check if inside the image rect
        if (drawingAreaImage != null &&
            RectTransformUtility.RectangleContainsScreenPoint(drawingAreaImage, screenPos, _mainCamera) &&
            IsOnArtwork(screenPos))
        {
            Vector3 worldPos = GetWorldPosition(screenPos);
            if (_drawnPoints.Count == 0 || Vector3.Distance(_drawnPoints[_drawnPoints.Count - 1], worldPos) > 0.1f)
            {
                _drawnPoints.Add(worldPos);
            }
            return;
        }

        // Outside or transparent - the stroke stops here. Anything it already
        // joined up is still kept, the rest is thrown away.
        _isDrawing = false;
        Evaluate(false);
    }

    /// <summary>Keeps the line on the page artwork instead of its transparent margin.</summary>
    private bool IsOnArtwork(Vector2 screenPos)
    {
        if (drawingTexture == null || drawingUIImage == null)
            return true;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingAreaImage, screenPos, _mainCamera, out localPoint);

        // Convert local point to texture coordinates
        Rect rect = drawingUIImage.rectTransform.rect;
        Vector2 normalized = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height
        );

        int texX = Mathf.FloorToInt(normalized.x * drawingTexture.width);
        int texY = Mathf.FloorToInt(normalized.y * drawingTexture.height);

        if (texX < 0 || texX >= drawingTexture.width || texY < 0 || texY >= drawingTexture.height)
            return false;

        return drawingTexture.GetPixel(texX, texY).a > 0.01f; // Only draw if alpha > 0
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        worldPos.z = 0;
        return worldPos;
    }

    /// <summary>
    /// Walks the finished stroke and keeps it up to every dot it joined in the
    /// right order. The moment it touches a dot that is not the next one, the
    /// rest of the stroke is thrown away.
    /// </summary>
    private void Evaluate(bool playFeedback)
    {
        int joined = 0;
        bool leftStartDot = false;

        if (_path.Count > 0 && _drawnPoints.Count > 0)
        {
            int startDot = _reached;

            int[] dots = new int[_drawnPoints.Count];
            for (int i = 0; i < _drawnPoints.Count; i++)
                dots[i] = DotAt(_drawnPoints[i]);

            // A tap that never leaves the dot it started on is not a wrong answer.
            leftStartDot = dots[dots.Length - 1] != startDot;

            int segmentStart = 0;
            int index = 0;

            while (index < _drawnPoints.Count && !IsComplete)
            {
                int dot = dots[index];

                // Nothing under this point, or still on the dot we set off from.
                if (dot < 0 || dot == _reached)
                {
                    index++;
                    continue;
                }

                if (dot != _reached + 1)
                    break; // joined the wrong dot

                // Keep the stroke all the way through the dot, not just up to
                // the edge where it first touched it.
                int segmentEnd = index;
                while (segmentEnd + 1 < _drawnPoints.Count && dots[segmentEnd + 1] == dot)
                    segmentEnd++;

                KeepSegment(segmentStart, segmentEnd);

                LetterDraw from = _pathDraws[_reached];
                if (from != null)
                    from.Reveal();

                _reached++;
                joined++;
                segmentStart = segmentEnd;
                index = segmentEnd + 1;
            }
        }

        _drawnPoints.Clear();
        RenderLine();

        if (joined == 0)
        {
            if (playFeedback && leftStartDot)
            {
                EventManager.WrongAnswer();
                AudioManager.audioManager.Play("wrong");
            }
            return;
        }

        AudioManager.audioManager.Play("correct");

        if (IsComplete)
        {
            AudioManager.audioManager.Play("end");
            EventManager.GameComplete();
        }
    }

    private void KeepSegment(int fromIndex, int toIndex)
    {
        for (int i = fromIndex; i <= toIndex; i++)
        {
            Vector3 point = _drawnPoints[i];
            if (_linePoints.Count > 0 && Vector3.Distance(_linePoints[_linePoints.Count - 1], point) < 0.001f)
                continue;

            _linePoints.Add(point);
        }
    }

    /// <summary>Index of the path dot sitting under a drawn point, or -1.</summary>
    private int DotAt(Vector3 worldPos)
    {
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

        for (int i = 0; i < _path.Count; i++)
        {
            RectTransform rect = _path[i] != null ? _path[i].transform as RectTransform : null;
            if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, _mainCamera))
                return i;
        }

        return -1;
    }

    private void RenderLine()
    {
        if (_lineRenderer == null)
            return;

        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.positionCount = _linePoints.Count + _drawnPoints.Count;

        for (int i = 0; i < _linePoints.Count; i++)
            _lineRenderer.SetPosition(i, _linePoints[i]);

        for (int i = 0; i < _drawnPoints.Count; i++)
            _lineRenderer.SetPosition(_linePoints.Count + i, _drawnPoints[i]);
    }

    /// <summary>Shows the follow-up dot, for pages that hide it until it is due.</summary>
    public void Reveal()
    {
        if (nextPoint == null)
            return;

        nextPoint.enabled = true;

        Image image = nextPoint.GetComponent<Image>();
        if (image != null)
            image.enabled = true;
    }

    public void ResetLine()
    {
        _drawnPoints.Clear();
        _linePoints.Clear();
        _isDrawing = false;
        _reached = 0;

        if (_lineRenderer != null)
            _lineRenderer.positionCount = 0;

        if (nextPoint != null)
        {
            nextPoint.enabled = false;

            Image image = nextPoint.GetComponent<Image>();
            if (image != null)
                image.enabled = false;
        }
    }

    private void OnEnable()
    {
        ResetLine();
    }
}
