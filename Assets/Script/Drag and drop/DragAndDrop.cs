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
        foreach (BSDrag drag in dragItems)
        {

            drag.Reset();
            
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
