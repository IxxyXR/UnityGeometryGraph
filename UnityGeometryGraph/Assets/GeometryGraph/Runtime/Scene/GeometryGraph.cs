﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;
using UnityEngine.Rendering;

namespace GeometryGraph.Runtime {
    public partial class GeometryGraph : MonoBehaviour {
        // References
        [SerializeField] private RuntimeGraphObject graph;
        [SerializeField] private MeshFilter meshFilter;

        // Scene data
        [SerializeField] private string graphGuid;
        [SerializeField] private GeometryGraphSceneData sceneData = new();
        [SerializeField] private CurveVisualizerSettings curveVisualizerSettings = new();
        [SerializeField] private InstancedGeometrySettings instancedGeometrySettings = new();

        // Evaluation data
        [SerializeField] private CurveData curveData;
        [SerializeField] private GeometryData geometryData;
        [SerializeField] private InstancedGeometryData instancedGeometryData;
        [SerializeField] private int instancedGeometryHashCode;

        [SerializeField] private bool realtimeEvaluation;
        [SerializeField] private bool realtimeEvaluationAsync;

        // Exporter fields
        private readonly GeometryExporter exporter = new();
        [SerializeField] private BakedInstancedGeometry bakedInstancedGeometry = new();
        [SerializeField] private bool initializedMeshFilter;
        [SerializeField] private Mesh meshFilterMesh;

        private bool isAsyncEvaluationComplete = true;

        internal RuntimeGraphObject Graph => graph;
        internal GeometryGraphSceneData SceneData => sceneData;
        internal InstancedGeometrySettings InstancedGeometrySettings => instancedGeometrySettings;

        internal string GraphGuid {
            get => graphGuid;
            set => graphGuid = value;
        }

        internal void OnPropertiesChanged(int newPropertyHashCode) {
            sceneData.PropertyHashCode = newPropertyHashCode;
            List<string> toRemove = SceneData.PropertyData.Keys.Where(key => Graph.RuntimeData.Properties.All(property => !string.Equals(property.Guid, key, StringComparison.InvariantCulture))).ToList();
            foreach (string key in toRemove) {
                SceneData.RemoveProperty(key);
            }

            foreach (Property property in Graph.RuntimeData.Properties) {
                if (!SceneData.PropertyData.ContainsKey(property.Guid)) {
                    SceneData.AddProperty(property.Guid, property.ReferenceName, new PropertyValue(property));
                }
                SceneData.EnsureCorrectReferenceName(property.Guid, property.ReferenceName);
            }

            SceneData.EnsureCorrectGuidsAndReferences(Graph.RuntimeData.Properties);
        }

        internal void OnDefaultPropertyValueChanged(Property property) {
            sceneData.UpdatePropertyDefaultValue(property.Guid, property.DefaultValue);
        }

        private void HandleEvaluationResult(GeometryGraphEvaluationResult evaluationResult, bool export = true) {
            curveData = evaluationResult.CurveData;
            geometryData = evaluationResult.GeometryData;
            instancedGeometryData = evaluationResult.InstancedGeometryData;

            if (!initializedMeshFilter) {
                Debug.Log("Initializing mesh filter");
                InitializeMeshFilter();
            }

            if (initializedMeshFilter && export) {
                exporter.Export(geometryData, meshFilterMesh);
            }

            if (instancedGeometryData != null) {
                BakeInstancedGeometry();
                instancedGeometryHashCode = instancedGeometryData.CalculateHashCode(GeometryData.HashCodeDepth.AttributeCount);
            } else {
                instancedGeometryHashCode = 0;
            }
        }

        private void InitializeMeshFilter() {
            if (meshFilter == null) return;

            meshFilterMesh = new Mesh {
                indexFormat = IndexFormat.UInt32,
                name = "GeometryGraph Mesh"
            };
            meshFilterMesh.MarkDynamic();

            if (meshFilter.sharedMesh != null) {
                DestroyImmediate(meshFilter.sharedMesh);
            }
            meshFilter.sharedMesh = meshFilterMesh;

            initializedMeshFilter = true;
        }

