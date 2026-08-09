using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LetterDraw : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _detectionLayer;
    [SerializeField] private RectTransform drawingAreaImage;
    [SerializeField] private Image drawingUIImage; // The Image component
    private Texture2D drawingTexture;


    private List<Vector3> _drawnPoints = new List<Vector3>();
    private bool _isDrawing = false;
    [SerializeField] private float _lineWidth = 0.1f;
    bool _isComplete = false;
    public LetterController letterController;
    public LetterDraw nextPoint;
    public GameObject[] endPoints;
    public List< GameObject> touchPoints;
    public List<RaycastResult> results = new List<RaycastResult>();
    private void Start()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (drawingUIImage != null && drawingUIImage.sprite != null)
        {
            drawingTexture = drawingUIImage.sprite.texture;
        }

        _lineRenderer.numCapVertices = 10;
        _lineRenderer.numCornerVertices = 5;

        if (nextPoint != null)
        {
            nextPoint.enabled = false;
            nextPoint.GetComponent<Image>().enabled = false;
        }
    }


    private void Update()
    {
        if (_isDrawing && _drawnPoints.Count > 1)
        {
            _lineRenderer.positionCount = _drawnPoints.Count;
            _lineRenderer.SetPositions(_drawnPoints.ToArray());
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        if (!_isDrawing && !_isComplete)
        {
            AudioManager.audioManager.Play("click");
            _drawnPoints.Clear();
            _isDrawing = true;
            AddPoint(eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDrawing)
        {
            AddPoint(eventData.position);
           
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isDrawing)
        {
            AudioManager.audioManager.Play("click");
            _isDrawing = false;
            DetectEndObject();
        }
    }

    private void AddPoint(Vector2 screenPos)
    {
        // Check if inside the image rect
        if (RectTransformUtility.RectangleContainsScreenPoint(drawingAreaImage, screenPos, _mainCamera))
        {
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

            if (texX >= 0 && texX < drawingTexture.width && texY >= 0 && texY < drawingTexture.height)
            {
                Color pixelColor = drawingTexture.GetPixel(texX, texY);
                if (pixelColor.a > 0.01f) // Only draw if alpha > 0
                {
                    Vector3 worldPos = GetWorldPosition(screenPos);
                    if (_drawnPoints.Count == 0 || Vector3.Distance(_drawnPoints[_drawnPoints.Count - 1], worldPos) > 0.1f)
                    {
                        _drawnPoints.Add(worldPos);
                    }
                    return;
                }
            }
        }

        // Outside or transparent — reset
        _drawnPoints.Clear();
        _lineRenderer.positionCount = 0;
        _isDrawing = false;
    }



    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        worldPos.z = 0;
        return worldPos;
    }

    private void DetectEndObject()
    {
        HashSet<GameObject> detectedEnds = new HashSet<GameObject>();
        HashSet<GameObject> touchedEnds = new HashSet<GameObject>();

        foreach (Vector3 point in _drawnPoints)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = _mainCamera.WorldToScreenPoint(point)
            };

            List<RaycastResult> tempResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, tempResults);

          
            if (touchPoints.Count != 0)
            {
                foreach (RaycastResult result in tempResults)
                {
                    
                  
                    if(touchPoints.Contains(result.gameObject))
                    {
                        touchedEnds.Add(result.gameObject);
                       
                    }
                    
                }
                foreach (RaycastResult results in tempResults)
                {
                        foreach (GameObject end in endPoints)
                        {
                          //  Debug.Log("end " + end.gameObject.name + "     " + "result " + results.gameObject.name);

                        if (touchPoints.Count == touchedEnds.Count)
                        {
                            if (results.gameObject == end && !detectedEnds.Contains(end))
                        {
                            if (!end.name.Contains('S'))
                            {
                                    detectedEnds.Add(end);
                                    LetterController.count++;
                           EventManager.GameComplete();
                        }

                            }
                        }
                    }
                }

            }








            if (touchPoints.Count == 0)
            {
                foreach (RaycastResult result in tempResults)
                {
                    foreach (GameObject end in endPoints)
                    {
                        if (result.gameObject == end && !detectedEnds.Contains(end))
                        {

                            // Debug.Log("Detected end point: " + end.name);
                            if (!end.name.Contains('S'))
                            {
                                detectedEnds.Add(end);
                                LetterController.count++;
                                //  Debug.Log(LetterController.count);
                                if (nextPoint != null)
                                {
                                    nextPoint.GetComponent<Image>().enabled = true;
                                    nextPoint.enabled = true;

                                }
                            }

                        }
                    }
                }
            }

           
        }
        if (LetterController.count == LetterController.totalCount-1)
        {
            Invoke("NextGame", 2f);
        }

        if (detectedEnds.Count == 0)
        {
            _lineRenderer.positionCount = 0;
        }
        else
        {
            _isComplete = true;
        }
    }

    void NextGame()
    {
        letterController.NextSection();
    }

    public void ResetLine()
    {
        if (nextPoint != null) nextPoint.enabled = false;
        _drawnPoints.Clear();
        _isDrawing = false;
        _isComplete = false;
        _lineRenderer.positionCount = 0;
        if (nextPoint != null)
        {
            nextPoint.enabled = false;
            nextPoint.GetComponent<Image>().enabled = false;
        }
    }
    private void OnEnable()
    {
        ResetLine();
    }
}
