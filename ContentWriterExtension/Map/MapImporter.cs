using Microsoft.Xna.Framework.Content.Pipeline;

using TImport = System.String;

namespace ContentWriterExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentImporter(".map", DisplayName = "Map Importer", DefaultProcessor = "MapProcessor")]
    public class MapImporter : ContentImporter<TImport>
    {
        public override TImport Import(string filename, ContentImporterContext context)
        {
            return System.IO.File.ReadAllText(filename);
        }
    }
}
