
namespace AutomaticNodePainter.Shapes {
    using AutomaticNodePainter.Util;
    using ColossalFramework.Math;
    using System.Collections.Generic;
    using TrafficManager.Manager.Impl;
    using UnityEngine;

    public struct LaneMarkerPoint {
        public PointDir3 PointDir;
        public LaneMarkerPoint(LaneData laneData1, LaneData laneData2, bool start) {
            ref NetLane lane1 = ref laneData1.LaneID.ToLane();
            ref NetLane lane2 = ref laneData2.LaneID.ToLane();

            ushort segmentID = lane1.m_segment;
            ref NetSegment segment = ref segmentID.ToSegment();
            Bezier3 bezier1 = lane1.m_bezier;
            Bezier3 bezier2 = lane2.m_bezier;
            float hw1 = laneData1.LaneInfo.m_width * 0.5f;
            float hw2 = laneData2.LaneInfo.m_width * 0.5f;
            Vector3 pos1, pos2;
            if (start) {
                PointDir.Dir = segment.m_startDirection;
                pos1 = bezier1.a;
                pos2 = bezier2.a;
            } else {
                PointDir.Dir = segment.m_endDirection;
                pos1 = bezier1.b;
                pos2 = bezier2.b;
            }

            Vector3 sidewayDir = (pos2 - pos1).normalized;
            pos1 += hw1 * sidewayDir;
            pos2 -= hw2 * sidewayDir;
            PointDir.Point = (pos1 + pos2) * 0.5f;
        }

        public LaneMarkerPoint(Vector3 dir, Vector3 pos) {
            PointDir.Dir = dir;
            PointDir.Point = pos;
        }

        /// <summary>
        /// Creates a control point to the other side of <paramref name="otherLaneData"/>
        /// useful for side lanes.
        /// </summary>
        public static LaneMarkerPoint CreateSidePoint(LaneData laneData, LaneData otherLaneData, bool start) {
            ref NetLane lane1 = ref laneData.LaneID.ToLane();
            ref NetLane lane2 = ref otherLaneData.LaneID.ToLane();
            ushort segmentID = lane1.m_segment;
            ref NetSegment segment = ref segmentID.ToSegment();
            Bezier3 bezier1 = lane1.m_bezier;
            Bezier3 bezier2 = lane2.m_bezier;
            float hw1 = laneData.LaneInfo.m_width * 0.5f;
            float hw2 = otherLaneData.LaneInfo.m_width * 0.5f;
            Vector3 pos1, pos2;
            Vector3 direction, position;
            if (start) {
                direction = segment.m_startDirection;
                pos1 = bezier1.a;
                pos2 = bezier2.a;
            } else {
                direction = segment.m_endDirection;
                pos1 = bezier1.b;
                pos2 = bezier2.b;
            }

            Vector3 sidewayDir = (pos2 - pos1).normalized;
            position = pos1 - hw1 * sidewayDir;
            return new LaneMarkerPoint(direction, position);
        }
    }

    public class SegmentHalfEndWrapper {
        public List<LaneMarkerPoint> ControlPoints; // sorted from outherside to inserside
        public LaneData[] Lanes; // sorted from outherside to inserside
        public bool IsSource { get; private set; }

        public SegmentHalfEndWrapper(ushort segmentID, bool startNode, bool isSource) {
            IsSource = isSource;
            bool target = !isSource;
            ControlPoints = new List<LaneMarkerPoint>();
            Lanes = NetUtil.GetSortedLanes(
                segmentId: segmentID,
                startNode: startNode ^ target, // target lanes are heading away
                laneType: LaneConnectionManager.LANE_TYPES,
                vehicleType: LaneConnectionManager.VEHICLE_TYPES);
            {
                var item = LaneMarkerPoint.CreateSidePoint(Lanes[0], Lanes[1], startNode);
                ControlPoints.Add(item);
            }
            for (int i = 1; i < Lanes.Length - 1; ++i) {
                var item = new LaneMarkerPoint(Lanes[i - 1], Lanes[i], startNode);
                ControlPoints.Add(item);
            }
            {
                int i = Lanes.Length - 1;
                var item = LaneMarkerPoint.CreateSidePoint(Lanes[i], Lanes[i - 1], startNode);
                ControlPoints.Add(item);
            }
        }

