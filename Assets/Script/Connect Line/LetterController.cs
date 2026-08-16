using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterController : MonoBehaviour
{
    public static int count;
    public static int totalCount;
    public int totalItem;
    [SerializeField] List<LetterDraw> checkObjects;
    public Image dot;
    private void Start()
    {
        count = 0;
        totalCount = totalItem;
    }
    public void ResetGame()
    {
      AudioManager.audioManager.Play("button");
        if (dot != null)
        {
            Color color = dot.color;
            color.a = 0;
            dot.color = color;
        }
      totalCount = totalItem;
      count = 0;
        foreach (var c in checkObjects)
        {
            c.ResetLine();
        }
      //  AudioManager.audioManager.Play("click");
    }
    private void OnEnable()
    {
        ResetGame();
        
    }

    public void NextSection()
    {
     
      AudioManager.audioManager.Play("drag "+gameObject.name);
    }

    public void ColorDot()
    {
        Color color = dot.color;
        color.a = 1;
        dot.color = color;
        count++;
        if (LetterController.count == LetterController.totalCount - 1)
        {
            Invoke("NextGame", 2f);
        }

    }
    void NextGame()
    {
       NextSection();
    }
}
