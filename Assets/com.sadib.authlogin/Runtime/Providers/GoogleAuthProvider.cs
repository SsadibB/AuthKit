using System;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab;
using PlayFab.ClientModels;

namespace SadibTools.AuthLogin.Providers
{
    /// <summary>
    /// Google Play Games Services -> PlayFab, via LoginWithGooglePlayGamesServices
    /// (the current, non-deprecated PlayFab Google path — see README for why).
    /// </summary>
    public class GoogleAuthProvider : IAuthProvider
    {
        public string ProviderId => "google";
        public bool IsSignedIn { get; private set; }

        private readonly bool _createPlayFabAccountIfMissing;
        private readonly bool _fetchPlayerProfileOnLogin;
        private bool _activated;

        public GoogleAuthProvider(bool createPlayFabAccountIfMissing = true, bool fetchPlayerProfileOnLogin = true)
        {
            _createPlayFabAccountIfMissing = createPlayFabAccountIfMissing;
            _fetchPlayerProfileOnLogin = fetchPlayerProfileOnLogin;
        }

        private void EnsureActivated()
        {
            if (_activated) return;
            PlayGamesPlatform.Activate();
            _activated = true;
        }

        public void SignIn(bool silent, Action<LoginResult> onSuccess, Action<PlayFabError> onPlayFabFailure, Action onProviderFailure)
        {
            EnsureActivated();

            var interactivity = silent ? SignInInteractivity.CanBeSilent : SignInInteractivity.CanPromptOnce;

            PlayGamesPlatform.Instance.Authenticate(interactivity, status =>
            {
                if (status != SignInStatus.Success)
                {
                    if (!silent)
                        Debug.LogWarning($"[GoogleAuthProvider] Google sign-in failed: {status}");
                    onProviderFailure?.Invoke();
                    return;
                }

                PlayGamesPlatform.Instance.RequestServerSideAccess(false, authCode =>
                {
                    if (string.IsNullOrEmpty(authCode))
                    {
                        Debug.LogError("[GoogleAuthProvider] Empty server auth code from Google Play Games.");
                        onProviderFailure?.Invoke();
                        return;
                    }

                    var request = new LoginWithGooglePlayGamesServicesRequest
                    {
                        ServerAuthCode = authCode,
                        CreateAccount = _createPlayFabAccountIfMissing,
                        InfoRequestParameters = _fetchPlayerProfileOnLogin
                            ? new GetPlayerCombinedInfoRequestParams { GetPlayerProfile = true }
                            : null
                    };

                    PlayFabClientAPI.LoginWithGooglePlayGamesServices(
                        request,
                        result =>
                        {
                            IsSignedIn = true;
                            onSuccess?.Invoke(result);
                        },
                        error =>
                        {
                            IsSignedIn = false;
                            onPlayFabFailure?.Invoke(error);
                        });
                });
            });
        }

        public void SignOut()
        {
            if (_activated)
                PlayGamesPlatform.Instance.SignOut();
            IsSignedIn = false;
        }
    }
}