        public int GetCorespondingControlPoint(int laneIndex, bool outerSide = false) =>
            outerSide ? laneIndex : laneIndex + 1;
    }

    public class SegmentEndWrapper {
        public SegmentHalfEndWrapper Source { get; private set; }
        public SegmentHalfEndWrapper Target { get; private set; }

        public SegmentEndWrapper(ushort segmentID, bool startNode) {
            Source = new SegmentHalfEndWrapper(segmentID, startNode, isSource: true);
            Target = new SegmentHalfEndWrapper(segmentID, startNode, isSource: false);
        }

        /// <summary>
        /// Returns a list of Control points order from outermost lane to innermost lane.
        /// </summary>
        /// <param name="startNode">which node</param>
        /// <param name="target">determines if lanes are heading away from the node determined by <paramref name="startNode"/></param>
        public static List<LaneMarkerPoint> CreateControlPoints(ushort segmentID, bool startNode, bool target, out LaneData[] lanes) {
            var ret = new List<LaneMarkerPoint>();
            lanes = NetUtil.GetSortedLanes(
                segmentId: segmentID,
                startNode: startNode ^ target, // target lanes are heading away
                laneType: LaneConnectionManager.LANE_TYPES,
                vehicleType: LaneConnectionManager.VEHICLE_TYPES);
            {
                var item = LaneMarkerPoint.CreateSidePoint(lanes[0], lanes[1], startNode);
                ret.Add(item);
            }
            for (int i = 1; i < lanes.Length - 1; ++i) {
                var item = new LaneMarkerPoint(lanes[i - 1], lanes[i], startNode);
                ret.Add(item);
            }
            {
                int i = lanes.Length - 1;
                var item = LaneMarkerPoint.CreateSidePoint(lanes[i], lanes[i - 1], startNode);
                ret.Add(item);
            }
            return ret;
        }
    }

    public class ConnectionWrapper {
        public SegmentEndWrapper SegmentEnd1, SegmentEnd2;
        public int[] Connections1;
        public int[] Connections2;

        public ConnectionWrapper(ushort segmentID1, ushort segmentID2) {
            ushort nodeID = segmentID1.ToSegment().GetSharedNode(segmentID2);
            bool startNode1 = NetUtil.IsStartNode(segmentID1, nodeID);
            bool startNode2 = NetUtil.IsStartNode(segmentID2, nodeID);
            SegmentEnd1 = new SegmentEndWrapper(segmentID1, startNode1);
            SegmentEnd2 = new SegmentEndWrapper(segmentID2, startNode2);

            Connections1 = CalculateConnections(SegmentEnd1.Source, SegmentEnd2.Target);
            Connections2 = CalculateConnections(SegmentEnd2.Source, SegmentEnd1.Target);
        }

        public static int[] CalculateConnections(SegmentHalfEndWrapper Source, SegmentHalfEndWrapper Target) {
            int[] connections = new int[Source.ControlPoints.Count];
            for (int i = 0; i < connections.Length; ++i)
                connections[i] = -1; // unconnected

            connections[0] = 0; //connect outermost side lane marker.
            connections[Source.ControlPoints.Count - 1] = Target.ControlPoints.Count - 1; // counnect the innermost side lane marker

            for (int sourceLaneIDX = 0; sourceLaneIDX < Source.Lanes.Length; ++sourceLaneIDX) {
                int targetLaneIDX = GetInnerMostConnectedTargetLane(Source.Lanes[sourceLaneIDX], Target.Lanes);
                int count = LaneConnectionUtil.CountSourceConnections(Target.Lanes[targetLaneIDX], Source.Lanes);
                if (count == 1) {
                    int j1 = Source.GetCorespondingControlPoint(sourceLaneIDX);
                    int j2 = Target.GetCorespondingControlPoint(targetLaneIDX);
                    connections[j1] = j2;
                }
            }

            for (int targetLaneIDX = 0; targetLaneIDX < Target.Lanes.Length; ++targetLaneIDX) {
                int sourceLaneIDX = GetInnerMostConnectedSourceLane(Target.Lanes[targetLaneIDX], Source.Lanes);
                int count = LaneConnectionUtil.CountTargetConnections(Source.Lanes[sourceLaneIDX], Target.Lanes);
                if (count == 1) {
                    int j1 = Source.GetCorespondingControlPoint(sourceLaneIDX);
                    int j2 = Target.GetCorespondingControlPoint(targetLaneIDX);
                    connections[j1] = j2;
                }
            }
            return connections;
        }


