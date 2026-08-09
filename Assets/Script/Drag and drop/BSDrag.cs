using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BSDrag : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public CanvasGroup canvasGroup;
    Vector2 _anchorMin;
    Vector2 _anchorMax;
    Vector2 _localPos;
    RectTransform _rectTransform;
    public string target;
    private Vector2 offset;
    bool IsSnap;
    void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _anchorMax = _rectTransform.anchorMax;
        _anchorMin = _rectTransform.anchorMin;
        _localPos = _rectTransform.anchoredPosition;
    }
    public void SetAnchorMax(Vector2 max)
    {
        _rectTransform.anchorMax = max;
    }
    public void SetAnchorMin(Vector2 min)
    {
        _rectTransform.anchorMin = min;
    }
    public void SetLocalPos(Vector2 pos)
    {
        _rectTransform.anchoredPosition = pos;
    }
    public bool CheckTarget(string img)
    {
        if (img == target)
        {
            IsSnap = true;
            return true;

        }
        else
        {
            return false;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.audioManager.Play("click");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsSnap)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, eventData.position, null, out Vector2 localPointerPOsition);
            offset = GetComponent<RectTransform>().anchoredPosition - localPointerPOsition;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsSnap)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform,eventData.position,null,out Vector2 localPOinterPosiotion);
            _rectTransform.anchoredPosition= localPOinterPosiotion+offset;
          // GetComponent<RectTransform>().anchoredPosition += eventData.delta;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsSnap)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Reset()
    {
        if (_rectTransform == null)
            return;
        _rectTransform.anchorMax = _anchorMax;
        _rectTransform.anchorMin = _anchorMin;
        _rectTransform.anchoredPosition = _localPos;
        IsSnap = false;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsSnap)
        {
            Reset();
        }
    }
}
