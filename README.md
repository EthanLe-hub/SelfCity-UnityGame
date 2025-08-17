# SelfCity Unity - City Building Game

A Unity-based city-building game that helps you build healthy real-life habits while constructing your dream city.

## 🎮 Game Overview
SelfCity is a 2D city-building game where players construct and manage their own city while building healthy habits in real life. The quest system encourages real-world wellness activities, with quests inspired by physical, mental, social, and creative wellness.

## ✨ Core Features

### 🏗️ City Building & Management
- **Grid-Based Building System**: Place buildings and decorations on a grid with drag-and-drop functionality
- **Region-Based City**: Four distinct regions (Health Harbor, Mind Palace, Social Square, Creative Commons)
- **Building Categories**: 80+ buildings across different regions with unlock progression
- **Visual States**: Buildings support regular and damaged visual states
- **Construction Timer System**: Buildings require 1-6 hours of construction time before becoming functional
- **Action Menu System**: Interactive menu for placed buildings (Store, Sell, Rotate, Close)
- **Construction Progress Saving**: Save and restore construction progress when storing/placing buildings

### 🎯 Quest & Habit System
- **Static Region Quests**: Each region has unique quests inspired by healthy habits
- **Daily Quests**: Rotating daily quests that refresh every 24 hours
- **Custom Tasks**: Players can create their own healthy habit quests
- **Skip Quest System**: Complete quests to instantly finish building construction
- **Quest Difficulty**: Expert, Hard, Medium, Easy quests based on construction time remaining
- **Skip Quest State Preservation**: Skip button text and quest state preserved when storing buildings

### 💰 Resource & Progression System
- **Region-Based Currencies**: Energy Crystals, Wisdom Orbs, Heart Tokens, Creativity Sparks
- **Dynamic Rewards**: Reward amounts and icons parsed from quest strings
- **Level-Up System**: Player progression with EXP-based leveling (1-40 levels)
- **Building Unlocks**: Buildings unlock progressively based on player level and assessment results
- **EXP Rewards**: Building placement and quest completion reward EXP with difficulty-based calculations
- **Shop Database Integration**: Building prices tied to region-specific shop databases
- **Premium Currency Bonuses**: Premium users get 8 currency per quest vs 5 for free users (60% increase)
- **Premium Resource Bonuses**: 50% bonus on all resources earned for premium subscribers

### 🎒 Inventory & Shop System
- **Persistent Inventory**: All decorations and buildings stored in persistent inventory
- **Drag-and-Drop**: Intuitive placement system with inventory integration
- **Shop System**: Purchase buildings and decorations with region-specific currencies
- **Visual Confirmations**: Purchase dialogs show building sprites before confirming
- **Region Type Preservation**: Building region types preserved when storing/placing

### 🔐 Authentication & Subscription
- **Multi-Platform Auth**: Google Sign-In, Apple Sign-In, Email/Password, Guest mode
- **Premium Features**: Exclusive content for premium subscribers
- **Profile Management**: Complete user profile system with data persistence
- **Journal System**: Personal journal with mood tracking and auto-save functionality

### 🎨 User Interface
- **Modern UI Design**: Clean, responsive interface with consistent theming
- **Region Zoom**: Click regions to zoom in with smooth camera transitions
- **Edit Mode**: Grid overlay for precise building placement
- **Visual Feedback**: EXP popups, level-up celebrations, unlock notifications
- **Mobile Optimization**: Touch-friendly controls and responsive design
- **Action Menu UI**: Context-sensitive menu for building interactions
- **Premium UI Synchronization**: Real-time updates of premium status across all UI components
- **Unified Status Display**: Consistent Premium/Free indicators in Profile, main UI, and quest systems
- **Smart Button Management**: Premium feature buttons automatically enable/disable based on subscription status
- **Event-Driven Updates**: UI components automatically refresh when subscription status changes

## 🛠️ Technical Details

