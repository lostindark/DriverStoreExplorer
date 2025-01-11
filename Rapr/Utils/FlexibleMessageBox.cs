using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace JR.Utils.GUI.Forms
{
    /*  FlexibleMessageBox – A flexible replacement for the .NET MessageBox
     * 
     *  Author:         Jörg Reichert (public@jreichert.de)
     *  Contributors:   Thanks to: David Hall, Roink
     *  Version:        1.3
     *  Published at:   http://www.codeproject.com/Articles/601900/FlexibleMessageBox
     *  
     ************************************************************************************************************
     * Features:
     *  - It can be simply used instead of MessageBox since all important static "Show"-Functions are supported
     *  - It is small, only one source file, which could be added easily to each solution 
     *  - It can be resized and the content is correctly word-wrapped
     *  - It tries to auto-size the width to show the longest text row
     *  - It never exceeds the current desktop working area
     *  - It displays a vertical scrollbar when needed
     *  - It does support hyperlinks in text
     * 
     *  Because the interface is identical to MessageBox, you can add this single source file to your project 
     *  and use the FlexibleMessageBox almost everywhere you use a standard MessageBox. 
     *  The goal was NOT to produce as many features as possible but to provide a simple replacement to fit my 
     *  own needs. Feel free to add additional features on your own, but please left my credits in this class.
     * 
     ************************************************************************************************************
     * Usage examples:
     * 
     *  FlexibleMessageBox.Show("Just a text");
     * 
     *  FlexibleMessageBox.Show("A text", 
     *                          "A caption"); 
     *  
     *  FlexibleMessageBox.Show("Some text with a link: www.google.com", 
     *                          "Some caption",
     *                          MessageBoxButtons.AbortRetryIgnore, 
     *                          MessageBoxIcon.Information,
     *                          MessageBoxDefaultButton.Button2);
     *  
     *  var dialogResult = FlexibleMessageBox.Show("Do you know the answer to life the universe and everything?", 
     *                                             "One short question",
     *                                             MessageBoxButtons.YesNo);     
     * 
     ************************************************************************************************************
     *  THE SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS", WITHOUT WARRANTY
     *  OF ANY KIND, EXPRESS OR IMPLIED. IN NO EVENT SHALL THE AUTHOR BE
     *  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY ARISING FROM,
     *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OF THIS
     *  SOFTWARE.
     *  
     ************************************************************************************************************
     * History:
     *  Version 1.3 - 19.Dezember 2014
     *  - Added refactoring function GetButtonText()
     *  - Used CurrentUICulture instead of InstalledUICulture
     *  - Added more button localizations. Supported languages are now: ENGLISH, GERMAN, SPANISH, ITALIAN
     *  - Added standard MessageBox handling for "copy to clipboard" with <Ctrl> + <C> and <Ctrl> + <Insert>
     *  - Tab handling is now corrected (only tabbing over the visible buttons)
     *  - Added standard MessageBox handling for ALT-Keyboard shortcuts
     *  - SetDialogSizes: Refactored completely: Corrected sizing and added caption driven sizing
     * 
     *  Version 1.2 - 10.August 2013
     *   - Do not ShowInTaskbar anymore (original MessageBox is also hidden in taskbar)
     *   - Added handling for Escape-Button
     *   - Adapted top right close button (red X) to behave like MessageBox (but hidden instead of deactivated)
     * 
     *  Version 1.1 - 14.June 2013
     *   - Some Refactoring
     *   - Added internal form class
     *   - Added missing code comments, etc.
     *  
     *  Version 1.0 - 15.April 2013
     *   - Initial Version
    */
    public class FlexibleMessageBox
    {
        #region Public statics

        /// <summary>
        /// Defines the maximum width for all FlexibleMessageBox instances in percent of the working area.
        /// 
        /// Allowed values are 0.2 - 1.0 where: 
        /// 0.2 means:  The FlexibleMessageBox can be at most half as wide as the working area.
        /// 1.0 means:  The FlexibleMessageBox can be as wide as the working area.
        /// 
        /// Default is: 70% of the working area width.
        /// </summary>
        public static double MAX_WIDTH_FACTOR = 0.7;

        /// <summary>
        /// Defines the maximum height for all FlexibleMessageBox instances in percent of the working area.
        /// 
        /// Allowed values are 0.2 - 1.0 where: 
        /// 0.2 means:  The FlexibleMessageBox can be at most half as high as the working area.
        /// 1.0 means:  The FlexibleMessageBox can be as high as the working area.
        /// 
        /// Default is: 90% of the working area height.
        /// </summary>
        public static double MAX_HEIGHT_FACTOR = 0.9;

        /// <summary>
        /// Defines the font for all FlexibleMessageBox instances.
        /// 
        /// Default is: SystemFonts.MessageBoxFont
        /// </summary>
        public static Font FONT = SystemFonts.MessageBoxFont;

        #endregion

        #region Public show functions

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(string text)
        {
            return FlexibleMessageBoxForm.Show(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(IWin32Window owner, string text)
        {
            return FlexibleMessageBoxForm.Show(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(string text, string caption)
        {
            return FlexibleMessageBoxForm.Show(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(IWin32Window owner, string text, string caption)
        {
            return FlexibleMessageBoxForm.Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="buttons">The buttons.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            return FlexibleMessageBoxForm.Show(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="buttons">The buttons.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
        {
            return FlexibleMessageBoxForm.Show(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="icon">The icon.</param>
        /// <returns></returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return FlexibleMessageBoxForm.Show(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="icon">The icon.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return FlexibleMessageBoxForm.Show(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="defaultButton">The default button.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return FlexibleMessageBoxForm.Show(null, text, caption, buttons, icon, defaultButton);
        }

        /// <summary>
        /// Shows the specified message box.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="defaultButton">The default button.</param>
        /// <returns>The dialog result.</returns>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return FlexibleMessageBoxForm.Show(owner, text, caption, buttons, icon, defaultButton);
        }

        #endregion

        #region Internal form class

        /// <summary>
        /// The form to show the customized message box.
        /// It is defined as an internal class to keep the public interface of the FlexibleMessageBox clean.
        /// </summary>
        class FlexibleMessageBoxForm : Form
        {
            #region Form-Designer generated code

            /// <summary>
            /// Erforderliche Designervariable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary>
            /// Verwendete Ressourcen bereinigen.
            /// </summary>
            /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            /// <summary>
            /// Erforderliche Methode für die Designerunterstützung.
            /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
            /// </summary>
            private void InitializeComponent()
            {
                bool rightToLeft = Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft;
                this.RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
                this.RightToLeftLayout = rightToLeft;

                this.components = new System.ComponentModel.Container();
                this.button1 = new System.Windows.Forms.Button();
                this.richTextBoxMessage = new System.Windows.Forms.RichTextBox();
                this.FlexibleMessageBoxFormBindingSource = new System.Windows.Forms.BindingSource(this.components);
                this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
                this.pictureBoxForIcon = new System.Windows.Forms.PictureBox();
                this.button2 = new System.Windows.Forms.Button();
                this.button3 = new System.Windows.Forms.Button();
                ((System.ComponentModel.ISupportInitialize)(this.FlexibleMessageBoxFormBindingSource)).BeginInit();
                this.tableLayoutPanel1.SuspendLayout();
                ((System.ComponentModel.ISupportInitialize)(this.pictureBoxForIcon)).BeginInit();
                this.SuspendLayout();
                // 
                // button1
                // 
                this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
                this.button1.AutoSize = true;
                this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.button1.Location = new System.Drawing.Point(11, 67);
                this.button1.MinimumSize = new System.Drawing.Size(0, 24);
                this.button1.Name = "button1";
                this.button1.Size = new System.Drawing.Size(75, 24);
                this.button1.TabIndex = 2;
                this.button1.Text = "OK";
                this.button1.UseVisualStyleBackColor = true;
                this.button1.Visible = false;
                // 
                // richTextBoxMessage
                // 
                this.richTextBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.richTextBoxMessage.BackColor = System.Drawing.Color.White;
                this.richTextBoxMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
                this.richTextBoxMessage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.FlexibleMessageBoxFormBindingSource, "MessageText", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
                this.richTextBoxMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.richTextBoxMessage.Location = new System.Drawing.Point(50, 26);
                this.richTextBoxMessage.Margin = new System.Windows.Forms.Padding(4);
                this.richTextBoxMessage.Name = "richTextBoxMessage";
                this.richTextBoxMessage.ReadOnly = true;
                this.richTextBoxMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
                this.richTextBoxMessage.Size = new System.Drawing.Size(200, 20);
                this.richTextBoxMessage.TabIndex = 0;
                this.richTextBoxMessage.TabStop = false;
                this.richTextBoxMessage.Text = "<Message>";
                this.richTextBoxMessage.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBoxMessage_LinkClicked);
                // 
                // tableLayoutPanel1
                // 
                this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
                this.tableLayoutPanel1.AutoSize = true;
                this.tableLayoutPanel1.ColumnCount = 2;
                this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
                this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
                this.tableLayoutPanel1.Controls.Add(this.pictureBoxForIcon, 0, 0);
                this.tableLayoutPanel1.Controls.Add(this.richTextBoxMessage, 1, 0);
                this.tableLayoutPanel1.Padding = new Padding(4);
                this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
                this.tableLayoutPanel1.Name = "tableLayoutPanel1";
                this.tableLayoutPanel1.RowCount = 1;
                this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
                this.tableLayoutPanel1.Size = new System.Drawing.Size(268, 59);
                this.tableLayoutPanel1.TabIndex = 1;
                // 
                // pictureBoxForIcon
                // 
                this.pictureBoxForIcon.BackColor = System.Drawing.Color.Transparent;
                this.pictureBoxForIcon.Location = new System.Drawing.Point(15, 19);
                this.pictureBoxForIcon.Name = "pictureBoxForIcon";
                this.pictureBoxForIcon.Size = new System.Drawing.Size(32, 32);
                this.pictureBoxForIcon.TabIndex = 8;
                this.pictureBoxForIcon.TabStop = false;
                // 
                // button2
                // 
                this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
                this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.button2.Location = new System.Drawing.Point(92, 67);
                this.button2.MinimumSize = new System.Drawing.Size(0, 24);
                this.button2.Name = "button2";
                this.button2.Size = new System.Drawing.Size(75, 24);
                this.button2.TabIndex = 3;
                this.button2.Text = "OK";
                this.button2.UseVisualStyleBackColor = true;
                this.button2.Visible = false;
                // 
                // button3
                // 
                this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
                this.button3.AutoSize = true;
                this.button3.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.button3.Location = new System.Drawing.Point(173, 67);
                this.button3.MinimumSize = new System.Drawing.Size(0, 24);
                this.button3.Name = "button3";
                this.button3.Size = new System.Drawing.Size(75, 24);
                this.button3.TabIndex = 0;
                this.button3.Text = "OK";
                this.button3.UseVisualStyleBackColor = true;
                this.button3.Visible = false;
                // 
                // FlexibleMessageBoxForm
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(260, 102);
                this.Controls.Add(this.button3);
                this.Controls.Add(this.button2);
                //this.Controls.Add(this.panel1);
                this.Controls.Add(this.tableLayoutPanel1);
                this.Controls.Add(this.button1);
                this.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.FlexibleMessageBoxFormBindingSource, "CaptionText", true));
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.MinimumSize = new System.Drawing.Size(276, 140);
                this.Name = "FlexibleMessageBoxForm";
                this.ShowIcon = false;
                this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
                this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                this.Text = "<Caption>";
                this.Shown += new System.EventHandler(this.FlexibleMessageBoxForm_Shown);
                ((System.ComponentModel.ISupportInitialize)(this.FlexibleMessageBoxFormBindingSource)).EndInit();
                this.tableLayoutPanel1.ResumeLayout(false);
                this.tableLayoutPanel1.PerformLayout();
                ((System.ComponentModel.ISupportInitialize)(this.pictureBoxForIcon)).EndInit();
                this.ResumeLayout(false);
                this.PerformLayout();
            }

            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.BindingSource FlexibleMessageBoxFormBindingSource;
            private System.Windows.Forms.RichTextBox richTextBoxMessage;
            private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            private System.Windows.Forms.PictureBox pictureBoxForIcon;
            private System.Windows.Forms.Button button2;
            private System.Windows.Forms.Button button3;

            #endregion

            #region Private constants

            //These separators are used for the "copy to clipboard" standard operation, triggered by Ctrl + C (behavior and clipboard format is like in a standard MessageBox)
            private static readonly String STANDARD_MESSAGEBOX_SEPARATOR_LINES = "---------------------------\n";
            private static readonly String STANDARD_MESSAGEBOX_SEPARATOR_SPACES = "   ";

            //These are the possible buttons (in a standard MessageBox)
            private enum ButtonID { OK = 0, CANCEL, YES, NO, ABORT, RETRY, IGNORE };

            //These are the buttons texts for different languages. 
            //If you want to add a new language, add it here and in the GetButtonText-Function
            private enum TwoLetterISOLanguageID { en, de, es, it };
            private static readonly String[] BUTTON_TEXTS_ENGLISH_EN = { "OK", "Cancel", "&Yes", "&No", "&Abort", "&Retry", "&Ignore" }; //Note: This is also the fallback language
            private static readonly String[] BUTTON_TEXTS_GERMAN_DE = { "OK", "Abbrechen", "&Ja", "&Nein", "&Abbrechen", "&Wiederholen", "&Ignorieren" };
            private static readonly String[] BUTTON_TEXTS_SPANISH_ES = { "Aceptar", "Cancelar", "&Sí", "&No", "&Abortar", "&Reintentar", "&Ignorar" };
            private static readonly String[] BUTTON_TEXTS_ITALIAN_IT = { "OK", "Annulla", "&Sì", "&No", "&Interrompi", "&Riprova", "&Ignora" };

            #endregion

            #region Private members

            private MessageBoxDefaultButton defaultButton;
            private int visibleButtonsCount;
            private TwoLetterISOLanguageID languageID = TwoLetterISOLanguageID.en;

            #endregion

            #region Private constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="FlexibleMessageBoxForm"/> class.
            /// </summary>
            private FlexibleMessageBoxForm()
            {
                InitializeComponent();

                //Try to evaluate the language. If this fails, the fallback language English will be used
                Enum.TryParse<TwoLetterISOLanguageID>(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, out this.languageID);

                this.KeyPreview = true;
                this.KeyUp += FlexibleMessageBoxForm_KeyUp;
            }

            #endregion

            #region Private helper functions

            /// <summary>
            /// Gets the string rows.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <returns>The string rows as 1-dimensional array</returns>
            private static string[] GetStringRows(string message)
            {
                if (string.IsNullOrEmpty(message)) return null;

                var messageRows = message.Split(new char[] { '\n' }, StringSplitOptions.None);
                return messageRows;
            }

            /// <summary>
            /// Gets the button text for the CurrentUICulture language.
            /// Note: The fallback language is English
            /// </summary>
            /// <param name="buttonID">The ID of the button.</param>
            /// <returns>The button text</returns>
            private string GetButtonText(ButtonID buttonID)
            {
                var buttonTextArrayIndex = Convert.ToInt32(buttonID);

                switch (this.languageID)
                {
                    case TwoLetterISOLanguageID.de: return BUTTON_TEXTS_GERMAN_DE[buttonTextArrayIndex];
                    case TwoLetterISOLanguageID.es: return BUTTON_TEXTS_SPANISH_ES[buttonTextArrayIndex];
                    case TwoLetterISOLanguageID.it: return BUTTON_TEXTS_ITALIAN_IT[buttonTextArrayIndex];

                    default: return BUTTON_TEXTS_ENGLISH_EN[buttonTextArrayIndex];
                }
            }

            /// <summary>
            /// Ensure the given working area factor in the range of  0.2 - 1.0 where: 
            /// 
            /// 0.2 means:  20 percent of the working area height or width.
            /// 1.0 means:  100 percent of the working area height or width.
            /// </summary>
            /// <param name="workingAreaFactor">The given working area factor.</param>
            /// <returns>The corrected given working area factor.</returns>
            private static double GetCorrectedWorkingAreaFactor(double workingAreaFactor)
            {
                const double MIN_FACTOR = 0.2;
                const double MAX_FACTOR = 1.0;

                if (workingAreaFactor < MIN_FACTOR) return MIN_FACTOR;
                if (workingAreaFactor > MAX_FACTOR) return MAX_FACTOR;

                return workingAreaFactor;
            }

            /// <summary>
            /// Set the dialogs start position when given. 
            /// Otherwise center the dialog on the current screen.
            /// </summary>
            /// <param name="flexibleMessageBoxForm">The FlexibleMessageBox dialog.</param>
            /// <param name="owner">The owner.</param>
            private static void SetDialogStartPosition(FlexibleMessageBoxForm flexibleMessageBoxForm, IWin32Window owner)
            {
                //If no owner given: Center on current screen
                if (owner == null)
                {
                    var screen = Screen.FromPoint(Cursor.Position);
                    flexibleMessageBoxForm.StartPosition = FormStartPosition.Manual;
                    flexibleMessageBoxForm.Left = screen.Bounds.Left + screen.Bounds.Width / 2 - flexibleMessageBoxForm.Width / 2;
                    flexibleMessageBoxForm.Top = screen.Bounds.Top + screen.Bounds.Height / 2 - flexibleMessageBoxForm.Height / 2;
                }
            }

            /// <summary>
            /// Calculate the dialogs start size (Try to auto-size width to show longest text row).
            /// Also set the maximum dialog size. 
            /// </summary>
            /// <param name="flexibleMessageBoxForm">The FlexibleMessageBox dialog.</param>
            /// <param name="text">The text (the longest text row is used to calculate the dialog width).</param>
            /// <param name="text">The caption (this can also affect the dialog width).</param>
            private static void SetDialogSizes(FlexibleMessageBoxForm flexibleMessageBoxForm, string text, string caption)
            {
                //First set the bounds for the maximum dialog size
                flexibleMessageBoxForm.MaximumSize = new Size(
                    Convert.ToInt32(SystemInformation.WorkingArea.Width * FlexibleMessageBoxForm.GetCorrectedWorkingAreaFactor(MAX_WIDTH_FACTOR)),
                    Convert.ToInt32(SystemInformation.WorkingArea.Height * FlexibleMessageBoxForm.GetCorrectedWorkingAreaFactor(MAX_HEIGHT_FACTOR)));

                //Get rows. Exit if there are no rows to render...
                var stringRows = GetStringRows(text);
                if (stringRows == null) return;

                //Calculate whole text height
                var textHeight = TextRenderer.MeasureText(text, FONT).Height;

                //Calculate width for longest text line
                const int SCROLLBAR_WIDTH_OFFSET = 15;
                var longestTextRowWidth = stringRows.Max(textForRow => TextRenderer.MeasureText(textForRow, FONT).Width);
                var captionWidth = TextRenderer.MeasureText(caption, SystemFonts.CaptionFont).Width;
                var textWidth = Math.Max(longestTextRowWidth + SCROLLBAR_WIDTH_OFFSET, captionWidth);

                //Calculate margins
                var marginWidth = flexibleMessageBoxForm.Width - flexibleMessageBoxForm.richTextBoxMessage.Width;
                var marginHeight = flexibleMessageBoxForm.Height - flexibleMessageBoxForm.richTextBoxMessage.Height;

                //Set calculated dialog size (if the calculated values exceed the maximums, they were cut by windows forms automatically)
                flexibleMessageBoxForm.Size = new Size(textWidth + marginWidth, textHeight + marginHeight);
            }

            /// <summary>
            /// Set the dialogs icon. 
            /// When no icon is used: Correct placement and width of rich text box.
            /// </summary>
            /// <param name="flexibleMessageBoxForm">The FlexibleMessageBox dialog.</param>
            /// <param name="icon">The MessageBoxIcon.</param>
            private static void SetDialogIcon(FlexibleMessageBoxForm flexibleMessageBoxForm, MessageBoxIcon icon)
            {
                switch (icon)
                {
                    case MessageBoxIcon.Information:
                        flexibleMessageBoxForm.pictureBoxForIcon.Image = SystemIcons.Information.ToBitmap();
                        break;
                    case MessageBoxIcon.Warning:
                        flexibleMessageBoxForm.pictureBoxForIcon.Image = SystemIcons.Warning.ToBitmap();
                        break;
                    case MessageBoxIcon.Error:
                        flexibleMessageBoxForm.pictureBoxForIcon.Image = SystemIcons.Error.ToBitmap();
                        break;
                    case MessageBoxIcon.Question:
                        flexibleMessageBoxForm.pictureBoxForIcon.Image = SystemIcons.Question.ToBitmap();
                        break;
                    default:
                        //When no icon is used: Correct placement and width of rich text box.
                        flexibleMessageBoxForm.pictureBoxForIcon.Visible = false;
                        flexibleMessageBoxForm.richTextBoxMessage.Left -= flexibleMessageBoxForm.pictureBoxForIcon.Width;
                        flexibleMessageBoxForm.richTextBoxMessage.Width += flexibleMessageBoxForm.pictureBoxForIcon.Width;
                        break;
                }
            }

            /// <summary>
            /// Set dialog buttons visibilities and texts. 
            /// Also set a default button.
            /// </summary>
            /// <param name="flexibleMessageBoxForm">The FlexibleMessageBox dialog.</param>
            /// <param name="buttons">The buttons.</param>
            /// <param name="defaultButton">The default button.</param>
            private static void SetDialogButtons(FlexibleMessageBoxForm flexibleMessageBoxForm, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
            {
                //Set the buttons visibilities and texts
                switch (buttons)
                {
                    case MessageBoxButtons.AbortRetryIgnore:
                        flexibleMessageBoxForm.visibleButtonsCount = 3;

                        flexibleMessageBoxForm.button1.Visible = true;
                        flexibleMessageBoxForm.button1.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.ABORT);
                        flexibleMessageBoxForm.button1.DialogResult = DialogResult.Abort;

                        flexibleMessageBoxForm.button2.Visible = true;
                        flexibleMessageBoxForm.button2.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.RETRY);
                        flexibleMessageBoxForm.button2.DialogResult = DialogResult.Retry;

                        flexibleMessageBoxForm.button3.Visible = true;
                        flexibleMessageBoxForm.button3.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.IGNORE);
                        flexibleMessageBoxForm.button3.DialogResult = DialogResult.Ignore;

                        flexibleMessageBoxForm.ControlBox = false;
                        break;

                    case MessageBoxButtons.OKCancel:
                        flexibleMessageBoxForm.visibleButtonsCount = 2;

                        flexibleMessageBoxForm.button2.Visible = true;
                        flexibleMessageBoxForm.button2.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.OK);
                        flexibleMessageBoxForm.button2.DialogResult = DialogResult.OK;

                        flexibleMessageBoxForm.button3.Visible = true;
                        flexibleMessageBoxForm.button3.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.CANCEL);
                        flexibleMessageBoxForm.button3.DialogResult = DialogResult.Cancel;

                        flexibleMessageBoxForm.CancelButton = flexibleMessageBoxForm.button3;
                        break;

                    case MessageBoxButtons.RetryCancel:
                        flexibleMessageBoxForm.visibleButtonsCount = 2;

                        flexibleMessageBoxForm.button2.Visible = true;
                        flexibleMessageBoxForm.button2.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.RETRY);
                        flexibleMessageBoxForm.button2.DialogResult = DialogResult.Retry;

                        flexibleMessageBoxForm.button3.Visible = true;
                        flexibleMessageBoxForm.button3.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.CANCEL);
                        flexibleMessageBoxForm.button3.DialogResult = DialogResult.Cancel;

                        flexibleMessageBoxForm.CancelButton = flexibleMessageBoxForm.button3;
                        break;

                    case MessageBoxButtons.YesNo:
                        flexibleMessageBoxForm.visibleButtonsCount = 2;

                        flexibleMessageBoxForm.button2.Visible = true;
                        flexibleMessageBoxForm.button2.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.YES);
                        flexibleMessageBoxForm.button2.DialogResult = DialogResult.Yes;

                        flexibleMessageBoxForm.button3.Visible = true;
                        flexibleMessageBoxForm.button3.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.NO);
                        flexibleMessageBoxForm.button3.DialogResult = DialogResult.No;

                        flexibleMessageBoxForm.ControlBox = false;
                        break;

                    case MessageBoxButtons.YesNoCancel:
                        flexibleMessageBoxForm.visibleButtonsCount = 3;

                        flexibleMessageBoxForm.button1.Visible = true;
                        flexibleMessageBoxForm.button1.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.YES);
                        flexibleMessageBoxForm.button1.DialogResult = DialogResult.Yes;

                        flexibleMessageBoxForm.button2.Visible = true;
                        flexibleMessageBoxForm.button2.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.NO);
                        flexibleMessageBoxForm.button2.DialogResult = DialogResult.No;

                        flexibleMessageBoxForm.button3.Visible = true;
                        flexibleMessageBoxForm.button3.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.CANCEL);
                        flexibleMessageBoxForm.button3.DialogResult = DialogResult.Cancel;

                        flexibleMessageBoxForm.CancelButton = flexibleMessageBoxForm.button3;
                        break;

                    case MessageBoxButtons.OK:
                    default:
                        flexibleMessageBoxForm.visibleButtonsCount = 1;
                        flexibleMessageBoxForm.button3.Visible = true;
                        flexibleMessageBoxForm.button3.Text = flexibleMessageBoxForm.GetButtonText(ButtonID.OK);
                        flexibleMessageBoxForm.button3.DialogResult = DialogResult.OK;

                        flexibleMessageBoxForm.CancelButton = flexibleMessageBoxForm.button3;
                        break;
                }

                //Set default button (used in FlexibleMessageBoxForm_Shown)
                flexibleMessageBoxForm.defaultButton = defaultButton;
            }

            #endregion

            #region Private event handlers

            /// <summary>
            /// Handles the Shown event of the FlexibleMessageBoxForm control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            private void FlexibleMessageBoxForm_Shown(object sender, EventArgs e)
            {
                int buttonIndexToFocus = 1;
                Button buttonToFocus;

                //Set the default button...
                switch (this.defaultButton)
                {
                    case MessageBoxDefaultButton.Button1:
                    default:
                        buttonIndexToFocus = 1;
                        break;
                    case MessageBoxDefaultButton.Button2:
                        buttonIndexToFocus = 2;
                        break;
                    case MessageBoxDefaultButton.Button3:
                        buttonIndexToFocus = 3;
                        break;
                }

                if (buttonIndexToFocus > this.visibleButtonsCount) buttonIndexToFocus = this.visibleButtonsCount;

                if (buttonIndexToFocus == 3)
                {
                    buttonToFocus = this.button3;
                }
                else if (buttonIndexToFocus == 2)
                {
                    buttonToFocus = this.button2;
                }
                else
                {
                    buttonToFocus = this.button1;
                }

                buttonToFocus.Focus();
            }

            /// <summary>
            /// Handles the LinkClicked event of the richTextBoxMessage control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.LinkClickedEventArgs"/> instance containing the event data.</param>
            private void richTextBoxMessage_LinkClicked(object sender, LinkClickedEventArgs e)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Process.Start(e.LinkText);
                }
                catch (Exception)
                {
                    //Let the caller of FlexibleMessageBoxForm decide what to do with this exception...
                    throw;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

            }

            /// <summary>
            /// Handles the KeyUp event of the richTextBoxMessage control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
            void FlexibleMessageBoxForm_KeyUp(object sender, KeyEventArgs e)
            {
                //Handle standard key strikes for clipboard copy: "Ctrl + C" and "Ctrl + Insert"
                if (e.Control && (e.KeyCode == Keys.C || e.KeyCode == Keys.Insert))
                {
                    var buttonsTextLine = (this.button1.Visible ? this.button1.Text + STANDARD_MESSAGEBOX_SEPARATOR_SPACES : string.Empty)
                                        + (this.button2.Visible ? this.button2.Text + STANDARD_MESSAGEBOX_SEPARATOR_SPACES : string.Empty)
                                        + (this.button3.Visible ? this.button3.Text + STANDARD_MESSAGEBOX_SEPARATOR_SPACES : string.Empty);

                    //Build same clipboard text like the standard .Net MessageBox
                    var textForClipboard = STANDARD_MESSAGEBOX_SEPARATOR_LINES
                                         + this.Text + Environment.NewLine
                                         + STANDARD_MESSAGEBOX_SEPARATOR_LINES
                                         + this.richTextBoxMessage.Text + Environment.NewLine
                                         + STANDARD_MESSAGEBOX_SEPARATOR_LINES
                                         + buttonsTextLine.Replace("&", string.Empty) + Environment.NewLine
                                         + STANDARD_MESSAGEBOX_SEPARATOR_LINES;

                    //Set text in clipboard
                    Clipboard.SetText(textForClipboard);
                }
            }

            #endregion

            #region Properties (only used for binding)

            /// <summary>
            /// The text that is been used for the heading.
            /// </summary>
            public string CaptionText { get; set; }

            /// <summary>
            /// The text that is been used in the FlexibleMessageBoxForm.
            /// </summary>
            public string MessageText { get; set; }

            #endregion

            #region Public show function

            /// <summary>
            /// Shows the specified message box.
            /// </summary>
            /// <param name="owner">The owner.</param>
            /// <param name="text">The text.</param>
            /// <param name="caption">The caption.</param>
            /// <param name="buttons">The buttons.</param>
            /// <param name="icon">The icon.</param>
            /// <param name="defaultButton">The default button.</param>
            /// <returns>The dialog result.</returns>
            public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
            {
                //Create a new instance of the FlexibleMessageBox form
                var flexibleMessageBoxForm = new FlexibleMessageBoxForm();
                flexibleMessageBoxForm.ShowInTaskbar = false;

                //Bind the caption and the message text
                flexibleMessageBoxForm.CaptionText = caption;
                flexibleMessageBoxForm.MessageText = text;
                flexibleMessageBoxForm.FlexibleMessageBoxFormBindingSource.DataSource = flexibleMessageBoxForm;

                //Set the buttons visibilities and texts. Also set a default button.
                SetDialogButtons(flexibleMessageBoxForm, buttons, defaultButton);

                //Set the dialogs icon. When no icon is used: Correct placement and width of rich text box.
                SetDialogIcon(flexibleMessageBoxForm, icon);

                //Set the font for all controls
                flexibleMessageBoxForm.Font = FONT;
                flexibleMessageBoxForm.richTextBoxMessage.Font = FONT;

                //Calculate the dialogs start size (Try to auto-size width to show longest text row). Also set the maximum dialog size. 
                SetDialogSizes(flexibleMessageBoxForm, text, caption);

                //Set the dialogs start position when given. Otherwise center the dialog on the current screen.
                SetDialogStartPosition(flexibleMessageBoxForm, owner);

                //Show the dialog
                return flexibleMessageBoxForm.ShowDialog(owner);
            }

            #endregion
        } //class FlexibleMessageBoxForm

        #endregion
    }
}
