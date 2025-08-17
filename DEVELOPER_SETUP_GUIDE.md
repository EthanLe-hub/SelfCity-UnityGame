# 🚀 Developer Setup Guide - SelfCity Unity Project

## 📋 Prerequisites

### Required Software
- **Unity 2022.3 LTS** or newer
- **Unity Hub** (latest version)
- **Git** (for version control)
- **Visual Studio** or **Rider** (for C# development)

### Unity Modules to Install
- **Universal Render Pipeline (URP)**
- **Android Build Support**
- **iOS Build Support** (if developing for iOS)
- **TextMesh Pro**

## 🔧 Project Setup

### 1. Clone the Repository
```bash
git clone [your-repository-url]
cd SelfCityUnity1
```

### 2. Open in Unity
1. **Launch Unity Hub**
2. **Click "Open"** → **"Add"**
3. **Select the project folder**
4. **Wait for Unity to import** (may take 5-10 minutes)

### 3. Open the Main Scene
- Navigate to `Assets/Scenes/SampleScene.unity`
- **Double-click** to open the scene

## 🏗️ Project Architecture

### Core Systems
```
Assets/Scripts/
├── Core/                    # Core game systems
│   ├── CityBuilder.cs       # Building placement and management
│   ├── GameManager.cs       # Main game controller
│   └── DecorationItem.cs    # Data structure for items
├── Systems/                 # Game systems
│   ├── ConstructionManager.cs    # Construction timers and quests
│   ├── PlayerLevelManager.cs     # Player progression
│   ├── QuestManager.cs           # Quest system
│   └── InventoryManager.cs       # Inventory management
├── UI/                      # User interface
│   ├── HoldDownInteraction.cs    # Action menu system
│   ├── BuildingConstructionTimer.cs # Construction UI
│   └── DraggableInventoryItem.cs # Inventory drag/drop
└── Shop/                    # Shop and commerce
    ├── BuildingShopDatabase.cs   # Shop data
    └── BuildingShopItem.cs       # Shop item data
```

### Key ScriptableObjects
```
Assets/ScriptableObjects/
├── Buildings/               # Building data
├── GameData/                # Game configuration
└── Resources/               # Game resources
```

## 🎮 Understanding the Game Systems

### 1. City Building System
- **Grid-based placement** with drag-and-drop
- **Region-based cities** (Health Harbor, Mind Palace, etc.)
- **Construction timers** with skip quest system
- **Action menu** for building interactions

### 2. Quest System
- **Static region quests** for each building type
- **Daily quests** that refresh every 24 hours
- **Custom quests** that players can create
- **Skip quests** to instantly complete construction

### 3. Progression System
- **EXP-based leveling** (1-40 levels)
- **Building unlocks** based on player level
- **Region unlocks** based on assessment results
- **Currency system** with region-specific currencies

### 4. Construction System
- **Time-based construction** (1-6 hours)
- **Progress saving** when storing buildings
- **Skip quest integration** for instant completion
- **UI synchronization** for real-time updates

## 🔧 Development Workflow

### Adding New Buildings
1. **Create building sprite** in `Assets/Sprites/Buildings/`
2. **Add to BuildingShopDatabase** in appropriate region
3. **Configure in PlayerLevelManager** for unlock requirements
4. **Test placement** and construction system

### Adding New Quests
1. **Add quest text** to appropriate region in `ConstructionQuestPool`
2. **Configure difficulty** and reward amounts
3. **Test quest generation** and completion

### Modifying UI
1. **Edit prefabs** in `Assets/Prefabs/UI/`
2. **Update scripts** in `Assets/Scripts/UI/`
3. **Test on mobile** using Unity Remote or builds

## 📱 Platform Development

### Android Development
1. **Switch to Android platform** in Build Settings
2. **Configure Player Settings** for Android
3. **Build APK** using `Assets/Editor/BuildScript.cs`
4. **Test on device** or emulator

### iOS Development
1. **Switch to iOS platform** in Build Settings
2. **Configure iOS settings** (see `iOS_SETUP_GUIDE.md`)
3. **Use cloud builds** via GitHub Actions
4. **Test with Unity Remote** (requires iTunes on Windows)

## 🧪 Testing

### Unity Editor Testing
- **Click Play** to test game mechanics
- **Use debug tools** in `Assets/Scripts/UI/`
- **Check Console** for errors and logs

### Mobile Testing
- **Unity Remote** for quick testing
- **Build APK/IPA** for full testing
- **Test on multiple devices** for compatibility

### Debug Tools
- **LevelUpDebugger**: Test level-up systems
- **ConstructionManager**: Debug construction timers
- **ActionMenuDebugger**: Test action menu system

## 🔄 Version Control

### Files to Include
```
✅ Assets/           # All game content
✅ ProjectSettings/  # Unity settings
✅ Packages/         # Dependencies
✅ .github/          # Build workflows
✅ README.md         # Documentation
```

### Files to Ignore
```
❌ Library/          # Unity cache (regenerated)
❌ Temp/             # Temporary files
❌ Logs/             # Unity logs
❌ build/            # Build outputs
❌ UserSettings/     # Personal preferences
```

## 🚀 Build & Deployment

### Local Builds
1. **Use BuildScript** (`Assets/Editor/BuildScript.cs`)
2. **Manual build** via Build Settings
3. **Test builds** before deployment

### Cloud Builds
1. **Push to GitHub** to trigger automated builds
2. **Download artifacts** from GitHub Actions
3. **Use Unity Cloud Build** as alternative

## 🐛 Common Issues & Solutions

### Build Errors
- **Missing dependencies**: Install required Unity modules
- **Script compilation errors**: Check Console for errors
- **Platform-specific issues**: Verify platform settings

### Performance Issues
- **Mobile optimization**: Check sprite sizes and compression
- **Memory usage**: Monitor with Unity Profiler
- **Frame rate**: Test on target devices

### Unity Remote Issues
- **Install iTunes** on Windows for iOS testing
- **Use Android device** for more reliable testing
- **Check USB connection** and device trust

## 📚 Additional Resources

### Documentation
- `iOS_SETUP_GUIDE.md`: iOS development setup
- `SETUP_IOS_WINDOWS.md`: Quick iOS setup
- `Authentication_Setup_Guide.md`: Auth system setup

### External Resources
- [Unity Documentation](https://docs.unity3d.com/)
- [Unity Mobile Development](https://unity.com/solutions/mobile)
- [GitHub Actions for Unity](https://github.com/marketplace/actions/unity-builder)

## 🤝 Contributing Guidelines

### Code Style
- **Follow C# conventions** and Unity best practices
- **Add comments** for complex logic
- **Use meaningful variable names**
- **Keep methods focused** and single-purpose

### Testing
- **Test new features** in Unity Editor
- **Test on mobile devices** when possible
- **Verify save/load functionality**
- **Check for memory leaks**

### Documentation
- **Update README.md** for new features
- **Add inline comments** for complex systems
- **Document API changes** in scripts

---

**Happy coding! 🎮**

*This guide covers the essential information for developers to continue building on the SelfCity project.* 