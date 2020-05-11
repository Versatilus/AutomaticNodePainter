using ICities;
using JetBrains.Annotations;
using System;
using AutomaticNodePainter.Util;

namespace AutomaticNodePainter {
    public class AutomaticNodePainterMod : IUserMod {
        public static Version ModVersion => typeof(AutomaticNodePainterMod).Assembly.GetName().Version;
        public static string VersionString => ModVersion.ToString(2);
        public string Name => "Automatic Node Painter" + VersionString;
        public string Description => "Automatically paints lane markers on transitional nodes";
        
        public void OnEnabled() {
            if (HelpersExtensions.InGame)
                LoadTool.Load();
#if DEBUG
            TestsExperiments.Run();
#endif
        }

        public void OnDisabled() {
            LoadTool.Release();
        }
    }

    public static class LoadTool {
        public static void Load() {
            TMPEUtil.Active = true;
            Tool.AutomaticNodePainterTool.Create();
        }
        public static void Release() {
            Tool.AutomaticNodePainterTool.Remove();
        }
    }

    public class LoadingExtention : LoadingExtensionBase {
        public override void OnLevelLoaded(LoadMode mode) {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
                LoadTool.Load();
        }

        public override void OnLevelUnloading() {
            LoadTool.Release();
        }
    }



} // end namesapce
