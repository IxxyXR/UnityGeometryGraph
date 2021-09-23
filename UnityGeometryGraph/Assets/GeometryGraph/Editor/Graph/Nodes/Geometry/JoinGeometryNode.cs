﻿using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Join Geometry")]
    public class JoinGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.JoinGeometryNode> {
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort resultPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Join Geometry", EditorView.DefaultNodePosition);

            aPort = GraphFrameworkPort.Create("Values", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);
            
            AddPort(aPort);
            AddPort(resultPort);
            
            RefreshExpandedState();
        }
        
        public override void BindPorts() {
            BindPort(aPort, RuntimeNode.APort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }
    }
}