using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BSDrop : MonoBehaviour, IDropHandler
{
    public string img;

    [Header("Animation")]
    [SerializeField] private float snapPunchScale = 0.18f;
    [SerializeField] private float snapPunchDuration = 0.35f;

    [Tooltip("How long this slot's own placeholder background takes to fade out once the correct piece has snapped into it.")]
    [SerializeField] private float placeholderFadeDuration = 0.25f;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        BSDrag _dragObject = eventData.pointerDrag.GetComponent<BSDrag>();
        if (_dragObject.CheckTarget(img))
        {
            AudioManager.audioManager.Play("correct");
            DragAndDrop.count++;
            RectTransform snapPlace = (RectTransform)transform;
            _dragObject.SetAnchorMax(snapPlace.anchorMax);
            _dragObject.SetAnchorMin(snapPlace.anchorMin);
            _dragObject.SetLocalPos(snapPlace.anchoredPosition);

            // Satisfying "snap into place" pop for a correct placement.
            Transform dragTransform = _dragObject.transform;
            dragTransform.DOKill();
            dragTransform.localScale = Vector3.one;
            dragTransform.DOPunchScale(Vector3.one * snapPunchScale, snapPunchDuration, 6, 0.8f)
                .SetUpdate(true);

            // Hide this slot's own placeholder background now that a piece is
            // sitting on top of it - otherwise it peeks out around any piece
            // whose shape doesn't exactly fill the slot's rectangle.
            HidePlaceholder();

            if(DragAndDrop.count == DragAndDrop.totalCount)
            {
                EventManager.GameComplete();
            }
        }else
      {
         EventManager.WrongAnswer();
      }
    }

    /// <summary>
    /// Fades this slot's own background image out once it's been filled correctly.
    /// </summary>
    private void HidePlaceholder()
    {
        if (_image == null)
            return;

        _image.DOKill();
        _image.DOFade(0f, placeholderFadeDuration).SetUpdate(true);
    }

    /// <summary>
    /// Instantly restores this slot's placeholder background to fully visible -
    /// used when the activity is reset.
    /// </summary>
    public void ResetPlaceholder()
    {
        if (_image == null)
            return;

        _image.DOKill();

        Color color = _image.color;
        color.a = 1f;
        _image.color = color;
    }
}