### Engine & Architecture
- **Unity 2022.3 LTS**: Universal Render Pipeline (URP)
- **C# Scripting**: Well-structured, modular codebase
- **Event-Driven Design**: Loose coupling between systems
- **Singleton Pattern**: Manager classes for easy access
- **ScriptableObject-Based**: Game data stored in ScriptableObjects

### Key Systems
- **CityBuilder**: Core building placement and management
- **ConstructionManager**: Construction timer and skip quest system with pause/resume functionality and premium time reduction
- **PlayerLevelManager**: Player progression and building unlocks
- **QuestManager**: Quest generation, tracking, and rewards
- **AuthenticationManager**: Multi-platform authentication
- **InventoryManager**: Inventory and item management
- **UIManager**: Centralized UI management with premium status synchronization
- **ProfileManager**: Profile management with unified subscription status display
- **SubscriptionManager**: Single source of truth for premium subscription status
- **HoldDownInteraction**: Action menu system for building interactions
- **BuildingConstructionTimer**: Individual building construction timers with UI
- **MoodManager**: Daily mood tracking and persistence with event-driven mood change system
- **WeatherSystem**: Dynamic weather effects based on player mood with smooth transitions and persistence

### Data Persistence
- **JSON Serialization**: Save data in JSON format
- **Local Storage**: Save files stored locally on device
- **Automatic Saving**: Game saves after key actions
- **Cross-Session Persistence**: All data maintained between sessions
- **Construction Progress**: Building construction state saved and restored
- **Mood & Weather Data**: Player mood selections and weather states persisted across sessions
- **AI Chat History**: Local chat history with export and clear functionality

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or newer
- Universal Render Pipeline (URP)
- Android Build Support (for mobile development)
- iOS Build Support (for iOS development)

### Installation
1. **Clone the Project**: Download and extract the project folder
2. **Open in Unity**: Launch Unity Hub and open the project
3. **Open Scene**: Navigate to `Assets/Scenes/SampleScene.unity`
4. **Press Play**: Start the game

### Authentication Setup
For full authentication functionality, see: `Assets/Scripts/UI/Authentication_Setup_Guide.md`

### iOS Setup
For iOS development and testing, see: `iOS_SETUP_GUIDE.md` and `SETUP_IOS_WINDOWS.md`

## 🎯 How to Play

1. **Build Your City**: Use the building menu to place structures
2. **Zoom and Edit**: Click regions to zoom in, enter edit mode for grid placement
3. **Interact with Buildings**: Tap placed buildings to open Action Menu (Store, Sell, Rotate)
4. **Complete Quests**: Add region quests, daily quests, or custom tasks to your To-Do list
5. **Earn Rewards**: Complete quests for region-based currencies and EXP
6. **Level Up**: Gain EXP to unlock new buildings and regions
7. **Manage Construction**: Wait for buildings to complete or complete skip quests to finish instantly

## 🆕 Latest Features

### 🌤️ Mood & Weather System
- **Daily Mood Tracking**: 24-hour mood check system with persistent mood selection (`MoodManager` + `MoodCheck`)
- **Mood-Based Weather**: Dynamic weather effects that change based on player mood (Happy=Sunny, Sad=Rainy, Moody=Cloudy, Stressed=Stormy)
- **Smooth Weather Transitions**: 2-second fade transitions between weather states with coroutine-based animation system
- **Weather Persistence**: Weather state saved and restored across game sessions (`WeatherSystem`)
- **Event-Driven Architecture**: Mood changes automatically trigger weather updates via event system
- **Debug Tools**: Context menu options for testing weather states and system status

### 🤖 AI Assistant System
- **In-Game AI Chat**: Context-aware assistant that understands your current level, resources, and quests
- **Chat History**: Local, persistent `AIChatHistory` with export and clear options
- **Modern UI**: Typing indicator, smooth animations, user/AI message styling (`AIAssistantModal`)
- **Configurable**: `AIConfiguration` ScriptableObject supports Azure OpenAI and OpenAI API (dev/prod modes)

