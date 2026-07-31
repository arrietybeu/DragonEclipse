using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Dam bao camera Overlay cua UI luon nam trong camera stack cua cac camera Base.
///
/// Game tu noi stack trong Awaken.Player.BaseCamera.OnEnable(), nhung no doc
/// Awaken.Player.OverlayCamera.CameraInstance - mot singleton tinh chi duoc gan trong
/// OverlayCamera.OnEnable(). Thu tu OnEnable giua hai GameObject trong cung mot scene la
/// KHONG XAC DINH: neu BaseCamera chay truoc thi CameraInstance con null, stack khong bao gio
/// duoc noi, va vi camera Overlay khong tu ve gi trong URP nen toan bo UI bien mat
/// (man hinh den). Editor tinh co dung thu tu; ban build thi khong.
/// </summary>
public class EnsureCameraStack : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var go = new GameObject("[EnsureCameraStack]") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(go);
        go.AddComponent<EnsureCameraStack>();
    }

    const float k_Interval = 0.5f;
    float _timer;

    void Update()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer < k_Interval)
            return;
        _timer = 0f;

        Camera overlay = null;
        var cameras = Camera.allCameras;
        foreach (var cam in cameras)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null && data.renderType == CameraRenderType.Overlay)
            {
                overlay = cam;
                break;
            }
        }

        if (overlay == null)
            return;

        foreach (var cam in cameras)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data == null || data.renderType != CameraRenderType.Base)
                continue;
            var stack = data.cameraStack;
            if (stack != null && !stack.Contains(overlay))
                stack.Add(overlay);
        }
    }
}
