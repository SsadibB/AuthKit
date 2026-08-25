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

        public string GoogleWebClientId => googleWebClientId == null ? string.Empty : googleWebClientId.Trim();

        public bool HasGoogleWebClientId => !string.IsNullOrEmpty(GoogleWebClientId);

        public static AuthSettings LoadFromResources()
        {
            return Resources.Load<AuthSettings>(ResourcesName);
        }
    }
}
