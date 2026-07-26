using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceController : MonoBehaviour
{
    public static int count;
    public static int totalCount;
    public int totalItem;
    [SerializeField] List<DrawTrace> lines;

    public void Start()
    {

    }
    public void Reset()
    {
        foreach (var trace in lines)
        {

            trace.ResetLine();
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
