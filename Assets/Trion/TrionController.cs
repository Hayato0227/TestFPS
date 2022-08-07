using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations;

public class TrionController : NetworkBehaviour
{
    //トリオンスピード
    public enum Speed
    {
        NormalSpeed,
        SlowSpeed,
        StopSpeed
    }

    //トリオン状態
    public enum Mode
    {
        Straight,
        Chase,
        Controll,
        Equip
    }

    //トリオン生成位置
    public NetworkVariable<PlayerController.Place> place = new();

    //コンポーネント
    private ParentConstraint positionConstraint;

    //初期状態：装備
    public Mode mode = Mode.Equip;
    public int trionPower = 5;

    //スピード
    private float normalSpeed = 30f;
    private static float speedRatio = 0.001f;

    //チェイス用トランスフォーム
    public GameObject targetObj = null;
    
    //その他詳細変数
    private float lifeTime = 200.0f;
    private float duration = 0f;
    public float speed = 0f;
    private float slowDuration = 0f;

    private void Start()
    {
        if(IsOwner)
        {
            //プレイヤーのtransform取得
            Transform tmpTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

            //プレイヤーの右手or左手に追加
            if (place.Value == PlayerController.Place.Right)
            {
                tmpTransform.GetComponent<PlayerController>().rightChargedTrion.Add(this.gameObject);
            }
            else
            {
                tmpTransform.GetComponent<PlayerController>().leftChargedTrion.Add(this.gameObject);
            }

            positionConstraint = GetComponent<ParentConstraint>();
            Vector3 tmpVec = Quaternion.Inverse(tmpTransform.rotation) * (transform.position - tmpTransform.position);

            ConstraintSource my_source = new ConstraintSource();
            my_source.sourceTransform = tmpTransform;

            my_source.weight = 1f;
            positionConstraint.AddSource(my_source);
            positionConstraint.SetTranslationOffset(0, tmpVec);
            positionConstraint.constraintActive = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (mode != Mode.Equip)
        {
            //生存時間を加算、時間が過ぎれば消す
            duration += deltaTime * speed;
            if (duration > lifeTime)
            {
                DestroyServerRpc();
            }
        }

        //クライアントは制御
        if (IsOwner)
        {
            if(slowDuration > 0f)
            {
                slowDuration -= Time.deltaTime;
                if (slowDuration <= 0f) SetTrionSpeed(Speed.NormalSpeed);
            }

            //モードでそれぞれの処理を変更
            if(mode == Mode.Chase && targetObj != null)
            {
                //敵の方向に向く
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetObj.transform.position - transform.position), deltaTime * (trionPower + 1));
            } else if(mode == Mode.Controll)
            {
                //マウス移動で移動する
                float xMove = Input.GetAxis("Mouse X") * (trionPower + 1) * 30;
                float yMove = Input.GetAxis("Mouse Y") * -(trionPower + 1) * 30;

                transform.Rotate(yMove * deltaTime, xMove * deltaTime, 0f);
            }

            //所持状態以外なら前に進む
            if(mode != Mode.Equip)
            {
                positionConstraint.constraintActive = false;
                transform.position += transform.forward * speed * deltaTime;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!CheckMyBarrier(other.transform)) return;

        //バリアに当たったら減速
        if (other.CompareTag("Barrier")) {
            SetTrionSpeed(Speed.SlowSpeed);
        } else if(other.transform.root.CompareTag("Player"))
        {
            //自分じゃないかチェック
            if (other.transform.root.gameObject == NetworkManager.Singleton.LocalClient.PlayerObject.gameObject) return;

            //敵のダメージ関数を呼ぶ
            other.transform.root.GetComponent<PlayerController>().DamageServerRpc(transform.localScale.x * 100f, NetworkManager.Singleton.LocalClientId);

            //アタック音を鳴らす
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.seClips[2], 1f);

            //自身を消去
            DestroyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)] private void DestroyServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
    private void OnTriggerExit(Collider other)
    {
        if (!CheckMyBarrier(other.transform)) return;

        //バリアを抜けたら加速
        if (other.CompareTag("Barrier"))
        {
            SetTrionSpeed(Speed.NormalSpeed);

        }
    }

    private bool CheckMyBarrier(Transform transform)
    {
        //自分以外は動かさない
        if (!IsOwner) return false;

        //自分のバリアは変えない
        if (NetworkManager.Singleton.LocalClientId == transform.root.GetComponent<NetworkObject>()?.OwnerClientId) return false;

        return true;
    }

    //スピード設定関数
    public void SetSpeedValue(float value)
    {
        normalSpeed = value;
    }

    //トリオンモード変更関数
    public void ChangeMode(Mode modeValue, int powerValue)
    {
        mode = modeValue;
        trionPower = powerValue;

        //ストレートの場合はスピードを変える
        if(modeValue == Mode.Straight)
        {
            SetSpeedValue(normalSpeed * (1f + (float)(trionPower - 5) / 6f));
        }

        if(modeValue != Mode.Equip)
        {
            SetTrionSpeed(Speed.NormalSpeed);
        }
    }


    //トリオンスピード変更関数
    public void SetTrionSpeed(Speed speed)
    {
        switch (speed)
        {
            case Speed.NormalSpeed:
                this.speed = normalSpeed;
                break;

            case Speed.SlowSpeed:
                this.speed = normalSpeed * speedRatio;
                break;

            case Speed.StopSpeed:
                this.speed = 0f;
                break;
        }
        slowDuration = 0.1f;
    }
}
