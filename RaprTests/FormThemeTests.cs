using System;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows.Forms;

using BrightIdeasSoftware;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Controls;
using Rapr.Utils;

namespace Rapr.Tests
{
    [STATestClass]
    [DoNotParallelize]
    public class FormThemeTests
    {
        private ApplicationSettingsBase settings;
        private object previousSetting;
        private ObjectListViewColorMode previousMode;

        [TestInitialize]
        public void Initialize()
        {
            Application.EnableVisualStyles();
            this.settings = (ApplicationSettingsBase)typeof(DSEForm).Assembly.GetType("Rapr.Properties.Settings")
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            this.previousSetting = this.settings["ThemeMode"];
            this.previousMode = ObjectListViewTheme.ColorMode;
            this.settings["ThemeMode"] = "Light";
            FormTheme.SetColorMode(ObjectListViewColorMode.Light);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.settings["ThemeMode"] = this.previousSetting;
            ObjectListViewTheme.ColorMode = this.previousMode;
            FormTheme.Initialize();
        }

        [TestMethod]
        public void ThemeRefreshUsesSharedColorsForCustomControlsAndListDefaults()
        {
            using (var form = new Form())
            using (var search = new EnhancedSearchTextBox { Text = "driver" })
            using (var details = new DriverDetailsPanel())
            using (var list = new DriverStoreListView { ShowGroups = false, UseAlternatingBackColors = true })
            {
                list.Columns.Add(new OLVColumn("INF", nameof(DriverStoreEntry.DriverInfName)));
                var entry = new DriverStoreEntry { DriverInfName = "test.inf" };
                list.SetObjects(new[] { entry, new DriverStoreEntry { DriverInfName = "second.inf" } });
                details.ShowEntry(entry, null);
                form.Controls.AddRange(new Control[] { search, details, list });
                var theme = AttachTheme(form);
                try
                {
                    ObjectListViewTheme.SetDarkColor(nameof(IThemeColors.InputColor), Color.Navy);
                    ObjectListViewTheme.SetDarkColor(nameof(IThemeColors.GrayTextColor), Color.Lime);
                    foreach (bool dark in new[] { true, false })
                    {
                        var colors = new ThemePalette(dark, false);
                        theme.Apply(colors);
                        Assert.AreEqual(colors.InputColor.ToArgb(), search.BackColor.ToArgb());
                        var text = search.Controls.OfType<TextBox>().Single();
                        Assert.AreEqual(colors.InputColor.ToArgb(), text.BackColor.ToArgb());
                        Assert.AreEqual("driver", search.Text);
                        Assert.AreSame(entry, details.Entry);
                        var content = details.Controls.OfType<RichTextBox>().Single();
                        content.Select(content.Text.IndexOf(Rapr.Lang.Language.Column_Size, StringComparison.Ordinal),
                            Rapr.Lang.Language.Column_Size.Length);
                        Assert.AreEqual(colors.GrayTextColor.ToArgb(), content.SelectionColor.ToArgb());
                        Assert.IsTrue(list.AlternateRowBackColor.IsEmpty);
                        Assert.AreEqual(colors.ControlColor.ToArgb(), list.GetItem(1).BackColor.ToArgb());
                        var overlay = (TextOverlay)list.EmptyListMsgOverlay;
                        Assert.AreEqual(colors.EmptyListBackgroundColor.ToArgb(), overlay.BackColor.ToArgb());
                        Assert.AreEqual(colors.EmptyListTextColor.ToArgb(), overlay.TextColor.ToArgb());
                    }
                }
                finally
                {
                    ObjectListViewTheme.ResetColors();
                }
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void AppThemePreparesOnlyTheFirstDarkFrame(bool dark)
        {
            this.settings["ThemeMode"] = dark ? "Dark" : "Light";
            using (var form = new Form { Opacity = 0.5, ShowInTaskbar = false })
            {
                AttachTheme(form);
                double? firstPaintOpacity = null;
                form.Paint += (sender, e) => firstPaintOpacity = firstPaintOpacity ?? form.Opacity;
                form.Show();
                form.Refresh();
                Assert.AreEqual(dark ? 0.0 : 0.5, firstPaintOpacity);
                Assert.AreEqual(0.5, form.Opacity);
                form.Hide();
                firstPaintOpacity = null;
                form.Show();
                form.Refresh();
                Assert.AreEqual(0.5, firstPaintOpacity);
                Assert.AreEqual(0.5, form.Opacity);
            }
        }

        [TestMethod]
        [DataRow(DSEForm.Status.Warning, false)]
        [DataRow(DSEForm.Status.Error, false)]
        [DataRow(DSEForm.Status.Success, false)]
        [DataRow(DSEForm.Status.Warning, true)]
        [DataRow(DSEForm.Status.Error, true)]
        [DataRow(DSEForm.Status.Success, true)]
        public void StatusHighlightsRemainVisibleWithTheSharedRenderer(DSEForm.Status status, bool dark)
        {
            var form = (DSEForm)FormatterServices.GetUninitializedObject(typeof(DSEForm));
            typeof(Form).GetConstructor(Type.EmptyTypes).Invoke(form, null);
            using (form)
            {
                typeof(DSEForm).GetMethod("InitializeComponent", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(form, null);
                var strip = (StatusStrip)typeof(DSEForm)
                    .GetField("statusStrip1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                var label = (ToolStripStatusLabel)typeof(DSEForm)
                    .GetField("lblStatus", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                typeof(DSEForm).GetField("currentStatus", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(form, status);
                var theme = AttachTheme(form, colors =>
                {
                    typeof(DSEForm).GetField("themeColors", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(form, colors);
                    typeof(DSEForm).GetMethod("ApplyStatusColors", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(form, null);
                });
                theme.Apply(new ThemePalette(dark, false));
                strip.PerformLayout();
                using (var bitmap = new Bitmap(strip.Width, strip.Height))
                {
                    strip.DrawToBitmap(bitmap, strip.ClientRectangle);
                    Assert.AreNotEqual(strip.BackColor.ToArgb(), label.BackColor.ToArgb());
                    Assert.AreEqual(label.BackColor.ToArgb(),
                        bitmap.GetPixel(label.Bounds.Right - 10, label.Bounds.Top + label.Height / 2).ToArgb());
                }
            }
        }

        [TestMethod]
        public void DetailsDividerFollowsThePaletteAcrossThemeChanges()
        {
            var form = (DSEForm)FormatterServices.GetUninitializedObject(typeof(DSEForm));
            typeof(Form).GetConstructor(Type.EmptyTypes).Invoke(form, null);
            using (form)
            {
                typeof(DSEForm).GetMethod("InitializeComponent", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(form, null);
                typeof(DSEForm).GetMethod("SetupDetailsPane", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(form, null);
                var splitter = (Splitter)typeof(DSEForm)
                    .GetField("detailsSplitter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                var theme = AttachTheme(form);
                foreach (var colors in new[] {
                    new ThemePalette(true, false),
                    new ThemePalette(false, false),
                    new ThemePalette(true, true),
                })
                {
                    theme.Apply(colors);
                    using (var bitmap = new Bitmap(splitter.Width, splitter.Height))
                    {
                        splitter.DrawToBitmap(bitmap, splitter.ClientRectangle);
                        Assert.AreEqual(colors.ControlColor.ToArgb(),
                            bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2).ToArgb());
                    }
                }
            }
        }

        [TestMethod]
        public void DetailsTextKeepsAnInsetFromEveryBorderAfterTheming()
        {
            using (var form = new Form { ClientSize = new Size(320, 420) })
            using (var details = new DriverDetailsPanel { Dock = DockStyle.Fill })
            {
                form.Controls.Add(details);
                AttachTheme(form);
                form.PerformLayout();
                details.PerformLayout();
                var content = (RichTextBox)details.Controls[0];
                Assert.IsTrue(content.Left >= 8);
                Assert.IsTrue(details.ClientSize.Width - content.Right >= 8);
                Assert.IsTrue(details.ClientSize.Height - content.Bottom >= 8);
                Assert.IsTrue(content.Top > details.Font.Height);
            }
        }

        [TestMethod]
        public void ThemeRoundTripRestoresColorsAndPreservesListState()
        {
            using (var form = new Form())
            using (var list = new DriverStoreListView { ShowGroups = false, CheckBoxes = true, Dock = DockStyle.Fill })
            using (var label = new Label { ForeColor = Color.Purple })
            {
                form.Controls.Add(list);
                form.Controls.Add(label);
                var column = new OLVColumn("INF", nameof(DriverStoreEntry.DriverInfName));
                list.Columns.Add(column);
                list.PrimarySortColumn = column;
                list.PrimarySortOrder = SortOrder.Descending;
                var entry = new DriverStoreEntry { DriverInfName = "test.inf" };
                list.SetObjects(new[] { entry });
                list.CheckedObject = entry;
                list.SelectedObject = entry;
                var renderer = list.DefaultRenderer;
                bool ownerDraw = list.OwnerDraw;
                Color background = form.BackColor;
                Color alternateRowColor = list.AlternateRowBackColor;
                var theme = AttachTheme(form);

                theme.Apply(new ThemePalette(true, false));

                Assert.AreEqual(ObjectListViewTheme.Current.WindowColor.ToArgb(), list.BackColor.ToArgb());
                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), list.AlternateRowBackColorOrDefault.ToArgb());
                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), form.BackColor.ToArgb());
                Assert.AreEqual(Color.Purple, label.ForeColor);
                Assert.AreSame(renderer, list.DefaultRenderer);
                Assert.AreEqual(ownerDraw, list.OwnerDraw);
                Assert.AreSame(entry, list.SelectedObject);
                Assert.AreSame(entry, list.CheckedObject);
                Assert.AreSame(column, list.PrimarySortColumn);
                Assert.AreEqual(SortOrder.Descending, list.PrimarySortOrder);

                theme.Apply(new ThemePalette(false, false));

                Assert.AreEqual(background.ToArgb(), form.BackColor.ToArgb());
                Assert.AreEqual(alternateRowColor, list.AlternateRowBackColor);
                Assert.AreEqual(Color.Purple, label.ForeColor);
                Assert.AreSame(entry, list.SelectedObject);
                Assert.AreSame(entry, list.CheckedObject);
            }
        }

        [TestMethod]
        public void NewlyAddedControlsUseTheirOwnFormsTheme()
        {
            using (var darkForm = new Form())
            using (var lightForm = new Form())
            {
                var darkTheme = AttachTheme(darkForm);
                var lightTheme = AttachTheme(lightForm);
                darkTheme.Apply(new ThemePalette(true, false));
                var darkLabel = new Label();
                darkForm.Controls.Add(darkLabel);
                Assert.AreEqual(ObjectListViewTheme.Current.TextColor.ToArgb(), darkLabel.ForeColor.ToArgb());
                Assert.AreEqual(darkForm.BackColor, darkLabel.BackColor);

                lightTheme.Apply(new ThemePalette(false, false));
                var lightLabel = new Label();
                lightForm.Controls.Add(lightLabel);
                Assert.AreEqual(lightForm.BackColor, lightLabel.BackColor);
                Assert.AreNotEqual(darkLabel.BackColor, lightLabel.BackColor);
                var secondDarkLabel = new Label();
                darkForm.Controls.Add(secondDarkLabel);
                Assert.AreEqual(darkLabel.BackColor, secondDarkLabel.BackColor);

                darkTheme.Apply(new ThemePalette(false, false));
                Assert.AreEqual(darkForm.BackColor, darkLabel.BackColor);
                Assert.AreEqual(lightLabel.BackColor, darkLabel.BackColor);
            }
        }

        [TestMethod]
        public void HighContrastOverridesDarkAndRepaintsExistingDetails()
        {
            using (var form = new Form())
            using (var details = new DriverDetailsPanel())
            {
                form.Controls.Add(details);
                var entry = new DriverStoreEntry { DriverInfName = "test.inf", BootCritical = true };
                details.ShowEntry(entry, null);
                var content = (RichTextBox)details.Controls[0];
                var theme = AttachTheme(form);
                theme.Apply(new ThemePalette(true, false));
                Assert.AreSame(entry, details.Entry);
                content.Select(0, entry.DriverInfName.Length);
                Assert.AreEqual(ObjectListViewTheme.Current.TextColor.ToArgb(), content.SelectionColor.ToArgb());

                theme.Apply(new ThemePalette(true, true));

                Assert.AreEqual(SystemColors.Control.ToArgb(), form.BackColor.ToArgb());
                Assert.AreEqual(SystemColors.WindowText.ToArgb(), content.ForeColor.ToArgb());
                string tag = Rapr.Lang.Language.ResourceManager.GetString("Details_Tag_Boot_Critical", Rapr.Lang.Language.Culture);
                content.Select(content.Text.IndexOf(tag, StringComparison.Ordinal), tag.Length);
                Assert.AreEqual(SystemColors.HighlightText.ToArgb(), content.SelectionColor.ToArgb());
                Assert.AreEqual(SystemColors.Highlight.ToArgb(), content.SelectionBackColor.ToArgb());
            }
        }

        [TestMethod]
        public void RecreatedFormHandleAndNewMenusKeepTheTheme()
        {
            using (var form = new ThemeTestForm())
            using (var menu = new ContextMenuStrip())
            {
                var theme = AttachTheme(form);
                theme.Apply(new ThemePalette(true, false));
                form.CreateControl();
                form.RecreateTestHandle();
                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), form.BackColor.ToArgb());

                var item = new ToolStripMenuItem("Parent");
                menu.Items.Add(item);
                item.DropDownItems.Add("Child");
                theme.ApplyMenu(menu);
                Assert.AreEqual(ObjectListViewTheme.Current.TextColor.ToArgb(), item.ForeColor.ToArgb());
                Assert.AreEqual(ObjectListViewTheme.Current.MenuColor.ToArgb(),
                    ((ToolStripProfessionalRenderer)menu.Renderer).ColorTable.ToolStripDropDownBackground.ToArgb());

                theme.Dispose();
                var label = new Label { ForeColor = Color.Purple };
                form.Controls.Add(label);
                Assert.AreEqual(Color.Purple, label.ForeColor);
            }
        }

        [TestMethod]
        public void SearchControlThemesInnerTextBoxWithoutChangingSearchText()
        {
            using (var form = new Form())
            using (var search = new EnhancedSearchTextBox { Text = "driver" })
            {
                form.Controls.Add(search);
                var theme = AttachTheme(form);
                theme.Apply(new ThemePalette(true, false));
                foreach (Control child in search.Controls)
                {
                    if (child is TextBox text)
                    {
                        Assert.AreEqual(ObjectListViewTheme.Current.InputColor.ToArgb(), text.BackColor.ToArgb());
                        Assert.AreEqual(ObjectListViewTheme.Current.TextColor.ToArgb(), text.ForeColor.ToArgb());
                    }
                }
                Assert.AreEqual("driver", search.Text);

                theme.Apply(new ThemePalette(false, false));
                Assert.AreEqual(SystemColors.Window.ToArgb(), search.BackColor.ToArgb());
                Assert.AreEqual("driver", search.Text);
            }
        }

        [TestMethod]
        public void ThemeNotificationRefreshesThePreferenceAndRebuiltControls()
        {
            using (var form = new ThemeTestForm())
            {
                int changes = 0;
                AttachTheme(form, palette => changes++);
                form.Show();
                this.settings["ThemeMode"] = "Dark";
                ObjectListViewTheme.ColorMode = ObjectListViewColorMode.Dark;

                SendMessage(form.Handle, 0x001A, IntPtr.Zero, IntPtr.Zero);
                Application.DoEvents();

                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), form.BackColor.ToArgb());
                Assert.IsTrue(FormTheme.Current.Dark);
                int beforeRebuild = changes;
                form.Controls.Clear();
                var label = new Label();
                form.Controls.Add(label);
                AttachTheme(form);
                Assert.IsTrue(changes > beforeRebuild);
                Assert.AreEqual(ObjectListViewTheme.Current.TextColor.ToArgb(), label.ForeColor.ToArgb());

                this.settings["ThemeMode"] = "Light";
                RefreshTheme();
                Assert.AreEqual(SystemColors.Control.ToArgb(), form.BackColor.ToArgb());
                Assert.AreEqual(form.BackColor, label.BackColor);
            }
        }

        [TestMethod]
        public void DarkListHeadersRenderWithThePaletteAfterSwitchingAndHandleRecreation()
        {
            using (var form = new ThemeTestForm())
            using (var list = new DriverStoreListView { View = View.Details, ShowGroups = false, Size = new Size(300, 150) })
            using (var bitmap = new Bitmap(300, 150))
            {
                list.Columns.Add(new OLVColumn("INF", nameof(DriverStoreEntry.DriverInfName)) { Width = 180 });
                list.SetObjects(new[] { new DriverStoreEntry { DriverInfName = "test.inf" } });
                form.Controls.Add(list);
                AttachTheme(form);
                form.Show();
                this.settings["ThemeMode"] = "Dark";
                RefreshTheme();
                Application.DoEvents();

                list.DrawToBitmap(bitmap, list.ClientRectangle);
                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), bitmap.GetPixel(100, 5).ToArgb());
                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), bitmap.GetPixel(290, 5).ToArgb());

