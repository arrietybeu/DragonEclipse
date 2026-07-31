using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Cho phep game chay khi KHONG co Steam client.
///
/// VAN DE (da chung minh bang thuc nghiem 2026-07-31):
///   StoreFactory.Get()  =  if (Debug.isDebugBuild &amp;&amp; !Application.isEditor) DisabledStore
///                          else                                              SteamProvider.Init()
///   SteamProvider.Init() =  if (SteamAPI.Init())      return new SteamProvider();
///                           if (Application.isEditor) throw new Exception("Run Steam!");
///                           Application.Quit();       return null;
///   -> Ban Release + Steam khong chay = Application.Quit() ngay trong CompositionRoot.Awake().
///      Cua so hien ra roi tu dong dong sau ~13 giay (exit code 0). Trong Editor thi
///      nem "Exception: Run Steam!" va chuoi khoi dong chet tai do.
///   Ngoai ra CompositionRoot.RegisterServices (IL_0128) tao SteamAchievementsProvider
///   VO DIEU KIEN, ma ctor cua no goi SteamUserStats.RequestCurrentStats() -> nem
///   "Steamworks is not initialized" khi khong co Steam.
///
/// CACH VA: chi cat bo cac lenh goi Steam (thay bang nop), GIU NGUYEN phan goi lop cha.
///   - Awaken.StoreProvider.StoreFactory.Get        -> luon tra ve DisabledStore
///   - Awaken.Achievements.SteamAchievementsProvider -> ctor + 6 ham chi con goi
///                                                      AchievementsProvider (lop cha, chi ghi log)
///   - Awaken.Utility.LinkOpener.OpenLink           -> luon dung Application.OpenURL
/// DisabledStore va AchievementsProvider deu la ban cai dat hoan chinh san co trong game,
/// nen khong can them ma moi. Do dai IL giu nguyen tuyet doi, khong dung toi metadata.
///
/// AN TOAN: sao luu ban goc vao _backup_unity6/steam_disable/ truoc khi ghi, va co menu
/// khoi phuc. Moi mau byte phai xuat hien DUNG 1 lan, neu khong thi khong ghi gi ca.
/// </summary>
public static class SteamDisablePatcher
{
    const string k_Plugins = "Assets/Plugins/";
    const string k_Backup = "_backup_unity6/steam_disable/";

    struct Patch
    {
        public string File, Name, Find, Repl;
        public Patch(string f, string n, string find, string repl) { File = f; Name = n; Find = find; Repl = repl; }
    }

