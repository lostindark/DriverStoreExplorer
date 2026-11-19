using System;
using System.Reflection;
using System.Windows.Forms;

using BrightIdeasSoftware;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests
{
    // These tests build WinForms controls; run them on an STA thread, one at a time.
    [STATestClass]
    [DoNotParallelize]
    public class GhostDevicesFormTests
    {
        private static readonly DateTime NowUtc = DateTime.UtcNow;

        // Pre-selected (bus-enumerated) but used yesterday, so the default 30-day filter hides it.
        private static readonly GhostDevice RecentChecked = new GhostDevice(@"USB\VID_0001&PID_0001\RECENT", "Recent", "USB", NowUtc.AddDays(-1));
        private static readonly GhostDevice StaleChecked = new GhostDevice(@"USB\VID_0001&PID_0002\STALE", "Stale", "USB", NowUtc.AddDays(-100));
        private static readonly GhostDevice NoDateChecked = new GhostDevice(@"USB\VID_0001&PID_0003\NODATE", "No date", "USB");
        // Software-enumerated, so never pre-selected.
        private static readonly GhostDevice SoftwareUnchecked = new GhostDevice(@"ROOT\SYSTEM\0001", "Software", "System", NowUtc.AddDays(-100));

        [TestMethod]
        public void FilterExcludesHiddenCheckedDevicesAndRestoresThemWhenShown()
        {
            WithForm(new[] { RecentChecked, StaleChecked, NoDateChecked, SoftwareUnchecked }, (form, list, hideRecent, days, remove) =>
            {
                Assert.AreEqual(3, list.GetItemCount());
                CollectionAssert.AreEquivalent(new[] { StaleChecked, NoDateChecked }, form.SelectedDevices);

                hideRecent.Checked = false;
                Assert.AreEqual(4, list.GetItemCount());
                CollectionAssert.AreEquivalent(new[] { RecentChecked, StaleChecked, NoDateChecked }, form.SelectedDevices);

                // A choice made while the device is visible survives hiding and showing it again.
                list.UncheckObject(RecentChecked);
                list.CheckObject(SoftwareUnchecked);
                hideRecent.Checked = true;
                CollectionAssert.AreEquivalent(new[] { StaleChecked, NoDateChecked, SoftwareUnchecked }, form.SelectedDevices);
                hideRecent.Checked = false;
                CollectionAssert.AreEquivalent(new[] { StaleChecked, NoDateChecked, SoftwareUnchecked }, form.SelectedDevices);

                // A hidden device unchecked by the user is not checked again by a filter change either.
                list.CheckObject(RecentChecked);
                list.UncheckObject(StaleChecked);
                hideRecent.Checked = true;
                days.Text = "200";
                Assert.AreEqual(1, list.GetItemCount());
                CollectionAssert.AreEquivalent(new[] { NoDateChecked }, form.SelectedDevices);
                days.Text = "30";
                CollectionAssert.AreEquivalent(new[] { NoDateChecked, SoftwareUnchecked }, form.SelectedDevices);
                hideRecent.Checked = false;
                CollectionAssert.AreEquivalent(new[] { RecentChecked, NoDateChecked, SoftwareUnchecked }, form.SelectedDevices);
            });
        }

        [TestMethod]
        public void RemoveIsEnabledOnlyWhileAVisibleDeviceIsChecked()
        {
            WithForm(new[] { RecentChecked, SoftwareUnchecked }, (form, list, hideRecent, days, remove) =>
            {
                // The only checked device is hidden by the filter.
                Assert.AreEqual(1, list.GetItemCount());
                Assert.AreEqual(0, form.SelectedDevices.Count);
                Assert.IsFalse(remove.Enabled);

                list.CheckObject(SoftwareUnchecked);
                Assert.IsTrue(remove.Enabled);
                list.UncheckObject(SoftwareUnchecked);
                Assert.IsFalse(remove.Enabled);

                hideRecent.Checked = false;
                Assert.IsTrue(remove.Enabled);
                hideRecent.Checked = true;
                Assert.IsFalse(remove.Enabled);
            });
        }

        [TestMethod]
        public void RowToggledOutsideTheCheckStatePutterKeepsItsStateAcrossFilterChanges()
        {
            WithForm(new[] { RecentChecked, SoftwareUnchecked }, (form, list, hideRecent, days, remove) =>
            {
                // UI Automation flips the row's native check state without calling CheckStatePutter.
                var putter = list.CheckStatePutter;
                list.CheckStatePutter = null;
                ((ListViewItem)list.ModelToItem(SoftwareUnchecked)).Checked = true;
                list.CheckStatePutter = putter;
                CollectionAssert.AreEquivalent(new[] { SoftwareUnchecked }, form.SelectedDevices);

                hideRecent.Checked = false;
                CollectionAssert.AreEquivalent(new[] { RecentChecked, SoftwareUnchecked }, form.SelectedDevices);
            });
        }

        [TestMethod]
        public void InvalidDaysKeepTheLastValidFilter()
        {
            WithForm(new[] { RecentChecked, StaleChecked }, (form, list, hideRecent, days, remove) =>
            {
                days.Text = "200";
                Assert.AreEqual(0, list.GetItemCount());
                Assert.IsFalse(remove.Enabled);

                foreach (string text in new[] { "", "0", "abc" })
                {
                    days.Text = text;
                    Assert.AreEqual(0, list.GetItemCount(), $"'{text}' changed the filter");
                }

                days.Text = "30";
                CollectionAssert.AreEquivalent(new[] { StaleChecked }, form.SelectedDevices);
            });
        }

        private static void WithForm(GhostDevice[] devices, Action<GhostDevicesForm, ObjectListView, CheckBox, TextBox, Button> test)
        {
            using (var form = new GhostDevicesForm(devices))
            {
                var list = GetField<ObjectListView>(form, "listView");
                // Items only exist once the list has a window handle.
                form.CreateControl();
                list.CreateControl();
                test(
                    form,
                    list,
                    GetField<CheckBox>(form, "checkBoxHideRecent"),
                    GetField<TextBox>(form, "textRecentDays"),
                    GetField<Button>(form, "buttonRemove"));
            }
        }

        private static T GetField<T>(GhostDevicesForm form, string name) =>
            (T)typeof(GhostDevicesForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
    }
}