        public static int GetInnerMostConnectedTargetLane(LaneData sourceLane, LaneData[] targetLanes) {
            for (int i = targetLanes.Length - 1; i >= 0; ++i) {
                if (LaneConnectionManager.Instance.AreLanesConnected(
                    sourceLaneId: sourceLane.LaneID,
                    targetLaneId: targetLanes[i].LaneID,
                    sourceStartNode: sourceLane.StartNode))
                    return i;
            }
            return 0;
        }

        public static int GetInnerMostConnectedSourceLane(LaneData targetLane, LaneData[] sourceLanes) {
            for (int i = sourceLanes.Length - 1; i >= 0; ++i) {
                if (LaneConnectionManager.Instance.AreLanesConnected(
                    sourceLaneId: sourceLanes[i].LaneID,
                    targetLaneId: targetLane.LaneID,
                    sourceStartNode: sourceLanes[i].StartNode))
                    return i;
            }
            return 0;
        }


        public IEnumerable<Pair> IterateConnections() {
            for (int i = 0; i < Connections1.Length; ++i) {
                int j = Connections1[i];
                if (j < 0) continue;
                yield return new Pair {
                    a = SegmentEnd1.Source.ControlPoints[i],
                    b = SegmentEnd2.Target.ControlPoints[j]
                };
            }
            for (int i = 0; i < Connections2.Length; ++i) {
                int j = Connections2[i];
                if (j < 0) continue;
                yield return new Pair {
                    a = SegmentEnd2.Source.ControlPoints[i],
                    b = SegmentEnd1.Target.ControlPoints[j]
                };
            }
        }

        public struct Pair {
            public LaneMarkerPoint a;
            public LaneMarkerPoint b;
        }
    }


    public class LaneMarker {
        public CubicBezier3 bezier1;
        public CubicBezier3 bezier2;
        public NetInfo Info;

        public NodeWrapper Node1;
        public SegmentWrapper Segment1;
        public NodeWrapper MiddleNode;
        public SegmentWrapper Segment2;
        public NodeWrapper Node2;

        public LaneMarker(CubicBezier3 bezier1, CubicBezier3 bezier2, NetInfo info) {
            Node1 = new NodeWrapper(bezier1.Start.Point, 0, info);
            //HelpersExtensions.AssertEqual(bezier1.End.Point, bezier2.Start.Point);
            MiddleNode = new NodeWrapper(bezier1.End.Point, 0, info);
            Node2 = new NodeWrapper(bezier2.End.Point, 0, info);
            Segment1 = new SegmentWrapper(Node1, MiddleNode, bezier1.Start.Dir, bezier1.End.Dir);
            Segment2 = new SegmentWrapper(MiddleNode, Node2, bezier2.Start.Dir, bezier2.End.Dir);
        }

        public void Create() {
            Node1.Create();
            MiddleNode.Create();
            Node2.Create();
            Segment1.Create();
            Segment2.Create();
        }
    }

    public class TransitionMarkers {
        public List<LaneMarker> Markers;
        public static NetInfo Info;

        public TransitionMarkers(ushort segmentID1, ushort segmentID2) {
            var Connections = new ConnectionWrapper(segmentID1, segmentID2);
            Markers = new List<LaneMarker>();
            foreach (var pair in Connections.IterateConnections()) {
                CalculateBeziers(pair.a, pair.b, out var b1, out var b2);
                Markers.Add(new LaneMarker(b1, b2, Info));
            }
        }

        public void Create() {
            foreach (var item in Markers)
                item.Create();
        }

        public static void CalculateBeziers(
            LaneMarkerPoint control1, LaneMarkerPoint control2,
            out CubicBezier3 bezier1, out CubicBezier3 bezier2) {
            var bezier = new CubicBezier3 {
                Start = control1.PointDir.Reverse,
                End = control2.PointDir.Reverse,
            };
            var center = bezier.GetCenterPointAndDir();
            bezier1 = new CubicBezier3 {
                Start = bezier.Start,
                End = center.Reverse,
            };
            bezier2 = new CubicBezier3 {
                Start = center,
                End = bezier.End,
            };
        }
    }


    public static class LaneConnectionUtil {
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
