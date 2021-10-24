﻿using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public partial class GeometryData {
        public GeometryData(Mesh mesh, float duplicateNormalAngleThreshold) {
            using var method = Profiler.ProfileMethod();
            submeshCount = mesh.subMeshCount;
            var vertexPositions = new List<float3>(mesh.vertices.Select(vertex => new float3(vertex.x, vertex.y, vertex.z)));
            var meshUvs = new List<float2>(mesh.uv.Select(uv => new float2(uv.x, uv.y)));

            var faceNormals = new List<float3>();
            var uvs = new List<float2>();
            var faceMaterialIndices = new List<int>();
            var faceSmoothShaded = new List<bool>();

            BuildGeometry(mesh, vertexPositions, meshUvs, /*out*/faceNormals, /*out*/uvs, /*out*/faceMaterialIndices, /*out*/faceSmoothShaded, duplicateNormalAngleThreshold);

            attributeManager = new AttributeManager(this);
            FillBuiltinAttributes(vertexPositions, uvs, new List<float>(new float[edges.Count]), faceNormals, faceMaterialIndices, faceSmoothShaded);
        }
        
        private void BuildGeometry(
            Mesh mesh, List<float3> vertexPositions, List<float2> uvs, 
            /*out*/ List<float3> faceNormals, List<float2> correctUvs, List<int> faceMaterialIndices, List<bool> smoothShaded, float duplicateNormalAngleThreshold
        ) {
            using var method = Profiler.ProfileMethod();
            edges = new List<Edge>(mesh.triangles.Length);
            faces = new List<Face>(mesh.triangles.Length / 3);
            faceCorners = new List<FaceCorner>(mesh.triangles.Length);
            BuildElements(mesh, vertexPositions, uvs, faceNormals, correctUvs, faceMaterialIndices, smoothShaded);
            RemoveDuplicates(vertexPositions, faceNormals, duplicateNormalAngleThreshold);

            this.vertices = new List<Vertex>(vertexPositions.Count);
            for (var i = 0; i < vertexPositions.Count; i++) {
                this.vertices.Add(new Vertex());
            }

            FillElementMetadata();

            Cleanup();
        }
        
        
        private void BuildElements(
            Mesh mesh, List<float3> vertexPositions, List<float2> uvs, 
            List<float3> faceNormals, List<float2> correctUvs, List<int> materialIndices, List<bool> smoothShaded
        ) {
            using var method = Profiler.ProfileMethod();
            var meshNormals = mesh.normals;
            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++) {
                var triangles = mesh.GetTriangles(submesh);
                var length = triangles.Length;
            
                for (var i = 0; i < length; i += 3) {
                    var idxA = triangles[i];
                    var idxB = triangles[i + 1];
                    var idxC = triangles[i + 2];

                    var vertA = vertexPositions[idxA];
                    var vertB = vertexPositions[idxB];
                    var vertC = vertexPositions[idxC];

                    var AB = vertB - vertA;
                    var AC = vertC - vertA;
                    var computedNormal = math.normalize(math.cross(AB, AC));
                    var meshNormal = (float3)(meshNormals[idxA] + meshNormals[idxB] + meshNormals[idxC]) / 3.0f;
                    faceNormals.Add(computedNormal);
                    materialIndices.Add(submesh);

                    smoothShaded.Add(math.lengthsq(computedNormal - meshNormal) > 0.0001f); 

                    var face = new Face(
                        idxA, idxB, idxC,
                        faceCorners.Count, faceCorners.Count + 1, faceCorners.Count + 2,
                        edges.Count, edges.Count + 1, edges.Count + 2
                    );

                    var edgeA = new Edge(idxA, idxB, edges.Count) { FaceA = faces.Count };
                    var edgeB = new Edge(idxB, idxC, edges.Count + 1) { FaceA = faces.Count };
                    var edgeC = new Edge(idxC, idxA, edges.Count + 2) { FaceA = faces.Count };

                    var faceCornerA = new FaceCorner(faces.Count);
                    var faceCornerB = new FaceCorner(faces.Count);
                    var faceCornerC = new FaceCorner(faces.Count);

                    faces.Add(face);
                    edges.Add(edgeA);
                    edges.Add(edgeB);
                    edges.Add(edgeC);
                    faceCorners.Add(faceCornerA);
                    faceCorners.Add(faceCornerB);
                    faceCorners.Add(faceCornerC);

                    if (uvs.Count > 0) {
                        correctUvs.Add(uvs[idxA]);
                        correctUvs.Add(uvs[idxB]);
                        correctUvs.Add(uvs[idxC]);
                    }
                }
            }
        }

        private void RemoveDuplicates(List<float3> vertexPositions, List<float3> faceNormals, float duplicateNormalAngleThreshold) {
            using var method = Profiler.ProfileMethod();
            
            // Find out which edges are duplicates of each other based on vertex position 
            var duplicates = GetDuplicateEdges(vertexPositions, faceNormals, duplicateNormalAngleThreshold);
            
            // unique vertex => [duplicate vertices...] (Dict<int, List<int>>)
            var duplicateVerticesMap = GetDuplicateVerticesMap(duplicates);
            
            // duplicate vertex => unique vertex (Dict<int, int>)
            var reverseDuplicatesMap = RemoveInvalidDuplicates(duplicateVerticesMap);
            
            // Remap indices from old/duplicate index to unique index
            RemapDuplicateElements(vertexPositions, duplicates, reverseDuplicatesMap);
            
            // Remove all duplicates from geometry
            RemoveDuplicateElements(vertexPositions, duplicates, reverseDuplicatesMap);
            
            CheckForErrors(vertexPositions.Count);
        }
        
        private List<(int, int, bool)> GetDuplicateEdges(List<float3> vertexPositions, List<float3> faceNormals, float duplicateNormalAngleThreshold) {
            using var method = Profiler.ProfileMethod();
            var equalityComparer = new EdgeEqualityComparer(vertexPositions);
            // NOTE: I don't actually care about the HashSet, everything I need is stored in the comparer lol
            var _ = new HashSet<Edge>(edges, equalityComparer);

            IEnumerable<(int EdgeA, int EdgeB)> potentialDuplicates = equalityComparer.Duplicates
                                                      // Convert (Key => [values]) to [Key, ...values]
                                                      .Select(pair => pair.Value.Prepend(pair.Key).ToList()) 
                                                      // Get all subsets of length 2, so [1,2,3] becomes [(1,2), (1,3), (2,3)]
                                                      .SelectMany(values => values.SubSets2());

            var excluded = new HashSet<int>();
            // Find real duplicates based on the face normal angle
            var actualDuplicates = new List<(int, int, bool)>();
            foreach (var (edgeAIndex, edgeBIndex) in potentialDuplicates) {
                if (excluded.Contains(edgeAIndex) || excluded.Contains(edgeBIndex)) continue;
                
                var edgeA = edges[edgeAIndex];
                var edgeB = edges[edgeBIndex];
                var normalAngle = math_util.angle(faceNormals[edgeA.FaceA], faceNormals[edgeB.FaceA]);

                if (normalAngle > duplicateNormalAngleThreshold) continue;

                excluded.Add(edgeAIndex);
                excluded.Add(edgeBIndex);
                
                var edgeAVertA = vertexPositions[edgeA.VertA];
                var edgeAVertB = vertexPositions[edgeA.VertB];
                var edgeBVertA = vertexPositions[edgeB.VertA];
                var edgeBVertB = vertexPositions[edgeB.VertB];

                actualDuplicates.Add((edgeAIndex, edgeBIndex, edgeAVertA.Equals(edgeBVertA) && edgeAVertB.Equals(edgeBVertB)));
                edgeA.FaceB = edgeB.FaceA;
            }

            return actualDuplicates;
        }

        private Dictionary<int, List<int>> GetDuplicateVerticesMap(List<(int, int, bool)> duplicates) {
            using var method = Profiler.ProfileMethod();
            // Make a dictionary of (UniqueVertex => [Duplicates])
            var duplicateVerticesMap = new Dictionary<int, List<int>>();
            foreach (var duplicate in duplicates) {
                var edgeA = edges[duplicate.Item1];
                var edgeB = edges[duplicate.Item2];

                // Check which vertices in each edge are duplicates
                int edgeAVertA = -1, edgeAVertB = -1, edgeBVertA = -1, edgeBVertB = -1;
                if (duplicate.Item3) {
                    edgeAVertA = edgeA.VertA;
                    edgeBVertA = edgeB.VertA;
                    edgeAVertB = edgeA.VertB;
                    edgeBVertB = edgeB.VertB;
                } else {
                    edgeAVertA = edgeA.VertA;
                    edgeBVertA = edgeB.VertB;
                    edgeAVertB = edgeA.VertB;
                    edgeBVertB = edgeB.VertA;
                }

                // sort as (lower num, bigger num)
                if (edgeAVertA > edgeBVertA) (edgeAVertA, edgeBVertA) = (edgeBVertA, edgeAVertA);
                if (edgeAVertB > edgeBVertB) (edgeAVertB, edgeBVertB) = (edgeBVertB, edgeAVertB);

                if (!duplicateVerticesMap.ContainsKey(edgeAVertA)) {
                    duplicateVerticesMap[edgeAVertA] = new List<int>();
                }

                duplicateVerticesMap[edgeAVertA].Add(edgeBVertA);

                if (!duplicateVerticesMap.ContainsKey(edgeAVertB)) {
                    duplicateVerticesMap[edgeAVertB] = new List<int>();
                }

                duplicateVerticesMap[edgeAVertB].Add(edgeBVertB);
            }

            return duplicateVerticesMap;
        }
        
        private Dictionary<int, int> RemoveInvalidDuplicates(Dictionary<int, List<int>> duplicateVerticesMap) {
            using var method = Profiler.ProfileMethod();
            // Remove trivial invalid entries
            var removalList = new List<int>();
            foreach (var (unique, duplicates) in duplicateVerticesMap) {
                duplicates.RemoveAll(duplicate => duplicate == unique);
                if (duplicates.Count == 0) removalList.Add(unique);
            }

            removalList.ForEach(unique => duplicateVerticesMap.Remove(unique));

            // Remove remaining invalid entries by joining all duplicate entries
            // For example (1=>{2, 3}, 3=>{7,12}, 4=>{12, 14}) becomes (1=>{2, 3, 4, 7, 8, 12, 14})
            var sortedKeys = duplicateVerticesMap.Keys.ToList().QuickSorted();
            var existsMap = new Dictionary<int, int>(); // maps a vertex index to the actual map
            var actualMap = new Dictionary<int, HashSet<int>>();

            foreach (var sortedKey in sortedKeys) {
                if (existsMap.ContainsKey(sortedKey)) {
                    var target = existsMap[sortedKey];
                    if (actualMap.ContainsKey(target)) {
                        actualMap[target].AddRange(duplicateVerticesMap[sortedKey]);
                        duplicateVerticesMap[sortedKey].ForEach(i => existsMap[i] = target);
                        continue;
                    }
                }
                
                var valueExists = duplicateVerticesMap[sortedKey].FirstOrGivenDefault(i => existsMap.ContainsKey(i), -1);
                if (valueExists != -1) {
                    var target = existsMap[valueExists]; 
                    actualMap[target].Add(sortedKey);
                    actualMap[target].AddRange(duplicateVerticesMap[sortedKey]);
                    existsMap[sortedKey] = target;
                    duplicateVerticesMap[sortedKey].ForEach(i => existsMap[i] = target);
                } else {
                    actualMap[sortedKey] = new HashSet<int>(duplicateVerticesMap[sortedKey]);
                    existsMap[sortedKey] = sortedKey;
                    duplicateVerticesMap[sortedKey].ForEach(i => existsMap[i] = sortedKey);    
                }
            }
            
            // Copy actualMap to duplicateVerticesMap
            duplicateVerticesMap.Clear();
            foreach (var pair in actualMap) {
                duplicateVerticesMap[pair.Key] = new List<int>(pair.Value);
                existsMap.Remove(pair.Key);
            }

            return existsMap;
        }
        
        private void RemapDuplicateElements(List<float3> vertices, List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
            using var method = Profiler.ProfileMethod();
            // Remap the vertex indices for faces and edges
            var edgeReverseMap = new Dictionary<int, int>();
            foreach (var duplicate in duplicates) {
                edgeReverseMap[duplicate.Item2] = duplicate.Item1;
            }

            var allEdgeIndices = faces.SelectMany(face => new[] { face.EdgeA, face.EdgeB, face.EdgeC }).Distinct().Except(edgeReverseMap.Keys).ToList().InsertionSorted();
            var edgeRemap = new Dictionary<int, int>();
            var remapIndex = 0;
            foreach (var sortedKey in allEdgeIndices) {
                edgeRemap[sortedKey] = remapIndex++;
            }

            var allVertexIndices = Enumerable.Range(0, vertices.Count).Except(reverseDuplicatesMap.Keys);
            var vertexRemap = new Dictionary<int, int>();
            remapIndex = 0;
            foreach (var key in allVertexIndices) {
                vertexRemap[key] = remapIndex++;
            }

            foreach (var face in faces) {
                RemapEdge(face.EdgeA, reverseDuplicatesMap, vertexRemap);
                RemapEdge(face.EdgeB, reverseDuplicatesMap, vertexRemap);
                RemapEdge(face.EdgeC, reverseDuplicatesMap, vertexRemap);

                RemapFace(face, reverseDuplicatesMap, edgeReverseMap, edgeRemap, vertexRemap);
            }
        }

        private void RemapFace(Face face, Dictionary<int, int> reverseDuplicateMap, Dictionary<int, int> edgeReverseMap, Dictionary<int, int> edgeIndexRemap, Dictionary<int, int> vertexIndexRemap) {
            if (reverseDuplicateMap.ContainsKey(face.VertA)) {
                face.VertA = vertexIndexRemap[reverseDuplicateMap[face.VertA]];
            } else {
                face.VertA = vertexIndexRemap[face.VertA];
            }

            if (reverseDuplicateMap.ContainsKey(face.VertB)) {
                face.VertB = vertexIndexRemap[reverseDuplicateMap[face.VertB]];
            } else {
                face.VertB = vertexIndexRemap[face.VertB];
            }

            if (reverseDuplicateMap.ContainsKey(face.VertC)) {
                face.VertC = vertexIndexRemap[reverseDuplicateMap[face.VertC]];
            } else {
                face.VertC = vertexIndexRemap[face.VertC];
            }

            if (edgeReverseMap.ContainsKey(face.EdgeA)) {
                face.EdgeA = edgeIndexRemap[edgeReverseMap[face.EdgeA]];
            } else {
                face.EdgeA = edgeIndexRemap[face.EdgeA];
            }

            if (edgeReverseMap.ContainsKey(face.EdgeB)) {
                face.EdgeB = edgeIndexRemap[edgeReverseMap[face.EdgeB]];
            } else {
                face.EdgeB = edgeIndexRemap[face.EdgeB];
            }

            if (edgeReverseMap.ContainsKey(face.EdgeC)) {
                face.EdgeC = edgeIndexRemap[edgeReverseMap[face.EdgeC]];
            } else {
                face.EdgeC = edgeIndexRemap[face.EdgeC];
            }
        }

        private void RemapEdge(int edgeIndex, Dictionary<int, int> reverseDuplicateMap, Dictionary<int, int> vertexRemap) {
            var edge = edges[edgeIndex];
            if (reverseDuplicateMap.ContainsKey(edge.VertA)) {
                edge.VertA = vertexRemap[reverseDuplicateMap[edge.VertA]];
            } else {
                edge.VertA = vertexRemap[edge.VertA];
            }

            if (reverseDuplicateMap.ContainsKey(edge.VertB)) {
                edge.VertB = vertexRemap[reverseDuplicateMap[edge.VertB]];
            } else {
                edge.VertB = vertexRemap[edge.VertB];
            }
        }

        private void RemoveDuplicateElements(List<float3> vertexPositions, List<(int, int, bool)> duplicates, Dictionary<int, int> reverseDuplicatesMap) {
            using var method = Profiler.ProfileMethod();
            // Remove duplicate edges
            foreach (var edge in duplicates.Select(tuple=>edges[tuple.Item2]).ToList()) {
                edges.Remove(edge);
            }

            // Remove duplicate vertices
            var sortedDuplicateVertices = reverseDuplicatesMap.Keys.ToList().QuickSorted((key1, key2) => key2.CompareTo(key1));
            foreach (var vertexIndex in sortedDuplicateVertices) {
                vertexPositions.RemoveAt(vertexIndex);
            }
        }

        private void CheckForErrors(int vertexCount) {
            using var method = Profiler.ProfileMethod();
            // Check if there are any invalid`edges
            for (var i = 0; i < edges.Count; i++) {
                var edge = edges[i];
                if (edge.VertA >= vertexCount || edge.VertB >= vertexCount) {
                    Debug.LogError($"Edge at index {i} contains invalid vertices");
                }
            }

            // Check if there are any invalid faces
            for (var i = 0; i < faces.Count; i++) {
                var face = faces[i];
                if (face.EdgeA >= edges.Count || face.EdgeB >= edges.Count || face.EdgeC >= edges.Count ||
                    face.EdgeA == -1 || face.EdgeB == -1 || face.EdgeC == -1) {
                    Debug.LogError($"Face at index {i} contains invalid edges");
                }

                if (face.VertA >= vertexCount || face.VertB >= vertexCount || face.VertC >= vertexCount) {
                    Debug.LogError($"Face at index {i} contains invalid vertices");
                }
            }
            
            // TODO: maybe add checks for FaceCorners having vert idx and face idx in range, and for Faces having FaceCorners in range
        }
    }
}