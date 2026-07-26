using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawController : MonoBehaviour
{
    [SerializeField] List<DrawLine> lines;
    //[SerializeField] List<DrawStraightLine> straightLines;
    public static int count;
    public static int totalCount;
    public int totalItem;
    public void Start()
    {

    }
    public void ResetGame()
    {
        foreach (var line in lines)
        {
            line.ResetLine();
        }
        count = 0;
       AudioManager.audioManager.Play("button");
    }

    private void OnEnable()
    {
      foreach (var line in lines)
      {
         line.ResetLine();
      }
      count = 0;
      totalCount = totalItem;     
    }

}
