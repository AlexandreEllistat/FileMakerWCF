namespace FMWData
{
    class PostData
    {
        public string PieceId;
        public string ImportId;
        public string FileContent;

        public PostData(string p, string i, string fc)
        {
            PieceId = p;
            ImportId = i;
            FileContent = fc;
        }
    }
}
