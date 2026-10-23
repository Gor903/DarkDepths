using UnityEditor;
using UnityEngine;

namespace DarkDepths.EditorTools
{
    // How the game's own short clips are imported: decompressed so nothing waits on a decode,
    // ADPCM so they stay small, mono because every one of them is a point in the room.
    public static class AudioImport
    {
        public static void Apply(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is AudioImporter importer))
            {
                return;
            }

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if (settings.loadType == AudioClipLoadType.DecompressOnLoad &&
                settings.compressionFormat == AudioCompressionFormat.ADPCM &&
                importer.forceToMono)
            {
                return;
            }

            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;

            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.SaveAndReimport();
        }
    }
}
