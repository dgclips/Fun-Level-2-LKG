using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "LearningContent",
    menuName = "Learning System/Learning Content"
)]
public class LearningContentData : ScriptableObject
{
   public List<PageData> pages = new();

   [Header("Game Backgrounds")]
   [Tooltip("Assign one background to many pages at once by typing the page numbers/ranges it applies to (e.g. \"1-10,15,22-30\"), instead of setting a background on every single PageData.")]
   public List<PageBackgroundGroup> backgroundGroups = new();

   private Dictionary<int, Sprite> _backgroundLookup;

   /// <summary>
   /// Returns the background assigned to the given page number (1-based),
   /// or null if no group covers that page.
   /// </summary>
   public Sprite GetBackgroundForPage(int pageNumber)
   {
      if (_backgroundLookup == null)
      {
         BuildBackgroundLookup();
      }

      return _backgroundLookup.TryGetValue(pageNumber, out Sprite sprite) ? sprite : null;
   }

   private void BuildBackgroundLookup()
   {
      _backgroundLookup = new Dictionary<int, Sprite>();

      foreach (PageBackgroundGroup group in backgroundGroups)
      {
         if (group == null || group.background == null || string.IsNullOrWhiteSpace(group.pages))
            continue;

         foreach (int pageNumber in ParsePageNumbers(group.pages))
         {
            // Later groups override earlier ones if pages overlap.
            _backgroundLookup[pageNumber] = group.background;
         }
      }
   }

   /// <summary>
   /// Parses a string like "1-10,15,22-30" into individual page numbers.
   /// </summary>
   private static IEnumerable<int> ParsePageNumbers(string pages)
   {
      string[] tokens = pages.Split(',');

      foreach (string rawToken in tokens)
      {
         string token = rawToken.Trim();

         if (token.Length == 0)
            continue;

         if (token.Contains("-"))
         {
            string[] bounds = token.Split('-');

            if (bounds.Length == 2
                && int.TryParse(bounds[0].Trim(), out int start)
                && int.TryParse(bounds[1].Trim(), out int end))
            {
               if (start > end)
               {
                  (start, end) = (end, start);
               }

               for (int p = start; p <= end; p++)
               {
                  yield return p;
               }
            }
         }
         else if (int.TryParse(token, out int single))
         {
            yield return single;
         }
      }
   }

#if UNITY_EDITOR
   private const string PageButtonFolder = "Assets/Learning/PageButtons";
   private const string ActivityPageFolder = "Assets/Learning/ActivityPages";

   private void OnValidate()
   {
      for (int i = 0; i < pages.Count; i++)
      {
         if (pages[i] == null)
            continue;

         // --------------------------------
         // PAGE NAME
         // --------------------------------

         // Uses each entry's own pageNumber (not its position in the list),
         // so pages can be listed out of order or with gaps and still load
         // the correct assets. Entries with no page number set yet (0) are
         // left alone rather than mislabeled/mismatched.
         int pageNumber = pages[i].pageNumber;

         if (pageNumber <= 0)
            continue;

         pages[i].pageName = pageNumber.ToString();


         // --------------------------------
         // PAGE BUTTON IMAGE
         // --------------------------------

         string imagePath =
             $"{PageButtonFolder}/{pageNumber}.png";

         pages[i].pageButtonImage =
             AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);


         // --------------------------------
         // ACTIVITY PAGES
         // --------------------------------

         if (pages[i].activities == null)
         {
            pages[i].activities = new ActivityData();
         }

         LoadActivityPages(
             pages[i].activities,
             pageNumber
         );
      }

      EditorUtility.SetDirty(this);

      // Background group edits may have changed which page maps to which
      // sprite - force the lookup to rebuild next time it's requested.
      _backgroundLookup = null;
   }


   private void LoadActivityPages(
    ActivityData activityData,
    int pageNumber)
   {
      activityData.pages.Clear();

      string[] prefixes =
      {
        $"P{pageNumber}",
        $"PO{pageNumber}"
    };

      string[] guids = AssetDatabase.FindAssets(
          "t:Prefab",
          new[] { ActivityPageFolder }
      );

      List<GameObject> foundPages = new();

      foreach (string guid in guids)
      {
         string path = AssetDatabase.GUIDToAssetPath(guid);

         GameObject prefab =
             AssetDatabase.LoadAssetAtPath<GameObject>(path);

         if (prefab == null)
            continue;

         string fileName =
             System.IO.Path.GetFileNameWithoutExtension(path);

         foreach (string prefix in prefixes)
         {
            // Example:
            // P4
            // PO4
            if (fileName == prefix)
            {
               foundPages.Add(prefab);
               break;
            }

            // Example:
            // P4.1
            // P4.2
            // PO4.1
            // PO4.2
            if (fileName.StartsWith(prefix + "."))
            {
               foundPages.Add(prefab);
               break;
            }
         }
      }

      // Sort activities
      foundPages.Sort((a, b) =>
      {
         int numberA = GetActivityNumber(a.name, pageNumber);
         int numberB = GetActivityNumber(b.name, pageNumber);

         return numberA.CompareTo(numberB);
      });

      // Add to ScriptableObject
      foreach (GameObject page in foundPages)
      {
         ActivityPageData activityPage =
             new ActivityPageData();

         activityPage.page = page;

         activityData.pages.Add(activityPage);
      }
   }

   private int GetActivityNumber(
       string objectName,
       int pageNumber)
   {
      string[] prefixes =
      {
        $"P{pageNumber}.",
        $"PO{pageNumber}."
    };

      foreach (string prefix in prefixes)
      {
         if (objectName.StartsWith(prefix))
         {
            string number =
                objectName.Substring(prefix.Length);

            if (int.TryParse(number, out int result))
            {
               return result;
            }
         }
      }

      // P4 / PO4 without activity number
      return 0;
   }
#endif
}


[System.Serializable]
public class PageBackgroundGroup
{
   public Sprite background;

   [Tooltip("Page numbers this background applies to, e.g. \"1-10,15,22-30\".")]
   public string pages;
}


[System.Serializable]
public class PageData
{
   [Header("Page")]
   [Tooltip("The real page number, used to auto-load PageButtons/{N}.png and ActivityPages/P{N}(.x) assets. Independent of this entry's position in the list, so pages can be listed with gaps/out of order.")]
   public int pageNumber;
   public string pageName;
   public Sprite pageButtonImage;

   [Header("Video")]
   public VideoData video;

   [Header("Activities")]
   public ActivityData activities;

   [Header("Sections")]
   [Tooltip("Optional. When enabled, selecting this page shows a list of section buttons instead of jumping straight to its activity list - picking a section then shows that section's own activity buttons. Off by default; enable it and fill in 'sections' yourself, nothing here is assigned automatically.")]
   public bool useSections;

   [Tooltip("The sections shown for this page when 'Use Sections' is enabled above, in order - each entry just holds that section's own activities list, assigned manually. The section buttons themselves live in the scene (see LearningPageButtonSpawner > sectionButtons).")]
   public List<SectionData> sections = new();
}


[System.Serializable]
public class VideoData
{
   public string videoName;

   [Tooltip("Remote video URL")]
   public string videoUrl;
}


[System.Serializable]
public class ActivityData
{
   [Header("Activity Pages")]
   public List<ActivityPageData> pages = new();
}


[System.Serializable]
public class SectionData
{
   [Tooltip("The activities shown after this section is picked. Assigned manually, same as a page's own activities list. The section button itself (label/image) is set up directly on its GameObject in the scene, not here.")]
   public ActivityData activities = new();
}


[System.Serializable]
public class ActivityPageData
{
   public GameObject page;
}