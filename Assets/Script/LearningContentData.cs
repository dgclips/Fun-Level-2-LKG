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

         int pageNumber = i + 1;

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
public class PageData
{
   [Header("Page")]
   public string pageName;
   public Sprite pageButtonImage;

   [Header("Video")]
   public VideoData video;

   [Header("Activities")]
   public ActivityData activities;
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
public class ActivityPageData
{
   public GameObject page;
}