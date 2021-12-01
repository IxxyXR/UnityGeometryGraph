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

        // Evaluation data
        [SerializeField] private CurveData curveData;
        [SerializeField] private GeometryData geometryData;
        [SerializeField] private InstancedGeometryData instancedGeometryData;

        [SerializeField] private bool realtimeEvaluation;
        [SerializeField] private bool realtimeEvaluationAsync;
        
        // Exporter fields
        [SerializeField] private MeshPool meshPool = new (IndexFormat.UInt32);
        [SerializeField] private GeometryExporter exporter = new GeometryExporter();
        [SerializeField] private List<Mesh> bakedInstancedGeometry = new();
        [SerializeField] private bool initializedMeshFilter;
        [SerializeField] private Mesh meshFilterMesh;
        


        private bool isAsyncEvaluationComplete = true;
        
        internal RuntimeGraphObject Graph => graph;
        internal GeometryGraphSceneData SceneData => sceneData;
        
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

        private void HandleEvaluationResult(GeometryGraphEvaluationResult evaluationResult) {
            curveData = evaluationResult.CurveData;
            geometryData = evaluationResult.GeometryData;
            instancedGeometryData = evaluationResult.InstancedGeometryData;

            if (meshFilter != null && !initializedMeshFilter) {
                if (meshFilter.sharedMesh != null) {
                    meshFilterMesh = meshFilter.sharedMesh;
                } else {
                    meshFilterMesh = new Mesh();
                    meshFilter.sharedMesh = meshFilterMesh;
                }
                meshFilterMesh.indexFormat = IndexFormat.UInt32;
                meshFilterMesh.name = "GeometryGraph Mesh";
                initializedMeshFilter = true;
            }
            
            exporter ??= new GeometryExporter();
            
            if (meshFilter != null) {
                exporter.Export(geometryData, meshFilterMesh);
            }

            if (instancedGeometryData != null) {
                BakeInstancedGeometry();
            }
        }

        private void BakeInstancedGeometry() {
            // TODO(#16): Implement GetHashCode() in GeometryData and check if the baked geometry is the same as the new geometry before re-baking
            foreach (Mesh mesh in bakedInstancedGeometry) {
                meshPool.Return(mesh);
            }
            bakedInstancedGeometry.Clear();
            for (int i = 0; i < instancedGeometryData.GeometryCount; i++) {
                GeometryData geometry = instancedGeometryData.Geometry(i);
                Mesh mesh = meshPool.Get();
                exporter.Export(geometry, mesh);
                bakedInstancedGeometry.Add(mesh);
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

    }
}