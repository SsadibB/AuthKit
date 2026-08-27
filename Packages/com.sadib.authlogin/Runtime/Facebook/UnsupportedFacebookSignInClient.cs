using System;

namespace SadibTools.AuthLogin
{
    internal sealed class UnsupportedFacebookSignInClient : IFacebookSignInClient
    {
        private readonly string _providerId;

        public UnsupportedFacebookSignInClient(string providerId)
        {
            _providerId = providerId;
        }

        public bool IsSupported => false;

        public void RequestAccessToken(
            string appId,
            string clientToken,
            string[] permissions,
            Action<string> onSuccess,
            Action<AuthError> onFailure)
        {
            onFailure?.Invoke(AuthError.Unsupported(
                _providerId,
                "Facebook / Instagram login requires a signed Android device build. It is not available in the Unity Editor."));
        }

        public void SignOut()
        {
        }
    }
}
