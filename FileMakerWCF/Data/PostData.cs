namespace FMWData
{
    class PostData
    {
        public string PieceId;
        public string MachineId;
        public string ImportId;
        public string FileContent;

        public PostData(string p, string m, string i, string fc)
        {
            PieceId = p;
            MachineId = m;
            ImportId = i;
            FileContent = fc;
        }
    }
}
