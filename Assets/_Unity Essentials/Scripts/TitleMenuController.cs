using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TitleMenuController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource; // InspectorでAudioSourceをアタッチ
    public AudioClip clickSound;    // 再生したい音ファイルをアタッチ

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        Button startButton = root.Q<Button>("StartButton");

        // ボタンがクリックされた時の処理を登録
        if (startButton != null)
        {
            startButton.clicked += OnStartButtonClicked;
        }

    }

    private void OnStartButtonClicked()
    {
        //音を鳴らす
        if(audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        StartCoroutine(DelayedLoadScene());
    }

    System.Collections.IEnumerator DelayedLoadScene()
    {
        yield return new WaitForSeconds(0.5f);
        // Scene 1 を読み込む
        SceneManager.LoadScene(1);
    }
}