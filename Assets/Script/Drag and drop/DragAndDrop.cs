using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    public static int count;
    public static int totalCount;
    public int totalItem;
    [SerializeField] List<BSDrag> dragItems;


    private void Start()
    {
        
    }
    public void Reset()
    {
      AudioManager.audioManager.Play("button");
        foreach (BSDrag drag in dragItems)
        {

            drag.ResetImmediate();

        }

        // Restore every slot's placeholder background too, in case a
        // previous playthrough left some of them faded out from correct drops.
        foreach (BSDrop drop in GetComponentsInChildren<BSDrop>(true))
        {
            drop.ResetPlaceholder();
        }

        count = 0;
    }
    private void OnEnable()
    {
        count = 0;
        totalCount = totalItem;
        Reset();
    }
}
