﻿using System;
using System.Linq;
using System.Collections.Generic;
using GeometryGraph.Runtime.Geometry;
using UnityEngine;

namespace GeometryGraph.Runtime.Data {
    [Serializable]
    public class InstancedGeometryData {
        [SerializeField] private List<GeometryData> geometryData;
        [SerializeField] private List<InstancedTransformData> transformData;
        [SerializeField] private List<int> transformCount;
        
        public int GeometryCount => geometryData.Count;
        public int TransformCount(int geometryIndex) => geometryIndex.InRange(transformCount) ? transformCount[geometryIndex] : MiscUtilities.CallThenReturn(() => Debug.LogException(new IndexOutOfRangeException($"Index {geometryIndex} out of range [0, {transformCount.Count - 1}]")), 0); 
        public GeometryData Geometry(int geometryIndex) => geometryIndex.InRange(geometryData) ? geometryData[geometryIndex] : MiscUtilities.CallThenReturn(() => Debug.LogException(new IndexOutOfRangeException($"Index {geometryIndex} out of range [0, {geometryData.Count - 1}]")), (GeometryData)null);
        public IEnumerable<InstancedTransformData> TransformData(int geometryIndex) => geometryIndex.InRange(transformData) ? GetTransformsImpl(geometryIndex) : MiscUtilities.CallThenReturn(() => Debug.LogException(new IndexOutOfRangeException($"Index {geometryIndex} out of range [0, {geometryData.Count - 1}]")), Enumerable.Empty<InstancedTransformData>());

        public InstancedGeometryData(List<GeometryData> geometryData, List<List<InstancedTransformData>> transformData) {
            this.geometryData = geometryData;
            this.transformData = transformData.Flatten();
            transformCount = new List<int>(transformData.Select(list => list.Count));
        }

        public InstancedGeometryData(GeometryData geometryData, List<InstancedTransformData> transformData) {
            this.geometryData = new List<GeometryData> {geometryData};
            this.transformData = new List<InstancedTransformData> (transformData);
            transformCount = new List<int> {transformData.Count};
        }

        private InstancedGeometryData() {
            geometryData = new List<GeometryData>();
            transformData = new List<InstancedTransformData>();
            transformCount = new List<int>();
        }

        private IEnumerable<InstancedTransformData> GetTransformsImpl(int geometryIndex) {
            if (!geometryIndex.InRange(transformCount)) {
                Debug.LogException(new IndexOutOfRangeException($"Index {geometryIndex} out of range [0, {transformCount.Count - 1}]"));
                return Enumerable.Empty<InstancedTransformData>();
            }
            
            int transformStartIndex = geometryIndex == 0 ? 0 : transformCount.Take(geometryIndex - 1).Sum();
            return transformData.Skip(transformStartIndex).Take(transformCount[geometryIndex]);
        }

        public static InstancedGeometryData Empty() => new InstancedGeometryData();
        
    }
}