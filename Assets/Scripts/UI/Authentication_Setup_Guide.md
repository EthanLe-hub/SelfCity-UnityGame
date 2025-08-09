# Authentication & Subscription System Setup Guide

## Overview

This guide explains how to implement the comprehensive authentication and subscription system for your SelfCity Unity game. The system supports multiple sign-in methods and premium subscription features.

## System Architecture

### Core Components
1. **AuthenticationManager** - Handles user authentication and session management
2. **AuthenticationUI** - Provides sign-in interface and user profile display
3. **SubscriptionManager** - Manages premium subscriptions and feature access
4. **ProfileManager** - Enhanced to work with authentication system

### Supported Authentication Methods
- **Google Sign-In** (Android/Web)
- **Apple Sign-In** (iOS - Required)
- **Email/Password** (Fallback)
- **Guest Mode** (Quick start)

### Premium Features
- **Premium Decor Chest** - Exclusive decoration items
- **Premium Buildings** - Special building types
- **Premium Resources** - Resource bonuses
- **Premium Journal Features** - Enhanced journal capabilities

## Unity Setup

### 1. Package Dependencies

Add these packages to your Unity project:

```
com.unity.purchasing (Unity IAP)
com.unity.textmeshpro
```

### 2. Scene Setup

#### Create Authentication GameObject
1. Create an empty GameObject named `AuthenticationManager`
2. Add the `AuthenticationManager` script
3. Configure settings in Inspector:
   - Enable Google Sign-In: `true`
   - Enable Apple Sign-In: `true`
   - Enable Email Sign-In: `true`
   - Enable Guest Mode: `true`

#### Create Subscription GameObject
1. Create an empty GameObject named `SubscriptionManager`
2. Add the `SubscriptionManager` script
3. Configure subscription settings:
   - Monthly Price: `$4.99`
   - Yearly Price: `$39.99`
   - Enable Premium Features: `true`

#### Create Authentication UI
1. Create a Canvas GameObject named `AuthenticationUI`
2. Add the `AuthenticationUI` script
3. Create UI panels:
   - `SignInPanel` - Contains sign-in buttons
   - `ProfilePanel` - Shows user profile and subscription status
   - `LoadingPanel` - Loading indicator
   - `EmailSignInForm` - Email/password form

### 3. UI Layout Structure

```
Canvas
├── AuthenticationUI
│   ├── SignInPanel
│   │   ├── GoogleSignInButton
│   │   ├── AppleSignInButton
│   │   ├── EmailSignInButton
│   │   └── GuestSignInButton
│   ├── ProfilePanel
│   │   ├── UserDisplayNameText
│   │   ├── UserEmailText
│   │   ├── SubscriptionStatusText
│   │   ├── PremiumBadge
│   │   ├── UpgradeToPremiumButton
│   │   └── SignOutButton
│   ├── LoadingPanel
│   │   └── LoadingSpinner
│   └── EmailSignInForm
│       ├── EmailInput
│       ├── PasswordInput
│       ├── SubmitButton
│       └── BackButton
```

## Platform-Specific Setup

### Android Setup (Google Sign-In)

#### 1. Google Play Console Setup
1. Create a Google Play Console account
2. Create a new app
3. Enable Google Play Games Services
4. Configure OAuth 2.0 credentials

#### 2. Unity Configuration
1. Set platform to Android
2. Configure Player Settings:
   - Package Name: `com.yourcompany.selfcity`
   - Minimum API Level: `API 21`
   - Target API Level: `API 33`

#### 3. Google Sign-In Integration
```csharp
// In AuthenticationManager.cs, replace the simulation with:
#if UNITY_ANDROID
private void InitializeGoogleSignIn()
{
    // Initialize Google Sign-In SDK
    GoogleSignIn.Configuration = new GoogleSignInConfiguration
    {
        WebClientId = "your-web-client-id.apps.googleusercontent.com",
        RequestIdToken = true
    };
}

public async Task<AuthResult> SignInWithGoogle()
{
    try
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = "your-web-client-id.apps.googleusercontent.com",
            RequestIdToken = true
        };

        var signIn = GoogleSignIn.DefaultInstance.SignIn();
        var result = await signIn;
        
        if (result.Status == GoogleSignIn.SignInStatus.Success)
        {
            var user = new AuthUser
            {
                userId = result.UserId,
                displayName = result.DisplayName,
                email = result.Email,
                photoUrl = result.ImageUrl?.AbsoluteUri,
                provider = AuthProvider.Google,
                lastSignInTime = DateTime.Now,
                isEmailVerified = true
            };

            return await CompleteSignIn(user);
        }
        else
        {
            return new AuthResult { Success = false, Error = AuthError.Cancelled };
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"Google Sign-In failed: {e.Message}");
        return new AuthResult { Success = false, Error = AuthError.NetworkError };
    }
}
#endif
```

### iOS Setup (Apple Sign-In)

#### 1. Apple Developer Account Setup
1. Create Apple Developer account
2. Configure App ID with Sign In with Apple capability
3. Create provisioning profile

#### 2. Unity Configuration
1. Set platform to iOS
2. Configure Player Settings:
   - Bundle Identifier: `com.yourcompany.selfcity`
   - Target minimum iOS Version: `13.0`

