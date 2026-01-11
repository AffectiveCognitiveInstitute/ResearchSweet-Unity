using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpecificFolderCleanup : EditorWindow
{
    // Hier den Pfad zu deinem spezifischen Ordner eintragen (relativ zu Assets)
    private const string TargetFolder = "Assets/";

    [MenuItem("Tools/Cleanup/Remove Underscore Files in Specific Folder")]
    public static void Cleanup()
    {
        // Den absoluten Pfad auf dem System ermitteln
        string absolutePath = Path.Combine(Application.dataPath, TargetFolder.Replace("Assets/", ""));

        if (!Directory.Exists(absolutePath))
        {
            Debug.LogError($"[Cleanup] Der Ordner wurde nicht gefunden: {absolutePath}");
            return;
        }

        // 1. Dateien mit dem Namen "_" finden und löschen
        string[] files = Directory.GetFiles(absolutePath, "_._", SearchOption.AllDirectories);
        int fileCount = 0;

        foreach (var file in files)
        {
            if (DeleteFileSystemEntry(file)) fileCount++;
        }

        // 2. Ordner mit dem Namen "_" finden und löschen
        string[] dirs = Directory.GetDirectories(absolutePath, "_._", SearchOption.AllDirectories);
        int dirCount = 0;

        foreach (var dir in dirs)
        {
            if (DeleteFileSystemEntry(dir)) dirCount++;
        }

        Debug.Log($"[Cleanup] Fertig! {fileCount} Dateien und {dirCount} Ordner mit dem Namen '_' wurden entfernt.");
        
        // Unity mitteilen, dass sich die Assets geändert haben
        AssetDatabase.Refresh();
    }

    private static bool DeleteFileSystemEntry(string path)
    {
        try
        {
            // Prüfen ob es eine Datei oder ein Verzeichnis ist
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            // Unity .meta Datei ebenfalls löschen, falls vorhanden
            string metaFile = path + ".meta";
            if (File.Exists(metaFile)) File.Delete(metaFile);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Cleanup] Fehler beim Löschen von {path}: {e.Message}");
            return false;
        }
    }
}
