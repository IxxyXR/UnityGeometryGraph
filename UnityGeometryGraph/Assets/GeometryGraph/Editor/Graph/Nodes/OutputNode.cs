﻿using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Graph Output")]
    public class OutputNode : AbstractNode<GeometryGraph.Runtime.Graph.OutputNode> {
        private GraphFrameworkPort valuePort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Graph Output", EditorView.DefaultNodePosition);

            valuePort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener);
            AddPort(valuePort);
        }

        public override void BindPorts() {
            BindPort(valuePort, RuntimeNode.Input);
        }

        protected internal override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            // Do nothing
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            return null;
        }
    }
}