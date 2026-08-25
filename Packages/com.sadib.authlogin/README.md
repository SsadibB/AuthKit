# com.sadib.authlogin

Reusable PlayFab login module. **Google Account** is the first provider (`LoginWithGoogleAccount`).
Apple/Facebook/etc can be added later as new `IAuthProvider` implementations without changing
the API your games call (`AuthManager.Instance.SignInWithGoogle()` stays the same).

This is **not** Google Play Games Services. The native Android plugin uses Google's
Identity **Authorization API** (`requestOfflineAccess`) to get an OAuth server auth code,
then PlayFab exchanges that code using the **Web application** client ID + secret stored
in Game Manager.

## Recommended dev setup

Build and test this in its **own Unity project** (e.g. `AuthKit`):

```
AuthKit/
├── Packages/
│   └── com.sadib.authlogin/     <- this package (embedded)
└── Assets/
    └── Demo/                    <- throwaway test scene
```

Consuming projects still need the **PlayFab Unity SDK** installed. This package does not bundle PlayFab.

Google Account login does **not** work in the Unity Editor. Test on a signed Android device build.

## Usage

1. Create **Auth Login → Auth Settings** and paste your Google **Web application** client ID.
2. Add `AuthManager` to a persistent GameObject and assign that asset.
3. Call sign-in from a button:

```csharp
void Start()
{
    AuthManager.Instance.OnLoginSuccess += session =>
        Debug.Log($"Welcome PlayFabId={session.PlayFabId} new={session.NewlyCreated}");
    AuthManager.Instance.OnLoginFailure += error =>
        Debug.LogError(error.ToString());
}

void OnGoogleButtonPressed()
{
    AuthManager.Instance.SignInWithGoogle(silent: false);
}
```

`AuthManager` Inspector toggles:

- `autoSignInSilentlyOnStart` — try a silent Google login on scene start; fails quietly if it cannot.
- `createPlayFabAccountIfMissing` — auto-create a PlayFab account on first login.
- `fetchPlayerProfileOnLogin` — pull PlayFab account/profile info in the same call.

## One-time setup per game project

### PlayFab

1. Import the PlayFab Unity SDK and set **Title ID** (this harness uses `10B581`).
2. Game Manager → Add-ons → **Google**.
3. Paste the OAuth **Web application** client ID **and** client secret. The secret never goes in the Unity client.

### Google Cloud Console

1. Create an OAuth **Web application** client. This ID goes in `AuthSettings` and in the PlayFab Google add-on.
2. Create an OAuth **Android** client:
   - Package name = Unity Android application id (`com.sadib.authkit` in this harness)
   - SHA-1 of the **debug** keystore and the **release** keystore (Play App Signing SHA-1 if you use it)
3. If the OAuth consent screen is in Testing, add every test Gmail as a tester.

Mismatched SHA-1, or pasting the Android client ID where the Web client ID is required, is the usual production failure.

### Android

- Application id must match the Android OAuth client.
- Internet permission is required (this package's Android library declares it).
- After import, let External Dependency Manager resolve `play-services-auth` when prompted.

## Shipping into other projects

- **Git URL (recommended)** — push `Packages/com.sadib.authlogin` to its own repo and add it from Package Manager.
- **`.unitypackage` export** — simpler, no auto-updates.

Every consuming project still needs its own PlayFab SDK plus Google Cloud / PlayFab Google add-on configuration.

## Adding a new provider later (e.g. Apple)

1. Add `Providers/AppleAuthProvider.cs` implementing `IAuthProvider`.
2. In `AuthManager`, add a private `_apple` field and `SignInWithApple(bool silent = false)` that calls the same `SignIn` helper.
3. Nothing else changes. Projects only using `SignInWithGoogle()` are unaffected.
