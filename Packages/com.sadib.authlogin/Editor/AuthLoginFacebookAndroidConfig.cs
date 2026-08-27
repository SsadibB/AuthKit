using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SadibTools.AuthLogin.Editor
{
    /// <summary>
    /// Writes Facebook App ID / Client Token from AuthSettings into the Android library strings.xml
    /// so the Facebook SDK can register the fb{appId} custom-tab scheme.
    /// </summary>
    internal sealed class AuthLoginFacebookAndroidConfig : IPreprocessBuildWithReport
    {
        private const string StringsPath =
            "Packages/com.sadib.authlogin/Plugins/Android/AuthKitGoogleSignIn.androidlib/res/values/strings.xml";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            WriteStrings(AuthSettings.LoadFromResources());
        }

        [MenuItem("Auth Login/Write Facebook Android Strings")]
        private static void WriteFromMenu()
        {
            WriteStrings(AuthSettings.LoadFromResources());
        }

        internal static void WriteStrings(AuthSettings settings)
        {
            string appId = settings != null ? settings.FacebookAppId : string.Empty;
            string clientToken = settings != null ? settings.FacebookClientToken : string.Empty;
            if (string.IsNullOrEmpty(appId))
                appId = "0";
            if (string.IsNullOrEmpty(clientToken))
                clientToken = "placeholder";

            string scheme = "fb" + appId;
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xml.AppendLine("<resources>");
            xml.AppendLine("    <string name=\"facebook_app_id\">" + EscapeXml(appId) + "</string>");
            xml.AppendLine("    <string name=\"facebook_client_token\">" + EscapeXml(clientToken) + "</string>");
            xml.AppendLine("    <string name=\"fb_login_protocol_scheme\">" + EscapeXml(scheme) + "</string>");
            xml.AppendLine("</resources>");

            string fullPath = Path.GetFullPath(StringsPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, xml.ToString());
            Debug.Log("[AuthLogin] Wrote Facebook Android strings for App ID " + appId);
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}
