using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterController : MonoBehaviour
{
    public int totalItem;
    [SerializeField] List<LetterDraw> checkObjects;
    public Image dot;

    // The one checkpoint that carries the ordered dot list; every other dot on
    // the page forwards its pointer events to it.
    private LetterDraw manager;

    public LetterDraw GetManager()
    {
        if (manager != null)
            return manager;

        if (checkObjects != null)
        {
            foreach (LetterDraw checkObject in checkObjects)
            {
                if (checkObject != null && checkObject.HasPath)
                {
                    manager = checkObject;
                    return manager;
                }
            }
        }

        foreach (LetterDraw checkObject in GetComponentsInChildren<LetterDraw>(true))
        {
            if (checkObject.HasPath)
            {
                manager = checkObject;
                break;
            }
        }

        return manager;
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

        if (checkObjects != null)
        {
            foreach (var c in checkObjects)
            {
                if (c != null)
                    c.ResetLine();
            }
        }

        // The page's progress lives on the manager, so make sure it is cleared
        // even when it is not listed in checkObjects.
        LetterDraw pathOwner = GetManager();
        if (pathOwner != null)
            pathOwner.ResetLine();
    }

    private void OnEnable()
    {
        ResetGame();
    }

    public void NextSection()
    {
        AudioManager.audioManager.Play("drag " + gameObject.name);
    }

    public void ColorDot()
    {
        if (dot == null)
            return;

        Color color = dot.color;
        color.a = 1;
        dot.color = color;
    }
}
