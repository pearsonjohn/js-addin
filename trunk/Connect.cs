using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace JS_addin.Addin
{
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{

		private DTE m_dte = null;
		private AddIn m_addin = null;
		private NavigationTreeView _navigationTreeView = null;
		private Window m_toolWindow = null;
		private DocumentEvents documentEvents = null;
		private TextEditorEvents textEditorEvents = null;
		private SolutionEvents solutionEvents = null;
		private WindowEvents windowEvents = null;

		private static bool _initialize = false;

		public Connect()
		{
		}

		private bool Exist(CommandBar parent, string name)
		{
			for (int i = parent.Controls.Count; i > 0; i--)
			{
				if (parent.Controls[i].get_accName(0) == name)
				{
					return true;
				}
			}
			return false;
		}

		private void AddMenu(Command command, CommandBar owner, string name)
		{
			if (!Exist(owner, name))
			{
				command.AddControl(owner, owner.accChildCount + 1);
			}
		}

		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			//System.Diagnostics.Debugger.Break();
			m_dte = (DTE)application;
			m_addin = (AddIn)addInInst;
			if (!_initialize)
			{
				object[] contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)m_dte.Commands;
				string appInName = "JS_addin";

				CommandBar menuBarCommandBar = ((CommandBars)m_dte.CommandBars)["MenuBar"];
				CommandBarControl toolsControl = menuBarCommandBar.Controls["Tools"];
				CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;
				try
				{
					Command command = commands.AddNamedCommand2(m_addin, appInName,
						"JS addin", "Shows the javascript utility plugin", true, 59,
						ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
						(int)vsCommandStyle.vsCommandStylePictAndText,
						vsCommandControlType.vsCommandControlTypeButton);

					if ((command != null) && (toolsPopup != null))
						command.AddControl(toolsPopup.CommandBar, 1);
				}
				catch (System.ArgumentException e)
				{
				}
				
				Events events = m_dte.Events;
				documentEvents = events.get_DocumentEvents(null);
				textEditorEvents = events.get_TextEditorEvents(null);
				solutionEvents = events.SolutionEvents;
				windowEvents = events.get_WindowEvents(null);

				documentEvents.DocumentClosing += new _dispDocumentEvents_DocumentClosingEventHandler(documentEvents_DocumentClosing);
				documentEvents.DocumentOpened += new _dispDocumentEvents_DocumentOpenedEventHandler(documentEvents_DocumentOpened);
				documentEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(documentEvents_DocumentSaved);
				solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(solutionEvents_Opened);
				windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
				_initialize = true;
			}
		}

		void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
		{
			if (GotFocus == null || GotFocus.Kind != "Document") return;
			if (_navigationTreeView == null || GotFocus.Document == null) return;

			try
			{
				_navigationTreeView.Init(m_dte, GotFocus.Document);
				_navigationTreeView.LoadFunctionList();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + Environment.NewLine + ex.Source);
			}
		}

		void solutionEvents_Opened()
		{
			ShowWindow();
		}

		/// <summary>
		/// Creates control
		/// </summary>
		/// <returns></returns>
		private bool EnsureWindowCreated()
		{
			if (_navigationTreeView != null) return true;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				object obj = null;
				string guid = "{cae4f06e-3b94-46e5-9721-e135c20260a4}";
				Windows2 windows2 = (Windows2)m_dte.Windows;
				Assembly asm = Assembly.GetExecutingAssembly();

				try
				{
					m_toolWindow = windows2.CreateToolWindow2(m_addin, asm.Location, "JS_addin.Addin.NavigationTreeView", "JavaScript Parser", guid, ref obj);
				}
				catch
				{
					return false;
				}
				_navigationTreeView = obj as NavigationTreeView;
				if (_navigationTreeView == null || m_toolWindow == null) return false;
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Show coltrol
		/// </summary>
		/// <returns></returns>
		private bool ShowWindow()
		{
			if (EnsureWindowCreated())
			{
				if (!m_toolWindow.Linkable)
					m_toolWindow.Linkable = true;
				if (m_toolWindow.IsFloating)
					m_toolWindow.IsFloating = false;
				if (!m_toolWindow.Visible)
					m_toolWindow.Visible = true;
				return true;
			}
			else
				return false;
		}

		#region Documetn Event handlers

		void documentEvents_DocumentOpened(Document Document)
		{
		}

		void documentEvents_DocumentSaved(Document doc)
		{
			if (_navigationTreeView != null)
				_navigationTreeView.LoadFunctionList();
		}

		void documentEvents_DocumentOpening(string DocumentPath, bool ReadOnly)
		{
		}

		void documentEvents_DocumentClosing(Document doc)
		{
			if (_navigationTreeView != null)
				_navigationTreeView.Clear();
		}

		#endregion


		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}
		public void OnAddInsUpdate(ref Array custom)
		{
		}
		public void OnStartupComplete(ref Array custom)
		{
		}
		public void OnBeginShutdown(ref Array custom)
		{
		}

		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				bool commandEnabled = false;
				if (commandName == "JS_addin.Addin.Connect.JS_addin")
				{
					commandEnabled = true;
				}

				status = vsCommandStatus.vsCommandStatusSupported;
				if (commandEnabled)
					status |= vsCommandStatus.vsCommandStatusEnabled;
				else
					status |= vsCommandStatus.vsCommandStatusInvisible;
			}
		}

		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				if (commandName == "JS_addin.Addin.Connect.JS_addin")
				{
					ShowWindow();
					handled = true;
					return;
				}
			}
		}
	}
}