using System;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using SadibTools.AuthLogin.Providers;

namespace SadibTools.AuthLogin
{
    /// <summary>
    /// The single entry point your game code talks to. Drop on a persistent GameObject
    /// (or let it self-instantiate) in each project's login flow.
    ///
    ///   AuthManager.Instance.SignInWithGoogle();
    ///
    /// When Apple/Facebook/etc are added, they get their own SignInWithX() method here
    /// that goes through the same OnLoginSuccess/OnLoginFailure events — existing calling
    /// code in projects using this package doesn't need to change.
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        [Header("Google provider settings")]
        [Tooltip("On Start, try a silent Google sign-in (no UI) so returning players skip the login screen.")]
        [SerializeField] private bool autoSignInSilentlyOnStart = true;
        [SerializeField] private bool createPlayFabAccountIfMissing = true;
        [SerializeField] private bool fetchPlayerProfileOnLogin = true;

        /// <summary>Fired after a successful PlayFab login, regardless of which provider was used.</summary>
        public event Action<LoginResult> OnLoginSuccess;

        /// <summary>Fired if the provider credential was fine but PlayFab rejected the login.</summary>
        public event Action<PlayFabError> OnLoginFailure;

        /// <summary>Fired if the provider's own sign-in (e.g. Google account picker) failed or was cancelled.</summary>
        public event Action<string> OnProviderSignInFailed; // arg = provider id

        public bool IsSignedIn { get; private set; }
        public string LastPlayFabId { get; private set; }
        public string LastProviderId { get; private set; }

        private GoogleAuthProvider _google;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _google = new GoogleAuthProvider(createPlayFabAccountIfMissing, fetchPlayerProfileOnLogin);
        }

        private void Start()
        {
            if (autoSignInSilentlyOnStart)
                SignInWithGoogle(silent: true);
        }

        /// <summary>Call from a "Sign in with Google" button with silent:false to show the account picker.</summary>
        public void SignInWithGoogle(bool silent = false)
        {
            SignIn(_google, silent);
        }

        // Future: public void SignInWithApple(bool silent = false) => SignIn(_apple, silent);

        private void SignIn(IAuthProvider provider, bool silent)
        {
            if (provider.IsSignedIn) return;

            provider.SignIn(
                silent,
                onSuccess: result =>
                {
                    IsSignedIn = true;
                    LastPlayFabId = result.PlayFabId;
                    LastProviderId = provider.ProviderId;
                    Debug.Log($"[AuthManager] Login OK via {provider.ProviderId}. PlayFabId={result.PlayFabId} NewAccount={result.NewlyCreated}");
                    OnLoginSuccess?.Invoke(result);
                },
                onPlayFabFailure: error =>
                {
                    IsSignedIn = false;
                    Debug.LogError($"[AuthManager] PlayFab login failed via {provider.ProviderId}: {error.GenerateErrorReport()}");
                    OnLoginFailure?.Invoke(error);
                },
                onProviderFailure: () =>
                {
                    IsSignedIn = false;
                    OnProviderSignInFailed?.Invoke(provider.ProviderId);
                });
        }

        public void SignOut()
        {
            _google.SignOut();
            IsSignedIn = false;
            LastPlayFabId = null;
            LastProviderId = null;
        }
    }
}
