using UnityEngine; // <summary> 
using TMPro; // Add this for TMP_Text support. 
using UnityEngine.UI; // Add this for Image support. 
// Represents metadata for a ToDo item in the game. 

namespace LifeCraft.UI
{
    public class ToDoItemMeta : MonoBehaviour
    {
        public bool isFromDailyQuest = false; // Indicates if this item is from a daily quest. 
        public string dailyQuestText = ""; // The text of the daily quest this item is associated with. 
        public int rewardAmount = 5; // The dynamic reward amount for this to-do item (set when the item is created).
        public TMP_Text RewardAmountText; // Assign in Inspector: displays the dynamic reward amount for this to-do item (e.g., "+5").
        public TMP_Text LabelText; // Assign in Inspector: displays the quest/task label for this to-do item.
        public Image ResourceIcon; // Assign in Inspector: displays the resource icon for this to-do item. 
    }
}