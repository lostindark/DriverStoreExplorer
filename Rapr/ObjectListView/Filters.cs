/*
 * Filters - Filtering on ObjectListViews
 *
 * Author: Phillip Piper
 * Date: 03/03/2010 17:00 
 *
 * Change log:
 * 2010-06-23  JPP  Extended TextMatchFilter to handle regular expressions and string prefix matching.
 * v2.4
 * 2010-03-03  JPP  Initial version
 *
 * TO DO:
 *
 * Copyright (C) 2010 Phillip Piper
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
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing;

namespace BrightIdeasSoftware
{
    /// <summary>
    /// Interface for model-by-model filtering
    /// </summary>
    public interface IModelFilter
    {
        /// <summary>
        /// Should the given model be included when this filter is installed
        /// </summary>
        /// <param name="modelObject">The model object to consider</param>
        /// <returns>Returns true if the model will be included by the filter</returns>
        bool Filter(object modelObject);
    }

    /// <summary>
    /// Interface for whole list filtering
    /// </summary>
    public interface IListFilter
    {
        /// <summary>
        /// Return a subset of the given list of model objects as the new
        /// contents of the ObjectListView
        /// </summary>
        /// <param name="modelObjects">The collection of model objects that the list will possibly display</param>
        /// <returns>The filtered collection that holds the model objects that will be displayed.</returns>
        IEnumerable Filter(IEnumerable modelObjects);
    }

    /// <summary>
    /// Base class for model-by-model filters
    /// </summary>
    public class AbstractModelFilter : IModelFilter
    {
        /// <summary>
        /// Should the given model be included when this filter is installed
        /// </summary>
        /// <param name="modelObject">The model object to consider</param>
        /// <returns>Returns true if the model will be included by the filter</returns>
        virtual public bool Filter(object modelObject) {
            return true;
        }
    }

    /// <summary>
    /// This filter calls a given Predicate to decide if a model object should be included
    /// </summary>
    public class ModelFilter : IModelFilter
    {
        /// <summary>
        /// Create a filter based on the given predicate
        /// </summary>
        /// <param name="predicate">The function that will filter objects</param>
        public ModelFilter(Predicate<object> predicate) {
            this.Predicate = predicate;
        }

        /// <summary>
        /// Gets or sets the predicate used to filter model objects
        /// </summary>
        protected Predicate<object> Predicate {
            get { return predicate; }
            set { predicate = value; }
        }
        private Predicate<object> predicate;

        /// <summary>
        /// Should the given model object be included?
        /// </summary>
        /// <param name="modelObject"></param>
        /// <returns></returns>
        virtual public bool Filter(object modelObject) {
            return this.Predicate == null ? true : this.Predicate(modelObject);
        }
    }

    /// <summary>
    /// Base class for whole list filters
    /// </summary>
    public class AbstractListFilter : IListFilter
    {
        /// <summary>
        /// Return a subset of the given list of model objects as the new
        /// contents of the ObjectListView
        /// </summary>
        /// <param name="modelObjects">The collection of model objects that the list will possibly display</param>
        /// <returns>The filtered collection that holds the model objects that will be displayed.</returns>
        virtual public IEnumerable Filter(IEnumerable modelObjects) {
            return modelObjects;
        }

        /// <summary>
        /// Return the given enumerable as a list, creating a new
        /// collection only if necessary.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        protected IList EnumerableToList(IEnumerable enumerable) {
            IList list = enumerable as IList;
            if (list == null) {
                list = new ArrayList();
                foreach (object x in enumerable)
                    list.Add(x);
            }
            return list;
        }
    }

    /// <summary>
    /// Instance of this class implement delegate based whole list filtering
    /// </summary>
    public class ListFilter : AbstractListFilter
    {
        /// <summary>
        /// A delegate that filters on a whole list
        /// </summary>
        /// <param name="rowObjects"></param>
        /// <returns></returns>
        public delegate IEnumerable ListFilterDelegate(IEnumerable rowObjects);

        /// <summary>
        /// Create a ListFilter
        /// </summary>
        /// <param name="function"></param>
        public ListFilter(ListFilterDelegate function) {
            this.Function = function;
        }

        /// <summary>
        /// Gets or sets the delegate that will filter the list
        /// </summary>
        public ListFilterDelegate Function {
            get { return function; }
            set { function = value; }
        }
        private ListFilterDelegate function;

        /// <summary>
        /// Do the actual work of filtering
        /// </summary>
        /// <param name="modelObjects"></param>
        /// <returns></returns>
        public override IEnumerable Filter(IEnumerable modelObjects) {
            if (this.Function == null)
                return modelObjects;

            return this.Function(modelObjects);
        }
    }

    /// <summary>
    /// Filter the list so only the last N entries are displayed
    /// </summary>
    public class TailFilter : AbstractListFilter
    {
        /// <summary>
        /// Create a no-op tail filter
        /// </summary>
        public TailFilter() {
        }

        /// <summary>
        /// Create a filter that includes on the last N model objects
        /// </summary>
        /// <param name="numberOfObjects"></param>
        public TailFilter(int numberOfObjects) {
            this.Count = numberOfObjects;
        }

        /// <summary>
        /// Gets or sets the number of model objects that will be 
        /// returned from the tail of the list
        /// </summary>
        public int Count;

        /// <summary>
        /// Return the last N subset of the model objects
        /// </summary>
        /// <param name="modelObjects"></param>
        /// <returns></returns>
        public override IEnumerable Filter(IEnumerable modelObjects) {
            if (this.Count <= 0)
                return modelObjects;

            ArrayList list = ArrayList.Adapter(this.EnumerableToList(modelObjects));

            if (this.Count > list.Count)
                return list;

            object[] tail = new object[this.Count];
            list.CopyTo(list.Count - this.Count, tail, 0, this.Count);
            return new ArrayList(tail);
        }
    }

    /// <summary>
    /// Instances of this class include only those rows of the listview
    /// that contain a given string.
    /// </summary>
    public class TextMatchFilter : AbstractModelFilter
    {
        /// <summary>
        /// What sort of matching should the text filter use?
        /// </summary>
        public enum MatchKind
        {
            /// <summary>
            /// Match any text in the cell
            /// </summary>
            Text = 0,

            /// <summary>
            /// Match any cell that starts with the given text
            /// </summary>
            StringStart = 2,

            /// <summary>
            /// Match any cell that matches the given regex
            /// </summary>
            Regex = 4
        }

        #region Life and death

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        public TextMatchFilter() {
        }

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        /// <param name="olv"></param>
        public TextMatchFilter(ObjectListView olv) 
            : this(olv, null, null) {
        }

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="text"></param>
        public TextMatchFilter(ObjectListView olv, string text)
            : this(olv, text, null) {
        }

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="text"></param>
        /// <param name="comparison"></param>
        public TextMatchFilter(ObjectListView olv, string text, StringComparison comparison)
            : this(olv, text, null, MatchKind.Text, comparison) {
        }

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="text"></param>
        /// <param name="match"></param>
        public TextMatchFilter(ObjectListView olv, string text, MatchKind match)
            : this(olv, text, null, match, StringComparison.InvariantCultureIgnoreCase) {
        }

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="text"></param>
        /// <param name="match"></param>
        /// <param name="comparison"></param>
        public TextMatchFilter(ObjectListView olv, string text, MatchKind match, StringComparison comparison)
            : this(olv, text, null, match, comparison) {
        }

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="text"></param>
        /// <param name="columns"></param>
        public TextMatchFilter(ObjectListView olv, string text, OLVColumn[] columns)
            : this(olv, text, columns, MatchKind.Text, StringComparison.InvariantCultureIgnoreCase) {
        }

        /// <summary>
        /// Create a TextFilter
        /// </summary>
        /// <param name="olv"></param>
        /// <param name="text"></param>
        /// <param name="columns"></param>
        /// <param name="matchKind"></param>
        /// <param name="comparison"></param>
        public TextMatchFilter(ObjectListView olv, string text, OLVColumn[] columns, MatchKind matchKind, StringComparison comparison) {
            this.ListView = olv;
            this.Text = text;
            this.Match = matchKind;
            this.StringComparison = comparison;
            this.Columns = columns;
        }

        #endregion

        #region Configuration properties

        /// <summary>
        /// Which columns will be used for the comparisons? If this is null, all columns will be used
        /// </summary>
        public OLVColumn[] Columns {
            get { return columns; }
            set { columns = value; }
        }
        private OLVColumn[] columns;

		/// <summary>
		/// Gets or set the ObjectListView upon which this filter will work
		/// </summary>
		/// <remarks>
		/// You cannot really rebase a filter after it is created, so do not change this value.
		/// It is included so that it can be set in an object initializer.
		/// </remarks>
        public ObjectListView ListView {
			get { return listView; }
			set { listView = value; }
		}
        private ObjectListView listView;
		
        /// <summary>
        /// Gets or sets how the filter string will be matched to the values in the cells.
        /// </summary>
        public MatchKind Match {
            get { return match; }
            set { 
                match = value;
                this.Regex = null;
            }
        }
        private MatchKind match;

        /// <summary>
        /// Gets or sets the options that will be used when compiling the regular expression.
        /// This is only used when MatchKind is Regex.
        /// If this is not set specifically, the appropriate options are chosen to match the
        /// StringComparison setting (culture invariant, case sensitive).
        /// </summary>
        public RegexOptions RegexOptions {
            get {
                if (!regexOptions.HasValue) {
                    switch (this.StringComparison) {
                        case StringComparison.CurrentCulture:
                            regexOptions = RegexOptions.None;
                            break;
                        case StringComparison.CurrentCultureIgnoreCase:
                            regexOptions = RegexOptions.IgnoreCase;
                            break;
                        case StringComparison.InvariantCulture:
                            regexOptions = RegexOptions.CultureInvariant;
                            break;
                        case StringComparison.InvariantCultureIgnoreCase:
                            regexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
                            break;
                        default:
                            regexOptions = RegexOptions.None;
                            break;
                    }
                }
                return regexOptions.Value; 
            }
            set { 
                regexOptions = value;
                this.Regex = null;
            }
        }
        private RegexOptions? regexOptions;

        /// <summary>
        /// Gets or  sets how the filter will match text
        /// </summary>
        public StringComparison StringComparison {
            get { return this.stringComparison; }
            set {
                this.stringComparison = value;
                this.Regex = null;
            }
        }
        private StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase;

        /// <summary>
        /// Gets or sets the text that will be matched
        /// </summary>
        /// <remarks>
        /// If the filter is set to Regex, but Text is not a valid regular expression,
        /// the filter will NOT throw an exception. Instead, it will simply match everything.
		/// </remarks>
        public string Text {
            get { return this.text; }
            set {
                this.text = value;
                this.Regex = null;
            }
        }
        private string text;

        #endregion

        #region Implementation properties

        /// <summary>
        /// Gets or sets a compilex regular expression, based on our current Text and RegexOptions.
        /// </summary>
        /// <remarks>
        /// If Text fails to compile as a regular expression, this will return a Regex object
        /// that will match all strings.
        /// </remarks>
        protected Regex Regex {
            get {
                if (this.regex == null && this.Match == MatchKind.Regex) {
                    try {
                        this.regex = new Regex(this.Text, this.RegexOptions);
                    } catch (ArgumentException) {
                        this.regex = TextMatchFilter.InvalidRegexMarker;
                    }
                }
                return this.regex;
            }
            set {
                this.regex = value;
            }
        }
        private Regex regex;

        /// <summary>
        /// Gets whether or not our current regular expression is a valid regex
        /// </summary>
        protected bool IsRegexInvalid {
            get {
                return this.Regex == TextMatchFilter.InvalidRegexMarker;
            }
        }
        static private Regex InvalidRegexMarker = new Regex(".*");

        #endregion

        #region Implementation

        /// <summary>
        /// Do the actual work of filtering
        /// </summary>
        /// <param name="modelObject"></param>
        /// <returns></returns>
        public override bool Filter(object modelObject) {
            if (this.ListView == null || String.IsNullOrEmpty(this.Text))
                return true;

            // Don't do anything if the Regex is invalid
            if (this.Match == MatchKind.Regex && this.IsRegexInvalid)
                return true;

            foreach (OLVColumn column in this.IterateColumns()) {
                if (column.IsVisible) {
                    string cellText = column.GetStringValue(modelObject);
                    if (this.MatchesText(cellText))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the given text match the filter
        /// </summary>
        /// <param name="cellText"></param>
        /// <returns></returns>
        protected bool MatchesText(string cellText) {
            switch (this.Match) {
                case MatchKind.Text:
                    return (cellText.IndexOf(this.Text, this.StringComparison) != -1);
                case MatchKind.StringStart:
                    return (cellText.StartsWith(this.Text, this.StringComparison));
                case MatchKind.Regex:
                    return (this.Regex.Match(cellText).Success);
            }
            // Should never reach here
            return false;
        }

        /// <summary>
        /// Find all the ways in which this filter matches the given string.
        /// </summary>
        /// <remarks>This is used by the renderer to decide which bits of
        /// the string should be highlighted</remarks>
        /// <param name="cellText"></param>
        /// <returns>A list of character ranges indicating the matched substrings</returns>
        public IEnumerable<CharacterRange> FindAllMatchedRanges(string cellText) {
            List<CharacterRange> ranges = new List<CharacterRange>();

            switch (this.Match) {
                case MatchKind.Text:
                    int matchIndex = cellText.IndexOf(this.Text, this.StringComparison);
                    while (matchIndex != -1) {
                        ranges.Add(new CharacterRange(matchIndex, this.Text.Length));
                        matchIndex = cellText.IndexOf(this.Text, matchIndex + this.Text.Length, this.StringComparison);
                    }
                    break;
                case MatchKind.StringStart:
                    if (cellText.StartsWith(this.Text, this.StringComparison))
                        ranges.Add(new CharacterRange(0, this.Text.Length));
                    break;
                case MatchKind.Regex:
                    if (!this.IsRegexInvalid) {
                        foreach (Match match in this.Regex.Matches(cellText)) {
                            if (match.Length > 0)
                                ranges.Add(new CharacterRange(match.Index, match.Length));
                        }
                    }
                    break;
            }

            return ranges;
        }

        /// <summary>
        /// Loop over the columns that are being considering by the filter
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<OLVColumn> IterateColumns() {
            if (this.Columns == null) {
                foreach (OLVColumn column in this.ListView.Columns)
                    yield return column;
            } else {
                foreach (OLVColumn column in this.Columns)
                    yield return column;
            }
        }

        /// <summary>
        /// Is the given column one of the columns being used by this filter?
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal bool IsIncluded(OLVColumn column) {
            if (this.Columns == null) {
                return column.ListView == this.ListView;
            }

            foreach (OLVColumn x in this.Columns) {
                if (x == column)
                    return true;
            }

            return false;
        }


        #endregion
    }
}