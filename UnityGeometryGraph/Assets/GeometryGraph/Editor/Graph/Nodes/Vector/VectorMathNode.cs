﻿using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Operation = GeometryGraph.Runtime.Graph.VectorMathNode.VectorMathNode_Operation;

namespace GeometryGraph.Editor {
    [Title("Vector", "Math")]
    public class VectorMathNode : AbstractNode<GeometryGraph.Runtime.Graph.VectorMathNode> {
        protected override string Title => "Math (Vector)";
        protected override NodeCategory Category => NodeCategory.Vector;

        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort wrapMaxPort;
        private GraphFrameworkPort iorPort;
        private GraphFrameworkPort scalePort;
        private GraphFrameworkPort distancePort;
        private GraphFrameworkPort vectorResultPort;
        private GraphFrameworkPort floatResultPort;

        private EnumSelectionDropdown<Operation> operationDropdown;
        private Vector3Field xField;
        private Vector3Field yField;
        private Vector3Field wrapMaxField;
        private FloatField iorField;
        private FloatField scaleField;
        private FloatField distanceField;

        private Operation operation;
        private float3 x;
        private float3 y;
        private float3 wrapMax;
        private float ior;
        private float scale;
        private float distance;

        private static readonly SelectionTree mathOperationTree = new(new List<object>(Enum.GetValues(typeof(Operation)).Convert(o => o))) {
            new SelectionCategory("Operations", false, SelectionCategory.CategorySize.Large) {
                new("x + y", 0, false),
                new("x - y", 1, false),
                new("x * y", 2, false),
                new("x / y", 3, false),
                new("x scaled by y", 4, true),
                new("Length of x", 5, false),
                new("Squared length of x", 6, false),
                new("Distance between x and y", 7, false),
                new("Squared distance between x and y", 8, false),
                new("x normalized", 9, true),
                new("The dot product of x and y", 10, false),
                new("The cross product of x and y", 11, false),
                new("x projected on y", 12, false),
                new("x reflected around y", 13, false),
                new("x refracted on y, using IOR", 14, false),
                new("Linear interpolation between x and y using t", 41, false),
            },
            new SelectionCategory("Rounding", false, SelectionCategory.CategorySize.Normal) {
                new("x's components rounded to the nearest integers", 24, false),
                new("The largest integers smaller than x's components", 25, false),
                new("The smallest integers larger than x's components", 26, false),
                new("The integral part of x", 27, true),
                new("The fractional part of x", 28, false),
                new("The remainder of x / y", 29, false),
                new("x wrapped between min and max", 30, false),
                new("x snapped to the nearest multiple of increment", 31, false),
            },
            new SelectionCategory("Comparison", false, SelectionCategory.CategorySize.Medium) {
                new("Absolute value of x", 15, false),
                new("The minimum between x and y", 16, false),
                new("The maximum between x and y", 17, false),
                new("1 if x < y; 0 otherwise", 18, false),
                new("1 if x > y; 0 otherwise", 19, false),
                new("Sign of x", 20, false),
                new("1 if the distance between x and y is less than tolerance; 0 otherwise", 21, false),
                new("The minimum between x and y with smoothing", 22, false),
                new("The maximum between x and y with smoothing", 23, false),
            },
            new SelectionCategory("Conversion", true, SelectionCategory.CategorySize.Normal) {
                new("Converts degrees to radians", 39, false),
                new("Converts radians to degrees", 40, false),
            },
            new SelectionCategory("Trigonometry", true, SelectionCategory.CategorySize.Normal) {
                new("Sine of x (in radians)", 32, false),
                new("Cosine of x (in radians)", 33, false),
                new("Tangent of x (in radians)", 34, false),
                new("Inverse sine of x (in radians)", 35, true),
                new("Inverse cosine of x (in radians)", 36, false),
                new("Inverse tangent of x (in radians)", 37, false),
                new("Inverse tangent of x / y (in radians)", 38, false),
            }
        };

        protected override void CreateNode() {
            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("X", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateX(x));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Y", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateY(y));
            (wrapMaxPort, wrapMaxField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Max", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateWrapMax(wrapMax));
            (iorPort, iorField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("IOR", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateIOR(ior));
            (scalePort, scaleField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Scale", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateScale(scale));
            (distancePort, distanceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Distance", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateDistance(distance));

            vectorResultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            floatResultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            operationDropdown = new EnumSelectionDropdown<Operation>(operation, mathOperationTree);
            operationDropdown.RegisterCallback<ChangeEvent<Operation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
                OnOperationChanged();
            });

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                x = evt.newValue;
                RuntimeNode.UpdateX(x);
            });

            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                y = evt.newValue;
                RuntimeNode.UpdateY(y);
            });

            wrapMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change tolerance");
                wrapMax = evt.newValue;
                RuntimeNode.UpdateWrapMax(wrapMax);
            });

            iorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                ior = evt.newValue;
                RuntimeNode.UpdateIOR(ior);
            });

            scaleField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                scale = evt.newValue;
                RuntimeNode.UpdateScale(scale);
            });

            distanceField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                distance = evt.newValue;
                RuntimeNode.UpdateDistance(distance);
            });

            iorPort.Add(iorField);
            scalePort.Add(scaleField);
            distancePort.Add(distanceField);

            inputContainer.Add(operationDropdown);
            AddPort(xPort);
            inputContainer.Add(xField);
            AddPort(yPort);
            inputContainer.Add(yField);
            AddPort(wrapMaxPort);
            inputContainer.Add(wrapMaxField);
            AddPort(iorPort);
            AddPort(scalePort);
            AddPort(distancePort);
            AddPort(vectorResultPort);
            AddPort(floatResultPort);

            OnOperationChanged();

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(wrapMaxPort, RuntimeNode.WrapMaxPort);
            BindPort(iorPort, RuntimeNode.IORPort);
            BindPort(scalePort, RuntimeNode.ScalePort);
            BindPort(distancePort, RuntimeNode.DistancePort);
            BindPort(vectorResultPort, RuntimeNode.VectorResultPort);
            BindPort(floatResultPort, RuntimeNode.FloatResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["o"] = (int)operation;
            root["x"] = JsonConvert.SerializeObject(x, float3Converter.Converter);
            root["y"] = JsonConvert.SerializeObject(y, float3Converter.Converter);
            root["w"] = JsonConvert.SerializeObject(wrapMax, float3Converter.Converter);
            root["i"] = ior;
            root["s"] = scale;
            root["d"] = distance;

            return root;
        }

        private void OnOperationChanged() {
            bool showIOR = operation == Operation.Refract;
            bool showWrapMax = operation == Operation.Wrap;
            bool showDistance = operation is Operation.SmoothMinimum or Operation.SmoothMaximum or Operation.Lerp;
            bool showScale = operation == Operation.Scale;
            bool showY = operation is not
                (Operation.Length or Operation.LengthSquared or Operation.Scale or Operation.Sine or Operation.Cosine or Operation.Tangent
                or Operation.Arcsine or Operation.Arccosine or Operation.Arctangent or Operation.Fraction or Operation.Ceil
                or Operation.Floor or Operation.Absolute or Operation.Normalize);

            bool showFloatOutput = operation is Operation.Length or Operation.LengthSquared or Operation.Distance or Operation.DistanceSquared or Operation.DotProduct;

            if (showIOR) iorPort.Show();
            else iorPort.HideAndDisconnect();

            if (showWrapMax) wrapMaxPort.Show();
            else wrapMaxPort.HideAndDisconnect();

            distancePort.Label = operation == Operation.Lerp ? "T" : "Distance";
            if (showDistance) distancePort.Show();
            else distancePort.HideAndDisconnect();

            if (showScale) scalePort.Show();
            else scalePort.HideAndDisconnect();

            yPort.Label = showWrapMax ? "Min" : "Y";
            if(showY) yPort.Show();
            else yPort.HideAndDisconnect();

            if (showFloatOutput) {
                floatResultPort.Show();
                vectorResultPort.HideAndDisconnect();
            } else {
                floatResultPort.HideAndDisconnect();
                vectorResultPort.Show();
            }

        }

        protected internal override void Deserialize(JObject data) {
            operation = (Operation)data.Value<int>("o");
            x = JsonConvert.DeserializeObject<float3>(data.Value<string>("x")!, float3Converter.Converter);
            y = JsonConvert.DeserializeObject<float3>(data.Value<string>("y")!, float3Converter.Converter);
            wrapMax = JsonConvert.DeserializeObject<float3>(data.Value<string>("w")!, float3Converter.Converter);
            ior = data.Value<float>("i");
            scale = data.Value<float>("s");
            distance = data.Value<float>("d");

            operationDropdown.SetValueWithoutNotify(operation, 1);
            xField.SetValueWithoutNotify(x);
            yField.SetValueWithoutNotify(y);
            wrapMaxField.SetValueWithoutNotify(wrapMax);
            iorField.SetValueWithoutNotify(ior);
            scaleField.SetValueWithoutNotify(scale);
            distanceField.SetValueWithoutNotify(distance);

            RuntimeNode.UpdateOperation(operation);
            RuntimeNode.UpdateX(x);
            RuntimeNode.UpdateY(y);
            RuntimeNode.UpdateWrapMax(wrapMax);
            RuntimeNode.UpdateIOR(ior);
            RuntimeNode.UpdateScale(scale);
            RuntimeNode.UpdateDistance(distance);

            OnOperationChanged();

            base.Deserialize(data);
        }
    }
}