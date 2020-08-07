using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

using Rapr.Lang;

namespace Rapr
{
    public partial class AboutBox : Form
    {
        private readonly IUpdateManager updateManager;

        public AboutBox(IUpdateManager updateManager)
        {
            this.updateManager = updateManager ?? throw new ArgumentNullException(nameof(updateManager));

            this.InitializeComponent();
            this.Text = string.Format(Language.Product_About_Title, AssemblyTitle);
            this.labelVersionInfo.Text = $"v{AssemblyVersion}";

            _ = this.UpdateLatestVersionInfo();
        }

        private async Task UpdateLatestVersionInfo()
        {
            try
            {
                var result = await this.updateManager.GetLatestVersionInfo().ConfigureAwait(false);

                if (result?.Version != null)
                {
                    this.labelLink.Invoke(new Action(() =>
                    {
                        if (AssemblyVersion >= result.Version)
                        {
                            this.labelLink.Text = Language.About_VersionUpToDate;
                            this.labelLink.Links.Clear();
                        }
                        else
                        {
                            var versionStr = result.Version.ToString();
                            this.labelLink.Text = string.Format(Language.About_FoundNewVersion, versionStr, Language.About_Download);

                            var versionStart = this.labelLink.Text.IndexOf(versionStr, 0, StringComparison.Ordinal);

                            if (versionStart >= 0)
                            {
                                this.labelLink.Links.Add(new LinkLabel.Link(versionStart, versionStr.Length, result.PageUrl));
                            }

                            var linkStart = this.labelLink.Text.IndexOf(Language.About_Download, 0, StringComparison.Ordinal);

                            if (linkStart >= 0)
                            {
                                this.labelLink.Links.Add(new LinkLabel.Link(linkStart, Language.About_Download.Length, result.DownloadUrl));
                            }
                        }
                    }));
                }
            }
            catch (AggregateException ex) when (ex.InnerException is HttpRequestException)
            {
            }
            catch (HttpRequestException)
            {
            }
        }

        #region Assembly Attribute Accessors

        public static string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }

                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public static Version AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void LabelLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url;
            LinkLabel linkLabel = (LinkLabel)sender;

            if (e.Link.LinkData != null)
            {
                url = e.Link.LinkData.ToString();
            }
            else
            {
                url = linkLabel.Text.Substring(e.Link.Start, e.Link.Length);
            }

            Process.Start(url);
            linkLabel.LinkVisited = true;
        }

        private void TextBoxDescription_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
    }
}
