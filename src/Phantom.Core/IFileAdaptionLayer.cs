
namespace Phantom.Core
{
    interface IFileAdaptionLayer
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        string[] GetDirectories(string path, string searchPattern);
        string GetFileName(string file);
        string[] GetFiles(string path, string searchPattern);
    }
}
