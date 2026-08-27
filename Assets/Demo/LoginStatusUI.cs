using TMPro;
using UnityEngine;
using SadibTools.AuthLogin;

namespace AuthKit.Demo
{
    /// <summary>
    /// Shows "Google is connected" / Facebook / Instagram after a successful login,
    /// or the error text if login fails. Drop on the scene Canvas.
    /// </summary>
    public class LoginStatusUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;

        private void Awake()
        {
            if (statusText == null)
                statusText = CreateStatusText();
            SetStatus("Not signed in");
        }

        private void OnEnable()
        {
            var auth = AuthManager.Instance != null ? AuthManager.Instance : AuthManager.EnsureInstance();
            auth.OnLoginStarted += HandleStarted;
            auth.OnLoginSuccess += HandleSuccess;
            auth.OnLoginFailure += HandleFailure;
        }

        private void OnDisable()
        {
            if (AuthManager.Instance == null)
                return;
            AuthManager.Instance.OnLoginStarted -= HandleStarted;
            AuthManager.Instance.OnLoginSuccess -= HandleSuccess;
            AuthManager.Instance.OnLoginFailure -= HandleFailure;
        }

        private void HandleStarted(string providerId)
        {
            SetStatus("Connecting " + DisplayName(providerId) + "...");
        }

        private void HandleSuccess(AuthSession session)
        {
            string provider = DisplayName(session != null ? session.ProviderId : null);
            string extra = string.Empty;
            if (session != null && !string.IsNullOrEmpty(session.DisplayName))
                extra = "\n" + session.DisplayName;
            else if (session != null && !string.IsNullOrEmpty(session.Email))
                extra = "\n" + session.Email;
            SetStatus(provider + " is connected" + extra);
        }

        private void HandleFailure(AuthError error)
        {
            if (error == null)
            {
                SetStatus("Login failed");
                return;
            }

            SetStatus(DisplayName(error.ProviderId) + " failed\n" + error.Message);
        }

        private void SetStatus(string text)
        {
            if (statusText != null)
                statusText.text = text;
        }

        private static string DisplayName(string providerId)
        {
            switch (providerId)
            {
                case GoogleAuthProvider.Id:
                    return "Google";
                case FacebookAuthProvider.FacebookId:
                    return "Facebook";
                case FacebookAuthProvider.InstagramId:
                    return "Instagram";
                default:
                    return string.IsNullOrEmpty(providerId) ? "Account" : providerId;
            }
        }

        private TextMeshProUGUI CreateStatusText()
        {
            var canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;

            var go = new GameObject("Status", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, 0.08f);
            rt.anchorMax = new Vector2(0.92f, 0.28f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 42;
            text.color = Color.black;
            text.enableWordWrapping = true;
            if (TMP_Settings.defaultFontAsset != null)
                text.font = TMP_Settings.defaultFontAsset;
            return text;
        }
    }
}
