using System;
using System.Collections.Generic;
using System.IO;
using DarkDepths.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // Android test builds. The store-facing settings stay for T12.1 and T12.3.
    public static class AndroidBuild
    {
        private const string OutputFolder = "Builds";
        private const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";
        private const string SandboxSceneName = "Sandbox";

        [MenuItem("DarkDepths/Build/Android APK (development)")]
        public static void BuildDevelopmentApk()
        {
            Build(true);
        }

        [MenuItem("DarkDepths/Build/Android APK (release)")]
        public static void BuildReleaseApk()
        {
            Build(false);
        }

        private static void Build(bool development)
        {
            ApplyPlayerSettings();
            PrepareBootScene();

            Directory.CreateDirectory(OutputFolder);
            string path = Path.Combine(OutputFolder, development ? "DarkDepths-dev.apk" : "DarkDepths-release.apk");

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = development ? BuildOptions.Development : BuildOptions.None
            };

            BuildSummary summary = UnityEditor.BuildPipeline.BuildPlayer(options).summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Android build {summary.result}, errors: {summary.totalErrors}");
            }

            // summary.totalSize counts the symbol folder too, so report the package itself.
            long apkBytes = new FileInfo(path).Length;
            Debug.Log($"APK ready: {path}, {apkBytes / 1048576} MB");
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = "Dark Depths";
            PlayerSettings.companyName = "DarkDepths";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.darkdepths.devbuild");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }

        // Boot has no Menu scene to hand over to yet, so it starts Sandbox directly.
        private static void PrepareBootScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            var bootstrap = UnityEngine.Object.FindAnyObjectByType<Bootstrap>();
            if (bootstrap == null)
            {
                return;
            }

            var serialized = new SerializedObject(bootstrap);
            SerializedProperty nextScene = serialized.FindProperty("_nextScene");
            if (nextScene.stringValue == SandboxSceneName)
            {
                return;
            }

            nextScene.stringValue = SandboxSceneName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static string[] GetEnabledScenes()
        {
            var paths = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    paths.Add(scene.path);
                }
            }

            return paths.ToArray();
        }
    }
}
