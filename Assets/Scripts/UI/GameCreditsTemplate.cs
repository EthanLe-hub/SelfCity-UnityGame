using UnityEngine;
using TMPro;

namespace LifeCraft.UI
{
    /// <summary>
    /// Template for Game Credits panel content
    /// </summary>
    public class GameCreditsTemplate : MonoBehaviour
    {
        [Header("Company Information")]
        [SerializeField] private TMP_Text companyNameText;
        [SerializeField] private TMP_Text gameTitleText;
        [SerializeField] private TMP_Text versionText;

        [Header("Team Credits")]
        [SerializeField] private TMP_Text gameDeveloperText;
        [SerializeField] private TMP_Text graphicDesignerText;
        [SerializeField] private TMP_Text conceptCreatorsText;

        [Header("Additional Credits")]
        [SerializeField] private TMP_Text specialThanksText;
        [SerializeField] private TMP_Text copyrightText;

        private void Start()
        {
            PopulateCredits();
        }

        /// <summary>
        /// Populate the credits with team information
        /// </summary>
        private void PopulateCredits()
        {
            // Company Information
            if (companyNameText != null)
                companyNameText.text = "Sparq Capital";

            if (gameTitleText != null)
                gameTitleText.text = "SelfCity";

            if (versionText != null)
                versionText.text = "Version 1.0";

            // Team Credits
            if (gameDeveloperText != null)
                gameDeveloperText.text = "Game Developer\nEthan Le";

            if (graphicDesignerText != null)
                graphicDesignerText.text = "Graphic Designer\nMoneeza Azmat";

            if (conceptCreatorsText != null)
                conceptCreatorsText.text = "Concept & Design\nSarah Tanjoco\nJenney Nguyen\nEthan Le";

            // Additional Credits
            if (specialThanksText != null)
                specialThanksText.text = "Special Thanks\nTo all the players who believed in our vision\nand helped make this game possible.";

            if (copyrightText != null)
                copyrightText.text = "© 2025 Sparq Capital\nAll rights reserved.";
        }

        /// <summary>
        /// Update credits with custom information
        /// </summary>
        public void UpdateCredits(string companyName, string developerName, string designerName, 
            string creator1Name, string creator2Name, string creator3Name)
        {
            if (companyNameText != null)
                companyNameText.text = companyName;

            if (gameDeveloperText != null)
                gameDeveloperText.text = $"Game Developer\n{developerName}";

            if (graphicDesignerText != null)
                graphicDesignerText.text = $"Graphic Designer\n{designerName}";

            if (conceptCreatorsText != null)
                conceptCreatorsText.text = $"Concept & Design\n{creator1Name}\n{creator2Name}\n{creator3Name}";

            if (copyrightText != null)
                copyrightText.text = $"© 2024 {companyName}\nAll rights reserved.";
        }
    }
} 