                form.RecreateTestHandle();
                Application.DoEvents();
                list.DrawToBitmap(bitmap, list.ClientRectangle);
                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), bitmap.GetPixel(100, 5).ToArgb());
                Assert.AreEqual(ObjectListViewTheme.Current.ControlColor.ToArgb(), bitmap.GetPixel(290, 5).ToArgb());
            }
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void DisabledCaptionsRenderReadableTextAndRestoreLightStyles(bool checkBox, bool rtl)
        {
            using (var form = new ThemeTestForm())
            using (ButtonBase button = checkBox ? (ButtonBase)new CheckBox() : new Button())
            using (var bitmap = new Bitmap(240, 40))
            {
                button.Text = "Disabled action";
                button.Size = bitmap.Size;
                button.Enabled = false;
                button.RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;
                var style = button.FlatStyle;
                bool visualStyle = button.UseVisualStyleBackColor;
                form.Controls.Add(button);
                AttachTheme(form);
                form.Show();
                this.settings["ThemeMode"] = "Dark";
                RefreshTheme();
                Application.DoEvents();

                button.DrawToBitmap(bitmap, button.ClientRectangle);
                Color foreground = checkBox ? ObjectListViewTheme.Current.GrayTextColor : ObjectListViewTheme.Current.ButtonDisabledTextColor;
                Assert.IsTrue(ContainsColor(bitmap, new Rectangle(40, 0, 160, 40), foreground),
                    "The disabled caption should use the readable muted foreground, not native black text.");
                Assert.IsFalse(button.Enabled);

                this.settings["ThemeMode"] = "Light";
                RefreshTheme();
                Assert.AreEqual(style, button.FlatStyle);
                Assert.AreEqual(visualStyle, button.UseVisualStyleBackColor);
            }
        }

        [TestMethod]
        public void NativeGroupCaptionsRenderReadableTextWithoutReplacingGroups()
        {
            Application.EnableVisualStyles();
            using (var form = new ThemeTestForm())
            using (var list = new DriverStoreListView { View = View.Details, ShowGroups = true, Size = new Size(350, 200) })
            using (var bitmap = new Bitmap(350, 200))
            {
                var column = new OLVColumn("Class", nameof(DriverStoreEntry.DriverClass)) { Width = 250 };
                list.Columns.Add(column);
                list.PrimarySortColumn = column;
                list.PrimarySortOrder = SortOrder.Ascending;
                list.SetObjects(new[] { new DriverStoreEntry { DriverClass = "Grouped devices" } });
                form.Controls.Add(list);
                AttachTheme(form);
                form.Show();
                this.settings["ThemeMode"] = "Dark";
                RefreshTheme();
                Application.DoEvents();
                Assert.AreEqual(1, list.OLVGroups.Count);
                var group = list.OLVGroups[0];
                var rectangle = new NativeRectangle { Top = 1 };
                const int GetGroupRectangle = 0x1000 + 98;
                Assert.AreNotEqual(IntPtr.Zero, SendMessage(list.Handle, GetGroupRectangle, (IntPtr)group.GroupId, ref rectangle),
                    $"GroupId={group.GroupId}, Groups={list.Groups.Count}, CanShowGroups={list.CanShowGroups}, VisualStyles={Application.RenderWithVisualStyles}, Items={list.Items.Count}");
                var label = new NativeRectangle { Top = 2 };
                Assert.AreNotEqual(IntPtr.Zero, SendMessage(list.Handle, GetGroupRectangle, (IntPtr)group.GroupId, ref label));

                list.DrawToBitmap(bitmap, list.ClientRectangle);

                Rectangle header = Rectangle.FromLTRB(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
                Assert.IsTrue(ContainsColor(bitmap, header, ObjectListViewTheme.Current.GroupHeaderTextColor),
                    "Native group captions should not retain their unreadable light-theme blue.");
                Assert.IsFalse(ContainsColor(bitmap,
                    Rectangle.FromLTRB(rectangle.Left, rectangle.Top, label.Left, rectangle.Bottom), ObjectListViewTheme.Current.GroupHeaderTextColor),
                    "Keep the native caption inset and leave the collapse-button area untouched.");
                Assert.AreSame(group, list.OLVGroups[0]);
                Assert.AreEqual(1, list.GetItemCount());

                form.RecreateTestHandle();
                Application.DoEvents();
                list.DrawToBitmap(bitmap, list.ClientRectangle);
                Assert.IsTrue(ContainsColor(bitmap, header, ObjectListViewTheme.Current.GroupHeaderTextColor));
            }
        }

        private static bool ContainsColor(Bitmap bitmap, Rectangle bounds, Color color)
        {
            bounds.Intersect(new Rectangle(Point.Empty, bitmap.Size));
            for (int y = bounds.Top; y < bounds.Bottom; y++)
            {
                for (int x = bounds.Left; x < bounds.Right; x++)
                {
                    if (bitmap.GetPixel(x, y).ToArgb() == color.ToArgb())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static FormTheme AttachTheme(Form form, Action<IThemeColors> changed = null)
        {
            typeof(DSEForm).Assembly.GetType("Rapr.Utils.DriverStoreTheme")
                .GetMethod("Attach", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { form, changed });
            return FormTheme.Attach(form);
        }

        private static void RefreshTheme()
        {
            typeof(DSEForm).Assembly.GetType("Rapr.Utils.DriverStoreTheme")
                .GetMethod("Initialize", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, null);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, ref NativeRectangle rectangle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

        private sealed class ThemeTestForm : Form
        {
            protected override bool ShowWithoutActivation => true;

            public void RecreateTestHandle() => this.RecreateHandle();
        }
    }
}
