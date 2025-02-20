﻿using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Float", "Clamp")]
    public class ClampFloatNode : AbstractNode<GeometryGraph.Runtime.Graph.ClampFloatNode> {
        protected override string Title => "Clamp (Float)";
        protected override NodeCategory Category => NodeCategory.Float;

        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort resultPort;

        private FloatField inputField;
        private FloatField minField;
        private FloatField maxField;

        private float inputValue;
        private float minValue = 0.0f;
        private float maxValue = 1.0f;

        protected override void CreateNode() {
            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Input", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateInput(inputValue));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Min", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMin(minValue));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Max", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMax(maxValue));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                inputValue = evt.newValue;
                RuntimeNode.UpdateInput(inputValue);
            });

            minField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                minValue = evt.newValue;
                RuntimeNode.UpdateMin(minValue);
            });

            maxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                maxValue = evt.newValue;
                RuntimeNode.UpdateMax(maxValue);
            });

            minField.SetValueWithoutNotify(0.0f);
            maxField.SetValueWithoutNotify(1.0f);

            inputPort.Add(inputField);
            minPort.Add(minField);
            maxPort.Add(maxField);

            AddPort(inputPort);
            AddPort(minPort);
            AddPort(maxPort);
            AddPort(resultPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["i"] = inputValue;
            root["m"] = minValue;
            root["M"] = maxValue;

            return root;
        }

        protected internal override void Deserialize(JObject data) {
            inputValue = data.Value<float>("i");
            minValue = data.Value<float>("m");
            maxValue = data.Value<float>("M");

            inputField.SetValueWithoutNotify(inputValue);
            minField.SetValueWithoutNotify(minValue);
            maxField.SetValueWithoutNotify(maxValue);

            RuntimeNode.UpdateInput(inputValue);
            RuntimeNode.UpdateMin(minValue);
            RuntimeNode.UpdateMax(maxValue);

            base.Deserialize(data);
        }
    }
}