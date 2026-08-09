using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwoColorPainting : MonoBehaviour
{
    [SerializeField] List<Paint> paintImages;
    Color _currentColor;
    public int totalCount;
    int count;
    [System.Serializable]
    public class Paint
    {
        public Button button;
        public Image image;
        public bool yellow;
        public bool isColored;
    }
    void Start()
    {
        foreach (var paint in paintImages)
        {
            paint.image.alphaHitTestMinimumThreshold = 0.1f;
            paint.button.onClick.AddListener(()=> PaintImage(paint.yellow,paint.image,paint));
        }
    }

    public void ColorYellow()
    {
        _currentColor = Color.yellow;
    }
    public void ColorRed()
    {
        _currentColor = Color.red;
    }

    public void PaintImage(bool yellow,Image image,Paint paint)
    {
        if((_currentColor == Color.yellow) && yellow)
        {
            if(!paint.isColored)
                count++;
            paint.isColored = true;
            image.color = _currentColor;
            if (count == totalCount)
            {
                EventManager.GameComplete();
            }
        }
        else if((_currentColor == Color.red) && !yellow)
        {
            if (!paint.isColored)
                count++;
            paint.isColored = true;
            image.color = _currentColor;
            if(count == totalCount)
            {
                EventManager.GameComplete();
            }
        }
    }

    public void Reset()
    {
        foreach (var paint in paintImages)
        {
            paint.image.color = Color.white;
            paint.isColored = false;
        }
        count = 0;
    }
    private void OnEnable()
    {
        Reset();
    }
}
