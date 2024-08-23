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

    // �R���X�g���N�^�FBoxClient��������
    public BoxUploader(string clientId, string clientSecret, string developerToken)
    {
        try
        {
            // BoxClient�̐ݒ���쐬
            var config = new BoxConfigBuilder(clientId, clientSecret).Build();
            // �Z�b�V�������쐬
            var session = new OAuthSession(developerToken, "refresh-token", 3600, "bearer");
            // BoxClient��������
            _client = new BoxClient(config, session);

            if (_client == null)
            {
                Debug.LogError("BoxClient�̍쐬�Ɏ��s���܂����B");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("BoxUploader�̏������Ɏ��s���܂���: " + ex.Message);
        }
    }

    // �摜��Box�ɃA�b�v���[�h���A���L�����N���擾����񓯊����\�b�h
    public async Task<string> UploadImageAsync(string filePath, string folderId = "0")
    {
        try
        {
            // BoxClient������������Ă��邩�m�F
            if (_client == null)
            {
                throw new System.Exception("BoxClient������������Ă��܂���B");
            }

            // FilesManager������������Ă��邩�m�F
            if (_client.FilesManager == null)
            {
                throw new System.Exception("FilesManager������������Ă��܂���B");
            }

            // �w�肳�ꂽ�t�@�C���p�X�����݂��邩�m�F
            if (!File.Exists(filePath))
            {
                throw new System.Exception("�t�@�C���p�X�Ƀt�@�C�������݂��܂���: " + filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                // �t�@�C����Box�ɃA�b�v���[�h
                var boxFile = await _client.FilesManager.UploadAsync(new BoxFileRequest
                {
                    Name = Path.GetFileName(filePath),
                    Parent = new BoxRequestEntity { Id = folderId }
                }, fileStream);

                if (boxFile == null)
                {
                    throw new System.Exception("�t�@�C���̃A�b�v���[�h�Ɏ��s���܂����BBoxFile��null�ł��B");
                }

                // ���L�����N���Ȃ��ꍇ�A�V���ɍ쐬
                if (boxFile.SharedLink == null)
                {
                    var sharedLinkRequest = new BoxSharedLinkRequest
                    {
                        Access = BoxSharedLinkAccessType.open
                    };

                    boxFile = await _client.FilesManager.CreateSharedLinkAsync(boxFile.Id, sharedLinkRequest);

                    if (boxFile.SharedLink == null)
                    {
                        throw new System.Exception("�A�b�v���[�h���ꂽ�t�@�C���̋��L�����N�쐬�Ɏ��s���܂����B");
                    }
                }

                // ���L�����N��URL��Ԃ�
                return boxFile.SharedLink.Url;
            }
        }
        catch (BoxAPIException apiEx)
        {
            // Box API����̗�O���L���b�`
            Debug.LogError($"Box API�G���[: {apiEx.Message}");
            Debug.LogError($"�X�e�[�^�X�R�[�h: {apiEx.StatusCode}");
            Debug.LogError($"���X�|���X�w�b�_�[: {apiEx.ResponseHeaders}");
            throw;
        }
        catch (System.Exception ex)
        {
            // ���̑��̗�O���L���b�`
            Debug.LogError($"UploadImageAsync�Ɏ��s���܂���: {ex.Message}");
            throw;
        }
    }
}
