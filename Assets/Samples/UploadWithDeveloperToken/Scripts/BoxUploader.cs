using Box.V2;
using Box.V2.Config;
using Box.V2.Models;
using Box.V2.Auth;
using Box.V2.Exceptions;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class BoxUploader
{
    private BoxClient _client;

    // コンストラクタ：BoxClientを初期化
    public BoxUploader(string clientId, string clientSecret, string developerToken)
    {
        try
        {
            // BoxClientの設定を作成
            var config = new BoxConfigBuilder(clientId, clientSecret).Build();
            // セッションを作成
            var session = new OAuthSession(developerToken, "refresh-token", 3600, "bearer");
            // BoxClientを初期化
            _client = new BoxClient(config, session);

            if (_client == null)
            {
                Debug.LogError("BoxClientの作成に失敗しました。");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("BoxUploaderの初期化に失敗しました: " + ex.Message);
        }
    }

    // 画像をBoxにアップロードし、共有リンクを取得する非同期メソッド
    public async Task<string> UploadImageAsync(string filePath, string folderId = "0")
    {
        try
        {
            // BoxClientが初期化されているか確認
            if (_client == null)
            {
                throw new System.Exception("BoxClientが初期化されていません。");
            }

            // FilesManagerが初期化されているか確認
            if (_client.FilesManager == null)
            {
                throw new System.Exception("FilesManagerが初期化されていません。");
            }

            // 指定されたファイルパスが存在するか確認
            if (!File.Exists(filePath))
            {
                throw new System.Exception("ファイルパスにファイルが存在しません: " + filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                // ファイルをBoxにアップロード
                var boxFile = await _client.FilesManager.UploadAsync(new BoxFileRequest
                {
                    Name = Path.GetFileName(filePath),
                    Parent = new BoxRequestEntity { Id = folderId }
                }, fileStream);

                if (boxFile == null)
                {
                    throw new System.Exception("ファイルのアップロードに失敗しました。BoxFileがnullです。");
                }

                // 共有リンクがない場合、新たに作成
                if (boxFile.SharedLink == null)
                {
                    var sharedLinkRequest = new BoxSharedLinkRequest
                    {
                        Access = BoxSharedLinkAccessType.open
                    };

                    boxFile = await _client.FilesManager.CreateSharedLinkAsync(boxFile.Id, sharedLinkRequest);

                    if (boxFile.SharedLink == null)
                    {
                        throw new System.Exception("アップロードされたファイルの共有リンク作成に失敗しました。");
                    }
                }

                // 共有リンクのURLを返す
                return boxFile.SharedLink.Url;
            }
        }
        catch (BoxAPIException apiEx)
        {
            // Box APIからの例外をキャッチ
            Debug.LogError($"Box APIエラー: {apiEx.Message}");
            Debug.LogError($"ステータスコード: {apiEx.StatusCode}");
            Debug.LogError($"レスポンスヘッダー: {apiEx.ResponseHeaders}");
            throw;
        }
        catch (System.Exception ex)
        {
            // その他の例外をキャッチ
            Debug.LogError($"UploadImageAsyncに失敗しました: {ex.Message}");
            throw;
        }
    }
}
