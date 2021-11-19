﻿using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Integer", "Branch")]
    public class IntegerBranchNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerBranchNode> {
        
        private GraphFrameworkPort conditionPort;
        private GraphFrameworkPort ifTruePort;
        private GraphFrameworkPort ifFalsePort;
        private GraphFrameworkPort resultPort;

        private Toggle conditionToggle;
        private IntegerField ifTrueField;
        private IntegerField ifFalseField;

        private bool condition;
        private int ifTrue;
        private int ifFalse;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Branch (Integer)");

            (conditionPort, conditionToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Condition", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateCondition(condition));
            (ifTruePort, ifTrueField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("If True", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateIfTrue(ifTrue));
            (ifFalsePort, ifFalseField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("If False", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateIfFalse(ifFalse));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Integer, edgeConnectorListener, this);

            conditionToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == condition) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node condition");
                condition = evt.newValue;
                RuntimeNode.UpdateCondition(condition);
            });
            
            ifTrueField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == ifTrue) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node if true value");
                ifTrue = evt.newValue;
                RuntimeNode.UpdateIfTrue(ifTrue);
            });
            
            ifFalseField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == ifFalse) return;
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
        
        public override void BindPorts() {
            BindPort(conditionPort, RuntimeNode.ConditionPort);
            BindPort(ifTruePort, RuntimeNode.IfTruePort);
            BindPort(ifFalsePort, RuntimeNode.IfFalsePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            var array = new JArray {
                condition ? 1 : 0,
                ifTrue,
                ifFalse,
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            var array = jsonData["d"] as JArray;
            
            condition = array!.Value<int>(0) == 1;
            ifTrue = array.Value<int>(1);
            ifFalse = array.Value<int>(2);
            
            conditionToggle.SetValueWithoutNotify(condition);
            ifTrueField.SetValueWithoutNotify(ifTrue);
            ifFalseField.SetValueWithoutNotify(ifFalse);
            
            RuntimeNode.UpdateCondition(condition);
            RuntimeNode.UpdateIfTrue(ifTrue);
            RuntimeNode.UpdateIfFalse(ifFalse);
            
            base.SetNodeData(jsonData);
        }
    }
}