using ColossalFramework;
using ColossalFramework.UI;
using System;
using UnityEngine;
using AutomaticNodePainter.Util;
using AutomaticNodePainter.UI;
using AutomaticNodePainter.Shapes;
using JetBrains.Annotations;
using ColossalFramework.Math;
using AutomaticNodePainter.GUI;

namespace AutomaticNodePainter.Tool {
    using static Util.RenderUtil;

    public sealed class AutomaticNodePainterTool : KianToolBase {
        UIButton button;

        protected override void Awake() {
            var uiView = UIView.GetAView();
            //button = AutomaticNodePainterButton.CreateButton();
            base.Awake();
        }

        public static AutomaticNodePainterTool Create() {
            Log.Debug("AutomaticNodePainterTool.Create()");
            GameObject toolModControl = ToolsModifierControl.toolController.gameObject;
            var tool = toolModControl.GetComponent<AutomaticNodePainterTool>() ?? toolModControl.AddComponent<AutomaticNodePainterTool>();
            return tool;
        }

        public static AutomaticNodePainterTool Instance {
            get {
                GameObject toolModControl = ToolsModifierControl.toolController?.gameObject;
                return toolModControl?.GetComponent<AutomaticNodePainterTool>();
            }
        }

        public static void Remove() {
            Log.Debug("AutomaticNodePainterTool.Remove()");
            var tool = Instance;
            if (tool != null)
                Destroy(tool);
        }

        protected override void OnDestroy() {
            Log.Debug("AutomaticNodePainterTool.OnDestroy()\n" + Environment.StackTrace);
            button?.Hide();
            Destroy(button);
            base.OnDestroy();
        }

        //public override void EnableTool() => ToolsModifierControl.SetTool<AutomaticNodePainterTool>();

        protected override void OnEnable() {
            Log.Debug("AutomaticNodePainterTool.OnEnable");
            button.Focus();
            base.OnEnable();
            button.Focus();
            button.Invalidate();
        }

        protected override void OnDisable() {
            Log.Debug("AutomaticNodePainterTool.OnDisable");
            button?.Unfocus();
            base.OnDisable();
            button?.Unfocus();
            button?.Invalidate();

        }

        protected override void OnToolUpdate() {
            base.OnToolUpdate();
            ToolCursor = HoverValid ? NetUtil.netTool.m_upgradeCursor : null;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);
            if (!HoverValid) return;
            if (!IsSuitableJunction()) return;

            DrawNodeCircle(cameraInfo, HoveredNodeId, Color.yellow);
            DrawOverlayCircle(cameraInfo, Color.red, HitPos, 1, true);
        }

        protected override void OnPrimaryMouseClicked() {
            if (!HoverValid)
                return;
            Log.Info($"OnPrimaryMouseClicked: segment {HoveredSegmentId} node {HoveredNodeId}");

            SimulationManager.instance.AddAction(delegate () { 
                NodePainting paiting = new NodePainting(HoveredNodeId);
                paiting.Create();
            });
        }

        protected override void OnSecondaryMouseClicked() {
            //throw new System.NotImplementedException();
        }

        bool IsSuitableJunction() {
            if (HoveredNodeId == 0)
                return false;
            NetNode node = HoveredNodeId.ToNode();
            if (node.CountSegments() < 2)
                return false;

            return true;
        }

    } //end class
}
