using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

// Note: Aliases for easily differentiating between runtime and editor classes 
using RuntimeConnection = GeometryGraph.Runtime.Graph.Connection;
using RuntimeProperty = GeometryGraph.Runtime.Graph.Property;

namespace GeometryGraph.Editor {
    [ScriptedImporter(0, Extension, 3)]
    public class GraphFrameworkImporter : ScriptedImporter {
        public const string Extension = "geometrygraph";

        public override void OnImportAsset(AssetImportContext ctx) {
            var graphObject = GraphFrameworkUtility.LoadGraphAtPath(ctx.assetPath);
            var icon = Resources.Load<Texture2D>(GraphFrameworkResources.IconBig);
            var runtimeIcon = Resources.Load<Texture2D>(GraphFrameworkResources.RuntimeIconBig);

            if (string.IsNullOrEmpty(graphObject.AssetGuid) || graphObject.AssetGuid != AssetDatabase.AssetPathToGUID(ctx.assetPath)) {
                graphObject.RecalculateAssetGuid(ctx.assetPath);
                GraphFrameworkUtility.SaveGraph(graphObject, false);
            }

            ctx.AddObjectToAsset("MainAsset", graphObject.RuntimeGraph, icon);
            ctx.SetMainObject(graphObject.RuntimeGraph);

            /*// Add properties
            runtimeObject.Properties = new List<RuntimeProperty>(
                graphObject.GraphData.Properties.Select(
                    property =>
                        new RuntimeProperty {
                            Type = property.Type, DisplayName = property.DisplayName, ReferenceName = property.ReferenceName, Guid = property.GUID
                        }
                )
            );

            // Add nodes
            runtimeObject.Nodes = new List<RuntimeNode>();
            foreach (var node in graphObject.GraphData.Nodes) {
                /*var nodeData = JObject.Parse(node.NodeData);
                var runtimeNode = new RuntimeNode {
                    Guid = node.GUID
                };#1#
                /*switch (node.Type) {
                    default: throw new NotSupportedException($"Invalid node type {node.Type}.");
                }#1#

                // runtimeObject.Nodes.Add(runtimeNode);
            }

            // Add edges
            runtimeObject.Connections = new List<RuntimeConnection>(
                /*graphObject.GraphData.Edges.Select(
                    edge =>
                        new RuntimeEdge {
                            FromNode = edge.Output, FromPort = edge.OutputPort, ToNode = edge.Input, ToPort = edge.InputPort
                        }
                )#1#
            );
            runtimeObject.BuildGraph();*/

            graphObject.hideFlags = HideFlags.HideInHierarchy;
            ctx.AddObjectToAsset("GraphAsset", graphObject, runtimeIcon);
            AssetDatabase.Refresh();

            // EditorUtility.SetDirty(graphObject);
        }
    }
}