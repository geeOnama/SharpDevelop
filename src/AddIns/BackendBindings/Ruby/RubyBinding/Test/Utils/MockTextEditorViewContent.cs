﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using ICSharpCode.AvalonEdit;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Editor.AvalonEdit	;
using ICSharpCode.SharpDevelop.Dom.Refactoring;

namespace RubyBinding.Tests.Utils
{
	/// <summary>
	/// A mock IViewContent implementation that also implements the
	/// ITextEditorControlProvider interface.
	/// </summary>
	public class MockTextEditorViewContent : MockViewContent, ITextEditorProvider
	{
		ITextEditor textEditor;
		
		public MockTextEditorViewContent()
		{
			textEditor = new AvalonEditTextEditorAdapter(new TextEditor());
		}
		
		public ITextEditor TextEditor {
			get { return textEditor; }
		}
		
		public IDocument GetDocumentForFile(OpenedFile file)
		{
			throw new NotImplementedException();
		}
	}
}