    static string Nop(int n)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < n; i++) sb.Append(" 00");
        return sb.ToString();
    }

    static List<Patch> BuildPatches()
    {
        const string store = "Awaken.StoreProvider.dll";
        const string ach = "Awaken.Achievements.dll";
        const string util = "Awaken.Utility.dll";

        return new List<Patch>
        {
            new Patch(store, "StoreFactory.Get -> luon DisabledStore",
                "28 06 00 00 0A 2C 0D 28 07 00 00 0A 2D 06 28 08 00 00 06 2A 28 16 00 00 06 2A",
                Nop(14) + " 28 08 00 00 06 2A 28 16 00 00 06 2A"),

            new Patch(ach, "SteamAchievementsProvider..ctor",
                "02 03 04 28 03 00 00 06 28 0C 00 00 0A 2D 0A 72 23 02 00 70 28 0D 00 00 0A 2A",
                "02 03 04 28 03 00 00 06" + Nop(17) + " 2A"),

            new Patch(ach, "SteamAchievementsProvider.ResetAll",
                "02 28 05 00 00 06 17 28 0E 00 00 0A 26 2A",
                "02 28 05 00 00 06" + Nop(7) + " 2A"),

            new Patch(ach, "SteamAchievementsProvider.SetCompleted",
                "02 03 28 06 00 00 06 02 7B 01 00 00 04 03 6F 0F 00 00 0A 6F 10 00 00 0A 28 11 00 00 0A 26 28 12 00 00 0A 26 2A",
                "02 03 28 06 00 00 06" + Nop(29) + " 2A"),

            new Patch(ach, "SteamAchievementsProvider.IsCompleted",
                "02 03 28 07 00 00 06 26 02 7B 01 00 00 04 03 6F 0F 00 00 0A 6F 10 00 00 0A 12 00 28 13 00 00 0A 26 06 2A",
                "02 03 28 07 00 00 06" + Nop(27) + " 2A"),

            new Patch(ach, "SteamAchievementsProvider.IncrementValue",
                "02 03 28 08 00 00 06 02 7B 01 00 00 04 03 6F 14 00 00 0A 0A 06 6F 15 00 00 0A 12 01 28 16 00 00 0A 26 28 0C 00 00 0A 2C 41 06 6F 15 00 00 0A 07 17 58 28 17 00 00 0A 2D 15 72 5F 02 00 70 06 6F 15 00 00 0A 28 0A 00 00 0A 28 0D 00 00 0A 28 12 00 00 0A 2D 15 72 85 02 00 70 06 6F 15 00 00 0A 28 0A 00 00 0A 28 0D 00 00 0A 2A",
                "02 03 28 08 00 00 06" + Nop(99) + " 2A"),

            new Patch(ach, "SteamAchievementsProvider.SetStatValue",
                "02 03 04 28 09 00 00 06 02 7B 01 00 00 04 03 6F 14 00 00 0A 0A 28 0C 00 00 0A 2C 40 06 6F 15 00 00 0A 04 28 17 00 00 0A 2D 15 72 5F 02 00 70 06 6F 15 00 00 0A 28 0A 00 00 0A 28 0D 00 00 0A 28 12 00 00 0A 2D 2B 72 85 02 00 70 06 6F 15 00 00 0A 28 0A 00 00 0A 28 0D 00 00 0A 2A 72 B1 02 00 70 06 6F 15 00 00 0A 28 0A 00 00 0A 28 0D 00 00 0A 2A",
                "02 03 04 28 09 00 00 06" + Nop(105) + " 2A"),

            new Patch(ach, "SteamAchievementsProvider.GetStatValue",
                "02 03 28 0A 00 00 06 26 02 7B 01 00 00 04 03 6F 14 00 00 0A 6F 15 00 00 0A 12 00 28 16 00 00 0A 26 02 72 EF 02 00 70 06 8C 11 00 00 01 28 07 00 00 0A 28 0B 00 00 06 06 2A",
                "02 03 28 0A 00 00 06" + Nop(49) + " 2A"),

            new Patch(util, "LinkOpener.OpenLink -> luon Application.OpenURL",
                "03 2C 08 02 16 28 47 00 00 0A 2A 02 28 48 00 00 0A 2A",
                Nop(11) + " 02 28 48 00 00 0A 2A"),
        };
    }

    static byte[] Hex(string s)
    {
        var parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var b = new byte[parts.Length];
        for (var i = 0; i < parts.Length; i++) b[i] = Convert.ToByte(parts[i], 16);
        return b;
    }

    static int FindUnique(byte[] hay, byte[] needle, out int hits)
    {
        hits = 0;
        var at = -1;
        for (var i = 0; i <= hay.Length - needle.Length; i++)
        {
            if (hay[i] != needle[0]) continue;
            var match = true;
            for (var j = 1; j < needle.Length; j++)
                if (hay[i + j] != needle[j]) { match = false; break; }
            if (match) { hits++; at = i; }
        }
        return at;
    }

    static string Root => Directory.GetCurrentDirectory().Replace('\\', '/') + "/";

    [MenuItem("Tools/Dragon Eclipse/Steam/1. Kiem tra trang thai (khong ghi gi)", priority = 0)]
    public static void Inspect()
    {
        var sb = new StringBuilder("[TatSteam] Kiem tra:\n");
        foreach (var p in BuildPatches())
        {
            var path = Root + k_Plugins + p.File;
            if (!File.Exists(path)) { sb.AppendLine("  ? THIEU " + p.File); continue; }
            var bytes = File.ReadAllBytes(path);
            int hitsFind, hitsRepl;
            FindUnique(bytes, Hex(p.Find), out hitsFind);
            FindUnique(bytes, Hex(p.Repl), out hitsRepl);
            if (hitsRepl == 1 && hitsFind == 0) sb.AppendLine("  DA VA   " + p.Name);
            else if (hitsFind == 1) sb.AppendLine("  chua va " + p.Name);
            else sb.AppendLine("  ? KHONG RO (" + hitsFind + " goc / " + hitsRepl + " da va)  " + p.Name);
        }
        Debug.Log(sb.ToString());
    }

    [MenuItem("Tools/Dragon Eclipse/Steam/2. TAT Steam (va DLL)", priority = 1)]
    public static void Disable()
    {
        var patches = BuildPatches();
        var files = new List<string>();
        foreach (var p in patches) if (!files.Contains(p.File)) files.Add(p.File);

        // Buoc 1: kiem tra toan bo truoc, khong ghi gi.
        var offsets = new Dictionary<string, int>();
        var report = new StringBuilder();
        var ok = true;
        foreach (var p in patches)
        {
            var path = Root + k_Plugins + p.File;
            if (!File.Exists(path)) { report.AppendLine("  X THIEU " + p.File); ok = false; continue; }
            var find = Hex(p.Find);
            var repl = Hex(p.Repl);
            if (find.Length != repl.Length)
            {
                report.AppendLine("  X " + p.Name + " : do dai lech " + find.Length + " vs " + repl.Length);
                ok = false; continue;
            }
            var bytes = File.ReadAllBytes(path);
            int hits;
            var at = FindUnique(bytes, find, out hits);
            if (hits == 0 && FindUnique(bytes, repl, out hits) >= 0 && hits == 1)
            {
                report.AppendLine("  - " + p.Name + " : da va tu truoc, bo qua");
                continue;
            }
            if (hits != 1)
            {
                report.AppendLine("  X " + p.Name + " : tim thay " + hits + " lan (can dung 1)");
                ok = false; continue;
            }
            offsets[p.Name] = at;
            report.AppendLine("  OK " + p.Name + " : offset 0x" + at.ToString("X") + ", " + find.Length + " byte");
        }

        if (!ok)
        {
            Debug.LogError("[TatSteam] KHONG GHI GI CA - co muc khong khop:\n" + report);
            EditorUtility.DisplayDialog("Tat Steam", "Khong va duoc, xem Console.", "OK");
            return;
        }
        if (offsets.Count == 0)
        {
            Debug.Log("[TatSteam] Moi thu da duoc va tu truoc.\n" + report);
            return;
        }

        // Buoc 2: sao luu roi ghi.
        var backupDir = Root + k_Backup;
        Directory.CreateDirectory(backupDir);
        foreach (var f in files)
        {
            var path = Root + k_Plugins + f;
            var bak = backupDir + f + ".original";
            if (!File.Exists(bak)) { File.Copy(path, bak); report.AppendLine("  sao luu -> " + bak); }

            var bytes = File.ReadAllBytes(path);
            var touched = false;
            foreach (var p in patches)
            {
                if (p.File != f || !offsets.ContainsKey(p.Name)) continue;
                var repl = Hex(p.Repl);
                var at = offsets[p.Name];
                for (var j = 0; j < repl.Length; j++) bytes[at + j] = repl[j];
                touched = true;
            }
            if (!touched) continue;
            try { File.WriteAllBytes(path, bytes); report.AppendLine("  >> ghi xong " + f); }
            catch (Exception e) { report.AppendLine("  >> GHI THAT BAI " + f + " : " + e.Message); ok = false; }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        if (ok)
        {
            Debug.Log("[TatSteam] HOAN TAT - game se chay duoc khi khong co Steam.\n" + report);
            EditorUtility.DisplayDialog("Tat Steam",
                "Da va xong. Unity se nap lai assembly.\nBuild lai de ban PC co hieu luc.", "OK");
        }
        else
        {
            Debug.LogError("[TatSteam] Co file ghi khong duoc (Unity dang khoa). Dong Unity roi thu lai.\n" + report);
        }
    }

    [MenuItem("Tools/Dragon Eclipse/Steam/3. Khoi phuc DLL goc", priority = 2)]
    public static void Restore()
    {
        var patches = BuildPatches();
        var files = new List<string>();
        foreach (var p in patches) if (!files.Contains(p.File)) files.Add(p.File);

        var sb = new StringBuilder("[TatSteam] Khoi phuc:\n");
        var any = false;
        foreach (var f in files)
        {
            var bak = Root + k_Backup + f + ".original";
            if (!File.Exists(bak)) { sb.AppendLine("  ? khong co ban sao luu cua " + f); continue; }
            try
            {
                File.Copy(bak, Root + k_Plugins + f, true);
                sb.AppendLine("  da tra lai " + f);
                any = true;
            }
            catch (Exception e) { sb.AppendLine("  X " + f + " : " + e.Message); }
        }
        if (any) AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log(sb.ToString());
    }
}
