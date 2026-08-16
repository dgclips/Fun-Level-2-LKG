using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [Header("Animation")]
    [SerializeField] private float pressPunch = 0.06f;
    [SerializeField] private float liftScale = 1.12f;
    [SerializeField] private float liftDuration = 0.15f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private float shakeStrength = 18f;

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

        if (!IsSnap)
        {
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * pressPunch, 0.15f, 6, 0.7f)
                .SetUpdate(true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsSnap)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, eventData.position, null, out Vector2 localPointerPOsition);
            offset = GetComponent<RectTransform>().anchoredPosition - localPointerPOsition;
            canvasGroup.blocksRaycasts = false;

            // Lift the item slightly and raise it above other items while dragging.
            transform.SetAsLastSibling();
            transform.DOKill();
            transform.DOScale(liftScale, liftDuration).SetEase(Ease.OutQuad).SetUpdate(true);
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

    /// <summary>
    /// Snaps the item back to its start position with a little "wrong spot" shake
    /// and settle-bounce, used when the player releases it on an invalid target.
    /// </summary>
    public void Reset()
    {
        if (_rectTransform == null)
            return;

        _rectTransform.anchorMax = _anchorMax;
        _rectTransform.anchorMin = _anchorMin;
        _rectTransform.anchoredPosition = _localPos;
        IsSnap = false;
        canvasGroup.blocksRaycasts = true;

        transform.DOKill();
        transform.localScale = Vector3.one;

        transform.DOShakePosition(returnDuration, shakeStrength, 12, 90, false, true)
            .SetUpdate(true);
    }

    /// <summary>
    /// Instantly resets the item with no animation - used for initial/level setup.
    /// </summary>
    public void ResetImmediate()
    {
        if (_rectTransform == null)
            return;

        transform.DOKill();
        transform.localScale = Vector3.one;

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
