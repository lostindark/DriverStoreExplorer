using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

using Rapr.Lang;

namespace Rapr
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            this.InitializeComponent();
            this.Text = string.Format(Language.Product_About_Title, this.AssemblyTitle);
            this.labelVersionInfo.Text = $"v{this.AssemblyVersion}";

            (Version latestVersion, string pageUrl, string downloadUrl) = UpdateManager.GetLatestVersionInfo();

            if (latestVersion != null)
            {
                if (this.AssemblyVersion >= latestVersion)
                {
                    this.labelLink.Text = Language.About_VersionUpToDate;
                    this.labelLink.Links.Clear();
                }
                else
                {
                    string versionStr = latestVersion.ToString();
                    this.labelLink.Text = string.Format(Language.About_FoundNewVersion, versionStr, Language.About_Download);

                    int versionStart = this.labelLink.Text.IndexOf(versionStr, 0);

                    if (versionStart >= 0)
                    {
                        this.labelLink.Links.Add(new LinkLabel.Link(versionStart, versionStr.Length, pageUrl));
                    }

                    int linkStart = this.labelLink.Text.IndexOf(Language.About_Download, 0);

                    if (linkStart >= 0)
                    {
                        this.labelLink.Links.Add(new LinkLabel.Link(linkStart, Language.About_Download.Length, downloadUrl));
                    }
                }
            }
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public Version AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
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
