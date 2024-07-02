namespace DotBoil.Email.Models
{
    public class Attachment
    {
        public string FileName { get; set; }
        public string MediaType { get; set; }
        public string SubMediaType { get; set; }
        public MemoryStream Stream { get; set; }

        public Attachment()
        {
        }

        public Attachment(string fileName,string mediaType, string subMediaType, MemoryStream stream)
        {
            FileName = fileName; 
            MediaType = mediaType;
            SubMediaType = subMediaType;
            Stream = stream;
        }
    }
}