### 👥 Friends & Social
- **Friends List**: Add/remove, pending requests, block/unblock, online status
- **Premium Limit Lift**: Freemium friend cap with Premium unlocking unlimited friends (`FriendsManager` + `SubscriptionManager`)
- **Persistence**: Local save/load with events for UI updates

### 🏗️ Action Menu System
- **Interactive Building Menu**: Tap placed buildings to open context menu
- **Store Functionality**: Return buildings to inventory with construction progress saved
- **Sell Functionality**: Sell buildings for 50% of original cost with confirmation dialog
- **Rotate Functionality**: Rotate buildings 90 degrees
- **Smart Positioning**: Menu appears next to building with screen bounds checking

### 🏗️ Construction Progress System
- **Pause/Resume Construction**: Store buildings under construction and resume later
- **Progress Preservation**: Construction time, quest state, and skip button text saved
- **Region Type Preservation**: Building region types maintained when storing/placing
- **Skip Quest State**: Skip button text and quest progress preserved across sessions
- **Premium Time Reduction**: Premium subscribers get 20% faster construction time on new buildings
- **Smart Time Management**: Construction time reduction only applied to new projects, not restored progress
- **Dual Registration System**: Separate methods for new construction vs. progress restoration

### 🏗️ Enhanced Building Management
- **Shop Database Integration**: Building prices from region-specific shop databases
- **Region Type Detection**: Automatic region detection based on building names
- **Construction Timer Pause**: Construction timers pause when buildings stored
- **UI Synchronization**: Skip button text updates immediately when placing buildings

### 📱 iOS Development Support
- **iOS Configuration**: Complete iOS setup for Windows developers
- **Cloud Build Support**: GitHub Actions workflow for automated iOS builds
- **Unity Cloud Build**: Alternative cloud-based iOS building
- **Info.plist Configuration**: Proper iOS app configuration
- **Build Automation**: Automated build scripts for iOS and Android

