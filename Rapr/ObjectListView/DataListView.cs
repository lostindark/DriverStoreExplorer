/*
 * DataListView - A data-bindable listview
 *
 * Author: Phillip Piper
 * Date: 27/09/2008 9:15 AM
 *
 * Change log:
 * v2.3
 * 2009-01-18   JPP  - Boolean columns are now handled as checkboxes
 *                   - Auto-generated columns would fail if the data source was 
 *                     reseated, even to the same data source
 * v2.0.1
 * 2009-01-07   JPP  - Made all public and protected methods virtual 
 * 2008-10-03   JPP  - Separated from ObjectListView.cs
 * 
 * Copyright (C) 2006-2008 Phillip Piper
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip_piper@bigfoot.com.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing.Design;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{

    /// <summary>
    /// A DataListView is a ListView that can be bound to a datasource (which would normally be a DataTable or DataView).
    /// </summary>
    /// <remarks>
    /// <para>This listview keeps itself in sync with its source datatable by listening for change events.</para>
    /// <para>If the listview has no columns when given a data source, it will automatically create columns to show all of the datatables columns.
    /// This will be only the simplest view of the world, and would look more interesting with a few delegates installed.</para>
    /// <para>This listview will also automatically generate missing aspect getters to fetch the values from the data view.</para>
    /// <para>Changing data sources is possible, but error prone. Before changing data sources, the programmer is responsible for modifying/resetting
    /// the column collection to be valid for the new data source.</para>
    /// <para>Internally, a CurrencyManager controls keeping the data source in-sync with other users of the data source (as per normal .NET
    /// behavior). This means that the model objects in the DataListView are DataRowView objects. If you write your own AspectGetters/Setters,
    /// they will be given DataRowView objects.</para>
    /// </remarks>
    public class DataListView : ObjectListView
    {
        /// <summary>
        /// Make a DataListView
        /// </summary>
        public DataListView()
            : base()
        {
        }

        #region Public Properties

        /// <summary>
        /// Get or set the DataSource that will be displayed in this list view.
        /// </summary>
        /// <remarks>The DataSource should implement either <see cref="IList"/>, <see cref="IBindingList"/>,
        /// or <see cref="IListSource"/>. Some common examples are the following types of objects:
        /// <list type="unordered">
        /// <item><description><see cref="DataView"/></description></item>
        /// <item><description><see cref="DataTable"/></description></item>
        /// <item><description><see cref="DataSet"/></description></item>
        /// <item><description><see cref="DataViewManager"/></description></item>
        /// <item><description><see cref="BindingSource"/></description></item>
        /// </list>
        /// <para>When binding to a list container (i.e. one that implements the
        /// <see cref="IListSource"/> interface, such as <see cref="DataSet"/>)
        /// you must also set the <see cref="DataMember"/> property in order
        /// to identify which particular list you would like to display. You
        /// may also set the <see cref="DataMember"/> property even when
        /// DataSource refers to a list, since <see cref="DataMember"/> can
        /// also be used to navigate relations between lists.</para>
        /// </remarks>
        [Category("Data"),
        TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
        public virtual Object DataSource
        {
            get { return dataSource; }
            set {
                //THINK: Should we only assign it if it is changed?
                //if (dataSource != value) {
                dataSource = value;
                this.RebindDataSource(true);
                //}
            }
        }
        private Object dataSource;

        /// <summary>
        /// Gets or sets the name of the list or table in the data source for which the DataListView is displaying data.
        /// </summary>
        /// <remarks>If the data source is not a DataSet or DataViewManager, this property has no effect</remarks>
        [Category("Data"),
         Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor)),
         DefaultValue("")]
        public virtual string DataMember
        {
            get { return dataMember; }
            set {
                if (dataMember != value) {
                    dataMember = value;
                    RebindDataSource();
                }
            }
        }
        private string dataMember = "";

        #endregion

        #region Initialization

        private CurrencyManager currencyManager = null;

        /// <summary>
        /// Our data source has changed. Figure out how to handle the new source
        /// </summary>
        protected virtual void RebindDataSource()
        {
            RebindDataSource(false);
        }

        /// <summary>
        /// Our data source has changed. Figure out how to handle the new source
        /// </summary>
        protected virtual void RebindDataSource(bool forceDataInitialization)
        {
            if (this.BindingContext == null)
                return;

            // Obtain the CurrencyManager for the current data source.
            CurrencyManager tempCurrencyManager = null;

            if (this.DataSource != null) {
                tempCurrencyManager = (CurrencyManager)this.BindingContext[this.DataSource, this.DataMember];
            }

            // Has our currency manager changed?
            if (this.currencyManager != tempCurrencyManager) {

                // Stop listening for events on our old currency manager
                if (this.currencyManager != null) {
                    this.currencyManager.MetaDataChanged -= new EventHandler(currencyManager_MetaDataChanged);
                    this.currencyManager.PositionChanged -= new EventHandler(currencyManager_PositionChanged);
                    this.currencyManager.ListChanged -= new ListChangedEventHandler(currencyManager_ListChanged);
                }

                this.currencyManager = tempCurrencyManager;

                // Start listening for events on our new currency manager
                if (this.currencyManager != null) {
                    this.currencyManager.MetaDataChanged += new EventHandler(currencyManager_MetaDataChanged);
                    this.currencyManager.PositionChanged += new EventHandler(currencyManager_PositionChanged);
                    this.currencyManager.ListChanged += new ListChangedEventHandler(currencyManager_ListChanged);
                }

                // Our currency manager has changed so we have to initialize a new data source
                forceDataInitialization = true;
            }

            if (forceDataInitialization)
                InitializeDataSource();
        }

        /// <summary>
        /// The data source for this control has changed. Reconfigure the control for the new source
        /// </summary>
        protected virtual void InitializeDataSource()
        {
            if (this.Frozen || this.currencyManager == null)
                return;

            this.CreateColumnsFromSource();
            this.CreateMissingAspectGettersAndPutters();
            this.SetObjects(this.currencyManager.List);

            // If we have some data, resize the new columns based on the data available.
            if (this.Items.Count > 0) {
                foreach (ColumnHeader column in this.Columns) {
                    if (column.Width == 0)
                        this.AutoResizeColumn(column.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
        }

        /// <summary>
        /// Create columns for the listview based on what properties are available in the data source
        /// </summary>
        /// <remarks>
        /// <para>This method will not replace existing columns.</para>
        /// </remarks>
        protected virtual void CreateColumnsFromSource()
        {
            if (this.currencyManager == null || this.Columns.Count != 0)
                return;

            PropertyDescriptorCollection properties = this.currencyManager.GetItemProperties();
            if (properties.Count == 0)
                return;

            bool hasBooleanColumns = false;
            for (int i = 0; i < properties.Count; i++) {
                PropertyDescriptor property = properties[i];

                // Relationships to other tables turn up as IBindibleLists. Don't make columns to show them.
                // CHECK: Is this always true? What other things could be here? Constraints? Triggers?
                if (property.PropertyType == typeof(IBindingList))
                    continue;

                // Create a column
                OLVColumn column = new OLVColumn(property.DisplayName, property.Name);
                if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(CheckState)) {
                    hasBooleanColumns = true;
                    column.TextAlign = HorizontalAlignment.Center;
                    column.Width = 32;
                    column.AspectName = property.Name;
                    column.CheckBoxes = true;
                    if (property.PropertyType == typeof(CheckState))
                        column.TriStateCheckBoxes = true;
                } else {
                    column.Width = 0; // zero-width since we will resize it once we have some data

                    // If our column is a BLOB, it could be an image, so assign a renderer to draw it.
                    // CONSIDER: Is this a common enough case to warrant this code?
                    if (property.PropertyType == typeof(System.Byte[]))
                        column.Renderer = new ImageRenderer();
                }
                column.IsEditable = !property.IsReadOnly;

                // Add it to our list
                this.Columns.Add(column);
            }

            if (hasBooleanColumns)
                this.SetupSubItemCheckBoxes();
        }

        /// <summary>
        /// Generate aspect getters and putters for any columns that are missing them (and for which we have
        /// enough information to actually generate a getter)
        /// </summary>
        protected virtual void CreateMissingAspectGettersAndPutters()
        {
            for (int i = 0; i < this.Columns.Count; i++) {
                OLVColumn column = this.GetColumn(i);
                if (column.AspectGetter == null && !String.IsNullOrEmpty(column.AspectName)) {
                    column.AspectGetter = delegate(object row) {
                        // In most cases, rows will be DataRowView objects
                        DataRowView drv = row as DataRowView;
                        if (drv != null)
                            return drv[column.AspectName];
                        else
                            return column.GetAspectByName(row);
                    };
                }
                if (column.IsEditable && column.AspectPutter == null && !String.IsNullOrEmpty(column.AspectName)) {
                    column.AspectPutter = delegate(object row, object newValue) {
                        // In most cases, rows will be DataRowView objects
                        DataRowView drv = row as DataRowView;
                        if (drv != null)
                            drv[column.AspectName] = newValue;
                        else
                            column.PutAspectByName(row, newValue);
                    };
                }
            }
        }

        #endregion

        #region Object manipulations

        /// <summary>
        /// Add the given collection of model objects to this control.
        /// </summary>
        /// <param name="modelObjects">A collection of model objects</param>
        /// <remarks>This is a no-op for data lists, since the data
        /// is controlled by the DataSource. Manipulate the data source
        /// rather than this view of the data source.</remarks>
        public override void AddObjects(ICollection modelObjects)
        {
        }

        /// <summary>
        /// Remove the given collection of model objects from this control.
        /// </summary>
        /// <remarks>This is a no-op for data lists, since the data
        /// is controlled by the DataSource. Manipulate the data source
        /// rather than this view of the data source.</remarks>
        public override void RemoveObjects(ICollection modelObjects)
        {
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// What should we do when the list is unfrozen
        /// </summary>
        protected override void DoUnfreeze()
        {
            this.RebindDataSource(true);
        }

        /// <summary>
        /// Handles binding context changes
        /// </summary>
        /// <param name="e">The EventArgs that will be passed to any handlers
        /// of the BindingContextChanged event.</param>
        protected override void OnBindingContextChanged(EventArgs e)
        {
            base.OnBindingContextChanged(e);

            // If our binding context changes, we must rebind, since we will
            // have a new currency managers, even if we are still bound to the
            // same data source.
            this.RebindDataSource(false);
        }

        /// <summary>
        /// Handles parent binding context changes
        /// </summary>
        /// <param name="e">Unused EventArgs.</param>
        protected override void OnParentBindingContextChanged(EventArgs e)
        {
            base.OnParentBindingContextChanged(e);

            // BindingContext is an ambient property - by default it simply picks
            // up the parent control's context (unless something has explicitly
            // given us our own). So we must respond to changes in our parent's
            // binding context in the same way we would changes to our own
            // binding context.
            this.RebindDataSource(false);
        }

        /// <summary>
        /// CurrencyManager ListChanged event handler.
        /// Deals with fine-grained changes to list items.
        /// </summary>
        /// <remarks>
        /// It's actually difficult to deal with these changes in a fine-grained manner.
        /// If our listview is grouped, then any change may make a new group appear or
        /// an old group disappear. It is rarely enough to simply update the affected row.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void currencyManager_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType) {

                // Well, usually fine-grained... The whole list has changed utterly, so reload it.
                case ListChangedType.Reset:
                    this.InitializeDataSource();
                    break;

                // A single item has changed, so just refresh that.
                // TODO: Even in this simple case, we should probably rebuild the list.
                case ListChangedType.ItemChanged:
                    Object changedRow = this.currencyManager.List[e.NewIndex];
                    this.RefreshObject(changedRow);
                    break;

                // A new item has appeared, so add that.
                // We get this event twice if certain grid controls are used to add a new row to a
                // datatable: once when the editing of a new row begins, and once again when that
                // editing commits. (If the user cancels the creation of the new row, we never see
                // the second creation.) We detect this by seeing if this is a view on a row in a
                // DataTable, and if it is, testing to see if it's a new row under creation.
                case ListChangedType.ItemAdded:
                    Object newRow = this.currencyManager.List[e.NewIndex];
                    DataRowView drv = newRow as DataRowView;
                    if (drv == null || !drv.IsNew) {
                        // Either we're not dealing with a view on a data table, or this is the commit
                        // notification. Either way, this is the final notification, so we want to
                        // handle the new row now!
                        this.InitializeDataSource();
                    }
                    break;

                // An item has gone away.
                case ListChangedType.ItemDeleted:
                    this.InitializeDataSource();
                    break;

                // An item has changed its index.
                case ListChangedType.ItemMoved:
                    this.InitializeDataSource();
                    break;

                // Something has changed in the metadata.
                // CHECK: When are these events actually fired?
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                    this.InitializeDataSource();
                    break;
            }
        }


        /// <summary>
        /// The CurrencyManager calls this if the data source looks
        /// different. We just reload everything.
        /// CHECK: Do we need this if we are handle ListChanged metadata events?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void currencyManager_MetaDataChanged(object sender, EventArgs e)
        {
            this.InitializeDataSource();
        }

        
        /// <summary>
        /// Called by the CurrencyManager when the currently selected item
        /// changes. We update the ListView selection so that we stay in sync
        /// with any other controls bound to the same source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void currencyManager_PositionChanged(object sender, EventArgs e)
        {
            int index = this.currencyManager.Position;

            // Make sure the index is sane (-1 pops up from time to time)
            if (index < 0 || index >= this.Items.Count)
                return;

            // Avoid recursion. If we are currently changing the index, don't
            // start the process again.
            if (this.isChangingIndex)
                return;

            try {
                this.isChangingIndex = true;

                // We can't use the index directly, since our listview may be sorted
                this.SelectedObject = this.currencyManager.List[index];

                // THINK: Do we always want to bring it into view?
                if (this.SelectedItems.Count > 0)
                    this.SelectedItems[0].EnsureVisible();

            }
            finally {
                this.isChangingIndex = false;
            }
        }
        private bool isChangingIndex = false;

        /// <summary>
        /// Handle a SelectedIndexChanged event
        /// </summary>
        /// <param name="e">The event</param>
        /// <remarks>
        /// Called by Windows Forms when the currently selected index of the
        /// control changes. This usually happens because the user clicked on
        /// the control. In this case we want to notify the CurrencyManager so
        /// that any other bound controls will remain in sync. This method will
        /// also be called when we changed our index as a result of a
        /// notification that originated from the CurrencyManager, and in that
        /// case we avoid notifying the CurrencyManager back!
        /// </remarks>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            // Prevent recursion
            if (this.isChangingIndex)
                return;

            // If we are bound to a datasource, and only one item is selected,
            // tell the currency manager which item is selected.
            if (this.SelectedIndices.Count == 1 && this.currencyManager != null) {
                try {
                    this.isChangingIndex = true;

                    // We can't use the selectedIndex directly, since our listview may be sorted.
                    // So we have to find the index of the selected object within the original list.
                    this.currencyManager.Position = this.currencyManager.List.IndexOf(this.SelectedObject);
                }
                finally {
                    this.isChangingIndex = false;
                }
            }
        }

        #endregion

    }
}
