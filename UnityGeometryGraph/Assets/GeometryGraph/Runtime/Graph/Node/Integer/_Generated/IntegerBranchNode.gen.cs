// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GeometryGraph Code Generator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

using GeometryGraph.Runtime.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.Graph {
    [SourceClass("GeometryGraph.Runtime::GeometryGraph.Runtime.Graph::IntegerBranchNode")]
    public partial class IntegerBranchNode : RuntimeNode {
        public RuntimePort ConditionPort { get; }
        public RuntimePort IfTruePort { get; }
        public RuntimePort IfFalsePort { get; }
        public RuntimePort ResultPort { get; }

        public IntegerBranchNode(string guid) : base(guid) {
            ConditionPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            IfTruePort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            IfFalsePort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }

        public void UpdateCondition(bool newValue) {
            if(Condition == newValue) return;
            Condition = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfTrue(int newValue) {
            if(IfTrue == newValue) return;
            IfTrue = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateIfFalse(int newValue) {
            if(IfFalse == newValue) return;
            IfFalse = newValue;
            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port == ResultPort) return Condition ? IfTrue : IfFalse;
            return null;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == ConditionPort) {
                var newValue = GetValue(connection, Condition);
                if(Condition == newValue) return;
                Condition = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == IfTruePort) {
                var newValue = GetValue(connection, IfTrue);
                if(IfTrue == newValue) return;
                IfTrue = newValue;
                NotifyPortValueChanged(ResultPort);
            } else if (port == IfFalsePort) {
                var newValue = GetValue(connection, IfFalse);
                if(IfFalse == newValue) return;
                IfFalse = newValue;
                NotifyPortValueChanged(ResultPort);
            }
        }

        public override string GetCustomData() {
            return new JArray {
                Condition ? 1 : 0,
                IfTrue,
                IfFalse,
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string data) {
            JArray array = JArray.Parse(data);
            Condition = array.Value<int>(0) == 1;
            IfTrue = array.Value<int>(1);
            IfFalse = array.Value<int>(2);

            NotifyPortValueChanged(ResultPort);
        }
    }
}