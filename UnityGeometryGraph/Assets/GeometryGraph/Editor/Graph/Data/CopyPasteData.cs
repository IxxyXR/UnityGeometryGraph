using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GeometryGraph.Editor {
    [Serializable]
    public class CopyPasteData : ISerializationCallbackReceiver {
        [NonSerialized] private HashSet<SerializedNode> nodes = new();
        [NonSerialized] private HashSet<SerializedEdge> edges = new();
        [NonSerialized] private HashSet<AbstractProperty> properties = new();
        // these are the properties that don't get copied but are required by property nodes that get copied
        [NonSerialized] private HashSet<AbstractProperty> metaProperties = new();

        [SerializeField] private List<SerializedNode> serializedNodes = new();
        [SerializeField] private List<SerializedEdge> serializedEdges = new();
        [SerializeField] private List<SerializedProperty> serializedProperties = new();
        [SerializeField] private List<SerializedProperty> serializedMetaProperties = new();

        public IEnumerable<SerializedNode> Nodes => nodes;
        public IEnumerable<SerializedEdge> Edges => edges;
        public IEnumerable<SerializedNode> SerializedNodes => serializedNodes;
        public IEnumerable<SerializedEdge> SerializedEdges => serializedEdges;
        public IEnumerable<SerializedProperty> SerializedProperties => serializedProperties;
        public IEnumerable<SerializedProperty> SerializedMetaProperties => serializedMetaProperties;
        public IEnumerable<AbstractProperty> Properties => properties;
        public IEnumerable<AbstractProperty> MetaProperties => metaProperties;

        private EditorView editorView;

        public CopyPasteData(EditorView editorView, IEnumerable<SerializedNode> nodes, IEnumerable<SerializedEdge> edges, IEnumerable<AbstractProperty> properties, IEnumerable<AbstractProperty> metaProperties) {
            this.editorView = editorView;

            foreach (SerializedNode node in nodes) {
                AddNode(node);
                foreach (SerializedEdge edge in GetAllEdgesForNode(node)) {
                    AddEdge(edge);
                }
            }

            foreach (SerializedEdge edge in edges) {
                AddEdge(edge);
            }

            foreach (AbstractProperty property in properties) {
                AddProperty(property);
            }

            foreach (AbstractProperty metaProperty in metaProperties) {
                AddMetaProperty(metaProperty);
            }
        }

        private void AddNode(SerializedNode node) {
            nodes.Add(node);
        }

        private void AddEdge(SerializedEdge edge) {
            edges.Add(edge);
        }
        private void AddProperty(AbstractProperty property) {
            properties.Add(property);
        }
        private void AddMetaProperty(AbstractProperty property) {
            metaProperties.Add(property);
        }

        public void OnBeforeSerialize() {
            serializedNodes = new List<SerializedNode>();
            foreach (SerializedNode node in nodes) {
                serializedNodes.Add(node);
            }

            serializedEdges = new List<SerializedEdge>();
            foreach (SerializedEdge edge in edges) {
                serializedEdges.Add(edge);
            }

            serializedProperties = new List<SerializedProperty>();
            foreach (AbstractProperty property in properties) {
                serializedProperties.Add(new SerializedProperty(property));
            }

            serializedMetaProperties = new List<SerializedProperty>();
            foreach (AbstractProperty property in metaProperties) {
                serializedMetaProperties.Add(new SerializedProperty(property));
            }
        }

        public void OnAfterDeserialize() {
            nodes = new HashSet<SerializedNode>();
            edges = new HashSet<SerializedEdge>();
            properties = new HashSet<AbstractProperty>();
            metaProperties = new HashSet<AbstractProperty>();
            foreach (SerializedNode node in serializedNodes) {
                nodes.Add(node);
            }
            foreach (SerializedEdge edge in serializedEdges) {
                edges.Add(edge);
            }
            foreach (SerializedProperty prop in serializedProperties) {
                properties.Add(prop.Deserialize());
            }
            foreach (SerializedProperty prop in serializedMetaProperties) {
                metaProperties.Add(prop.Deserialize());
            }
        }

        private IEnumerable<SerializedEdge> GetAllEdgesForNode(SerializedNode node) {
            List<SerializedEdge> edges = new();
            foreach (IEnumerable<Edge> portConnections in node.GuidPortDictionary.Values.Select(port => port.connections)) {
                edges.AddRange(portConnections.Select(edge => edge.userData).OfType<SerializedEdge>());
            }
            return edges;
        }

        public static CopyPasteData FromJson(string json) {
            try {
                return JsonUtility.FromJson<CopyPasteData>(json);
            } catch {
                // ignored. just means json was not a CopyPasteData object
                return null;
            }
        }
    }
}