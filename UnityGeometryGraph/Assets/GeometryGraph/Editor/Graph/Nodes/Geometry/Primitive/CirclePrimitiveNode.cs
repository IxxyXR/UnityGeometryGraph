﻿using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Circle")]
    public class CirclePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.CirclePrimitiveNode> {
        protected override string Title => "Circle Primitive";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField radiusField;
        private ClampedIntegerField pointsField;

        private float radius = 1.0f;
        private int points = 8;

        protected override void CreateNode() {
            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Radius", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateRadius(radius));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            resultPort = GraphFrameworkPort.Create("Circle", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            radiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            radiusField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                radius = newValue;
                RuntimeNode.UpdateRadius(radius);
            });

            pointsField.Min = Constants.MIN_CIRCULAR_GEOMETRY_POINTS;
            pointsField.Max = Constants.MAX_CIRCULAR_GEOMETRY_POINTS;
            pointsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);
                if (newValue == points) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });

            radiusField.SetValueWithoutNotify(1.0f);
            pointsField.SetValueWithoutNotify(8);

            radiusPort.Add(radiusField);
            pointsPort.Add(pointsField);

            AddPort(radiusPort);
            AddPort(pointsPort);
            AddPort(resultPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(radiusPort, RuntimeNode.RadiusPort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["r"] = radius;
            root["p"] = points;

            return root;
        }

        protected internal override void Deserialize(JObject data) {
            radius = data.Value<float>("r");
            points = data.Value<int>("p");

            radiusField.SetValueWithoutNotify(radius);
            pointsField.SetValueWithoutNotify(points);

            RuntimeNode.UpdateRadius(radius);
            RuntimeNode.UpdatePoints(points);

            base.Deserialize(data);
        }
    }
}