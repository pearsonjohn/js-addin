﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsParserCore.Code;
using System.Windows.Forms;

namespace JsParserTest
{
	public class SimpleCodeProvider :ICodeProvider
	{
		private string _name;
		private string _path;
		private RichTextBox _textBox;

		public SimpleCodeProvider(RichTextBox textBox, string path, string name)
		{
			_textBox = textBox;
			_name = name;
			_path = path;
		}

		#region ICodeProvider Members

		public string LoadCode()
		{
			return _textBox.Text;
		}

		public string Path
		{
			get { return _path; }
		}

		public string Name
		{
			get { return _name; }
		}

		public void SelectionMoveToLineAndOffset(int startLine, int startColumn)
		{
			var textLen = _textBox.Lines.Take(startLine - 1).Select(l => l.Length + 1).Aggregate((t, l) => t += l);
			_textBox.Select(textLen + startColumn - 1, 0);
		}

		public void SetFocus()
		{
			_textBox.Focus();
		}

		public void GetCursorPos(out int line, out int column)
		{
			try
			{
				var cumLength = 0;
				int i = 0;
				var cursorPos = _textBox.SelectionStart;
				for (; i < _textBox.Lines.Length; ++i)
				{
					var curLineLength = _textBox.Lines[i].Length + 1;
					cumLength += curLineLength;
					
					if (cumLength > cursorPos)
					{
						line = i + 1; //one-based index
						column = cursorPos - cumLength + curLineLength;
						return;
					}
				}
				line = 1;
				column = 1;
			}
			catch
			{
				line = -1;
				column = -1;
			}
		}

		#endregion
	}
}
