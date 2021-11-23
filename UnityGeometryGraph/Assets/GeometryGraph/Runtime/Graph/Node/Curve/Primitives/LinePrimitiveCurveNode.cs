﻿using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class LinePrimitiveCurveNode : RuntimeNode {
        private MinMaxInt points = new MinMaxInt(2, Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
        private float3 start = float3.zero;
        private float3 end = float3_ext.right;
        private CurveData curve;

        public RuntimePort PointsPort { get; private set; }
        public RuntimePort StartPort { get; private set; }
        public RuntimePort EndPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public LinePrimitiveCurveNode(string guid) : base(guid) {
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            StartPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            EndPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Curve, PortDirection.Output, this);
        }

        private void CalculateResult() {
            if (RuntimeGraphObjectData.IsDuringSerialization) {
                DebugUtility.Log("Attempting to generate curve during serialization. Aborting.");
                curve = null;
                return;
            }

            curve = CurvePrimitive.Line(points - 1, start, end);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == PointsPort) {
                int newValue = GetValue(connection, (int)points);
                if (newValue == points) return;
                points.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == StartPort) {
                float3 newValue = GetValue(connection, start);
                if (newValue.Equals(start)) return;
                start = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == EndPort) {
                float3 newValue = GetValue(connection, end);
                if (newValue.Equals(end)) return;
                end = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            if (curve == null) CalculateResult();
            return curve == null ? CurveData.Empty : curve.Clone();
        }

        public override string GetCustomData() {
            JArray array = new JArray {
                (int)points,
                JsonConvert.SerializeObject(start, float3Converter.Converter),
                JsonConvert.SerializeObject(end, float3Converter.Converter),
            };
            return array.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            JArray data = JArray.Parse(json);
            points = new MinMaxInt(data.Value<int>(0), Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            start = JsonConvert.DeserializeObject<float3>(data.Value<string>(1)!, float3Converter.Converter);
            end = JsonConvert.DeserializeObject<float3>(data.Value<string>(2)!, float3Converter.Converter);
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdatePoints(int newPoints) {
            newPoints = newPoints.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            if (newPoints == points) return;

            points.Value = newPoints;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateStart(float3 newStart) {
            if (start.Equals(newStart)) return;
            start = newStart;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateEnd(float3 newEnd) {
            if (end.Equals(newEnd)) return;
            end = newEnd;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
    }
}