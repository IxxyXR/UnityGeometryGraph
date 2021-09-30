﻿using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Cube")]
    public class CubePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.CubePrimitiveNode> {
        private GraphFrameworkPort sizePort;
        private GraphFrameworkPort resultPort;

        private Vector3Field sizeField;

        private float3 size = float3_util.one;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Cube Primitive", EditorView.DefaultNodePosition);

            (sizePort, sizeField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Size", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateSize(size));
            resultPort = GraphFrameworkPort.Create("Cube", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            sizeField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                size = evt.newValue;
                RuntimeNode.UpdateSize(size);
            });

            sizeField.SetValueWithoutNotify(float3_util.one);

            AddPort(sizePort);
            inputContainer.Add(sizeField);
            AddPort(resultPort);

            Refresh();
        }

        public override void BindPorts() {
            BindPort(sizePort, RuntimeNode.SizePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["w"] = size.x;
            root["h"] = size.y;
            root["d"] = size.z;

            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            size.x = jsonData.Value<float>("w");
            size.y = jsonData.Value<float>("h");
            size.z = jsonData.Value<float>("d");

            sizeField.SetValueWithoutNotify(size);
            RuntimeNode.UpdateSize(size);
            
            base.SetNodeData(jsonData);
        }
    }
}