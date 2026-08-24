# com.sadib.authlogin

Reusable PlayFab login module. Google Play Games Services is the first provider; built so
Apple/Facebook/etc can be added later as new `IAuthProvider` implementations without changing
the API your games call (`AuthManager.Instance.SignInWithGoogle()` stays the same).

## Recommended dev setup

Build and test this in its **own Unity project** (e.g. `AuthKit`), never inside a game project:

```
AuthKit/                          <- throwaway dev project, exists only to build/test this package
├── Packages/
│   └── com.sadib.authlogin/      <- THIS folder, as an embedded local package
│       ├── package.json
│       └── Runtime/
│           ├── IAuthProvider.cs
│           ├── AuthManager.cs
│           └── Providers/
│               └── GoogleAuthProvider.cs
└── Assets/
    └── Demo/                     <- throwaway test scene: a login button + "signed in as X" label
```

Unity auto-detects anything under `Packages/` with a `package.json` as a local package and gives
you real Package Manager integration (isolated compilation, versioning) while you iterate.

You still need PlayFab SDK + Play Games Plugin for Unity installed **in this dev project** too —
they're dependencies needed to compile/test, not bundled inside the package.

## Shipping it into your other projects

Once it works on your Realme 10 from the demo scene:

- **Git repo (recommended)** — push `Packages/com.sadib.authlogin` to its own repo. In Ludo,
  Sudoku, Area Forge, etc: Package Manager → "Add package from git URL" → point at the repo.
  Push a fix once, pull it into every project.
- **`.unitypackage` export** — simpler, no git, but no auto-updates; re-export/re-import per change.

Every consuming project still needs its own PlayFab SDK + Play Games Plugin installed (same as
any other project dependency) — the package can't silently pull those in for you.

## Why LoginWithGooglePlayGamesServices, not LoginWithGoogleAccount

PlayFab's older `LoginWithGoogleAccount` API needs a Google OAuth scope that recent versions of
the Play Games Plugin for Unity can no longer request — it now only works with a browser-based
deep-link workaround. `LoginWithGooglePlayGamesServices` is PlayFab's current recommended
replacement and needs no workaround, so that's what `GoogleAuthProvider` uses.

## One-time setup per game project

1. Import **PlayFab Unity SDK**.
2. Import **Play Games Plugin for Unity** (v2).
3. Google Play Console → set up the app under Play Games Services.
4. Google Cloud Console → create an OAuth 2.0 **Web application** client ID (not Android type).
5. PlayFab Game Manager → your title → Add-ons → **Google** → paste that client ID + secret.
6. Register your app's SHA-1 (debug **and** release keystore) against the Android OAuth client
   in Google Cloud Console — mismatched SHA-1 is the #1 cause of silent sign-in failures on
   release builds.

## Usage

```csharp
void Start()
{
    AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
    AuthManager.Instance.OnLoginFailure += HandleLoginFailure;
    AuthManager.Instance.OnProviderSignInFailed += HandleProviderFailure;
}

void OnGoogleButtonPressed()
{
    AuthManager.Instance.SignInWithGoogle(silent: false); // false = allow account picker UI
}

void HandleLoginSuccess(PlayFab.ClientModels.LoginResult result)
{
    Debug.Log($"Welcome, PlayFabId={result.PlayFabId}, new account={result.NewlyCreated}");
}
```

`AuthManager` Inspector toggles:
- `autoSignInSilentlyOnStart` — try a silent Google login on scene start; fails quietly if it can't.
- `createPlayFabAccountIfMissing` — auto-create a PlayFab account on first login.
- `fetchPlayerProfileOnLogin` — pull PlayFab player profile (display name, etc.) in the same call.

## Adding a new provider later (e.g. Apple)

1. Add `Providers/AppleAuthProvider.cs` implementing `IAuthProvider`.
2. In `AuthManager`, add a private `_apple` field and a `SignInWithApple(bool silent = false)`
   method that calls the private `SignIn(_apple, silent)` helper — same pattern as Google.
3. Nothing else changes. Projects only using `SignInWithGoogle()` are unaffected.

## Testing

Google Play Games sign-in generally does **not** work in the Unity Editor — test on a real
Android device (Realme 10) with a signed build.
