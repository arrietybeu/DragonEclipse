using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Awaken.Community.Localization
{
    /// <summary>
    /// Tra bang dich cho cac ngon ngu duoc them vao sau. Voi moi ngon ngu khac,
    /// tra ve handle rong de Localization tu quay lai duong nap Addressables goc.
    /// </summary>
    public class ExtraLocaleTableProvider : ITableProvider
    {
        readonly ExtraLocaleData _data;

        public ExtraLocaleTableProvider(ExtraLocaleData data)
        {
            _data = data;
        }

        public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(string tableCollectionName, Locale locale)
            where TTable : LocalizationTable
        {
            if (_data == null || _data.Locale == null || locale == null)
                return default;
            if (locale.Identifier.Code != _data.Locale.Identifier.Code)
                return default;
            if (_data.Tables == null)
                return default;

            for (var i = 0; i < _data.Tables.Length; i++)
            {
                var table = _data.Tables[i];
                if (table == null || table.SharedData == null)
                    continue;
                if (table.SharedData.TableCollectionName != tableCollectionName)
                    continue;

                var casted = table as TTable;
                if (casted == null)
                    return default;
                return Addressables.ResourceManager.CreateCompletedOperation(casted, null);
            }

            return default;
        }
    }

    public static class ExtraLocaleInstaller
    {
        const string k_ResourcePath = "ExtraLocales";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var data = Resources.Load<ExtraLocaleData>(k_ResourcePath);
            if (data == null || data.Locale == null)
                return;

            // Khong duoc cham vao LocalizationSettings.InitializationOperation khi settings chua ton tai:
            // getter do se tu tao mot settings mac dinh va in canh bao "Could not find localization settings".
            // Thu tu chay giua RuntimeInitializeOnLoadMethod cua ta va cua Localization la khong xac dinh,
            // nen phai cho den khi HasSettings bat len.
            if (LocalizationSettings.HasSettings)
                Begin(data);
            else
                Waiter.Run(data);
        }

        static void Begin(ExtraLocaleData data)
        {
            var init = LocalizationSettings.InitializationOperation;
            if (init.IsDone)
                Apply(data);
            else
                init.Completed += _ => Apply(data);
        }

        static void Apply(ExtraLocaleData data)
        {
            var locales = LocalizationSettings.AvailableLocales;
            if (locales == null)
                return;

            if (locales.GetLocale(data.Locale.Identifier) == null)
                locales.AddLocale(data.Locale);

            LocalizationSettings.StringDatabase.TableProvider = new ExtraLocaleTableProvider(data);
            Debug.Log($"[ExtraLocales] Da dang ky {data.Locale.Identifier.Code} voi {(data.Tables == null ? 0 : data.Tables.Length)} bang.");
        }

        /// <summary>Cho LocalizationSettings san sang ma khong ep no khoi tao som.</summary>
        sealed class Waiter : MonoBehaviour
        {
            const float k_TimeoutSeconds = 30f;
            ExtraLocaleData _data;

            internal static void Run(ExtraLocaleData data)
            {
                var go = new GameObject("[ExtraLocales] Waiter") { hideFlags = HideFlags.HideAndDontSave };
                DontDestroyOnLoad(go);
                var waiter = go.AddComponent<Waiter>();
                waiter._data = data;
            }

            IEnumerator Start()
            {
                var deadline = Time.realtimeSinceStartup + k_TimeoutSeconds;
                while (!LocalizationSettings.HasSettings)
                {
                    if (Time.realtimeSinceStartup > deadline)
                    {
                        Debug.LogWarning($"[ExtraLocales] Khong tim thay LocalizationSettings sau {k_TimeoutSeconds:0}s, bo qua {_data.Locale.Identifier.Code}.");
                        Destroy(gameObject);
                        yield break;
                    }
                    yield return null;
                }

                Begin(_data);
                Destroy(gameObject);
            }
        }
    }
}
