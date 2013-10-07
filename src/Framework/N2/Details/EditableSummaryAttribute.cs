﻿using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System;

namespace N2.Details
{
	/// <summary>
	/// Extracts a summary text from another detail and stores
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class EditableSummaryAttribute : EditableTextBoxAttribute, IContentTransformer
	{
		public EditableSummaryAttribute()
			: this(null, 30, "Text")
		{
			
		}

		public EditableSummaryAttribute(string title, int sortOrder, params string[] sourceDetailNames)
			: base(title, sortOrder)
		{
			IsViewEditable = false;
			Sources = sourceDetailNames;
			TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
			Columns = 72;
			Rows = 3;
			SummaryMaxLength = 250;
			ManualSummaryText = "Write manually";
		}

		public override void UpdateEditor(ContentItem item, System.Web.UI.Control editor)
		{
			base.UpdateEditor(item, editor);
			
			var cbManual = editor.FindControl(Name + "_ManualSummary") as CheckBox;
			if (cbManual != null)
			{
				cbManual.Checked = IsManualSummary(item);
				if (!cbManual.Checked)
				{
					var tb = (TextBox)editor;
					tb.ReadOnly = true;
					tb.Enabled = false;
					tb.CssClass += " autogenerated";
				}
			}
		}

		protected override System.Web.UI.Control AddEditor(System.Web.UI.Control container)
		{
			var editor = (TextBox)base.AddEditor(container);
			editor.CssClass = "input-xxlarge summaryEditor";

			var cbContainer = new HtmlGenericControl("span");
			cbContainer.Attributes["class"] = "editorOption";
			container.Controls.Add(cbContainer);

			var cb = new CheckBox();
			cb.ID = Name + "_ManualSummary";
			cb.Text = ManualSummaryText;
			cb.Attributes["onclick"] = "if(this.checked) $(this).closest('.editDetail').find(':text,textarea').removeAttr('readonly').removeAttr('disabled').removeClass('autogenerated').focus(); else $(this).closest('.editDetail').find(':text,textarea').attr('readonly', 'readonly').attr('disabled', 'disabled').addClass('autogenerated');";
			cbContainer.Controls.Add(cb);

			return editor;
		}

		public override bool UpdateItem(ContentItem item, System.Web.UI.Control editor)
		{
			var cbManual = editor.FindControl(Name + "_ManualSummary") as CheckBox;
			if (cbManual != null)
			{
				item[Name + "_ManualSummary"] = cbManual.Checked ? (object)true : null;
			}

			return base.UpdateItem(item, editor);
		}

		public string Source 
		{ 
			get { return Sources != null ? Sources.FirstOrDefault() : null; }
			set { Sources = new[] { value }; }
		}
		public string[] Sources { get; set; }

		public int SummaryMaxLength { get; set; }

		public string ManualSummaryText { get; set; }

		#region IContentTransformer Members

		ContentState IContentTransformer.ChangingTo
		{
			get { return ContentState.New | ContentState.Published; }
		}

		bool IContentTransformer.Transform(ContentItem item)
		{
			if (item.State == ContentState.New && DefaultValue != null)
			{
				item[Name] = DefaultValue;
				return true;
			}
			if (item.State == ContentState.Published && Source != null && !IsManualSummary(item))
			{
				if (Sources != null && Sources.Length > 0)
				{
					StringBuilder sb = new StringBuilder();
					foreach (string s in Sources)
					{
						string content = item[s] as string;
						if (content == null)
							continue;
						if (sb.Length > 0)
							sb.Append(" ");
						sb.Append(content);
					}
					item[Name] = Utility.ExtractFirstSentences(sb.ToString(), SummaryMaxLength);
				}
				return true;
			}

			return false;
		}

		private bool IsManualSummary(ContentItem item)
		{
			return true.Equals(item[Name + "_ManualSummary"]);
		}

		#endregion
	}
}