### ⭐ Premium Subscription Enhancements
- **Simulation Mode**: Safe, local-only subscription testing (no real purchases)
- **Benefits**: Premium Decor Chest, premium buildings/resources, premium journal features
- **Progress Boosts**: 20% faster construction, region/resource bonuses
- **Unlimited Friends**: Removes free-tier friend cap
- **Status UI**: Clear Premium/Free indicators across Profile/UI
- **Construction Time Reduction**: Premium users get 20% faster building construction
- **Smart Reduction Logic**: Time reduction only applies to new buildings, preserving saved progress integrity
- **Quest Currency Bonuses**: Premium users get 8 currency per quest (60% increase from free user's 5)
- **Resource Bonuses**: 50% bonus on all resources earned for premium subscribers
- **Unified Premium System**: Single source of truth for subscription status across all UI components
- **Real-time UI Synchronization**: All premium indicators update instantly when subscription status changes
- **Comprehensive Feature Gating**: Premium buttons, badges, and status text synchronized across Profile, UI, and Quest systems

### 🔐 Authentication & Profile Improvements
- **Auth Methods**: Google, Apple, Email/Password, Guest (simulated flows with session restore)
- **Unified UI**: `AuthenticationUI` integrates with Profile and subscription status
- **Profile & Journal**: Auto-save, mood tracking, entry edit/delete, premium drawings/media, daily prompt recommendations

### 🧭 Onboarding Assessment
- **Wellness Quiz**: Recommends a starting region based on answers (`AssessmentQuizManager`)
- **Progress Display**: Icons, descriptions, and question progress

### ⚙️ Configuration & Build
- **Billing Mode**: `Assets/Resources/BillingMode.json` selects Android store (e.g., GooglePlay)
- **AI Config**: Create `AIConfiguration` asset and set API keys; keep keys out of source control

## 📁 Project Structure
```
Assets/
├── Scripts/
│   ├── Core/           # Game management and core systems
│   ├── Buildings/      # Building-related scripts
│   ├── Systems/        # Quest, construction, authentication systems
│   ├── Shop/           # Shop and subscription management
│   └── UI/            # User interface scripts
├── Prefabs/           # Reusable game objects
├── Sprites/           # Visual assets (500+ sprites)
├── ScriptableObjects/ # Game data and configurations
├── Plugins/iOS/       # iOS-specific configuration files
├── Editor/            # Build scripts and editor tools
└── Scenes/           # Game scenes
```

## 🎨 Visual Assets
- **Building Sprites**: 141+ buildings with regular and damaged states
- **Ground & Roads**: 109+ ground and road sprites
- **Vehicles**: 167+ vehicle sprites for city atmosphere
- **Props & Vegetation**: 49+ prop and vegetation sprites
- **People**: 23+ character sprites for city population
- **Icons**: 24+ icon sprites for UI elements

## 🔧 Development Tools
- **BatchAssignPrefab**: Automated prefab assignment for building types
- **BatchAssignSprites**: Automated sprite assignment for UI elements
- **CityBuilderAutoPopulateEditor**: Automated building type population
- **LevelUpDebugger**: Debug script for testing level-up systems
- **ConstructionManager**: Debug tools for testing construction system
- **BuildScript**: Automated build scripts for iOS and Android

## 📱 Platform Support

### Android
- **Full Support**: Complete Android build and testing
- **Touch Controls**: Optimized for mobile touch interaction
- **Performance**: Optimized for mobile devices

### iOS
- **Build Support**: Automated iOS builds via GitHub Actions
- **Cloud Building**: Unity Cloud Build integration
- **Configuration**: Complete iOS setup for Windows developers
- **Testing**: Unity Remote support (requires iTunes on Windows)

## 📊 Development Status

### ✅ Completed Features
- [x] Core city building mechanics with grid system
- [x] Persistent save/load system
- [x] Inventory management with drag-and-drop
- [x] Quest system (static, daily, custom, skip quests)
- [x] Construction timer and skip quest system with premium time reduction
- [x] Level-up and progression system
- [x] Multi-platform authentication
- [x] Premium subscription system with construction benefits and unified UI synchronization
- [x] Profile and journal system with real-time premium status updates
- [x] Shop system with visual confirmations
- [x] Region unlock system
- [x] EXP and reward system
- [x] Mobile optimization
- [x] Action Menu system for building interactions
- [x] Construction progress saving and restoration with smart time management
- [x] iOS development setup and cloud builds
- [x] Enhanced building management with region preservation
- [x] Comprehensive premium UI synchronization across all components
- [x] Mood tracking system with daily mood checks and persistence
- [x] Dynamic weather system with mood-based weather effects and smooth transitions
- [x] AI Assistant system with context-aware wellness advice and chat history

### 🔄 Current Development
- **Performance Optimization**: Ongoing system improvements
- **Cross-Platform Testing**: iOS and Android testing
- **UI Polish**: Advanced UI features and refinements

## 🚀 Build & Deployment

### Local Development
1. **Open project** in Unity 2022.3 LTS
2. **Switch platform** (Android/iOS) in Build Settings
3. **Build project** using BuildScript or manual build

### Cloud Builds
- **GitHub Actions**: Automated builds on push to main/develop
- **Unity Cloud Build**: Alternative cloud-based building
- **Build Artifacts**: Download .apk/.ipa files from cloud builds

### Platform Requirements
- **Android**: Android Build Support in Unity
- **iOS**: macOS with Xcode (or cloud builds for Windows)

## 🔒 Security & Privacy
- **Local-Only Saves**: Player data and AI chat history stored locally; no external storage by default
- **Clear Data**: In-game options to clear chat history and personal data
- **Secrets**: Keep API keys outside source; use `AIConfiguration` and environment-secure methods for production

## 🤝 Contributing
This is a personal development project. For questions or suggestions, please refer to the development notes within the codebase.

## 📄 License
This project uses various asset packs with their respective licenses. Please refer to individual license files in the Sprites directory.

---

**Built with ❤️ using Unity**

*Last Updated: August 2025.*
*Developed By: Ethan Le.*
*Development Status: Active Development - Core Systems Complete with Mood & Weather Integration.* 