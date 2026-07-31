using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Awaken.Community.Localization
{
    /// <summary>
    /// Chua mot ngon ngu duoc them vao ngoai cac ngon ngu da nam trong Addressable bundle goc.
    /// Asset that phai nam trong thu muc Resources de nap duoc luc chay ma khong can Addressables.
    /// </summary>
    public class ExtraLocaleData : ScriptableObject
    {
        [Tooltip("Locale asset cua ngon ngu them vao, vi du Vietnamese (vi).")]
        public Locale Locale;

        [Tooltip("Toan bo StringTable cua ngon ngu do. Moi bang phai co SharedData tro dung collection.")]
        public StringTable[] Tables;
    }
}
