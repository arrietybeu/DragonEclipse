using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Chep steam_appid.txt vao canh file exe sau moi lan build standalone.
/// Thieu file nay thi SteamAPI.Init() that bai (du Steam dang chay), va vi
/// StoreFactory.Get() chi tra ve DisabledStore khi (Debug.isDebugBuild &amp;&amp; !Application.isEditor)
/// nen ban release luon di duong Steam -> SteamAchievementsProvider nem
/// "Steamworks is not initialized" ngay trong CompositionRoot.Awake() -> man hinh den.
/// </summary>
public class SteamAppIdPostBuild : IPostprocessBuildWithReport
{
    const string k_FileName = "steam_appid.txt";

    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        var target = report.summary.platform;
        if (target != BuildTarget.StandaloneWindows64 && target != BuildTarget.StandaloneWindows &&
            target != BuildTarget.StandaloneLinux64 && target != BuildTarget.StandaloneOSX)
            return;

        var source = Path.Combine(Directory.GetCurrentDirectory(), k_FileName);
        if (!File.Exists(source))
        {
            Debug.LogWarning($"[SteamAppId] Khong tim thay {k_FileName} o thu muc goc project, bo qua.");
            return;
        }

        var outputDir = Path.GetDirectoryName(report.summary.outputPath);
        if (string.IsNullOrEmpty(outputDir))
            return;

        var destination = Path.Combine(outputDir, k_FileName);
        File.Copy(source, destination, true);
        Debug.Log($"[SteamAppId] Da chep {k_FileName} -> {destination}");
    }
}
