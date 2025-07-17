using UnityEngine;
using TMPro;
using System;

namespace LifeCraft.Shop
{
    public class PrizePoolTimerUI : MonoBehaviour
    {
        public PrizePoolManager prizePoolManager; // Assign in Inspector
        public TextMeshProUGUI timerText;         // Assign in Inspector

        private void Update()
        {
            if (prizePoolManager == null || timerText == null) return;

            DateTime lastReset = prizePoolManager.GetLastResetTime();
            TimeSpan timeSinceReset = DateTime.UtcNow - lastReset;
            TimeSpan timeToNextReset = TimeSpan.FromHours(24) - timeSinceReset;

            if (timeToNextReset.TotalSeconds < 0)
                timeToNextReset = TimeSpan.Zero;

            timerText.text = $"Next refresh in: {timeToNextReset.Hours:D2}:{timeToNextReset.Minutes:D2}:{timeToNextReset.Seconds:D2}";
        }
    }
}