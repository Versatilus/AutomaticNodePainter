namespace AutomaticNodePainter.Util {
    using TrafficManager.Manager.Impl;
    public static class TMPEUtil {
        public static int CountTargetConnections(LaneData sourceLane, LaneData[] TargetLanes) {
            int ret = 0;
            foreach (var targetLane in TargetLanes) {
                if (LaneConnectionManager.Instance.AreLanesConnected(
                    sourceLaneId: sourceLane.LaneID,
                    targetLaneId: targetLane.LaneID,
                    sourceStartNode: sourceLane.StartNode))
                    ret++;
            }
            return ret;
        }

        public static int CountSourceConnections(LaneData targetLane, LaneData[] sourceLanes) {
            int ret = 0;
            foreach (var sourceLane in sourceLanes) {
                if (LaneConnectionManager.Instance.AreLanesConnected(
                    sourceLaneId: sourceLane.LaneID,
                    targetLaneId: targetLane.LaneID,
                    sourceStartNode: sourceLane.StartNode))
                    ret++;
            }
            return ret;
        }
    }
}
