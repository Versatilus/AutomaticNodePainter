namespace AutomaticNodePainter.Util {
    public static class  PrefabUtil {
        public static NetInfo defaultPrefab => SolidLine;
        public static NetInfo SolidLine =>
            GetInfo("1708100811.Solid Line (White)_Data");
      
        public static NetInfo GetInfo(string name) {
            int count = PrefabCollection<NetInfo>.LoadedCount();
            for (uint i = 0; i < count; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (info.name == name)
                    return info;
                //Helpers.Log(info.name);
            }
            throw new System.Exception("NetInfo not found!");
        }
    }
}
