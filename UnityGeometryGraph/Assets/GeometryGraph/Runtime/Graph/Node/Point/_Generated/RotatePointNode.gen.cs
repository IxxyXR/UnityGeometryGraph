// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using GeometryGraph.Runtime.Serialization;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::RotatePointNode")]
    public partial class RotatePointNode : RuntimeNode {
        public RuntimePort InputPort { get; }
        public RuntimePort RotationPort { get; }
        public RuntimePort RotationAttributePort { get; }
        public RuntimePort AxisPort { get; }
        public RuntimePort AxisAttributePort { get; }
        public RuntimePort AnglePort { get; }
        public RuntimePort AngleAttributePort { get; }
        public RuntimePort ResultPort { get; }

        public RotatePointNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            RotationPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            RotationAttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            AxisPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AxisAttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            AnglePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            AngleAttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateInput(GeometryData newValue) {
            Input = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateRotation(float3 newValue) {
            if(math.distancesq(Rotation, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
            Rotation = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateRotationAttribute(string newValue) {
            if(string.Equals(RotationAttribute, newValue, StringComparison.InvariantCulture)) return;
            RotationAttribute = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAxis(float3 newValue) {
            if(math.distancesq(Axis, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
            Axis = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAxisAttribute(string newValue) {
            if(string.Equals(AxisAttribute, newValue, StringComparison.InvariantCulture)) return;
            AxisAttribute = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAngle(float newValue) {
            if(Math.Abs(Angle - newValue) < Constants.FLOAT_TOLERANCE) return;
            Angle = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAngleAttribute(string newValue) {
            if(string.Equals(AngleAttribute, newValue, StringComparison.InvariantCulture)) return;
            AngleAttribute = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateRotationMode(RotatePointNode_RotationMode newValue) {
            if(RotationMode == newValue) return;
            RotationMode = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAxisMode(RotatePointNode_AxisMode newValue) {
            if(AxisMode == newValue) return;
            AxisMode = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAngleMode(RotatePointNode_AngleMode newValue) {
            if(AngleMode == newValue) return;
            AngleMode = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateRotationType(RotatePointNode_RotationType newValue) {
            if(RotationType == newValue) return;
            RotationType = newValue;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) return Result;
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == InputPort) {
                var newValue = GetValue(connection, Input);
                Input = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RotationPort) {
                var newValue = GetValue(connection, Rotation);
                if(math.distancesq(Rotation, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                Rotation = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RotationAttributePort) {
                var newValue = GetValue(connection, RotationAttribute);
                if(string.Equals(RotationAttribute, newValue, StringComparison.InvariantCulture)) return;
                RotationAttribute = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AxisPort) {
                var newValue = GetValue(connection, Axis);
                if(math.distancesq(Axis, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                Axis = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AxisAttributePort) {
                var newValue = GetValue(connection, AxisAttribute);
                if(string.Equals(AxisAttribute, newValue, StringComparison.InvariantCulture)) return;
                AxisAttribute = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AnglePort) {
                var newValue = GetValue(connection, Angle);
                if(Math.Abs(Angle - newValue) < Constants.FLOAT_TOLERANCE) return;
                Angle = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AngleAttributePort) {
                var newValue = GetValue(connection, AngleAttribute);
                if(string.Equals(AngleAttribute, newValue, StringComparison.InvariantCulture)) return;
                AngleAttribute = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                JsonConvert.SerializeObject(Rotation, float3Converter.Converter),
                RotationAttribute,
                JsonConvert.SerializeObject(Axis, float3Converter.Converter),
                AxisAttribute,
                Angle,
                AngleAttribute,
                (int)RotationMode,
                (int)AxisMode,
                (int)AngleMode,
                (int)RotationType,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            Rotation = JsonConvert.DeserializeObject<float3>(array.Value<string>(0), float3Converter.Converter);
            RotationAttribute = array.Value<string>(1);
            Axis = JsonConvert.DeserializeObject<float3>(array.Value<string>(2), float3Converter.Converter);
            AxisAttribute = array.Value<string>(3);
            Angle = array.Value<float>(4);
            AngleAttribute = array.Value<string>(5);
            RotationMode = (RotatePointNode_RotationMode) array.Value<int>(6);
            AxisMode = (RotatePointNode_AxisMode) array.Value<int>(7);
            AngleMode = (RotatePointNode_AngleMode) array.Value<int>(8);
            RotationType = (RotatePointNode_RotationType) array.Value<int>(9);

            Calculate();
            NotifyPortValueChanged(ResultPort);
        }
    }
}