using System.Collections;
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

   [Header("Section UI")]
   [Tooltip("Shown instead of the activity Selection panel for pages with LearningContentData > useSections enabled. Leave unassigned for projects that don't use sections.")]
   [SerializeField] private GameObject SectionPanel;

   [Tooltip("Pre-placed buttons under SectionPanel, filled in order from the selected page's 'sections' list (same fixed-array pattern as activityButtons).")]
   [SerializeField] private GameObject[] sectionButtons;

   [Header("Activity Canvases")]
   [SerializeField] private Transform pCanvas;
   [SerializeField] private Transform poCanvas;

   [SerializeField] private GameObject closeBUtton;

   [Header("Video Button")]
   [SerializeField] private Button videoButton;

   [Header("Game Background")]
   [Tooltip("Image swapped to the page's assigned background (see LearningContentData > Game Backgrounds) when a game/activity button is clicked.")]
   [SerializeField] private Image gameBackgroundImage;

   [Header("Animation")]
   [SerializeField] private float buttonSpawnDuration = 0.35f;
   [SerializeField] private float buttonSpawnStagger = 0.05f;
   [SerializeField] private float panelFadeDuration = 0.22f;
   [SerializeField] private float activityFadeDuration = 0.3f;
   [SerializeField] private Ease popEase = Ease.OutBack;

   [Header("Preload")]
   [Tooltip("How many activities to Instantiate() per frame while preloading - kept low so the preload itself doesn't cause a hitch.")]
   [SerializeField] private int preloadPerFrame = 1;

   private GameObject currentActivity;
   private PageData currentPage;
   private Sprite _pageSelectionBackground;

   private readonly System.Collections.Generic.Dictionary<GameObject, GameObject>
       spawnedActivities = new();

   private void Start()
   {
      // gameBackgroundImage doubles as the page-selection screen's background,
      // so remember whatever it starts with - ApplyGameBackground() swaps it
      // per page while an activity is open, and DisableAllPage() restores this
      // one fixed sprite so the page-selection UI always looks the same.
      if (gameBackgroundImage != null)
         _pageSelectionBackground = gameBackgroundImage.sprite;

      SpawnButtons();

      if (videoButton != null)
      {
         videoButton.onClick.AddListener(() =>
         {
            AudioManager.audioManager.Play("button");
            PunchButton(videoButton.transform);
         });
      }

      // Warm up every activity in the background (spread across frames) so
      // the first time a player actually opens one, it's already
      // Instantiate()'d and its assets already loaded instead of paying
      // that cost right when they tap it.
      StartCoroutine(PreloadAllActivitiesRoutine());
   }


   /// <summary>
   /// Instantiates and caches every activity across every page (and every
   /// section) up front - mirrors ShowActivity()'s own spawn/position logic
   /// so a real open later just reuses the cached instance. Deactivates each
   /// one immediately after spawning it, so nothing is visible while this runs.
   /// </summary>
   public IEnumerator PreloadAllActivitiesRoutine()
   {
      if (learningContent == null)
      {
         yield break;
      }

      int sinceYield = 0;

      foreach (PageData page in learningContent.pages)
      {
         if (page == null)
         {
            continue;
         }

         if (page.activities != null && page.activities.pages != null)
         {
            foreach (ActivityPageData activityPage in page.activities.pages)
            {
               PreloadActivity(activityPage?.page);

               if (++sinceYield >= preloadPerFrame)
               {
                  sinceYield = 0;
                  yield return null;
               }
            }
         }

         if (page.useSections && page.sections != null)
         {
            foreach (SectionData section in page.sections)
            {
               if (section?.activities?.pages == null)
               {
                  continue;
               }

               foreach (ActivityPageData activityPage in section.activities.pages)
               {
                  PreloadActivity(activityPage?.page);

                  if (++sinceYield >= preloadPerFrame)
                  {
                     sinceYield = 0;
                     yield return null;
                  }
               }
            }
         }
      }
   }


   /// <summary>
   /// Instantiates one activity (if it isn't already cached) and immediately
   /// deactivates it.
   /// </summary>
   private void PreloadActivity(GameObject activity)
   {
      if (activity == null || spawnedActivities.ContainsKey(activity))
      {
         return;
      }

      Transform targetCanvas;

      // Check PO first because PO also starts with P - same rule as OnActivitySelected().
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
         return;
      }

      if (targetCanvas == null)
      {
         return;
      }

      GameObject spawnedActivity = Instantiate(activity, targetCanvas);

      RectTransform rect = spawnedActivity.GetComponent<RectTransform>();

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

      spawnedActivity.SetActive(false);

      spawnedActivities.Add(activity, spawnedActivity);
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
               AudioManager.audioManager.Play("button");
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
      currentPage = page;

      // Pages can optionally show a section list first (LearningContentData >
      // useSections) instead of jumping straight to their activity buttons.
      // Falls through to the normal activity list if sections are enabled
      // but none were actually assigned, so an unfinished page never dead-ends.
      if (page.useSections && page.sections != null && page.sections.Count > 0)
      {
         SetupSectionButtons(page.sections);

         // ---- ANIMATION: cross-fade page list out, section list in ----
         HidePanel(PageButtons, () => ShowPanel(SectionPanel));
      }
      else
      {
         SetupActivityButtons(page.activities);

         // ---- ANIMATION: cross-fade page list out, selection in ----
         HidePanel(PageButtons, () => ShowPanel(Selection));
      }
   }


   private void OnSectionSelected(SectionData section)
   {
      SetupActivityButtons(section.activities);

      // ---- ANIMATION: cross-fade section list out, selection in ----
      HidePanel(SectionPanel, () => ShowPanel(Selection));
   }


   /// <summary>
   /// Fills the fixed activityButtons array from the given activities list -
   /// shared by the direct page -> activities flow and the page -> section ->
   /// activities flow.
   /// </summary>
   private void SetupActivityButtons(ActivityData activities)
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
      int count = activities != null && activities.pages != null ? activities.pages.Count : 0;

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
             activities.pages[i].page;

         if (activity == null)
         {
            Debug.LogWarning(
                $"Activity {i + 1} is empty in Page {currentPage?.pageName}"
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
            AudioManager.audioManager.Play("button");
            PunchButton(buttonTransform);
            OnActivitySelected(selectedActivity);
         });

         // ---- ANIMATION: staggered pop-in, after the panel fades in ----
         AnimatePopIn(activityButton, panelFadeDuration + (shown * buttonSpawnStagger));

         shown++;
      }
   }


   /// <summary>
   /// Fills the fixed sectionButtons array from the selected page's sections list.
   /// </summary>
   private void SetupSectionButtons(System.Collections.Generic.List<SectionData> sections)
   {
      if (sectionButtons == null || sectionButtons.Length == 0)
      {
         Debug.LogError(
             "useSections is enabled but no sectionButtons are assigned on LearningPageButtonSpawner."
         );

         return;
      }

      // Hide all section buttons
      for (int i = 0; i < sectionButtons.Length; i++)
      {
         sectionButtons[i].transform.DOKill();
         sectionButtons[i].SetActive(false);

         // Remove previous listeners
         Button button =
             sectionButtons[i].GetComponent<Button>();

         if (button != null)
         {
            button.onClick.RemoveAllListeners();
         }
      }

      int shown = 0;

      for (int i = 0; i < sections.Count && i < sectionButtons.Length; i++)
      {
         SectionData section = sections[i];

         if (section == null)
            continue;

         GameObject sectionButton =
             sectionButtons[i];

         Button button =
             sectionButton.GetComponent<Button>();

         if (button == null)
         {
            Debug.LogError(
                "Section button does not have a Button component: "
                + sectionButton.name
            );

            continue;
         }

         // Section buttons carry only an activities list - their label/image
         // is set up directly on the GameObject in the scene, so there's
         // nothing to push onto them here beyond activating and wiring them.
         sectionButton.SetActive(true);

         // Capture the section for the lambda
         SectionData selectedSection = section;
         Transform buttonTransform = sectionButton.transform;

         // Assign click
         button.onClick.AddListener(() =>
         {
            AudioManager.audioManager.Play("button");
            PunchButton(buttonTransform);
            OnSectionSelected(selectedSection);
         });

         // ---- ANIMATION: staggered pop-in, after the panel fades in ----
         AnimatePopIn(sectionButton, panelFadeDuration + (shown * buttonSpawnStagger));

         shown++;
      }
   }


   private void OnActivitySelected(GameObject activity)
   {
      if (activity == null)
      {
         Debug.LogError("Activity GameObject is null.");
         return;
      }


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
      ResetPanelVisibility(closeBUtton);
      AnimatePopIn(closeBUtton, 0f);

      HidePanel(PageButtonBg);

      ApplyGameBackground();

      AudioManager.audioManager.DuckBgmVolume();

      ShowActivity(activity, targetCanvas);
   }


   /// <summary>
   /// Swaps the game background to whichever sprite the current page's
   /// number falls under in LearningContentData's background groups.
   /// </summary>
   private void ApplyGameBackground()
   {
      if (gameBackgroundImage == null || learningContent == null || currentPage == null)
         return;

      if (!int.TryParse(currentPage.pageName, out int pageNumber))
         return;

      Sprite background = learningContent.GetBackgroundForPage(pageNumber);

      if (background != null)
      {
         gameBackgroundImage.sprite = background;
      }
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

      HidePanel(Selection);
   }


   public void DisableAllPage()
   {
      AudioManager.audioManager.Play("button");

      // Always show the same background on the page-selection UI, regardless
      // of which per-page background the last-played activity swapped in.
      if (gameBackgroundImage != null)
         gameBackgroundImage.sprite = _pageSelectionBackground;

      // Back to normal BGM volume now that no activity is open.
      AudioManager.audioManager.RestoreBgmVolume();

      // Fully hide the close/back UI first, THEN show the page list.
      // Doing this sequentially (instead of in parallel) prevents the
      // old and new panels from being visible on top of each other
      // during the fade, which made the back/close button look like
      // it was overlapping the incoming UI before disappearing.
      // SectionPanel is included here too so backing out lands on the
      // page list correctly whether the player was on the section list,
      // the activity list, or inside an activity.
      HidePanel(closeBUtton, () =>
      {
         HidePanel(Selection, () =>
         {
            HidePanel(SectionPanel, () =>
            {
               PageButtons.SetActive(true);
               ShowPanel(PageButtons);

               PageButtonBg.SetActive(true);
               ShowPanel(PageButtonBg);
            });
         });
      });

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

   }


   // ------------------------------------------------------------
   //  ANIMATION HELPERS
   // ------------------------------------------------------------

   /// <summary>
   /// Restores full visibility/interactivity on a panel whose CanvasGroup was left
   /// faded out (alpha 0, non-interactable) by a previous HidePanel() call - needed
   /// whenever a panel is shown again via a raw SetActive() + scale-only animation
   /// (e.g. AnimatePopIn) instead of ShowPanel(), which handles this itself.
   /// </summary>
   private void ResetPanelVisibility(GameObject target)
   {
      if (target == null)
         return;

      CanvasGroup group = GetCanvasGroup(target);
      group.DOKill();
      group.alpha = 1f;
      group.interactable = true;
      group.blocksRaycasts = true;
   }


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