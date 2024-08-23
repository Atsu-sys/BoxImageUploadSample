using System.IO;
using UnityEngine;
using System.Threading.Tasks;

public class ScreenshotHandler : MonoBehaviour
{
    [SerializeField] private string clientId = "";
    [SerializeField] private string clientSecret = "";
    [SerializeField] private string developerToken = "";
    private BoxUploader boxUploader;

    private void Start()
    {
        // BoxUploaderの初期化（適切なクライアントID、クライアントシークレット、デベロッパートークンを設定）
        boxUploader = new BoxUploader(clientId, clientSecret, developerToken);
    }

    public async void CaptureAndUploadScreenshot()
    {
        // 現在の日付と時刻を取得し、ファイル名に使用
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = "Screenshot_" + timestamp + ".png";

        // パスを構築
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // スクリーンショットを保存
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Screenshot saved at: " + path);

        // 保存が完了するまで少し待機（スクリーンショットが保存される時間）
        await Task.Delay(500);  // 0.5秒待機

        // スクリーンショットをBoxにアップロード
        try
        {
            string url = await boxUploader.UploadImageAsync(path);
            Debug.Log("Uploaded file URL: " + url);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to upload screenshot: " + ex.Message);
        }
    }
}
