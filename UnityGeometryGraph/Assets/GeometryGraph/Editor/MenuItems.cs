﻿using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace GeometryGraph.Editor {
    public static class MenuItems {
        [MenuItem("Geometry Graph/Recompile Code", priority = 0)]
        public static void RecompileCode() {
            CompilationPipeline.RequestScriptCompilation();
        }

        [MenuItem("Geometry Graph/Toggle Debug", priority = 1)]
        public static void ToggleDebug() {
            Debug.Log(RuntimeGraphObject.DebugEnabled ? "Disabled debug." : "Enabled debug.");
            RuntimeGraphObject.DebugEnabled = !RuntimeGraphObject.DebugEnabled;
        }

        [MenuItem("Geometry Graph/Print InstanceID", priority = 100)]
        public static void PrintInstanceId() {
            Object selection = Selection.activeObject;
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(selection));
            foreach (Object asset in allAssets) {
                Debug.LogWarning($"{asset.name}:{asset.GetType().Name}:{asset.GetInstanceID()}");

            }
        }

        [MenuItem("Geometry Graph/Find all instances", priority = 101)]
        public static void FindGFOInstances() {
            GraphFrameworkObject[] instances = Resources.FindObjectsOfTypeAll<GraphFrameworkObject>();
            Debug.Log($"Graph Objects: {instances.Length}");
            foreach (GraphFrameworkObject graphFrameworkObject in instances) {
                Debug.Log($"GFO:{graphFrameworkObject.GetInstanceID()}");
            }

            RuntimeGraphObject[] runtimeGraphObjects = Resources.FindObjectsOfTypeAll<RuntimeGraphObject>();
            Debug.Log($"Runtime GO: {runtimeGraphObjects.Length}");
            foreach (RuntimeGraphObject runtimeGraphObject in runtimeGraphObjects) {
                Debug.Log($"RGO:{runtimeGraphObject.name}/{runtimeGraphObject.GetInstanceID()}");
            }
        }

        [MenuItem("Geometry Graph/Delete all instances", priority = 102)]
        public static void DeleteGFOInstances() {
            GraphFrameworkObject[] instances = Resources.FindObjectsOfTypeAll<GraphFrameworkObject>();
            Debug.Log(instances.Length);
            foreach (GraphFrameworkObject graphFrameworkObject in instances) {
                Debug.Log($"{graphFrameworkObject.name}:{graphFrameworkObject.GetInstanceID()}");
                Object.DestroyImmediate(graphFrameworkObject, true);
            }
        }

        [MenuItem("Geometry Graph/Delete all instances (+RGO)", priority = 103)]
        public static void DeleteAllInstances() {
            GraphFrameworkObject[] instances = Resources.FindObjectsOfTypeAll<GraphFrameworkObject>();
            Debug.Log(instances.Length);
            foreach (GraphFrameworkObject graphFrameworkObject in instances) {
                Debug.Log($"{graphFrameworkObject.name}:{graphFrameworkObject.GetInstanceID()}");
                Object.DestroyImmediate(graphFrameworkObject, true);
            }

            RuntimeGraphObject[] runtimeGraphObjects = Resources.FindObjectsOfTypeAll<RuntimeGraphObject>();
            foreach (RuntimeGraphObject runtimeGraphObject in runtimeGraphObjects) {
                Debug.Log($"Delete: {runtimeGraphObject.GetInstanceID()}");
                Object.DestroyImmediate(runtimeGraphObject, true);
            }
        }

        [MenuItem("Geometry Graph/Delete negative instances", priority = 104)]
        public static void DeleteNegativeGFOInstances() {
            GraphFrameworkObject[] instances = Resources.FindObjectsOfTypeAll<GraphFrameworkObject>();
            foreach (GraphFrameworkObject graphFrameworkObject in instances) {
                if (graphFrameworkObject.GetInstanceID() < 0) {
                    Debug.Log($"Delete: {graphFrameworkObject.GetInstanceID()}");
                    Object.DestroyImmediate(graphFrameworkObject, true);
                }
            }

            RuntimeGraphObject[] runtimeGraphObjects = Resources.FindObjectsOfTypeAll<RuntimeGraphObject>();
            foreach (RuntimeGraphObject runtimeGraphObject in runtimeGraphObjects) {
                if (runtimeGraphObject.GetInstanceID() < 0) {
                    Debug.Log($"Delete: {runtimeGraphObject.GetInstanceID()}");
                    Object.DestroyImmediate(runtimeGraphObject, true);
                }
            }
        }
    }
}