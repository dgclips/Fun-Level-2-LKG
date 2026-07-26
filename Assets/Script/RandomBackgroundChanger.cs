using UnityEngine;
using UnityEngine.UI;

public class RandomBackgroundChanger : MonoBehaviour
{
   [SerializeField] private Image backgroundImage;
   [SerializeField] private Sprite[] backgrounds;

   private int lastIndex = -1;

   private void Start()
   {
      ChangeRandomBackground();
   }


   public void ChangeRandomBackground()
   {
      if (backgrounds == null || backgrounds.Length == 0)
      {
         Debug.LogWarning("No backgrounds assigned!");
         return;
      }

      int randomIndex;

      // Prevent selecting the same background twice in a row
      do
      {
         randomIndex = Random.Range(0, backgrounds.Length);
      }
      while (backgrounds.Length > 1 && randomIndex == lastIndex);

      lastIndex = randomIndex;
      backgroundImage.sprite = backgrounds[randomIndex];
   }
}