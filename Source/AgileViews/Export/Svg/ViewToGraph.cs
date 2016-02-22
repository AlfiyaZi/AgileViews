using System;
using System.Collections.Generic;
using System.Linq;
using AgileViews.Model;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Prototype.Ranking;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Label = Microsoft.Msagl.Drawing.Label;
using Node = Microsoft.Msagl.Drawing.Node;

namespace AgileViews.Export.Svg
{
    public interface INodeStyle
    {
        void Apply(Node node);
    }

    public class NodeStyle : INodeStyle
    {
        /// <summary>
        /// Space within the node around the label
        /// </summary>
        public int Margin { get; set; }
        public double Radius { get; set; }
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }
        public double BorderWidth { get; set; }

        public Shape Shape { get; set; }
        public void Apply(Node node)
        {
            node.Attr.LabelMargin = Margin;
            node.Attr.FillColor = FillColor;
            node.Attr.Shape = Shape;
            node.Attr.XRadius = Radius;
            node.Attr.YRadius = Radius;
            node.Attr.LineWidth = BorderWidth;
        }
    }

    public interface IEdgeStyle
    {
        void Apply(Edge edge);
    }

    public class EdgeStyle : IEdgeStyle
    {
        public Color Color { get; set; }
        public double LineWidth { get; set; }

        public ArrowStyle SourceStyle { get; set; }

        public ArrowStyle TargetStyle { get; set; }

        public void Apply(Edge edge)
        {
            edge.Attr.ArrowheadAtSource = SourceStyle;
            edge.Attr.ArrowheadAtTarget = TargetStyle;
            edge.Attr.LineWidth = LineWidth;
            edge.Attr.Color = Color;
        }
    }


    public class ViewToGraph
    {
        private Action<Element, Node> _nodeDecorator = (e, n) =>
        {
            n.Attr.LabelMargin = 10;
            n.Attr.Shape = Shape.Box;
            n.Attr.XRadius = 0;
            n.Attr.YRadius = 0;
            n.Attr.LineWidth = 2;
        };

        private Action<Relationship, Edge> _edgeDecorator = (r, e) =>
        {
            e.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
        };

        public ViewToGraph()
        {
            
        }

        public Graph Convert(View view)
        {
            var graph = new Graph();

            view.Elements.ToDictionary(e => e, e =>
            {
                var node = graph.AddNode((string) e.Alias);
                node.LabelText = e.Name.Replace("<", "&lt;").Replace(">", "&gt;");
                node.Attr.Uri = e.GetInformation(Information.URL).FirstOrDefault();
                _nodeDecorator(e,node);
                return node;
            });

            foreach (var rel in view.Relationships)
            {
                var edge = graph.AddEdge(rel.Source.Alias, rel.Target.Alias);
                if (!string.IsNullOrEmpty(rel.Label))
                {
                    edge.LabelText = rel.Label;
                }
                _edgeDecorator(rel,edge);
            }

            SetConsolasFontAndSize(graph, 13);

            graph.LayoutAlgorithmSettings = PickLayoutAlgorithmSettings(graph.NodeCount, graph.EdgeCount);

            var renderer = new GraphRenderer(graph);
            renderer.CalculateLayout();

            SetConsolasFontAndSize(graph, 11);

            return graph;
        }

        static void SetConsolasFontAndSize(Graph graph, int size)
        {
            var labels = GetAllLabels(graph);
            foreach (var label in labels)
            {
                label.FontName = "Consolas";
                label.FontSize = size;
            }
        }

        static IEnumerable<Label> GetAllLabels(Graph graph)
        {
            foreach (var node in graph.Nodes)
                if (node.Label != null)
                    yield return node.Label;
            foreach (var edge in graph.Edges)
                if (edge.Label != null)
                    yield return edge.Label;
        }

        static void EnlargeLabelMargins(Graph graph)
        {
            foreach (var node in graph.Nodes)
                node.Attr.LabelMargin = 8;
        }

        static LayoutAlgorithmSettings PickLayoutAlgorithmSettings(int edges, int nodes)
        {
            LayoutAlgorithmSettings settings;
            const int sugiaymaTreshold = 200;
            const double bundlingTreshold = 3.0;
            bool bundling = nodes != 0 && ((double)edges / nodes >= bundlingTreshold || edges > 100);
            if (nodes < sugiaymaTreshold && edges < sugiaymaTreshold)
            {
                settings = new SugiyamaLayoutSettings();

                if (bundling)
                    settings.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.SplineBundling;
            }
            else {
                MdsLayoutSettings mdsSettings;
                settings = mdsSettings = new MdsLayoutSettings
                {
                    EdgeRoutingSettings = {
                        EdgeRoutingMode
                            =
                            bundling
                                ? EdgeRoutingMode.SplineBundling
                                : EdgeRoutingMode.Spline
                    }
                };
                if (bundling)
                    settings.EdgeRoutingSettings.BundlingSettings = new BundlingSettings();
                double scale = FigureOutScaleForMdsLayout(nodes);
                mdsSettings.ScaleX = scale;
                mdsSettings.ScaleY = scale;
            }
            return settings;
        }

        static double FigureOutScaleForMdsLayout(int nodes)
        {
            const int maxScale = 900;
            const int minScale = 400;
            return Math.Min(nodes + minScale, maxScale);
        }

        static void SetBoxRadiuses(Graph graph)
        {
            foreach (var node in graph.Nodes)
            {
                var attr = node.Attr;
                var r = Math.Min(node.Width, node.Height) / 10;
                attr.XRadius = r;
                attr.YRadius = r;
            }
        }
    }
}