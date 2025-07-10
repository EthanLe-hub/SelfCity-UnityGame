# SelfCity Unity - City Building Game

A Unity-based city-building game that helps you build healthy real-life habits while constructing your dream city.

## 🎮 Game Overview

SelfCity is a 2D city-building game where players construct and manage their own city while also building healthy habits in real life. The quest system is inspired by real-world wellness, creativity, social, and health activities, encouraging players to take positive actions each day. The game features an intuitive UI system with drag-and-drop functionality, daily quests, a custom quest system, and a comprehensive building system.

## ✨ Features

- **🏗️ City Building System** - Place and manage various building types
- **📋 Quest & Habit System**
  - **Static Region Quests**: Each region (Health Harbor, Mind Palace, Social Square, Creative Commons) has its own unique set of static quests, all inspired by healthy habits (physical, mental, social, and creative wellness).
  - **Daily Quests**: A separate pool of daily quests is generated every 24 hours, with each quest tagged by its region and based on real-life healthy activities.
  - **Custom Tasks**: Players can create their own custom quests, assign them to a region, and add them to their To-Do list via a dedicated modal window—perfect for tracking your own healthy habits.
- **💰 Resource Management**
  - Completing quests rewards you with the correct region-based currency (crystals, hearts, magical, stars, etc.).
  - **Dynamic Rewards:** The reward amount and icon are parsed directly from the quest string (e.g., "+5"), and are displayed and rewarded dynamically in all quest and To-Do list UIs.
- **🎯 Interactive UI**
  - Modern, responsive user interface with modal windows for quests, custom tasks, and chest opening confirmations
  - Confirmation modal prevents accidental spending of tickets when opening chests
  - All quest lists and the To-Do list display both the reward amount and the correct resource icon for each quest.
- **📦 Inventory & Shop System**
  - Decorations and buildings won from chests or purchased from the shop are stored in a persistent inventory
  - Flexible shop UI for region-specific buildings and currencies
  - Drag-and-drop from inventory to city
- **📱 Drag & Drop** - Intuitive building placement system
- **🎨 Visual Assets** - Rich collection of sprites and visual elements
- **🔧 Modular Architecture** - Well-structured codebase for easy expansion

## 🛠️ Technical Details

- **Engine:** Unity 2022.3 LTS (or newer)
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Input System:** Unity's new Input System
- **UI Framework:** Unity UI with TextMesh Pro
- **Scripting:** C#

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Game management and core systems
│   ├── Buildings/      # Building-related scripts
│   ├── Districts/      # District management
│   ├── Systems/        # Quest and unlock systems
│   └── UI/            # User interface scripts (including dynamic quest/To-Do UI)
├── Scenes/            # Unity scenes
├── Prefabs/           # Reusable game objects (including ToDoItemPrefab, QuestItem, etc.)
├── Sprites/           # Visual assets and icons
├── Materials/         # Materials and shaders
└── ScriptableObjects/ # Game data and configurations
```

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or newer
- Universal Render Pipeline (URP) - will be auto-installed

### Installation
1. **Clone or Download the Project**
   - You can clone the repository or download and unzip the project folder.
   - To zip your project for sharing, include only the following folders:
     - `Assets/`
     - `Packages/`
     - `ProjectSettings/`
     - (optionally) `UserSettings/`
   - Do **not** include `Library/`, `Temp/`, or `Logs/` folders.

2. **Open in Unity Hub**
   - Launch Unity Hub
   - Click "Open" and select the project folder
   - Wait for Unity to import and install packages

3. **Open the main scene**
   - Navigate to `Assets/Scenes/SampleScene.unity`
   - Press Play to start the game

## 🎯 How to Play

1. **Build Your City** - Use the building menu to place structures and grow your city.
2. **Complete Quests**
   - **Region Quests**: Click a region button to view and add static quests to your To-Do list. Each quest is inspired by a real-life healthy habit (e.g., exercise, mindfulness, social connection, creativity).
   - **Daily Quests**: Open the Daily Quests modal to view a set of daily quests (refreshes every 24 hours), each encouraging a positive real-world action.
   - **Custom Tasks**: Click "Add Custom Task" to open a modal where you can create your own healthy habits, assign them to a region, and add them to your To-Do list.
3. **Earn Rewards**
   - Completing quests in the To-Do list gives you the correct region-based currency and amount, as shown in the UI.
4. **Open Chests with Confirmation**
   - When opening a Decor Chest or Premium Decor Chest, a confirmation modal appears to prevent accidental spending of Balance Tickets. Confirm to open and receive a random reward, which is added to your inventory.
5. **Manage Your Inventory**
   - All decorations and buildings you win or purchase are stored in your inventory. Use the inventory UI to view, filter, and drag items into your city.
6. **Expand Districts** - Unlock new areas as you progress

## 📝 Custom Tasks Feature

- Click the **Add Custom Task** button to open the custom quest modal.
- Enter your custom activity and select a region (Health Harbor, Mind Palace, Social Square, or Creative Commons).
- Your custom quest will appear in the modal's list, with an "Add To-Do" button next to it.
- You can add any custom quest to your To-Do list, and completing it will reward you with the correct region currency and amount.

## 🔄 Daily Quest Refresh Logic
- Daily quests are generated from a separate pool and are tagged by region.
- The list of daily quests automatically refreshes with new quests every 24 hours.
- The region label at the end of each quest ensures the correct reward is given.

## 🧩 Dynamic Rewards System
- The reward amount and icon for each quest are parsed from the quest string (e.g., "+5").
- Changing the reward in the quest string automatically updates the UI and the actual reward given to the player.
- **Anyone who downloads this repository and opens it in Unity will see the exact same UI, quest system, and dynamic rewards as the project author.**

## 🔧 Development

### Key Scripts
- `GameManager.cs` - Main game controller
- `CityBuilder.cs` - Building placement system
- `QuestManager.cs` - Quest system management (static, daily, and custom quests)
- `ResourceManager.cs` - Resource tracking
- `UIManager.cs` - User interface controller
- `CustomTaskModalHandler.cs` - Handles the custom quest modal logic
- `QuestItemUI.cs` - Handles quest item display and "Add To-Do" logic
- `ToDoListManager.cs` - Manages the To-Do list and quest completion
- `DecorChestManager.cs` - Handles chest opening logic and confirmation modal integration
- `RewardModal.cs` - Displays reward popups for chests and shop purchases
- `PurchaseConfirmModal.cs` - Displays confirmation modal before spending tickets or making purchases
- `InventoryManager.cs` - Manages player inventory for decorations and buildings
- `ShopBuildingItemUI.cs` - Handles shop item display and purchase logic

### Adding New Features
1. Create scripts in appropriate folders
2. Follow the existing naming conventions
3. Use ScriptableObjects for data-driven features
4. Test thoroughly before committing

## 📸 Screenshots

*Add screenshots of your game here*

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Unity Technologies for the game engine
- TextMesh Pro for advanced text rendering
- All contributors and testers

---

**Built with ❤️ using Unity** 