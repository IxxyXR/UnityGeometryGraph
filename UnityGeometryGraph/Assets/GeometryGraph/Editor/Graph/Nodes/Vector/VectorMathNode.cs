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
using Which = GeometryGraph.Runtime.Graph.VectorMathNode.VectorMathNode_Which;
using Operation = GeometryGraph.Runtime.Graph.VectorMathNode.VectorMathNode_Operation;

namespace GeometryGraph.Editor {
    [Title("Vector", "Math")]
    public class VectorMathNode : AbstractNode<GeometryGraph.Runtime.Graph.VectorMathNode> {
        
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort wrapMaxPort;
        private GraphFrameworkPort iorPort;
        private GraphFrameworkPort scalePort;
        private GraphFrameworkPort distancePort;
        private GraphFrameworkPort vectorResultPort;
        private GraphFrameworkPort floatResultPort;

        private EnumSelectionButton<Operation> operationButton;
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

        private static readonly SelectionTree mathOperationTree = new SelectionTree(new List<object>(Enum.GetValues(typeof(Operation)).Convert(o => o))) {
            new SelectionCategory("Operations", false, SelectionCategory.CategorySize.Large) {
                new SelectionEntry("x + y", 0, false),
                new SelectionEntry("x - y", 1, false),
                new SelectionEntry("x * y", 2, false),
                new SelectionEntry("x / y", 3, false),
                new SelectionEntry("x scaled by y", 4, true),
                new SelectionEntry("Length of x", 5, false),
                new SelectionEntry("Squared length of x", 6, false),
                new SelectionEntry("Distance between x and y", 7, false),
                new SelectionEntry("Squared distance between x and y", 8, false),
                new SelectionEntry("x normalized", 9, true),
                new SelectionEntry("The dot product of x and y", 10, false),
                new SelectionEntry("The cross product of x and y", 11, false),
                new SelectionEntry("x projected on y", 12, false),
                new SelectionEntry("x reflected around y", 13, false),
                new SelectionEntry("x refracted on y, using IOR", 14, false),
            },
            new SelectionCategory("Rounding", false, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("x's components rounded to the nearest integers", 24, false),
                new SelectionEntry("The largest integers smaller than x's components", 25, false),
                new SelectionEntry("The smallest integers larger than x's components", 26, false),
                new SelectionEntry("The integral part of x", 27, true),
                new SelectionEntry("The fractional part of x", 28, false),
                new SelectionEntry("The remainder of x / y", 29, false),
                new SelectionEntry("x wrapped between min and max", 30, false),
                new SelectionEntry("x snapped to the nearest multiple of increment", 31, false),
            },
            new SelectionCategory("Comparison", false, SelectionCategory.CategorySize.Medium) {
                new SelectionEntry("Absolute value of x", 15, false),
                new SelectionEntry("The minimum between x and y", 16, false),
                new SelectionEntry("The maximum between x and y", 17, false),
                new SelectionEntry("1 if x < y; 0 otherwise", 18, false),
                new SelectionEntry("1 if x > y; 0 otherwise", 19, false),
                new SelectionEntry("Sign of x", 20, false),
                new SelectionEntry("1 if the distance between x and y is less than tolerance; 0 otherwise", 21, false),
                new SelectionEntry("The minimum between x and y with smoothing", 22, false),
                new SelectionEntry("The maximum between x and y with smoothing", 23, false),
            },
            new SelectionCategory("Conversion", true, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Converts degrees to radians", 39, false),
                new SelectionEntry("Converts radians to degrees", 40, false),
            },
            new SelectionCategory("Trigonometry", true, SelectionCategory.CategorySize.Normal) {
                new SelectionEntry("Sine of x (in radians)", 32, false),
                new SelectionEntry("Cosine of x (in radians)", 33, false),
                new SelectionEntry("Tangent of x (in radians)", 34, false),
                new SelectionEntry("Inverse sine of x (in radians)", 35, true),
                new SelectionEntry("Inverse cosine of x (in radians)", 36, false),
                new SelectionEntry("Inverse tangent of x (in radians)", 37, false),
                new SelectionEntry("Inverse tangent of x / y (in radians)", 38, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Vector Math", EditorView.DefaultNodePosition);

            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("X", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(x, Which.X));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Y", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(y, Which.Y));
            (wrapMaxPort, wrapMaxField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Max", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateValue(wrapMax, Which.WrapMax));
            (iorPort, iorField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("IOR", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(ior, Which.IOR));
            (scalePort, scaleField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Scale", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(scale, Which.Scale));
            (distancePort, distanceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Distance", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(distance, Which.Distance));
            
            vectorResultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener, this);
            floatResultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);

            operationButton = new EnumSelectionButton<Operation>(operation, mathOperationTree);
            operationButton.RegisterCallback<ChangeEvent<Operation>>(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = evt.newValue;
                RuntimeNode.UpdateOperation(operation);
                OnOperationChanged();
            });

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                x = evt.newValue;
                RuntimeNode.UpdateValue(x, Which.X);
            });

            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                y = evt.newValue;
                RuntimeNode.UpdateValue(y, Which.Y);
            });
            
            wrapMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change tolerance");
                wrapMax = evt.newValue;
                RuntimeNode.UpdateValue(wrapMax, Which.WrapMax);
            });
            
            iorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                ior = evt.newValue;
                RuntimeNode.UpdateValue(ior, Which.IOR);
            });

            scaleField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                scale = evt.newValue;
                RuntimeNode.UpdateValue(scale, Which.Scale);
            });
            
            distanceField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                distance = evt.newValue;
                RuntimeNode.UpdateValue(distance, Which.Distance);
            });
            
            iorPort.Add(iorField);
            scalePort.Add(scaleField);
            distancePort.Add(distanceField); 
            
            inputContainer.Add(operationButton);
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

        public override void BindPorts() {
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(wrapMaxPort, RuntimeNode.WrapMaxPort);
            BindPort(iorPort, RuntimeNode.IorPort);
            BindPort(scalePort, RuntimeNode.ScalePort);
            BindPort(distancePort, RuntimeNode.DistancePort);
            BindPort(vectorResultPort, RuntimeNode.VectorResultPort);
            BindPort(floatResultPort, RuntimeNode.FloatResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

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
            var showIOR = operation == Operation.Refract;
            var showWrapMax = operation == Operation.Wrap;
            var showDistance = operation == Operation.SmoothMinimum || operation == Operation.SmoothMaximum;
            var showScale = operation == Operation.Scale;
            var showY =
                !(operation == Operation.Length || operation == Operation.LengthSquared || operation == Operation.Scale || operation == Operation.Sine || 
                  operation == Operation.Cosine || operation == Operation.Tangent || operation == Operation.Arcsine || operation == Operation.Arccosine || 
                  operation == Operation.Arctangent || operation == Operation.Fraction || operation == Operation.Ceil || operation == Operation.Floor || 
                  operation == Operation.Absolute || operation == Operation.Normalize);

            var showFloatOutput = operation == Operation.Length || operation == Operation.LengthSquared || operation == Operation.Distance || 
                                  operation == Operation.DistanceSquared || operation == Operation.DotProduct;

            if (showIOR) iorPort.Show();
            else iorPort.HideAndDisconnect();

            if (showWrapMax) wrapMaxPort.Show();
            else wrapMaxPort.HideAndDisconnect();

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

        public override void SetNodeData(JObject jsonData) {
            operation = (Operation)jsonData.Value<int>("o");
            x = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("x"), float3Converter.Converter);
            y = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("y"), float3Converter.Converter);
            wrapMax = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("w"), float3Converter.Converter);
            ior = jsonData.Value<float>("i");
            scale = jsonData.Value<float>("s");
            distance = jsonData.Value<float>("d");
            
            operationButton.SetValueWithoutNotify(operation, 1);
            xField.SetValueWithoutNotify(x);
            yField.SetValueWithoutNotify(y);
            wrapMaxField.SetValueWithoutNotify(wrapMax);
            iorField.SetValueWithoutNotify(ior);
            scaleField.SetValueWithoutNotify(scale);
            distanceField.SetValueWithoutNotify(distance); 
            
            RuntimeNode.UpdateOperation(operation);
            RuntimeNode.UpdateValue(x, Which.X);
            RuntimeNode.UpdateValue(y, Which.Y);
            RuntimeNode.UpdateValue(wrapMax, Which.WrapMax);
            RuntimeNode.UpdateValue(ior, Which.IOR);
            RuntimeNode.UpdateValue(scale, Which.Scale);
            RuntimeNode.UpdateValue(distance, Which.Distance);

            OnOperationChanged();

            base.SetNodeData(jsonData);
        }
    }
}