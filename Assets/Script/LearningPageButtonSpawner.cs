using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LearningPageButtonSpawner : MonoBehaviour
{
   [Header("Data")]
   [SerializeField] private LearningContentData learningContent;

   [Header("Page UI")]
   [SerializeField] private Transform buttonContainer;
   [SerializeField] private GameObject buttonPrefab;

   [SerializeField] private GameObject Selection;
   [SerializeField] private GameObject PageButtons;
   [SerializeField] private GameObject PageButtonBg;

   [Header("Activity Buttons")]
   [SerializeField] private GameObject[] activityButtons;

   [Header("Activity Canvases")]
   [SerializeField] private Transform pCanvas;
   [SerializeField] private Transform poCanvas;

   [SerializeField] private GameObject closeBUtton;

   [Header("Animation")]
   [SerializeField] private float buttonSpawnDuration = 0.35f;
   [SerializeField] private float buttonSpawnStagger = 0.05f;
   [SerializeField] private float panelFadeDuration = 0.22f;
   [SerializeField] private float activityFadeDuration = 0.3f;
   [SerializeField] private Ease popEase = Ease.OutBack;

   private GameObject currentActivity;

   private readonly System.Collections.Generic.Dictionary<GameObject, GameObject>
       spawnedActivities = new();

   private void Start()
   {
      SpawnButtons();
   }


   public void SpawnButtons()
   {
      // Clear existing page buttons
      foreach (Transform child in buttonContainer)
      {
         child.DOKill();
         Destroy(child.gameObject);
      }

      if (learningContent == null)
      {
         Debug.LogError("Learning Content is not assigned.");
         return;
      }

      // Spawn page buttons
      for (int i = 0; i < learningContent.pages.Count; i++)
      {
         PageData page = learningContent.pages[i];

         GameObject buttonObject =
             Instantiate(buttonPrefab, buttonContainer);

         // Set image
         Image buttonImage = buttonObject.GetComponent<Image>();

         if (buttonImage != null)
         {
            buttonImage.sprite = page.pageButtonImage;
         }

         // Set page name
         TMP_Text text =
             buttonObject.GetComponentInChildren<TMP_Text>();

         if (text != null)
         {
            text.text = page.pageName;
         }

         // Page button click
         Button button =
             buttonObject.GetComponent<Button>();

         if (button != null)
         {
            PageData selectedPage = page;
            Transform buttonTransform = buttonObject.transform;

            button.onClick.AddListener(() =>
            {
               PunchButton(buttonTransform);
               OnPageSelected(selectedPage);
            });
         }

         // ---- ANIMATION: staggered pop-in ----
         AnimatePopIn(buttonObject, i * buttonSpawnStagger);
      }
   }


   private void OnPageSelected(PageData page)
   {
      // Hide all activity buttons
      for (int i = 0; i < activityButtons.Length; i++)
      {
         activityButtons[i].transform.DOKill();
         activityButtons[i].SetActive(false);

         // Remove previous listeners
         Button button =
             activityButtons[i].GetComponent<Button>();

         if (button != null)
         {
            button.onClick.RemoveAllListeners();
         }
      }

      // Number of activities
      int count = page.activities.pages.Count;

      // Setup activity buttons
      int shown = 0;

      for (int i = 0; i < count && i < activityButtons.Length; i++)
      {
         GameObject activityButton =
             activityButtons[i];

         Button button =
             activityButton.GetComponent<Button>();

         if (button == null)
         {
            Debug.LogError(
                "Activity button does not have a Button component: "
                + activityButton.name
            );

            continue;
         }

         // Get the activity GameObject
         GameObject activity =
             page.activities.pages[i].page;

         if (activity == null)
         {
            Debug.LogWarning(
                $"Activity {i + 1} is empty in Page {page.pageName}"
            );

            continue;
         }

         activityButton.SetActive(true);

         // Capture the activity for the lambda
         GameObject selectedActivity = activity;
         Transform buttonTransform = activityButton.transform;

         // Assign click
         button.onClick.AddListener(() =>
         {
            PunchButton(buttonTransform);
            OnActivitySelected(selectedActivity);
         });

         // ---- ANIMATION: staggered pop-in, after the panel fades in ----
         AnimatePopIn(activityButton, panelFadeDuration + (shown * buttonSpawnStagger));

         shown++;
      }

      // ---- ANIMATION: cross-fade page list out, selection in ----
      HidePanel(PageButtons, () => ShowPanel(Selection));

      Debug.Log("Selected Page: " + page.pageName);
      Debug.Log("Video: " + page.video?.videoUrl);
      Debug.Log("Activities: " + count);
   }


   private void OnActivitySelected(GameObject activity)
   {
      if (activity == null)
      {
         Debug.LogError("Activity GameObject is null.");
         return;
      }

      Debug.Log("Selected Activity: " + activity.name);

      Transform targetCanvas = null;

      // Check PO first because PO also starts with P
      if (activity.name.StartsWith("PO"))
      {
         targetCanvas = poCanvas;
      }
      else if (activity.name.StartsWith("P"))
      {
         targetCanvas = pCanvas;
      }
      else
      {
         Debug.LogWarning(
             "Activity name must start with P or PO: " + activity.name
         );

         return;
      }

      closeBUtton.SetActive(true);
      AnimatePopIn(closeBUtton, 0f);

      HidePanel(PageButtonBg);

      ShowActivity(activity, targetCanvas);
   }


   private void ShowActivity(
    GameObject activity,
    Transform parentCanvas)
   {
      if (parentCanvas == null)
      {
         Debug.LogError("Activity canvas is not assigned.");
         return;
      }

      // Hide currently displayed activity
      if (currentActivity != null)
      {
         HidePanel(currentActivity);
      }

      // Check if activity was already spawned
      if (spawnedActivities.TryGetValue(activity, out GameObject existingActivity))
      {
         currentActivity = existingActivity;

         FadeInActivity(existingActivity);

         Debug.Log("Showing existing activity: " + activity.name);

         HidePanel(Selection);
         return;
      }

      // Spawn only the first time
      GameObject spawnedActivity =
          Instantiate(activity, parentCanvas);

      RectTransform rect =
          spawnedActivity.GetComponent<RectTransform>();

      if (rect != null)
      {
         rect.localPosition = Vector3.zero;
         rect.localRotation = Quaternion.identity;
         rect.localScale = Vector3.one;

         rect.anchorMin = Vector2.zero;
         rect.anchorMax = Vector2.one;
         rect.offsetMin = Vector2.zero;
         rect.offsetMax = Vector2.zero;
      }

      // Save reference
      spawnedActivities.Add(activity, spawnedActivity);

      currentActivity = spawnedActivity;

      FadeInActivity(spawnedActivity);

      Debug.Log("Spawned new activity: " + activity.name);

      HidePanel(Selection);
   }


   public void DisableAllPage()
   {
      HidePanel(closeBUtton);

      PageButtons.SetActive(true);
      ShowPanel(PageButtons);

      PageButtonBg.SetActive(true);
      ShowPanel(PageButtonBg);

      // Disable all children of P Canvas
      if (pCanvas != null)
      {
         foreach (Transform child in pCanvas)
         {
            HidePanel(child.gameObject);
         }
      }

      // Disable all children of PO Canvas
      if (poCanvas != null)
      {
         foreach (Transform child in poCanvas)
         {
            HidePanel(child.gameObject);
         }
      }

      // Clear current activity reference
      currentActivity = null;

      Debug.Log("All activities disabled.");
   }


   // ------------------------------------------------------------
   //  ANIMATION HELPERS
   // ------------------------------------------------------------

   /// <summary>
   /// Returns the CanvasGroup on the object, adding one if missing.
   /// </summary>
   private CanvasGroup GetCanvasGroup(GameObject target)
   {
      CanvasGroup group = target.GetComponent<CanvasGroup>();

      if (group == null)
      {
         group = target.AddComponent<CanvasGroup>();
      }

      return group;
   }


   /// <summary>
   /// Fades and scales a panel in. Activates it first.
   /// </summary>
   private void ShowPanel(GameObject panel, System.Action onComplete = null)
   {
      if (panel == null)
      {
         onComplete?.Invoke();
         return;
      }

      panel.SetActive(true);

      CanvasGroup group = GetCanvasGroup(panel);

      group.DOKill();
      panel.transform.DOKill();

      group.alpha = 0f;
      group.interactable = false;
      group.blocksRaycasts = false;

      group.DOFade(1f, panelFadeDuration)
           .SetEase(Ease.OutQuad)
           .SetUpdate(true)
           .SetLink(panel)
           .OnComplete(() =>
           {
              group.interactable = true;
              group.blocksRaycasts = true;
              onComplete?.Invoke();
           });

      panel.transform.localScale = Vector3.one * 0.94f;

      panel.transform.DOScale(1f, panelFadeDuration)
           .SetEase(popEase)
           .SetUpdate(true)
           .SetLink(panel);
   }


   /// <summary>
   /// Fades a panel out, then deactivates it.
   /// </summary>
   private void HidePanel(GameObject panel, System.Action onComplete = null)
   {
      if (panel == null || !panel.activeSelf)
      {
         onComplete?.Invoke();
         return;
      }

      CanvasGroup group = GetCanvasGroup(panel);

      group.DOKill();

      group.interactable = false;
      group.blocksRaycasts = false;

      group.DOFade(0f, panelFadeDuration)
           .SetEase(Ease.InQuad)
           .SetUpdate(true)
           .SetLink(panel)
           .OnComplete(() =>
           {
              panel.SetActive(false);
              onComplete?.Invoke();
           });
   }


   /// <summary>
   /// Fade + slight zoom used when an activity appears.
   /// </summary>
   private void FadeInActivity(GameObject activity)
   {
      activity.SetActive(true);

      CanvasGroup group = GetCanvasGroup(activity);

      group.DOKill();
      activity.transform.DOKill();

      group.alpha = 0f;
      group.interactable = false;
      group.blocksRaycasts = false;

      group.DOFade(1f, activityFadeDuration)
           .SetEase(Ease.OutQuad)
           .SetUpdate(true)
           .SetLink(activity)
           .OnComplete(() =>
           {
              group.interactable = true;
              group.blocksRaycasts = true;
           });

      activity.transform.localScale = Vector3.one * 0.96f;

      activity.transform.DOScale(1f, activityFadeDuration)
              .SetEase(Ease.OutCubic)
              .SetUpdate(true)
              .SetLink(activity);
   }


   /// <summary>
   /// Scale pop-in with an optional delay, used for buttons.
   /// </summary>
   private void AnimatePopIn(GameObject target, float delay)
   {
      if (target == null)
      {
         return;
      }

      target.transform.DOKill();
      target.transform.localScale = Vector3.zero;

      target.transform.DOScale(1f, buttonSpawnDuration)
            .SetEase(popEase)
            .SetDelay(delay)
            .SetUpdate(true)
            .SetLink(target);
   }


   /// <summary>
   /// Quick squash feedback when a button is pressed.
   /// </summary>
   private void PunchButton(Transform target)
   {
      if (target == null)
      {
         return;
      }

      target.DOKill(true);
      target.localScale = Vector3.one;

      target.DOPunchScale(Vector3.one * 0.12f, 0.25f, 8, 0.8f)
            .SetUpdate(true)
            .SetLink(target.gameObject);
   }


   private void OnDestroy()
   {
      DOTween.Kill(transform);
   }
}