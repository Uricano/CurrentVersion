using Framework.Core.Interfaces.ReadFiles;

namespace Framework.Core.ReadFiles
{
    public class FileData : IFileData
    {
        public FileData()
        {
        }
        
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}
