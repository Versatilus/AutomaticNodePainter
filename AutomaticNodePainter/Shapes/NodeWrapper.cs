using System;
using UnityEngine;
using AutomaticNodePainter.Util;
using static AutomaticNodePainter.Util.PrefabUtil;
using static AutomaticNodePainter.Util.NetUtil;
using AutomaticNodePainter.Math;

namespace AutomaticNodePainter.Shapes {
    public class NodeWrapper {
        public Vector2 point;
        public  NetInfo info;
        public ushort ID { get; private set; }
        public bool IsCreated => ID != 0;

        public NodeWrapper(Vector2 point, NetInfo info) {
            this.point = point;
            this.info = info;
        }

        public void Create() =>
            simMan.AddAction(_Create);

        void _Create() {
            if (IsCreated)
                throw new Exception("Node already has been created");
            Vector3 pos = Get3DPos(point);
            ID = CreateNode(pos, info);
            ID.ToNode().m_flags &= ~NetNode.Flags.Moveable;
        }

        static ushort CreateNode(Vector3 position, NetInfo info) {
            Log.Info($"creating node for {info.name} at position {position.ToString("000.000")}");
            bool res = netMan.CreateNode(node: out ushort nodeID, randomizer: ref simMan.m_randomizer,
                info: info, position: position, buildIndex: simMan.m_currentBuildIndex);
            if (!res)
                throw new NetServiceException("Node creation failed");
            simMan.m_currentBuildIndex++;
            return nodeID;
        }
        public static Vector3 Get3DPos(Vector2 point) {
            float terrainH = terrainMan.SampleDetailHeightSmooth(point.ToCS3D(0));
            return point.ToCS3D(terrainH);
        }

        public Vector3 Get3DPos() => Get3DPos(point);
    }
}
