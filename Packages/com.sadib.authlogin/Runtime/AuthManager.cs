using System;
using UnityEngine;

namespace SadibTools.AuthLogin
{
    /// <summary>
    /// The single entry point your game code talks to. Drop on a persistent GameObject
    /// (or let it self-instantiate) in each project's login flow.
    ///
    ///   AuthManager.Instance.SignInWithGoogle();
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private AuthSettings settings;
        [Tooltip("On Start, try a silent Google sign-in (no UI) so returning players skip the login screen.")]
        [SerializeField] private bool autoSignInSilentlyOnStart = true;
        [SerializeField] private bool createPlayFabAccountIfMissing = true;
        [SerializeField] private bool fetchPlayerProfileOnLogin = true;

        public event Action<AuthSession> OnLoginSuccess;
        public event Action<AuthError> OnLoginFailure;

        public bool IsSignedIn { get; private set; }
        public bool IsBusy { get; private set; }
        public AuthSession CurrentSession { get; private set; }
        public string LastPlayFabId => CurrentSession?.PlayFabId;
        public string LastProviderId => CurrentSession?.ProviderId;

        public AuthSettings Settings => settings;

        private GoogleAuthProvider _google;

        public static AuthManager EnsureInstance()
        {
            if (Instance != null)
                return Instance;

            var existing = FindObjectOfType<AuthManager>();
            if (existing != null)
                return existing;

            var go = new GameObject("AuthManager");
            return go.AddComponent<AuthManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            AuthMainThread.Ensure(gameObject);

            if (settings == null)
                settings = AuthSettings.LoadFromResources();

            RebuildProvider();
        }

        /// <summary>Assign AuthSettings at runtime before the first sign-in (e.g. from a sample scene).</summary>
        public void Configure(AuthSettings authSettings, bool autoSilentOnStart)
        {
            settings = authSettings;
            autoSignInSilentlyOnStart = autoSilentOnStart;
            RebuildProvider();
        }

        private void RebuildProvider()
        {
            _google = new GoogleAuthProvider(settings, createPlayFabAccountIfMissing, fetchPlayerProfileOnLogin);
        }

        private void Start()
        {
            if (autoSignInSilentlyOnStart)
                SignInWithGoogle(silent: true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Call from a "Sign in with Google" button with silent:false to show the account picker.</summary>
        public void SignInWithGoogle(bool silent = false)
        {
            SignIn(_google, silent);
        }

        public void SignOut()
        {
            _google?.SignOut();
            IsSignedIn = false;
            IsBusy = false;
            CurrentSession = null;
        }

        private void SignIn(IAuthProvider provider, bool silent)
        {
            if (provider == null)
            {
                OnLoginFailure?.Invoke(AuthError.Configuration("none", "Auth provider is not initialized."));
                return;
            }

            if (IsBusy)
            {
                if (!silent)
                    OnLoginFailure?.Invoke(AuthError.InProgress(provider.ProviderId));
                return;
            }

            if (provider.IsSignedIn && CurrentSession != null)
            {
                OnLoginSuccess?.Invoke(CurrentSession);
                return;
            }

            IsBusy = true;
            provider.SignIn(
                silent,
                onSuccess: result =>
                {
                    IsBusy = false;
                    IsSignedIn = true;
                    CurrentSession = AuthSession.FromLogin(provider.ProviderId, result);
                    Debug.Log($"[AuthManager] Login OK via {provider.ProviderId}. PlayFabId={result.PlayFabId} NewAccount={result.NewlyCreated}");
                    OnLoginSuccess?.Invoke(CurrentSession);
                },
                onFailure: error =>
                {
                    IsBusy = false;
                    IsSignedIn = false;
                    bool hideSilentCancel = silent && error.Code == AuthErrorCode.Cancelled;
                    if (!hideSilentCancel)
                    {
                        if (error.Code != AuthErrorCode.Cancelled && error.Code != AuthErrorCode.UnsupportedPlatform)
                            Debug.LogError($"[AuthManager] {error}");
                        OnLoginFailure?.Invoke(error);
                    }
                });
        }
    }
}
