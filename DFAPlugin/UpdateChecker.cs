using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Qitana.DFAPlugin
{
    public class ReleaseInfo
    {
        public string url;
        public string html_url;
        public int id;
        public string tag_name;
        public string name;
        public string body;
    }

    public class UpdateChecker
    {
        private const string EndPoint = @"https://api.github.com/repos/qitana/ACT_DFAPlugin/releases/latest";
        public static string ProductName
        {
            get
            {
                var product = (AssemblyProductAttribute)Attribute.GetCustomAttribute(
                    Assembly.GetExecutingAssembly(),
                    typeof(AssemblyProductAttribute));
                return product.Product;
            }
        }

        public static string Check()
        {
            var release = string.Empty;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            System.Net.Configuration.HttpWebRequestElement httpConfig = new System.Net.Configuration.HttpWebRequestElement();
            httpConfig.UseUnsafeHeaderParsing = true;

            try
            {
                var json = String.Empty;
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", @"ACT DFAPlugin UpdateChecker/1.0; using .NET WebClient/4.0.0");
                    using (var stream = wc.OpenRead(EndPoint))
                    using (var reader = new StreamReader(stream))
                    {
                        json = reader.ReadToEnd();
                    }
                }
                var serializer = new JavaScriptSerializer();
                ReleaseInfo ri = (ReleaseInfo)serializer.Deserialize(json, typeof(ReleaseInfo));
                string version = ri.tag_name.Replace("v", string.Empty);
                Version ignore;
                try
                {
                    ignore = new Version(Properties.Settings.Default.IgnoreVersion);
                }
                catch
                {
                    ignore = new Version("0.0.0.0");
                }
                var latest = new Version(version);
                var current = Assembly.GetExecutingAssembly().GetName().Version;
                if (current >= latest)
                {
                    release = String.Format(Messages.UpdateNotFound, current.ToString());
                    return release;
                }
                if (ignore >= latest)
                {
                    release = String.Format(Messages.UpdateAvailable, latest.ToString());
                    return release;
                }
                var message = Messages.NewVersionIsAvailable + Environment.NewLine + Environment.NewLine;
                message += String.Format(Messages.Update, latest.ToString()) + Environment.NewLine;
                message += String.Format(Messages.Current, current.ToString()) + Environment.NewLine;
                message += Environment.NewLine;
                message +=Messages.OpenSiteNow;
                var updateDialog = new UpdateDialog();
                if (Advanced_Combat_Tracker.FormActMain.ActiveForm != null)
                {
                    updateDialog.StartPosition = FormStartPosition.CenterParent;
                }
                else
                {
                    updateDialog.StartPosition = FormStartPosition.CenterScreen;
                }
                updateDialog.setDialogMessage(message);
                var result = updateDialog.ShowDialog();
                switch (result)
                {
                    case DialogResult.Yes:
                        Process.Start(ri.html_url);
                        break;
                    case DialogResult.Ignore:
                        Properties.Settings.Default.IgnoreVersion = latest.ToString();
                        Properties.Settings.Default.Save();
                        break;
                    default:
                        break;
                }
            } catch (Exception ex)
            {
                release = $"Update check error: {ex.ToString()}";
            }
            return release;
        }
    }
}
