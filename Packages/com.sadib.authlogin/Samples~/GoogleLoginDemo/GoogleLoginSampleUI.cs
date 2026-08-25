using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SadibTools.AuthLogin;

namespace SadibTools.AuthLogin.Samples
{
    /// <summary>
    /// Minimal on-screen Google login controls. Builds a Canvas at runtime so the sample
    /// works without a pre-wired scene.
    /// </summary>
    public class GoogleLoginSampleUI : MonoBehaviour
    {
        [SerializeField] private AuthSettings settings;
        [SerializeField] private bool autoSignInSilentlyOnStart;

        private Text _status;
        private Button _signIn;
        private Button _signOut;

        private void Awake()
        {
            BuildUiIfNeeded();
            var auth = AuthManager.EnsureInstance();
            if (settings != null)
                auth.Configure(settings, autoSignInSilentlyOnStart);
        }

        private void OnEnable()
        {
            var auth = AuthManager.Instance;
            if (auth == null)
                return;
            auth.OnLoginSuccess += HandleSuccess;
            auth.OnLoginFailure += HandleFailure;
        }

        private void Start()
        {
            RefreshButtons();
            if (AuthManager.Instance != null && AuthManager.Instance.IsSignedIn)
                HandleSuccess(AuthManager.Instance.CurrentSession);
            else
                SetStatus("Not signed in.");
        }

        private void OnDisable()
        {
            if (AuthManager.Instance == null)
                return;
            AuthManager.Instance.OnLoginSuccess -= HandleSuccess;
            AuthManager.Instance.OnLoginFailure -= HandleFailure;
        }

        private void HandleSuccess(AuthSession session)
        {
            string name = string.IsNullOrEmpty(session.DisplayName) ? "(no name)" : session.DisplayName;
            string email = string.IsNullOrEmpty(session.Email) ? "(no email)" : session.Email;
            SetStatus($"Signed in\nPlayFabId: {session.PlayFabId}\n{name}\n{email}\nNew account: {session.NewlyCreated}");
            RefreshButtons();
        }

        private void HandleFailure(AuthError error)
        {
            SetStatus(error.ToString());
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            bool signedIn = AuthManager.Instance != null && AuthManager.Instance.IsSignedIn;
            if (_signIn != null)
                _signIn.interactable = !signedIn;
            if (_signOut != null)
                _signOut.interactable = signedIn;
        }

        private void SetStatus(string text)
        {
            if (_status != null)
                _status.text = text;
        }

        private void BuildUiIfNeeded()
        {
            if (_status != null)
                return;

            var canvasGo = new GameObject("GoogleLoginCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.transform.SetParent(transform, false);

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }

            _status = CreateText(canvasGo.transform, "Status", new Vector2(0, 220), new Vector2(900, 360));
            _status.alignment = TextAnchor.MiddleCenter;
            _status.fontSize = 36;

            _signIn = CreateButton(canvasGo.transform, "Sign in with Google", new Vector2(0, -80), () =>
            {
                AuthManager.Instance.SignInWithGoogle(silent: false);
            });
            _signOut = CreateButton(canvasGo.transform, "Sign out", new Vector2(0, -220), () =>
            {
                AuthManager.Instance.SignOut();
                SetStatus("Signed out.");
                RefreshButtons();
            });
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(640, 100);
            rt.anchoredPosition = anchoredPos;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.18f, 0.45f, 0.85f, 1f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var text = CreateText(go.transform, "Label", Vector2.zero, new Vector2(640, 100));
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 34;
            return button;
        }
    }
}
