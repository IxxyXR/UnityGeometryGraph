﻿using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    public class GeometryObjectPropertyNode : RuntimeNode {
        private static readonly GeometryData defaultValue = GeometryData.Empty;
        
        [SerializeReference] public Property Property;
       
        public RuntimePort Port { get; private set; }
        public string PropertyGuid { get; private set; }
        
        public GeometryObjectPropertyNode(string guid) : base(guid) {
            Port = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }
        
        public override void RebindPorts() {
            Port = Ports[0];
        }

        public override object GetValueForPort(RuntimePort port) {
            var value = (GeometryObject)Property?.Value;
            return value == null ? defaultValue.Clone() : value.Geometry;
        }

        public override string GetCustomData() {
            return new JObject {
                ["p"] = Property.Guid
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            var jsonObject = JObject.Parse(json);
            PropertyGuid = jsonObject.Value<string>("p");
        }
    }
}