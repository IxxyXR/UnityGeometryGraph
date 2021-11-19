// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::RotateVectorNode")]
    public partial class RotateVectorNode : RuntimeNode {
        public RuntimePort VectorPort { get; }
        public RuntimePort CenterPort { get; }
        public RuntimePort AxisPort { get; }
        public RuntimePort EulerAnglesPort { get; }
        public RuntimePort AnglePort { get; }
        public RuntimePort ResultPort { get; }

        public RotateVectorNode(string guid) : base(guid) {
            VectorPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            CenterPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AxisPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            EulerAnglesPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AnglePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
        }

        public void UpdateVector(float3 newValue) {
            if(math.distancesq(Vector, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
            Vector = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateCenter(float3 newValue) {
            if(math.distancesq(Center, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
            Center = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAxis(float3 newValue) {
            if(math.distancesq(Axis, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
            Axis = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateEulerAngles(float3 newValue) {
            if(math.distancesq(EulerAngles, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
            EulerAngles = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateAngle(float newValue) {
            if(Math.Abs(Angle - newValue) < Constants.FLOAT_TOLERANCE) return;
            Angle = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateMode(RotateVectorNode_Mode newValue) {
            if(Mode == newValue) return;
            Mode = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) {
                return Mode switch {
                    RotateVectorNode_Mode.AxisAngle => math.rotate(quaternion.AxisAngle(Axis, Angle), Vector - Center) + Center,
                    RotateVectorNode_Mode.Euler => math.rotate(quaternion.Euler(EulerAngles), Vector - Center) + Center,
                    RotateVectorNode_Mode.X_Axis => math.rotate(quaternion.AxisAngle(float3_ext.right, Angle), Vector - Center) + Center,
                    RotateVectorNode_Mode.Y_Axis => math.rotate(quaternion.AxisAngle(float3_ext.up, Angle), Vector - Center) + Center,
                    RotateVectorNode_Mode.Z_Axis => math.rotate(quaternion.AxisAngle(float3_ext.forward, Angle), Vector - Center) + Center,
                    _ => throw new ArgumentOutOfRangeException(nameof(Mode), Mode, null)
                };
            }
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == VectorPort) {
                var newValue = GetValue(connection, Vector);
                if(math.distancesq(Vector, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                Vector = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == CenterPort) {
                var newValue = GetValue(connection, Center);
                if(math.distancesq(Center, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                Center = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == AxisPort) {
                var newValue = GetValue(connection, Axis);
                if(math.distancesq(Axis, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                Axis = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == EulerAnglesPort) {
                var newValue = GetValue(connection, EulerAngles);
                if(math.distancesq(EulerAngles, newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                EulerAngles = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == AnglePort) {
                var newValue = GetValue(connection, Angle);
                if(Math.Abs(Angle - newValue) < Constants.FLOAT_TOLERANCE) return;
                Angle = newValue;
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                JsonConvert.SerializeObject(Vector, float3Converter.Converter),
                JsonConvert.SerializeObject(Center, float3Converter.Converter),
                JsonConvert.SerializeObject(Axis, float3Converter.Converter),
                JsonConvert.SerializeObject(EulerAngles, float3Converter.Converter),
                Angle,
                (int)Mode,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            Vector = JsonConvert.DeserializeObject<float3>(array.Value<string>(0), float3Converter.Converter);
            Center = JsonConvert.DeserializeObject<float3>(array.Value<string>(1), float3Converter.Converter);
            Axis = JsonConvert.DeserializeObject<float3>(array.Value<string>(2), float3Converter.Converter);
            EulerAngles = JsonConvert.DeserializeObject<float3>(array.Value<string>(3), float3Converter.Converter);
            Angle = array.Value<float>(4);
            Mode = (RotateVectorNode_Mode) array.Value<int>(5);

            NotifyPortValueChanged(ResultPort);
        }
    }
}