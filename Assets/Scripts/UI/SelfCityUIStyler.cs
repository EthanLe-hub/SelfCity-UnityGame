using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LifeCraft.UI
{
    public class SelfCityUIStyler : MonoBehaviour
    {
        [Header("Sprites & Fonts")]
        public Sprite roundedButtonSprite;
        public TMP_FontAsset modernFont;

        [Header("Icons")]
        public Sprite healthHarborIcon;
        public Sprite creativityCommonsIcon;
        public Sprite mindPalaceIcon;
        public Sprite socialSquareIcon;

        [Header("Accent Colors")]
        public Color healthHarborColor = new Color(0.49f, 0.85f, 0.34f); // #7ED957
        public Color creativityCommonsColor = new Color(1f, 0.88f, 0.4f); // #FFE066
        public Color mindPalaceColor = new Color(0.7f, 0.62f, 0.86f); // #B39DDB
        public Color socialSquareColor = new Color(1f, 0.7f, 0.28f); // #FFB347

        [Header("UI References")]
        public Button healthHarborButton;
        public Button creativityCommonsButton;
        public Button mindPalaceButton;
        public Button socialSquareButton;

        void Start()
        {
            StyleButton(healthHarborButton, healthHarborColor, healthHarborIcon, "Health Harbor");
            StyleButton(creativityCommonsButton, creativityCommonsColor, creativityCommonsIcon, "Creativity Commons");
            StyleButton(mindPalaceButton, mindPalaceColor, mindPalaceIcon, "Mind Palace");
            StyleButton(socialSquareButton, socialSquareColor, socialSquareIcon, "Social Square");
        }

        void StyleButton(Button button, Color color, Sprite icon, string label)
        {
            if (button == null) return;

            // Set background
            var image = button.GetComponent<Image>();
            if (image != null && roundedButtonSprite != null)
            {
                image.sprite = roundedButtonSprite;
                image.type = Image.Type.Sliced;
                image.color = color;
            }

            // Set icon (assumes first child is icon)
            var iconImage = button.transform.GetChild(0).GetComponent<Image>();
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
            }

            // Set label (assumes second child is TMP_Text)
            var text = button.transform.GetChild(1).GetComponent<TMP_Text>();
            if (text != null)
            {
                text.text = label;
                if (modernFont != null) text.font = modernFont;
                text.fontSize = 32;
                text.color = Color.black;
            }

            // Optional: Adjust padding, spacing, etc. here
        }
    }
}