#### 3. Apple Sign-In Integration
```csharp
// In AuthenticationManager.cs, replace the simulation with:
#if UNITY_IOS
private void InitializeAppleSignIn()
{
    // Initialize Apple Sign-In SDK
    // Note: Requires native iOS plugin
}

public async Task<AuthResult> SignInWithApple()
{
    try
    {
        // Implement Apple Sign-In using native iOS plugin
        // This requires a custom native plugin or third-party SDK
        
        var user = new AuthUser
        {
            userId = "apple_user_id",
            displayName = "Apple User",
            email = "user@icloud.com",
            provider = AuthProvider.Apple,
            lastSignInTime = DateTime.Now,
            isEmailVerified = true
        };

        return await CompleteSignIn(user);
    }
    catch (Exception e)
    {
        Debug.LogError($"Apple Sign-In failed: {e.Message}");
        return new AuthResult { Success = false, Error = AuthError.NetworkError };
    }
}
#endif
```

## Unity IAP Integration

### 1. Configure Unity IAP
1. Open Window > Package Manager
2. Install Unity IAP package
3. Configure IAP Catalog:
   - Product ID: `premium_monthly`
   - Product Type: `Subscription`
   - Price: `$4.99`
   - Product ID: `premium_yearly`
   - Product Type: `Subscription`
   - Price: `$39.99`

### 2. IAP Integration Code
```csharp
// In SubscriptionManager.cs, replace simulation with:
using UnityEngine.Purchasing;

public class SubscriptionManager : MonoBehaviour, IStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    private void Start()
    {
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(monthlySubscriptionId, ProductType.Subscription);
        builder.AddProduct(yearlySubscriptionId, ProductType.Subscription);
        
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
    }

    public async Task<bool> PurchaseMonthlySubscription()
    {
        if (storeController != null)
        {
            storeController.InitiatePurchase(monthlySubscriptionId);
            return true;
        }
        return false;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == monthlySubscriptionId)
        {
            // Handle monthly subscription purchase
            hasActiveSubscription = true;
            currentSubscriptionType = SubscriptionType.Monthly;
            subscriptionExpiryDate = DateTime.Now.AddMonths(1);
            SaveSubscriptionData();
            OnSubscriptionStatusChanged?.Invoke(true);
        }
        else if (args.purchasedProduct.definition.id == yearlySubscriptionId)
        {
            // Handle yearly subscription purchase
            hasActiveSubscription = true;
            currentSubscriptionType = SubscriptionType.Yearly;
            subscriptionExpiryDate = DateTime.Now.AddYears(1);
            SaveSubscriptionData();
            OnSubscriptionStatusChanged?.Invoke(true);
        }

        return PurchaseProcessingResult.Complete;
    }
}
```

## Integration with Existing Systems

### 1. ProfileManager Integration
The `ProfileManager` automatically syncs with the authentication system:

```csharp
// In AuthenticationUI.cs
private void OnUserSignedIn(AuthenticationManager.AuthUser user)
{
    ShowProfilePanel();
    UpdateProfileDisplay();
    
    // Sync with ProfileManager
    if (profileManager != null)
    {
        var profileData = profileManager.GetProfileData();
        profileData.username = user.displayName;
        profileData.email = user.email;
        // ProfileManager will handle saving this data
    }
}
```

### 2. Shop System Integration
Premium features are automatically unlocked based on subscription status:

```csharp
// In DecorChestManager.cs
public bool CanOpenPremiumChest()
{
    return SubscriptionManager.Instance.HasPremiumDecorChestAccess();
}

// In BuildingShopDatabase.cs
public List<BuildingShopItem> GetAvailableBuildings()
{
    var buildings = new List<BuildingShopItem>();
    
    // Add free buildings
    buildings.AddRange(freeBuildings);
    
    // Add premium buildings if user has subscription
    if (SubscriptionManager.Instance.HasPremiumBuildingsAccess())
    {
        buildings.AddRange(premiumBuildings);
    }
    
    return buildings;
}
```

### 3. Resource System Integration
Premium users get resource bonuses:

```csharp
// In ResourceManager.cs
public void AddResource(ResourceType type, int amount)
{
    float bonus = SubscriptionManager.Instance.GetPremiumResourceBonus();
    int finalAmount = Mathf.RoundToInt(amount * bonus);
    
    // Add the bonus amount
    // ... existing resource addition logic
}
```

## Testing

### 1. Test Authentication Flow
1. Test each sign-in method
2. Verify session persistence
3. Test sign-out functionality
4. Verify profile data sync

### 2. Test Subscription Features
1. Test premium feature access
2. Verify subscription expiration
3. Test subscription renewal
4. Verify premium content availability

### 3. Test Integration Points
1. Verify shop integration
2. Test resource bonuses
3. Verify journal premium features
4. Test building unlocks

## Production Deployment

### 1. Security Considerations
- Implement server-side validation
- Use secure token storage
- Validate subscription status server-side
- Implement anti-fraud measures

### 2. Analytics Integration
- Track authentication methods used
- Monitor subscription conversion rates
- Track premium feature usage
- Monitor user retention

### 3. Support Systems
- Implement account recovery
- Add subscription management UI
- Provide customer support tools
- Implement refund handling

## Troubleshooting

### Common Issues
1. **Google Sign-In not working**: Check OAuth configuration
2. **Apple Sign-In failing**: Verify App ID configuration
3. **IAP not working**: Check store configuration
4. **Session not persisting**: Verify PlayerPrefs permissions

### Debug Tools
- Enable debug logging in AuthenticationManager
- Use Unity IAP test mode
- Monitor console for error messages
- Test with different user accounts

## Next Steps

1. **Implement server-side validation**
2. **Add analytics tracking**
3. **Implement account linking**
4. **Add social features**
5. **Implement cloud save**
6. **Add cross-platform sync**

This authentication and subscription system provides a solid foundation for monetizing your game while providing a seamless user experience across multiple platforms. 