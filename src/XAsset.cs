namespace BlackOps2Explorer
{
    enum XAssetType
    {
        Raw
    }

    class XAsset
    {
        public string Path { get; set; }
        public byte[] Data { get; set; }
        public XAssetType Type { get; set; }
        public bool IsRawText { get; set; }

        public XAsset(string path, byte[] data, XAssetType type, bool isRawText)
        {
            Path = path;
            Data = data;
            Type = type;
            IsRawText = isRawText;
        }

        public override string ToString()
        {
            return Path;
        }
    }

}
