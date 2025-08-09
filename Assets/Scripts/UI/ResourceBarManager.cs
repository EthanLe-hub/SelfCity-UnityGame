// AI Cursor + Ethan Le code with comments and explanations for code comprehension. Language: C# (C Sharp)

using UnityEngine;
using LifeCraft.Core;

namespace LifeCraft.UI
{
    public class ResourceBarManager : MonoBehaviour
    {
        [Header("Assign each ResourceDisplay for the five resources")]
        public ResourceDisplay energyCrystalsDisplay; // Add this field to the Inspector to assign the Energy Crystals ResourceDisplay. 
        public ResourceDisplay wisdomOrbsDisplay; // Add this field to the Inspector to assign the Wisdom Orbs ResourceDisplay.
        public ResourceDisplay heartTokensDisplay; // Add this field to the Inspector to assign the Heart Tokens ResourceDisplay.
        public ResourceDisplay creativitySparksDisplay; // Add this field to the Inspector to assign the Creativity Sparks ResourceDisplay.
        public ResourceDisplay balanceTicketsDisplay; // Add this field to the Inspector to assign the Balance Tickets ResourceDisplay.

        void Start()
        {
            // Initialize each display with its resource type and the current value from ResourceManager.Instance
            energyCrystalsDisplay.Initialize(ResourceManager.ResourceType.EnergyCrystals, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.EnergyCrystals));
            wisdomOrbsDisplay.Initialize(ResourceManager.ResourceType.WisdomOrbs, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.WisdomOrbs));
            heartTokensDisplay.Initialize(ResourceManager.ResourceType.HeartTokens, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.HeartTokens));
            creativitySparksDisplay.Initialize(ResourceManager.ResourceType.CreativitySparks, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.CreativitySparks));
            balanceTicketsDisplay.Initialize(ResourceManager.ResourceType.BalanceTickets, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.BalanceTickets));

            // --- Fix: Immediately update the UI to reflect the current resource values at game start ---
            // This ensures the UI is correct even before any resource changes occur.
            OnResourceUpdated(ResourceManager.ResourceType.EnergyCrystals, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.EnergyCrystals));
            OnResourceUpdated(ResourceManager.ResourceType.WisdomOrbs, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.WisdomOrbs));
            OnResourceUpdated(ResourceManager.ResourceType.HeartTokens, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.HeartTokens));
            OnResourceUpdated(ResourceManager.ResourceType.CreativitySparks, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.CreativitySparks));
            OnResourceUpdated(ResourceManager.ResourceType.BalanceTickets, ResourceManager.Instance.GetResourceTotal(ResourceManager.ResourceType.BalanceTickets));
        }

        void OnEnable()
        {
            ResourceManager.Instance.OnResourceUpdated.AddListener(OnResourceUpdated);
        }

        void OnDisable()
        {
            ResourceManager.Instance.OnResourceUpdated.RemoveListener(OnResourceUpdated);
        }

        private void OnResourceUpdated(ResourceManager.ResourceType resourceType, int newAmount)
        {
            switch (resourceType)
            {
                case ResourceManager.ResourceType.EnergyCrystals:
                    energyCrystalsDisplay.UpdateAmount(newAmount);
                    break;
                case ResourceManager.ResourceType.WisdomOrbs:
                    wisdomOrbsDisplay.UpdateAmount(newAmount);
                    break;
                case ResourceManager.ResourceType.HeartTokens:
                    heartTokensDisplay.UpdateAmount(newAmount);
                    break;
                case ResourceManager.ResourceType.CreativitySparks:
                    creativitySparksDisplay.UpdateAmount(newAmount);
                    break;
                case ResourceManager.ResourceType.BalanceTickets:
                    balanceTicketsDisplay.UpdateAmount(newAmount);
                    break;
            }
        }
    }
} 