﻿using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Transform Geometry")]
    public class TransformGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.TransformGeometryNode> {
        protected override string Title => "Transform Geometry";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private float3 translation;
        private float3 eulerRotation;
        private float3 scale = float3_ext.one;

        private Vector3Field translationField;
        private Vector3Field eulerRotationField;
        private Vector3Field scaleField;

        private GraphFrameworkPort inputGeometryPort;
        private GraphFrameworkPort translationPort;
        private GraphFrameworkPort rotationPort;
        private GraphFrameworkPort scalePort;

        private GraphFrameworkPort outputGeometryPort;

        protected override void CreateNode() {
            inputGeometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            outputGeometryPort = GraphFrameworkPort.Create("Geometry", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            (translationPort, translationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Translation", PortType.Vector, this, showLabelOnField: false,
                onDisconnect: (_, _) => RuntimeNode.UpdateTranslation(translation)
            );
            translationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Translation value");
                translation = evt.newValue;
                RuntimeNode.UpdateTranslation(translation);
            });
            (rotationPort, eulerRotationField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Rotation", PortType.Vector, this, showLabelOnField: false,
                onDisconnect: (_, _) => RuntimeNode.UpdateRotation(eulerRotation)
            );
            eulerRotationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Rotation value");
                eulerRotation = math_ext.wrap(evt.newValue, -180.0f, 180.0f);
                RuntimeNode.UpdateRotation(eulerRotation);
            });

            (scalePort, scaleField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "Scale", PortType.Vector, this, showLabelOnField: false,
                onDisconnect: (_, _) => RuntimeNode.UpdateScale(scale)
            );
            scaleField.SetValueWithoutNotify(float3_ext.one);
            scaleField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Scale value");
                scale = evt.newValue;
                RuntimeNode.UpdateScale(scale);
            });

            AddPort(inputGeometryPort);
            AddPort(translationPort);
            inputContainer.Add(translationField);
            AddPort(rotationPort);
            inputContainer.Add(eulerRotationField);
            AddPort(scalePort);
            inputContainer.Add(scaleField);

            AddPort(outputGeometryPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(inputGeometryPort, RuntimeNode.InputPort);
            BindPort(translationPort, RuntimeNode.TranslationPort);
            BindPort(rotationPort, RuntimeNode.RotationPort);
            BindPort(scalePort, RuntimeNode.ScalePort);
            BindPort(outputGeometryPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["t"] = JsonConvert.SerializeObject(translation, Formatting.None, float3Converter.Converter);
            root["r"] = JsonConvert.SerializeObject(eulerRotation, Formatting.None, float3Converter.Converter);
            root["s"] = JsonConvert.SerializeObject(scale, Formatting.None, float3Converter.Converter);

            return root;
        }

        protected internal override void Deserialize(JObject data) {

            translation = JsonConvert.DeserializeObject<float3>(data.Value<string>("t")!, float3Converter.Converter);
            eulerRotation = JsonConvert.DeserializeObject<float3>(data.Value<string>("r")!, float3Converter.Converter);
            scale = JsonConvert.DeserializeObject<float3>(data.Value<string>("s")!, float3Converter.Converter);

            translationField.SetValueWithoutNotify(translation);
            eulerRotationField.SetValueWithoutNotify(eulerRotation);
            scaleField.SetValueWithoutNotify(scale);

            RuntimeNode.UpdateTranslation(translation);
            RuntimeNode.UpdateRotation(eulerRotation);
            RuntimeNode.UpdateScale(scale);

            base.Deserialize(data); 
        }
    }
}