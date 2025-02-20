﻿using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Icosphere")]
    public class IcospherePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.IcospherePrimitiveNode> {
        protected override string Title => "Icosphere Primitive";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort subdivisionsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField radiusField;
        private ClampedIntegerField subdivisionsField;

        private float radius = 1.0f;
        private int subdivisions = 2;

        protected override void CreateNode() {
            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Radius", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateRadius(radius));
            (subdivisionsPort, subdivisionsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Subdivisions", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateSubdivisions(subdivisions));
            resultPort = GraphFrameworkPort.Create("Icosphere", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            radiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            radiusField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change radius");
                radius = newValue;
                RuntimeNode.UpdateRadius(radius);
            });

            subdivisionsField.Min = 0;
            subdivisionsField.Max = Constants.MAX_ICOSPHERE_SUBDIVISIONS;
            subdivisionsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(0, Constants.MAX_ICOSPHERE_SUBDIVISIONS);
                if (newValue == subdivisions) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change subdivisions");
                subdivisions = newValue;
                RuntimeNode.UpdateSubdivisions(subdivisions);
            });

            radiusField.SetValueWithoutNotify(1.0f);
            subdivisionsField.SetValueWithoutNotify(2);

            radiusPort.Add(radiusField);
            subdivisionsPort.Add(subdivisionsField);

            AddPort(radiusPort);
            AddPort(subdivisionsPort);
            AddPort(resultPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(radiusPort, RuntimeNode.RadiusPort);
            BindPort(subdivisionsPort, RuntimeNode.SubdivisionsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["r"] = radius;
            root["s"] = subdivisions;

            return root;
        }

        protected internal override void Deserialize(JObject data) {
            radius = data.Value<float>("r");
            subdivisions = data.Value<int>("s");

            radiusField.SetValueWithoutNotify(radius);
            subdivisionsField.SetValueWithoutNotify(subdivisions);

            RuntimeNode.UpdateRadius(radius);
            RuntimeNode.UpdateSubdivisions(subdivisions);

            base.Deserialize(data);
        }
    }
}