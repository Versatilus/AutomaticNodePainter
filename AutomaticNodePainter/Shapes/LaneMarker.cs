namespace AutomaticNodePainter.Shapes {
    using AutomaticNodePainter.Math;

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
            Node1 = new NodeWrapper(bezier1.Start.Point, info);
            //HelpersExtensions.AssertEqual(bezier1.End.Point, bezier2.Start.Point);
            MiddleNode = new NodeWrapper(bezier1.End.Point, info);
            Node2 = new NodeWrapper(bezier2.End.Point, info);
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
}
