using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [Header("Color Selection Buttons")]
    [SerializeField] private Button redButton;
    [SerializeField] private Button yellowButton;

    [Header("Animation")]
    [SerializeField] private float selectPunchScale = 0.25f;
    [SerializeField] private float correctPopDuration = 0.35f;
    [SerializeField] private float wrongShakeDuration = 0.35f;
    [SerializeField] private float wrongShakeStrength = 20f;

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
        AudioManager.audioManager.Play("button");

        if (yellowButton != null)
            PunchSelect(yellowButton.transform);
    }
    public void ColorRed()
    {
        _currentColor = Color.red;
        AudioManager.audioManager.Play("button");

        if (redButton != null)
            PunchSelect(redButton.transform);
    }

    public void PaintImage(bool yellow,Image image,Paint paint)
    {
        if((_currentColor == Color.yellow) && yellow)
        {
            if(!paint.isColored)
                count++;
            paint.isColored = true;
            image.color = _currentColor;

            AudioManager.audioManager.Play("correct");
            PlayCorrectPop(image.transform);

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

            AudioManager.audioManager.Play("correct");
            PlayCorrectPop(image.transform);

            if(count == totalCount)
            {
                EventManager.GameComplete();
            }
        }
        else
        {
            EventManager.WrongAnswer();
            AudioManager.audioManager.Play("wrong");
            PlayWrongShake(image.transform);
        }
    }

    /// <summary>
    /// Quick punch-scale used to highlight the just-picked color button.
    /// </summary>
    private void PunchSelect(Transform target)
    {
        target.DOKill();
        target.localScale = Vector3.one;

        target.DOPunchScale(Vector3.one * selectPunchScale, 0.3f, 6, 0.8f)
              .SetUpdate(true);
    }

    /// <summary>
    /// Satisfying bounce-in used when a section is painted with the correct color.
    /// </summary>
    private void PlayCorrectPop(Transform image)
    {
        image.DOKill();
        image.localScale = Vector3.one * 0.85f;

        image.DOScale(1f, correctPopDuration)
             .SetEase(Ease.OutBack)
             .SetUpdate(true);
    }

    /// <summary>
    /// Shake used to flag painting with the wrong color.
    /// </summary>
    private void PlayWrongShake(Transform image)
    {
        image.DOKill();
        image.localScale = Vector3.one;

        image.DOShakePosition(wrongShakeDuration, wrongShakeStrength, 12, 90, false, true)
             .SetUpdate(true);
    }

    public void Reset()
    {
        foreach (var paint in paintImages)
        {
            paint.image.transform.DOKill();
            paint.image.transform.localScale = Vector3.one;
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
