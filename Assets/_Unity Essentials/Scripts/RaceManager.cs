using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class RaceManager : MonoBehaviour
{
    private Label countdownLabel;
    private Label timerLabel;
    public PlayerController player;
    public CinemachineVirtualCamera vcam;

    private float elapsedTime = 0f;
    private bool isPlaying = false;

    [Header("UI Settings")]
    public UIDocument resultUIDocument;
    private Label resultTimeLabel;
    private Button retryButton;
    private Button titleButton;

    [Header("Audio Settings")]
    public AudioSource audioSource; // InspectorでAudioSourceをアタッチ
    public AudioClip clickSound;    // 再生したい音ファイルをアタッチ
    public AudioClip goalSound;
    public AudioClip failSound;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        countdownLabel = root.Q<Label>("CountdownLabel");
        timerLabel = root.Q<Label>("TimerLabel");

        //リザルト　隠しておく
        if(resultUIDocument != null)
        {
            resultUIDocument.rootVisualElement.style.display = DisplayStyle.None;

            var resultRoot = resultUIDocument.rootVisualElement;
            resultTimeLabel = resultRoot.Q<Label>("Result");
            retryButton = resultRoot.Q<Button>("RetryButton");
            titleButton = resultRoot.Q<Button>("TitleButton");

            retryButton.clicked += OnRetryClicked;
            titleButton.clicked += OnTitleClicked;
        }


        StartCoroutine(CountdownRoutine());
    }

    void Update()
    {
        if(isPlaying)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    IEnumerator CountdownRoutine()
    {
        // 3...
        countdownLabel.text = "3";
        yield return new WaitForSeconds(0.5f);

        // 2...
        countdownLabel.text = "2";
        yield return new WaitForSeconds(0.5f);

        // 1...
        countdownLabel.text = "1";
        yield return new WaitForSeconds(0.5f);

        // Go!
        countdownLabel.text = "Go!";
        isPlaying = true;

        // プレイヤーの操作を許可
        player.canControl = true;

        yield return new WaitForSeconds(0.5f); // Go!を1秒出してから消す

        //カウントダウンだけ消す 透明度をいじる
        countdownLabel.style.opacity = 0;

    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(elapsedTime % 60F);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100F) % 100F);

        timerLabel.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public void Goal()
    {
        if (!isPlaying) return;
        isPlaying = false;

        //カメラ追従オフ
        if (vcam != null)
        {
            vcam.Follow = null;
            vcam.LookAt = null;
        }

        //プレイヤー停止　PlayerControllerにて
        player.canControl = false;
        player.GoalDrift();

        //音を鳴らす
        if (audioSource != null && goalSound != null)
        {
            audioSource.PlayOneShot(goalSound);
        }

        ShowResult();
    }

    void ShowResult()
    {
        if(resultUIDocument != null)
        {
            // メインのタイマー表示を消し、リザルト画面を表示
            timerLabel.style.display = DisplayStyle.None;
            resultUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            resultTimeLabel.text = "Time: " + timerLabel.text;
        }
    }

    public void FallRetire()
    {
        if (!isPlaying) return;
        //タイマーストップ
        isPlaying = false;
        player.canControl = false;

        //カメラ追従オフ
        if (vcam != null)
        {
            vcam.Follow = null;
            vcam.LookAt = null;
        }

        //音を鳴らす
        if (audioSource != null && failSound != null)
        {
            audioSource.PlayOneShot(failSound);
        }

        if (resultUIDocument != null)
        {
            //UIチェンジ
            GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.None;
            resultUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;


            //テキスト書き換え
            var titleLabel = resultUIDocument.rootVisualElement.Q<Label>("Clear");
            if (titleLabel != null) titleLabel.text = "GameOver";
            if (resultTimeLabel != null) resultTimeLabel.text = "Fell into the abyss...";

        }
    }

    //リトライボタン　今のシーン再読み込み
    void OnRetryClicked()
    {
        //音を鳴らす
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        StartCoroutine(DelayedLoadSceneRetry());
    }

    System.Collections.IEnumerator DelayedLoadSceneRetry()
    {
        yield return new WaitForSeconds(0.5f);
        // Scene 0 を読み込む
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
   

    //タイトルボタン　タイトルシーンへ
    void OnTitleClicked()
    {
        //音を鳴らす
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        StartCoroutine(DelayedLoadSceneTitle());
    }

    System.Collections.IEnumerator DelayedLoadSceneTitle()
    {
        yield return new WaitForSeconds(0.5f);
        // Scene 0 を読み込む
        SceneManager.LoadScene(0);
    }
    

 }