# 🍎 iOS Setup Guide for Windows Users

## 📋 **What You CAN Do on Windows (Right Now)**

### **1. Configure Unity Project Settings**

#### **Player Settings**
1. **Open Unity** → **Edit** → **Project Settings** → **Player**
2. **Set Company Name**: `Sparq Capital` (already set)
3. **Set Product Name**: `SelfCity` (already set)
4. **Set Bundle Identifier**: `com.sparqcapital.selfcity`

#### **iOS-Specific Settings**
1. **Target Device**: iPhone + iPad
2. **Target minimum iOS Version**: 13.0 (recommended)
3. **Architecture**: ARM64
4. **Scripting Backend**: IL2CPP
5. **API Compatibility Level**: .NET Standard 2.1

#### **Orientation Settings**
- ✅ **Portrait**: Allowed
- ✅ **Portrait Upside Down**: Allowed  
- ✅ **Landscape Left**: Allowed
- ✅ **Landscape Right**: Allowed

#### **Graphics Settings**
- **Color Space**: Linear (already set)
- **Auto Graphics API**: Enabled
- **Metal API**: Enabled

### **2. Configure App Icons**
1. **Create Icons**: 1024x1024, 180x180, 120x120, 87x87, 80x80, 76x76, 60x60, 40x40, 29x29
2. **Place in**: `Assets/Plugins/iOS/`
3. **Set in Player Settings** → **iOS** → **App Icons**

### **3. Configure Splash Screen**
1. **Create Splash Image**: 2048x2048 (square)
2. **Set in Player Settings** → **Splash Screen**

### **4. Configure Info.plist**
Create `Assets/Plugins/iOS/Info.plist`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDisplayName</key>
    <string>SelfCity</string>
    <key>CFBundleIdentifier</key>
    <string>com.sparqcapital.selfcity</string>
    <key>CFBundleVersion</key>
    <string>1.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>UIRequiredDeviceCapabilities</key>
    <array>
        <string>armv7</string>
    </array>
    <key>UISupportedInterfaceOrientations</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationPortraitUpsideDown</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
    <key>UISupportedInterfaceOrientations~ipad</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationPortraitUpsideDown</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
</dict>
</plist>
```

## 🚫 **What You CANNOT Do on Windows**

### **1. Build iOS App (.ipa file)**
- ❌ Requires Xcode (macOS only)
- ❌ Requires iOS SDK (macOS only)

### **2. Code Signing**
- ❌ Requires Apple Developer Account ($99/year)
- ❌ Requires certificates (macOS only)

### **3. App Store Distribution**
- ❌ Requires Apple Developer Account
- ❌ Requires App Store Connect setup

## 🆓 **Free Alternatives for Windows Users**

### **1. Unity Cloud Build (Recommended)**
- **Cost**: Free tier available
- **What it does**: Builds iOS for you in the cloud
- **Setup**: 
  1. Connect your Unity project to GitHub
  2. Set up Unity Cloud Build
  3. Configure iOS build target
  4. Push code to trigger builds

### **2. GitHub Actions**
- **Cost**: Free (limited hours)
- **What it does**: Automated builds using macOS runners
- **Setup**: Create `.github/workflows/ios-build.yml`

### **3. Unity Remote (Testing Only)**
- **Cost**: Free
- **What it does**: Test on iOS device via USB
- **Setup**: Install Unity Remote app on iOS device

### **4. Virtual Machine (Not Recommended)**
- **Cost**: Free (but slow)
- **What it does**: Run macOS in VM
- **Limitation**: Against Apple's terms of service

## 🎯 **Recommended Workflow**

### **Phase 1: Windows Setup (Now)**
1. ✅ Configure Unity project settings
2. ✅ Create app icons and splash screen
3. ✅ Set up Info.plist
4. ✅ Test on Unity Remote (if you have iOS device)

### **Phase 2: Cloud Build (When Ready)**
1. 🔄 Set up Unity Cloud Build
2. 🔄 Connect to GitHub repository
3. 🔄 Configure iOS build target
4. 🔄 Test builds

### **Phase 3: Distribution (When Ready)**
1. 🔄 Get Apple Developer Account ($99/year)
2. 🔄 Set up App Store Connect
3. 🔄 Submit for review

## 📱 **Testing Options**

### **1. Unity Remote (Free)**
- Install Unity Remote app on iOS device
- Connect via USB
- Test gameplay (not performance)

### **2. Unity Cloud Build (Free tier)**
- Build and download .ipa file
- Install via TestFlight (requires Apple Developer Account)

### **3. Simulator Testing**
- Requires Mac access
- Test on iOS Simulator

## 💡 **Tips for Success**

### **1. Optimize for Mobile**
- Use mobile-optimized textures
- Implement touch controls
- Test performance on lower-end devices

### **2. Follow Apple Guidelines**
- Design for iOS Human Interface Guidelines
- Implement proper app lifecycle
- Handle memory efficiently

### **3. Plan for Distribution**
- Create App Store screenshots
- Write compelling app description
- Plan marketing strategy

## 🔗 **Useful Resources**

- [Unity iOS Build Guide](https://docs.unity3d.com/Manual/iphone-gettingstarted.html)
- [Apple Developer Documentation](https://developer.apple.com/documentation/)
- [Unity Cloud Build](https://unity.com/products/cloud-build)
- [GitHub Actions for Unity](https://github.com/marketplace/actions/unity-builder)

---

**Remember**: You can do 90% of the iOS setup on Windows. The actual building and distribution requires Apple's tools, but you can prepare everything else in advance! 