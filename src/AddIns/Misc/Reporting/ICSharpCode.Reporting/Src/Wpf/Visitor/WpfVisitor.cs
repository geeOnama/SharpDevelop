﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using ICSharpCode.Reporting.BaseClasses;
using ICSharpCode.Reporting.Exporter.Visitors;
using ICSharpCode.Reporting.Interfaces.Export;
using ICSharpCode.Reporting.PageBuilder.ExportColumns;
using ICSharpCode.Reporting.WpfReportViewer.Visitor.Graphics;
using System.Windows.Shapes;

namespace ICSharpCode.Reporting.WpfReportViewer.Visitor
{
	/// <summary>
	/// Description of WpfVisitor.
	/// </summary>
	/// 
	class WpfVisitor: AbstractVisitor {
		
		FixedPage fixedPage;
		Canvas sectionCanvas;
		
		
		public override void Visit(ExportPage page){
			fixedPage = FixedDocumentCreator.CreateFixedPage(page);
			FixedPage = fixedPage;
			foreach (var element in page.ExportedItems) {
				var acceptor = element as IAcceptor;
				acceptor.Accept(this);
				fixedPage.Children.Add(sectionCanvas);
			}
		}
		
		
		public override void Visit(ExportContainer exportContainer){
			
			sectionCanvas = FixedDocumentCreator.CreateContainer(exportContainer);
			sectionCanvas = RenderSectionContainer(exportContainer);
		}
	
		
		Canvas RenderSectionContainer (ExportContainer container) {
			var canvas = FixedDocumentCreator.CreateContainer(container);
			foreach (var element in container.ExportedItems) {
				if (IsContainer(element)) {
					if (IsGraphicsContainer(element)) {
						var graphContainer = RenderGraphicsContainer(element);
						canvas.Children.Add(graphContainer);
					}
				} else {
					var acceptor = element as IAcceptor;
					acceptor.Accept(this);
					canvas.Children.Add(UIElement);
				}
			}
			canvas.Background = FixedDocumentCreator.ConvertBrush(container.BackColor);
			return canvas;
		}
	
		
		Canvas RenderGraphicsContainer(IExportColumn column)
		{
			
			var graphicsContainer = column as GraphicsContainer;
			var graphCanvas = FixedDocumentCreator.CreateContainer(graphicsContainer);
			CanvasHelper.SetPosition(graphCanvas, column.Location.ToWpf());
			graphCanvas.Background = FixedDocumentCreator.ConvertBrush(column.BackColor);
			if (graphicsContainer != null) {
				var rect = column as ExportRectangle;
				if (rect != null) {
					Visit(rect);
				}
				
				var circle = column as ExportCircle;
				if (circle != null) {
					Visit(circle);
				}
				
				graphCanvas.Children.Add(UIElement);
			}
			return graphCanvas;
		}
		
		
		public override void Visit(ExportText exportColumn){
			
			var ft = FixedDocumentCreator.CreateFormattedText((ExportText)exportColumn);
			var visual = new DrawingVisual();
			var location = new Point(exportColumn.Location.X,exportColumn.Location.Y);
			using (var dc = visual.RenderOpen()){
				if (ShouldSetBackcolor(exportColumn)) {
					dc.DrawRectangle(FixedDocumentCreator.ConvertBrush(exportColumn.BackColor),
						null,
						new Rect(location,new Size(exportColumn.Size.Width,exportColumn.Size.Height)));
				}
				dc.DrawText(ft,location);
			}
			var dragingElement = new DrawingElement(visual);
			UIElement = dragingElement;
			
		}

		
		public override void Visit(ExportLine exportGraphics)
		{
			var pen = FixedDocumentCreator.CreateWpfPen(exportGraphics);
			var visual = new DrawingVisual();
			using (var dc = visual.RenderOpen()){
				dc.DrawLine(pen,
				            new Point(exportGraphics.Location.X, exportGraphics.Location.Y),
				            new Point(exportGraphics.Location.X + exportGraphics.Size.Width,exportGraphics.Location.Y));
			}
			var dragingElement = new DrawingElement(visual);
			UIElement = dragingElement;
		}
		
		
		public override void Visit(ExportRectangle exportRectangle)
		{
			var border = CreateBorder(exportRectangle);
			CanvasHelper.SetPosition(border, new Point(0,0));
			var panel = new StackPanel();
			panel.Orientation = Orientation.Horizontal;
			foreach (var element in exportRectangle.ExportedItems) {
				var acceptor = element as IAcceptor;
				acceptor.Accept(this);
				panel.Children.Add(UIElement);
			}
			border.Child = panel;
			UIElement = border;
		}
		
		
		public override void Visit(ExportCircle exportCircle)
		{
			var containerCanvas = new Canvas();
		
			var drawingElement = CircleVisual(exportCircle);
			
			containerCanvas.Children.Add(drawingElement);
			foreach (var element in exportCircle.ExportedItems) {
					var acceptor = element as IAcceptor;
					acceptor.Accept(this);
					containerCanvas.Children.Add(UIElement);
				}
			UIElement = containerCanvas;
		}
		
		
		bool IsGraphicsContainer (IExportColumn column) {
			return column is GraphicsContainer;
		}
		
		
		bool IsContainer (IExportColumn column) {
			return (column is ExportContainer)|| (column is GraphicsContainer);
		}
		
		
		DrawingElement CircleVisual(GraphicsContainer circle){
			var pen = FixedDocumentCreator.CreateWpfPen(circle);
			var rad = CalcRadius(circle.Size);
			
			var visual = new DrawingVisual();
			using (var dc = visual.RenderOpen()){
				dc.DrawEllipse(FixedDocumentCreator.ConvertBrush(circle.BackColor),
					pen,
					new Point( rad.X,rad.Y),	
					rad.X,
					rad.Y);                 
			}
			return new DrawingElement(visual);
		}
		
		
		Border CreateBorder(IExportColumn exportColumn)
		{
			double bT;
			var gc = IsGraphicsContainer(exportColumn);
			if (!IsGraphicsContainer(exportColumn)) {
				bT = 1;
			} else {
				bT = Convert.ToDouble(((GraphicsContainer)exportColumn).Thickness);
			}
			var border = new Border();
			border.BorderThickness = new Thickness(bT);
			border.BorderBrush = FixedDocumentCreator.ConvertBrush(exportColumn.ForeColor);
			border.Background = FixedDocumentCreator.ConvertBrush(exportColumn.BackColor);
			border.Width = exportColumn.Size.Width;
			border.Height = exportColumn.Size.Height;
			return border;
		}
		
		
		static Point CalcRadius(System.Drawing.Size size) {
			return  new Point(size.Width /2,size.Height /2);
		}
		
		
		protected UIElement UIElement {get;private set;}
		
		
		public FixedPage FixedPage {get; private set;}
	}
}
