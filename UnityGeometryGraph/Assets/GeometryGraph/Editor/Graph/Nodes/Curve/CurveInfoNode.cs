﻿using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Curve", "Curve Info")]
    public class CurveInfoNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveInfoNode> {
        protected override string Title => "Curve Info";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort inputCurvePort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort isClosedPort;

        protected override void CreateNode() {
            inputCurvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            pointsPort = GraphFrameworkPort.Create("Points", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);
            isClosedPort = GraphFrameworkPort.Create("Is Closed", Direction.Output, Port.Capacity.Multi, PortType.Boolean, this);

            AddPort(inputCurvePort);
            AddPort(pointsPort);
            AddPort(isClosedPort);
        }

        protected override void BindPorts() {
            BindPort(inputCurvePort, RuntimeNode.CurvePort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(isClosedPort, RuntimeNode.IsClosedPort);
        }
    }
}