        private void BakeInstancedGeometry() {
            bool sameGeometry = instancedGeometryHashCode == instancedGeometryData.CalculateHashCode(GeometryData.HashCodeDepth.AttributeCount);

            if (!sameGeometry) {
                foreach (Mesh mesh in bakedInstancedGeometry.Meshes) {
                    mesh.Clear();
                }

                if (bakedInstancedGeometry.Meshes.Count > instancedGeometryData.GeometryCount) {
                    int toRemove = bakedInstancedGeometry.Meshes.Count - instancedGeometryData.GeometryCount;
                    for (int i = 0; i < toRemove; i++) {
#if UNITY_EDITOR
                        if (!Application.isPlaying) DestroyImmediate(bakedInstancedGeometry.Meshes[i]);
                        else Destroy(bakedInstancedGeometry.Meshes[i]);
#else
                        Destroy(bakedInstancedGeometry.Meshes[i]);
#endif
                    }

                    bakedInstancedGeometry.Meshes.RemoveRange(instancedGeometryData.GeometryCount, toRemove);
                } else if (bakedInstancedGeometry.Meshes.Count < instancedGeometryData.GeometryCount) {
                    int toAdd = instancedGeometryData.GeometryCount - bakedInstancedGeometry.Meshes.Count;
                    for (int i = 0; i < toAdd; i++) {
                        Mesh mesh = new Mesh {
                            indexFormat = IndexFormat.UInt32,
                            name = "GeometryGraph Instanced Mesh"
                        };
                        mesh.MarkDynamic();
                        bakedInstancedGeometry.Meshes.Add(mesh);
                    }
                }
            }
            bakedInstancedGeometry.Matrices.Clear();

            for (int i = 0; i < instancedGeometryData.GeometryCount; i++) {
                GeometryData geometry = instancedGeometryData.Geometry(i);

                if (!sameGeometry) {
                    Mesh mesh = bakedInstancedGeometry.Meshes[i];
                    exporter.Export(geometry, mesh);
                }

                Matrix4x4[] matrices = instancedGeometryData
                                       .TransformData(i)
                                       .Select(t => transform.localToWorldMatrix * (Matrix4x4) t.Matrix)
                                       .ToArray();
                if (matrices.Length <= 1023) {
                    bakedInstancedGeometry.Matrices.Add(new[] { matrices });
                    continue;
                }

                int drawCallCount = matrices.Length / 1023;
                if (matrices.Length % 1023 != 0) {
                    drawCallCount++;
                }

                Matrix4x4[][] arrays = new Matrix4x4[drawCallCount][];
                for (int j = 0; j < drawCallCount; j++) {
                    int start = j * 1023;
                    int end = Math.Min(start + 1023, matrices.Length);
                    arrays[j] = matrices.Skip(start).Take(end - start).ToArray();
                }

                bakedInstancedGeometry.Matrices.Add(arrays);
            }
        }

        private void OnDrawGizmos() {
            if (!curveVisualizerSettings.Enabled || curveData == null || curveData.Type == CurveType.None) return;

            Handles.matrix = Gizmos.matrix = transform.localToWorldMatrix;

            if (curveVisualizerSettings.ShowPoints || curveVisualizerSettings.ShowSpline) {
                Vector3[] points = curveData.Position.Select(float3 => (Vector3)float3).ToArray();
                Handles.color = curveVisualizerSettings.SplineColor;
                if (curveVisualizerSettings.ShowSpline) {
                    Handles.DrawAAPolyLine(curveVisualizerSettings.SplineWidth, points);
                    if (curveData.IsClosed) Handles.DrawAAPolyLine(curveVisualizerSettings.SplineWidth, points[0], points[^1]);
                }

                if (curveVisualizerSettings.ShowPoints) {
                    Gizmos.color = curveVisualizerSettings.PointColor;
                    foreach (Vector3 p in points) {
                        Gizmos.DrawSphere(p, curveVisualizerSettings.PointSize);
                    }
                }
            }

            if (curveVisualizerSettings.ShowDirectionVectors) {
                for (int i = 0; i < curveData.Position.Count; i++) {
                    float3 p = curveData.Position[i];
                    float3 t = curveData.Tangent[i];
                    float3 n = curveData.Normal[i];
                    float3 b = curveData.Binormal[i];

                    Handles.color = curveVisualizerSettings.DirectionTangentColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + t * curveVisualizerSettings.DirectionVectorLength);
                    Handles.color = curveVisualizerSettings.DirectionNormalColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + n * curveVisualizerSettings.DirectionVectorLength);
                    Handles.color = curveVisualizerSettings.DirectionBinormalColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + b * curveVisualizerSettings.DirectionVectorLength);
                }
            }
        }

        private void EnsureCorrectState() {
            int graphPropertyHashCode = graph.RuntimeData.PropertyHashCode;
            if (sceneData.PropertyHashCode != graphPropertyHashCode) {
                OnPropertiesChanged(graphPropertyHashCode);
            }
        }
    }
}