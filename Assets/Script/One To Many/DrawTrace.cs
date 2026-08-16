using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class DrawTrace : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _detectionLayer;

    [SerializeField] RectTransform img1;
    [SerializeField] RectTransform img2;

    [Header("Animation")]
    [SerializeField] private float pressPunchScale = 0.15f;
    [SerializeField] private float correctPopDuration = 0.3f;
    [SerializeField] private float wrongShakeDuration = 0.35f;
    [SerializeField] private float wrongShakeStrength = 20f;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Transform _connectedObject;
    private bool _isDrawing = false;

    private void Start()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }
    private void Update()
    {
        if (_connectedObject != null) // Only update if line is connected
        {
            Vector3 worldPos1 = ConvertToWorldPosition(img1);
            Vector3 worldPos2 = ConvertToWorldPosition(img2);

            _lineRenderer.SetPosition(0, worldPos1);
            _lineRenderer.SetPosition(1, worldPos2);

            //UpdateCollider(); // Ensure collider updates as well
        }
    }

    Vector3 ConvertToWorldPosition(RectTransform rectTransform)
    {
        Vector3 screenPos = rectTransform.position; // UI elements are in screen space
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x+1, screenPos.y, _mainCamera.nearClipPlane + 10f));
        worldPos.z = 0; // Keep it in 2D
        return worldPos;
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isDrawing)
        {
            AudioManager.audioManager.Play("match click");
            _startPos = GetWorldPosition(img1.position);
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, _startPos);
            _lineRenderer.SetPosition(1, _startPos);

            // Tactile "picked up" feedback.
            img1.DOKill();
            img1.localScale = Vector3.one;
            img1.DOPunchScale(Vector3.one * pressPunchScale, 0.15f, 6, 0.7f)
                .SetUpdate(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDrawing)
        {
            _endPos = GetWorldPosition(eventData.position);
            _lineRenderer.SetPosition(1, _endPos);
            // UpdateCollider();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDrawing)
        {
            AudioManager.audioManager.Play("match click");
            if (img2 != null)
            {
                DetectEndObject();
            }else
            {
                _lineRenderer.positionCount = 0;
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        worldPos.z = 0; // Keep in 2D space
        return worldPos;
    }

    private void DetectEndObject()
    {
      bool errorMsg = false;
      PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition // Use actual screen position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {

            if (result.gameObject == img2.gameObject) // Ensure it's a UI element
            {
            AudioManager.audioManager.Play("correct");
            TraceController.count++;
                _isDrawing = true;
                _connectedObject = result.gameObject.transform;
                UpdateLineToConnectedObject();

                PlayCorrectPop(img2);
                PlayCorrectPop(img1);

                if (TraceController.count == TraceController.totalCount)
            {
               AudioManager.audioManager.Play("end");
               EventManager.GameComplete();
                }
                return;
            }
         else
         {
            if (!errorMsg)
            {
               errorMsg = true;
               EventManager.WrongAnswer();
               AudioManager.audioManager.Play("wrong");

               PlayWrongShake(img1);
            }
         }
      }
        _lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// Satisfying bounce-in used when a trace successfully connects to its match.
    /// </summary>
    private void PlayCorrectPop(RectTransform target)
    {
        target.DOKill();
        target.localScale = Vector3.one * 0.8f;

        target.DOScale(1f, correctPopDuration)
              .SetEase(Ease.OutBack)
              .SetUpdate(true);
    }

    /// <summary>
    /// Shake used to flag an incorrect connection attempt.
    /// </summary>
    private void PlayWrongShake(RectTransform target)
    {
        target.DOKill();
        target.localScale = Vector3.one;

        target.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
              .SetUpdate(true);
    }



    private void UpdateLineToConnectedObject()
    {
        if (_connectedObject != null)
        {
            _endPos = _connectedObject.position; // Snap line end to detected object
            _lineRenderer.SetPosition(1, _endPos);
            // UpdateCollider();
        }
    }

    public void ResetLine()
    {
        _connectedObject = null;
        _isDrawing = false;
        _lineRenderer.positionCount = 0;

        ResetPointScale(img1);
        ResetPointScale(img2);

      AudioManager.audioManager.Play("button");
   }
    private void OnEnable()
    {

      _connectedObject = null;
      _isDrawing = false;
      _lineRenderer.positionCount = 0;

      ResetPointScale(img1);
      ResetPointScale(img2);
   }

    /// <summary>
    /// Kills any in-flight tween and resets scale for the next playthrough.
    /// </summary>
    private void ResetPointScale(RectTransform target)
    {
        if (target == null)
            return;

        target.DOKill();
        target.localScale = Vector3.one;
    }


}
