using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BSDrop : MonoBehaviour, IDropHandler
{
    public string img;
    public void OnDrop(PointerEventData eventData)
    {
        BSDrag _dragObject = eventData.pointerDrag.GetComponent<BSDrag>();
        if (_dragObject.CheckTarget(img))
        {
            AudioManager.audioManager.Play("click");
            DragAndDrop.count++;
            RectTransform snapPlace = (RectTransform)transform;
            _dragObject.SetAnchorMax(snapPlace.anchorMax);
            _dragObject.SetAnchorMin(snapPlace.anchorMin);
            _dragObject.SetLocalPos(snapPlace.anchoredPosition);
            if(DragAndDrop.count == DragAndDrop.totalCount)
            {
                EventManager.GameComplete();
            }
        }
    }
}
