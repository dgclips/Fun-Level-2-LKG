using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawLine : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
   [SerializeField] private LineRenderer _lineRenderer;
   [SerializeField] private Camera _mainCamera;
   [SerializeField] private LayerMask _detectionLayer;

   [SerializeField] RectTransform ans;

   private RectTransform _selfRect;
   private Vector3 _startPos;
   private Vector3 _endPos;
   private Transform _connectedObject;
   private bool _isDrawing = false;

   private void Start()
   {
      if (_mainCamera == null)
         _mainCamera = Camera.main;

      // This script lives on the point itself, so use its own RectTransform
      // instead of a fixed serialized reference (img1).
      _selfRect = GetComponent<RectTransform>();
   }

   private void Update()
   {
      if (_connectedObject != null) // Only update if line is connected
      {
         Vector3 worldPos1 = ConvertToWorldPosition(_selfRect);
         Vector3 worldPos2 = ConvertToWorldPosition(ans);

         _lineRenderer.SetPosition(0, worldPos1);
         _lineRenderer.SetPosition(1, worldPos2);

         //UpdateCollider(); // Ensure collider updates as well
      }
   }

   Vector3 ConvertToWorldPosition(RectTransform rectTransform)
   {
      Vector3 screenPos = rectTransform.position; // UI elements are in screen space
      Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _mainCamera.nearClipPlane + 10f));
      worldPos.z = 0; // Keep it in 2D
      return worldPos;
   }



   public void OnPointerDown(PointerEventData eventData)
   {
      if (!_isDrawing)
      {
          AudioManager.audioManager.Play("match click");
         _startPos = GetWorldPosition(_selfRect.position);
         _lineRenderer.positionCount = 2;
         _lineRenderer.SetPosition(0, _startPos);
         _lineRenderer.SetPosition(1, _startPos);
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
         DetectEndObject();
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

         if (result.gameObject == ans.gameObject) // Ensure it's a UI element
         {
            AudioManager.audioManager.Play("correct");
            DrawController.count++;
            _isDrawing = true;
            _connectedObject = result.gameObject.transform;
            UpdateLineToConnectedObject();
            if (DrawController.count == DrawController.totalCount)
            {
               AudioManager.audioManager.Play("end");
               Debug.Log("completed");
               EventManager.GameComplete();
            }
            return;
         }else
         {
            if (!errorMsg)
            {
               errorMsg = true;
               EventManager.WrongAnswer();
               AudioManager.audioManager.Play("wrong");
            }
         }
      }
      _lineRenderer.positionCount = 0;
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

   }



}