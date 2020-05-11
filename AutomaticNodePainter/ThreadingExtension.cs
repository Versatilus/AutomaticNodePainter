
namespace AutomaticNodePainter {
    using System;
    using ICities;
    using UnityEngine;
    using static AutomaticNodePainter.Util.HelpersExtensions;
    using AutomaticNodePainter.Tool;

    public class ThreadingExtension : ThreadingExtensionBase{
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
            if (ControlIsPressed && AltIsPressed && !ShiftIsPressed && Input.GetKeyDown(KeyCode.T)) {
                SimulationManager.instance.m_ThreadingWrapper.QueueMainThread(
                    () => AutomaticNodePainterTool.Instance.ToggleTool());
            }
        }
    }
}
