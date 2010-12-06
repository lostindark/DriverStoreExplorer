/*
 * CellEditors - Several slightly modified controls that are used as celleditors within ObjectListView.
 *
 * Author: Phillip Piper
 * Date: 20/10/2008 5:15 PM
 *
 * Change log:
 * v2.3
 * 2009-08-13   JPP  - Standardized code formatting
 * v2.2.1
 * 2008-01-18   JPP  - Added special handling for enums
 * 2008-01-16   JPP  - Added EditorRegistry
 * v2.0.1
 * 2008-10-20   JPP  - Separated from ObjectListView.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace BrightIdeasSoftware
{
    /// <summary>
    /// A delegate that creates an editor for the given value
    /// </summary>
    /// <param name="model">The model from which that value came</param>
    /// <param name="column">The column for which the editor is being created</param>
    /// <param name="value">A representative value of the type to be edited. This value may not be the exact
    /// value for the column/model combination. It could be simply representative of
    /// the appropriate type of value.</param>
    /// <returns>A control which can edit the given value</returns>
    public delegate Control EditorCreatorDelegate(Object model, OLVColumn column, Object value);

    /// <summary>
    /// An editor registry gives a way to decide what cell editor should be used to edit
    /// the value of a cell. Programmers can register non-standard types and the control that 
    /// should be used to edit instances of that type. 
    /// </summary>
    /// <remarks>
    /// <para>All ObjectListViews share the same editor registry.</para>
    /// </remarks>
    public class EditorRegistry
    {
        #region Initializing

        /// <summary>
        /// Create an EditorRegistry
        /// </summary>
        public EditorRegistry() {
            this.InitializeStandardTypes();
        }

        private void InitializeStandardTypes() {
            this.Register(typeof(Boolean), typeof(BooleanCellEditor));
            this.Register(typeof(Int16), typeof(IntUpDown));
            this.Register(typeof(Int32), typeof(IntUpDown));
            this.Register(typeof(Int64), typeof(IntUpDown));
            this.Register(typeof(UInt16), typeof(UintUpDown));
            this.Register(typeof(UInt32), typeof(UintUpDown));
            this.Register(typeof(UInt64), typeof(UintUpDown));
            this.Register(typeof(Single), typeof(FloatCellEditor));
            this.Register(typeof(Double), typeof(FloatCellEditor));
            this.Register(typeof(DateTime), delegate(Object model, OLVColumn column, Object value) {
                DateTimePicker c = new DateTimePicker();
                c.Format = DateTimePickerFormat.Short;
                return c;
            });
            this.Register(typeof(Boolean), delegate(Object model, OLVColumn column, Object value) {
                CheckBox c = new BooleanCellEditor2();
                c.ThreeState = column.TriStateCheckBoxes;
                return c;
            });
        }

        #endregion

        #region Registering

        /// <summary>
        /// Register that values of 'type' should be edited by instances of 'controlType'.
        /// </summary>
        /// <param name="type">The type of value to be edited</param>
        /// <param name="controlType">The type of the Control that will edit values of 'type'</param>
        /// <example>
        /// ObjectListView.EditorRegistry.Register(typeof(Color), typeof(MySpecialColorEditor));
        /// </example>
        public void Register(Type type, Type controlType) {
            this.Register(type, delegate(Object model, OLVColumn column, Object value) {
                return controlType.InvokeMember("", BindingFlags.CreateInstance, null, null, null) as Control;
            });
        }

        /// <summary>
        /// Register the given delegate so that it is called to create editors
        /// for values of the given type
        /// </summary>
        /// <param name="type">The type of value to be edited</param>
        /// <param name="creator">The delegate that will create a control that can edit values of 'type'</param>
        /// <example>
        /// ObjectListView.EditorRegistry.Register(typeof(Color), CreateColorEditor);
        /// ...
        /// public Control CreateColorEditor(Object model, OLVColumn column, Object value)
        /// {
        ///     return new MySpecialColorEditor();
        /// }
        /// </example>
        public void Register(Type type, EditorCreatorDelegate creator) {
            this.creatorMap[type] = creator;
        }

        /// <summary>
        /// Register a delegate that will be called to create an editor for values
        /// that have not been handled.
        /// </summary>
        /// <param name="creator">The delegate that will create a editor for all other types</param>
        public void RegisterDefault(EditorCreatorDelegate creator) {
            this.defaultCreator = creator;
        }

        /// <summary>
        /// Register a delegate that will be given a chance to create a control
        /// before any other option is considered.
        /// </summary>
        /// <param name="creator">The delegate that will create a control</param>
        public void RegisterFirstChance(EditorCreatorDelegate creator) {
            this.firstChanceCreator = creator;
        }

        #endregion

        #region Accessing

        /// <summary>
        /// Create and return an editor that is appropriate for the given value.
        /// Return null if no appropriate editor can be found.
        /// </summary>
        /// <param name="model">The model involved</param>
        /// <param name="column">The column to be edited</param>
        /// <param name="value">The value to be edited. This value may not be the exact
        /// value for the column/model combination. It could be simply representative of
        /// the appropriate type of value.</param>
        /// <returns>A Control that can edit the given type of values</returns>
        public Control GetEditor(Object model, OLVColumn column, Object value) {
            Control editor;

            if (this.firstChanceCreator != null) {
                editor = this.firstChanceCreator(model, column, value);
                if (editor != null)
                    return editor;
            }

            if (value != null && this.creatorMap.ContainsKey(value.GetType())) {
                editor = this.creatorMap[value.GetType()](model, column, value);
                if (editor != null)
                    return editor;
            }

            if (this.defaultCreator != null)
                return this.defaultCreator(model, column, value);

            if (value != null && value.GetType().IsEnum)
                return this.CreateEnumEditor(value.GetType());

            return null;
        }

        /// <summary>
        /// Create and return an editor that will edit values of the given type
        /// </summary>
        /// <param name="type">A enum type</param>
        protected Control CreateEnumEditor(Type type) {
            return new EnumCellEditor(type);
        }

        #endregion

        #region Private variables

        private EditorCreatorDelegate firstChanceCreator;
        private EditorCreatorDelegate defaultCreator;
        private Dictionary<Type, EditorCreatorDelegate> creatorMap = new Dictionary<Type, EditorCreatorDelegate>();

        #endregion
    }

    /// <summary>
    /// These items allow combo boxes to remember a value and its description.
    /// </summary>
    internal class ComboBoxItem
    {
        public ComboBoxItem(Object key, String description) {
            this.key = key;
            this.description = description;
        }
        private String description;

        public Object Key {
            get { return key; }
        }
        private Object key;

        public override string ToString() {
            return this.description;
        }
    } 

    //-----------------------------------------------------------------------
    // Cell editors
    // These classes are simple cell editors that make it easier to get and set
    // the value that the control is showing.
    // In many cases, you can intercept the CellEditStarting event to 
    // change the characteristics of the editor. For example, changing
    // the acceptable range for a numeric editor or changing the strings
    // that respresent true and false values for a boolean editor.

    /// <summary>
    /// This editor shows and auto completes values from the given listview column.
    /// </summary>
    [ToolboxItem(false)]
    public class AutoCompleteCellEditor : ComboBox
    {
        /// <summary>
        /// Create an AutoCompleteCellEditor
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="column"></param>
        public AutoCompleteCellEditor(ObjectListView lv, OLVColumn column) {
            this.DropDownStyle = ComboBoxStyle.DropDown;

            Dictionary<String, bool> alreadySeen = new Dictionary<string, bool>();
            for (int i = 0; i < Math.Min(lv.GetItemCount(), 1000); i++) {
                String str = column.GetStringValue(lv.GetModelObject(i));
                if (!alreadySeen.ContainsKey(str)) {
                    this.Items.Add(str);
                    alreadySeen[str] = true;
                }
            }

            this.Sorted = true;
            this.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.AutoCompleteMode = AutoCompleteMode.Append;
        }
    }

    /// <summary>
    /// This combo box is specialised to allow editing of an enum.
    /// </summary>
    internal class EnumCellEditor : ComboBox
    {
        public EnumCellEditor(Type type) {
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ValueMember = "Key";

            ArrayList values = new ArrayList();
            foreach (object value in Enum.GetValues(type))
                values.Add(new ComboBoxItem(value, Enum.GetName(type, value)));

            this.DataSource = values;
        }
    }

    /// <summary>
    /// This editor simply shows and edits integer values.
    /// </summary>
    internal class IntUpDown : NumericUpDown
    {
        public IntUpDown() {
            this.DecimalPlaces = 0;
            this.Minimum = -9999999;
            this.Maximum = 9999999;
        }

        new public int Value {
            get { return Decimal.ToInt32(base.Value); }
            set { base.Value = new Decimal(value); }
        }
    }

    /// <summary>
    /// This editor simply shows and edits unsigned integer values.
    /// </summary>
    internal class UintUpDown : NumericUpDown
    {
        public UintUpDown() {
            this.DecimalPlaces = 0;
            this.Minimum = 0;
            this.Maximum = 9999999;
        }

        new public uint Value {
            get { return Decimal.ToUInt32(base.Value); }
            set { base.Value = new Decimal(value); }
        }
    }

    /// <summary>
    /// This editor simply shows and edits boolean values.
    /// </summary>
    internal class BooleanCellEditor : ComboBox
    {
        public BooleanCellEditor() {
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ValueMember = "Key";

            ArrayList values = new ArrayList();
            values.Add(new ComboBoxItem(false, "False"));
            values.Add(new ComboBoxItem(true, "True"));

            this.DataSource = values;
        }
    }

    /// <summary>
    /// This editor simply shows and edits boolean values using a checkbox
    /// </summary>
    internal class BooleanCellEditor2 : CheckBox
    {
        public BooleanCellEditor2() {
        }

        public bool? Value {
            get {
                switch (this.CheckState) {
                    case CheckState.Checked: return true;
                    case CheckState.Indeterminate: return null;
                    case CheckState.Unchecked: 
                    default: return false;
                }
            }
            set {
                if (value.HasValue) 
                    this.CheckState = value.Value ? CheckState.Checked : CheckState.Unchecked;
                else
                    this.CheckState = CheckState.Indeterminate;
            }
        }

        public new HorizontalAlignment TextAlign {
            get {
                switch (this.CheckAlign) {
                    case ContentAlignment.MiddleRight: return HorizontalAlignment.Right;
                    case ContentAlignment.MiddleCenter: return HorizontalAlignment.Center;
                    case ContentAlignment.MiddleLeft: 
                    default: return HorizontalAlignment.Left;
                }
            }
            set {
                switch (value) {
                    case HorizontalAlignment.Left:
                        this.CheckAlign = ContentAlignment.MiddleLeft;
                        break;
                    case HorizontalAlignment.Center:
                        this.CheckAlign = ContentAlignment.MiddleCenter;
                        break;
                    case HorizontalAlignment.Right:
                        this.CheckAlign = ContentAlignment.MiddleRight;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// This editor simply shows and edits floating point values.
    /// </summary>
    /// <remarks>You can intercept the CellEditStarting event if you want
    /// to change the characteristics of the editor. For example, by increasing
    /// the number of decimal places.</remarks>
    internal class FloatCellEditor : NumericUpDown
    {
        public FloatCellEditor() {
            this.DecimalPlaces = 2;
            this.Minimum = -9999999;
            this.Maximum = 9999999;
        }

        new public double Value {
            get { return Convert.ToDouble(base.Value); }
            set { base.Value = Convert.ToDecimal(value); }
        }
    }
}
