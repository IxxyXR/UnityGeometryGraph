﻿using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Integer", "Clamp")]
    public class ClampIntegerNode : AbstractNode<GeometryGraph.Runtime.Graph.ClampIntegerNode> {
        protected override string Title => "Clamp (Integer)";
        protected override NodeCategory Category => NodeCategory.Integer;

        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort resultPort;

        private IntegerField inputField;
        private IntegerField minField;
        private IntegerField maxField;

        private int inputValue;
        private int minValue = 0;
        private int maxValue = 1;

        protected override void CreateNode() {
            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Input", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateInput(inputValue));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMin(minValue));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMax(maxValue));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);

            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change clamp integer input value");
                inputValue = evt.newValue;
                RuntimeNode.UpdateInput(inputValue);
            });

            minField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change clamp integer minimum value");
                minValue = evt.newValue;
                RuntimeNode.UpdateMin(minValue);
            });

            maxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change clamp integer maximum value");
                maxValue = evt.newValue;
                RuntimeNode.UpdateMax(maxValue);
            });

            minField.SetValueWithoutNotify(0);
            maxField.SetValueWithoutNotify(1);

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
            inputValue = data.Value<int>("i");
            minValue = data.Value<int>("m");
            maxValue = data.Value<int>("M");

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