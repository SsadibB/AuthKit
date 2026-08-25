using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SadibTools.AuthLogin;

namespace AuthKit.Demo
{
    /// <summary>
    /// AuthKit harness UI. Builds a Canvas at runtime so the demo scene only needs this script.
    /// </summary>
    public class GoogleLoginUI : MonoBehaviour
    {
        [SerializeField] private AuthSettings settings;

        private Text _status;
        private Button _signIn;
        private Button _signOut;

        private void Awake()
        {
            BuildUi();
            var auth = AuthManager.EnsureInstance();
            if (settings != null)
                auth.Configure(settings, autoSilentOnStart: false);
        }

        private void OnEnable()
        {
            if (AuthManager.Instance == null)
                return;
            AuthManager.Instance.OnLoginSuccess += HandleSuccess;
            AuthManager.Instance.OnLoginFailure += HandleFailure;
        }

        private void Start()
        {
            RefreshButtons();
            SetStatus("Not signed in.\nAndroid device build required for Google Account login.");
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

        private void BuildUi()
        {
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
                DontDestroyOnLoad(es);
            }

            _status = CreateText(canvasGo.transform, "Status", new Vector2(0, 240), new Vector2(920, 400));
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
