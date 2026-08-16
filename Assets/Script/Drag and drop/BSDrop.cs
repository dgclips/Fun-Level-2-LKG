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

            if(DragAndDrop.count == DragAndDrop.totalCount)
            {
                EventManager.GameComplete();
            }
        }else
      {
         EventManager.WrongAnswer();
      }
    }
}
