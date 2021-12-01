﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class IntegerPropertyNode : RuntimeNode {
        [SerializeReference] public Property Property;
       
        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }
        
        public IntegerPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Integer, PortDirection.Output, this);
        }
        
        protected override object GetValueForPort(RuntimePort port) {
            DebugUtility.Log($"Returning property value {Property} => [{Property?.Value}]");
            return Property.GetValueOrDefault<int>(Property, 0);
        }

        public override string Serialize() {
            return new JObject {
                ["p"] = Property?.Guid
            }.ToString(Formatting.None);
        }

        public override void Deserialize(string json) {
            JObject jsonObject = JObject.Parse(json);
            PropertyGuid = jsonObject.Value<string>("p");
        }
    }
}