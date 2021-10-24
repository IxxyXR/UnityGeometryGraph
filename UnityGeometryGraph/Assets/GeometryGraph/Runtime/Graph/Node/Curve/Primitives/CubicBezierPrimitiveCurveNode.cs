﻿using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class CubicBezierPrimitiveCurveNode : RuntimeNode {
        private MinMaxInt points = new MinMaxInt(32, Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
        private bool isClosed;
        private float3 start = float3.zero;
        private float3 controlA = float3_util.forward;
        private float3 controlB = float3_util.right + float3_util.forward;
        private float3 end = float3_util.right;
        
        private CurveData curve;

        public RuntimePort PointsPort { get; private set; }
        public RuntimePort ClosedPort { get; private set; }
        public RuntimePort StartPort { get; private set; }
        public RuntimePort ControlAPort { get; private set; }
        public RuntimePort ControlBPort { get; private set; }
        public RuntimePort EndPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CubicBezierPrimitiveCurveNode(string guid) : base(guid) {
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ClosedPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            StartPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ControlAPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ControlBPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            EndPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Curve, PortDirection.Output, this);
        }

        private void CalculateResult() {
            curve = CurvePrimitive.CubicBezier(points - 1, isClosed, start, controlA, controlB, end);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == PointsPort) {
                var newValue = GetValue(connection, (int)points);
                if (newValue == points) return;
                points.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ClosedPort) {
                var newValue = GetValue(connection, isClosed);
                if (newValue == isClosed) return;
                isClosed = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == StartPort) {
                var newValue = GetValue(connection, start);
                if (newValue.Equals(start)) return;
                start = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ControlAPort) {
                var newValue = GetValue(connection, controlA);
                if (newValue.Equals(controlA)) return;
                controlA = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ControlBPort) {
                var newValue = GetValue(connection, controlB);
                if (newValue.Equals(controlB)) return;
                controlB = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == EndPort) {
                var newValue = GetValue(connection, end);
                if (newValue.Equals(end)) return;
                end = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            if (curve == null) CalculateResult();
            return curve.Clone();
        }

        public override string GetCustomData() {
            var array = new JArray {
                (int)points,
                isClosed ? 1 : 0,
                JsonConvert.SerializeObject(start, float3Converter.Converter),
                JsonConvert.SerializeObject(controlA, float3Converter.Converter),
                JsonConvert.SerializeObject(controlB, float3Converter.Converter),
                JsonConvert.SerializeObject(end, float3Converter.Converter),
            };
            return array.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            var data = JArray.Parse(json);
            points = new MinMaxInt(data.Value<int>(0), Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            isClosed = data.Value<int>(1) == 1;
            start = JsonConvert.DeserializeObject<float3>(data.Value<string>(2)!, float3Converter.Converter);
            controlA = JsonConvert.DeserializeObject<float3>(data.Value<string>(3)!, float3Converter.Converter);
            controlB = JsonConvert.DeserializeObject<float3>(data.Value<string>(4)!, float3Converter.Converter);
            end = JsonConvert.DeserializeObject<float3>(data.Value<string>(5)!, float3Converter.Converter);
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdatePoints(int newValue) {
            newValue = newValue.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            if (newValue == points) return;
            
            points.Value = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateClosed(bool newValue) {
            if (newValue == isClosed) return;
            isClosed = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateStart(float3 newValue) {
            if (start.Equals(newValue)) return;
            start = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdateControlA(float3 newValue) {
            if (controlA.Equals(newValue)) return;
            controlA = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdateControlB(float3 newValue) {
            if (controlB.Equals(newValue)) return;
            controlB = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateEnd(float3 newValue) {
            if (end.Equals(newValue)) return;
            end = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public override void RebindPorts() {
            throw new System.NotImplementedException();
        }
    }
}