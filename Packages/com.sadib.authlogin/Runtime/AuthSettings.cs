using UnityEngine;

namespace SadibTools.AuthLogin
{
    [CreateAssetMenu(fileName = "AuthSettings", menuName = "Auth Login/Auth Settings")]
    public class AuthSettings : ScriptableObject
    {
        private const string ResourcesName = "AuthSettings";

        [SerializeField]
        [Tooltip("OAuth 2.0 Web application client ID from Google Cloud Console. Must match the client configured in PlayFab Game Manager → Add-ons → Google. Never put the client secret here.")]
        private string googleWebClientId = string.Empty;

        [SerializeField]
        [Tooltip("Meta / Facebook App ID from developers.facebook.com. Also required for Instagram (Meta uses Facebook Login). Enable the PlayFab Facebook add-on with the same app.")]
        private string facebookAppId = string.Empty;

        [SerializeField]
        [Tooltip("Facebook Client Token from Meta App Settings → Advanced. This is not the App Secret.")]
        private string facebookClientToken = string.Empty;

        public string GoogleWebClientId => googleWebClientId == null ? string.Empty : googleWebClientId.Trim();

        public bool HasGoogleWebClientId => !string.IsNullOrEmpty(GoogleWebClientId);

        public string FacebookAppId => facebookAppId == null ? string.Empty : facebookAppId.Trim();

        public string FacebookClientToken => facebookClientToken == null ? string.Empty : facebookClientToken.Trim();

        public bool HasFacebookAppId => !string.IsNullOrEmpty(FacebookAppId) && !string.IsNullOrEmpty(FacebookClientToken);

        public static AuthSettings LoadFromResources()
        {
            return Resources.Load<AuthSettings>(ResourcesName);
        }
    }
}
