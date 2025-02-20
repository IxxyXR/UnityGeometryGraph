﻿using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Float", "Branch")]
    public class FloatBranchNode : AbstractNode<GeometryGraph.Runtime.Graph.FloatBranchNode> {
        protected override string Title => "Branch (Float)";
        protected override NodeCategory Category => NodeCategory.Float;
    private GraphFrameworkPort conditionPort;
        private GraphFrameworkPort ifTruePort;
        private GraphFrameworkPort ifFalsePort;
        private GraphFrameworkPort resultPort;

        private Toggle conditionToggle;
        private FloatField ifTrueField;
        private FloatField ifFalseField;

        private bool condition;
        private float ifTrue;
        private float ifFalse;

        protected override void CreateNode() {(conditionPort, conditionToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Condition", PortType.Boolean, this, onDisconnect: (_, _) => RuntimeNode.UpdateCondition(condition));
            (ifTruePort, ifTrueField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("If True", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateIfTrue(ifTrue));
            (ifFalsePort, ifFalseField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("If False", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateIfFalse(ifFalse));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            conditionToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == condition) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node condition");
                condition = evt.newValue;
                RuntimeNode.UpdateCondition(condition);
            });

            ifTrueField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - ifTrue) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node if true value");
                ifTrue = evt.newValue;
                RuntimeNode.UpdateIfTrue(ifTrue);
            });

            ifFalseField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - ifFalse) < Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node if false value");
                ifFalse = evt.newValue;
                RuntimeNode.UpdateIfFalse(ifFalse);
            });


            conditionPort.Add(conditionToggle);
            ifTruePort.Add(ifTrueField);
            ifFalsePort.Add(ifFalseField);

            AddPort(conditionPort);
            AddPort(ifTruePort);
            AddPort(ifFalsePort);
            AddPort(resultPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(conditionPort, RuntimeNode.ConditionPort);
            BindPort(ifTruePort, RuntimeNode.IfTruePort);
            BindPort(ifFalsePort, RuntimeNode.IfFalsePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                condition ? 1 : 0,
                ifTrue,
                ifFalse,
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            condition = array!.Value<int>(0) == 1;
            ifTrue = array.Value<float>(1);
            ifFalse = array.Value<float>(2);

            conditionToggle.SetValueWithoutNotify(condition);
            ifTrueField.SetValueWithoutNotify(ifTrue);
            ifFalseField.SetValueWithoutNotify(ifFalse);

            RuntimeNode.UpdateCondition(condition);
            RuntimeNode.UpdateIfTrue(ifTrue);
            RuntimeNode.UpdateIfFalse(ifFalse);

            base.Deserialize(data);
        }
    }
}