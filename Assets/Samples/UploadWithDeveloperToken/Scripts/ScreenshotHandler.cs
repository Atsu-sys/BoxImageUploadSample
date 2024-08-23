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
        // BoxUploader�̏������i�K�؂ȃN���C�A���gID�A�N���C�A���g�V�[�N���b�g�A�f�x���b�p�[�g�[�N����ݒ�j
        boxUploader = new BoxUploader(clientId, clientSecret, developerToken);
    }

    public async void CaptureAndUploadScreenshot()
    {
        // ���݂̓��t�Ǝ������擾���A�t�@�C�����Ɏg�p
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = "Screenshot_" + timestamp + ".png";

        // �p�X���\�z
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // �X�N���[���V���b�g��ۑ�
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Screenshot saved at: " + path);

        // �ۑ�����������܂ŏ����ҋ@�i�X�N���[���V���b�g���ۑ�����鎞�ԁj
        await Task.Delay(500);  // 0.5�b�ҋ@

        // �X�N���[���V���b�g��Box�ɃA�b�v���[�h
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
