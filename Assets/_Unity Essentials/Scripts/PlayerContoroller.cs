using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Moves forward/backward and rotates with WASD/Arrow keys.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Basic Settings")]
    public float speed = 10.0f;
    public float rotationSpeed = 120.0f;
    public float jumpForce = 5.0f;
    public bool canControl = false;

    [Header("Change Settings")]
    public float chargeSpeedMultiplierDefault = 0.3f; // 直進チャージ中の速度（かなり遅め）
    public float chargeSpeedMultiplierDrift = 0.8f;   // 曲がりながらチャージ中の速度（速め）
    public float chargeRotationMultiplier = 2.0f;     // チャージ中の曲がりやすさ
    public float maxChargeTime = 0.7f; //最大チャージ時間
    public float boostForce = 10.0f; //ダッシュ時の追加速度
    public float boostDuration = 0.3f; //ダッシュの効果時間

    [Header("Effect Settings")]
    public ParticleSystem driveEffect; // 常時走っている時のエフェクト
    public GameObject driftEffect;  // チャージ曲がり中のエフェクト
    public GameObject chargeFullEffect;  // チャージ完了時のエフェクト
    public GameObject goalEffect; //ゴール時のエフェクト
    public GameObject dashEffect; //ダッシュ時のエフェクト

    [Header("Audio Settings")]
    public AudioSource engineAudioSource;
    public AudioSource chargeAudioSource;
    public AudioSource seAudioSource;

    public AudioClip dashSound;
    public AudioClip chargeFullSound;
    public float minEnginePitch = 1.0f;
    public float maxEnginePitch = 2.0f;
    private bool fullChargeFlag = false;


    private Rigidbody rb;
    private float currentChargeTime = 0f;
    private bool isCharging = false;
    private float currentBoostVelocity = 0f; //ブースト加算値
    private float boostTimer = 0f;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogWarning("PlayerController needs a Rigidbody.");

        //エンジンを鳴らす
        if (engineAudioSource != null)
        {
            engineAudioSource.loop = true;
            engineAudioSource.Play();
        }

        //スタースリップをつける
        driveEffect.Play();
    }

    private void Update()
    {
        if (!canControl)
        {
            //動けないとき、エンジンの音を下げるか止める
            if (engineAudioSource != null) engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, 0, Time.deltaTime);
            return;
        }

        //ジャンプ処理
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        HandleChargeInput(); //チャージの調整
        HandleAudioPitch(); //エンジンのオーディオ調整
    }


    private void HandleChargeInput()
    {
        //シフトキーもしくは右クリックでチャージ
        bool chargeInput = Keyboard.current.shiftKey.isPressed || Mouse.current.leftButton.isPressed;

        if(chargeInput) //チャージボタンが押されているとき
        {
            if(!isCharging) //チャージした瞬間
            {
                if (chargeAudioSource != null) chargeAudioSource.Play();
                fullChargeFlag = false; //チャージ開始時にフラグをリセット
            }

            isCharging = true;
            currentChargeTime += Time.deltaTime;
            //currentChargeTime が 0 未満にならないように、かつ maxChargeTimeを超えて増え続けないようにする
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0, maxChargeTime);
            
            //チャージが完了したとき
            if(currentChargeTime >= maxChargeTime && !fullChargeFlag)
            {
                if(seAudioSource != null && chargeFullSound != null)
                {
                    seAudioSource.PlayOneShot(chargeFullSound);
                    if (chargeFullEffect) Instantiate(chargeFullEffect, transform.position, transform.rotation);
                }
                fullChargeFlag = true;
                Debug.Log("full charge!");
            }

            //チャージ時間に合わせてピッチ音上げる
            if(chargeAudioSource != null)
            {
                chargeAudioSource.pitch = 1.0f + (currentChargeTime / maxChargeTime);
            }
        }
        else //チャージボタンが押されていないとき
        {
            if (isCharging) //開放の瞬間
            {
                if (chargeAudioSource != null) chargeAudioSource.Stop();

                if (currentChargeTime >= maxChargeTime)
                {
                    currentBoostVelocity = boostForce; //固定のパワーを設定
                    boostTimer = boostDuration;
                    if (seAudioSource != null && dashSound != null) seAudioSource.PlayOneShot(dashSound);
                    if (dashEffect) Instantiate(dashEffect, transform.position, transform.rotation);
                    Debug.Log("Dash");
                }
            }
            isCharging = false;
            currentChargeTime = 0f;
            fullChargeFlag = false;
        }

        //ダッシュ後の滑らかな減速
        if(boostTimer <= 0)
        {
            //currentBoostVelocity（ダッシュの速度）を、0 に向かってなめらかに減らす 時間にかけてる数を大きくすることでブレーキの強度が上がる
            currentBoostVelocity = Mathf.Lerp(currentBoostVelocity, 0, Time.deltaTime * 2f);
        }
        else
        {
            boostTimer -= Time.deltaTime;
        }
    }

    private void HandleAudioPitch()
    {
        if (engineAudioSource == null) return;

        float speedPercent = rb.linearVelocity.magnitude / (speed + boostForce);
        engineAudioSource.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedPercent);
    }

    private void FixedUpdate()
    {

        if (!canControl) return;

        Vector2 moveInput = Vector2.zero;
        moveInput.y = 1f;

        // Forward/backward
        //if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y = 1f;
        //if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y = -1f;

        // Left/right (rotation)
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x = 1f;

        //チャージとダッシュの変数
        float currentSpeed = speed;
        float currentRotSpeed = rotationSpeed;

        if(isCharging)
        {
            if (Mathf.Abs(moveInput.x) > 0.1f) // ハンドルを切っている（絶対値が0より大きい）
            {
                // ドリフト状態：速度を維持しつつ、クイックに曲がる
                currentSpeed *= chargeSpeedMultiplierDrift;
                currentRotSpeed *= chargeRotationMultiplier;

            }
            else
            {
                // 直進チャージ状態：強く減速する　かつ　チャージがたまらないようにする
                currentSpeed *= chargeSpeedMultiplierDefault;
                currentChargeTime -= Time.deltaTime;
            }
        }

        //チャージダッシュのブースト
        float finalForwardSpeed = currentSpeed + currentBoostVelocity;

        // Move in facing direction 
        Vector3 movement = transform.forward * moveInput.y * finalForwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // Y-axis rotation (invert when going backwards)
        float turnDirection = moveInput.x;
        if (moveInput.y < 0)
            turnDirection = -turnDirection;

        float turn = turnDirection * currentRotSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        if (isCharging && Mathf.Abs(moveInput.x) > 0.1f)
        {
            if (driftEffect) Instantiate(driftEffect, transform.position, transform.rotation);
        }
    }


    public void GoalDrift()
    {
        if (goalEffect)
        {
            Instantiate(goalEffect, transform.position, transform.rotation);
            Debug.Log("goaleffect");
        }
        if (driveEffect != null) driveEffect.Stop();
        StartCoroutine(GoalDriftRoutine());
    }

    IEnumerator GoalDriftRoutine()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startVelocity = transform.forward * speed;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, -90, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // なめらかに速度をゼロにする
            Vector3 currentVelocity = Vector3.Lerp(startVelocity, Vector3.zero, t);

            //動かす
            transform.position += currentVelocity * Time.deltaTime;

            // なめらかに90度回転させる
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

}
