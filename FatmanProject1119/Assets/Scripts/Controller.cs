﻿using UnityEngine;
using System.Collections;
using GameSystems;
using System.Collections.Generic;
using System.Linq;

public class Controller : MonoBehaviour {

    //移動判定かどうか
    private bool moveOk;
	
	//タップ判定かどうか
	private bool tapOk = false;
	
	//フリック用フリックなのか判定
	private bool flickOk = false;

	//プレイヤーの移動が逆になってしまった時用
	public bool reverse = false;
	
	//プレイヤーの移動スピード調整用変数
	public float speed = 1;
	
	//タッチされた座標
	private Vector2 touch;
	
	//移動先のワールド座標
	private Vector3 cm;
	private Vector3 moveTo;
	
	//フリック判定用タッチ判定時間
	private double touchJdg = 0.15;
	
	//フリック判定用タッチ判定移動量
	private double flickJdg = 30;
	
	//タッチ後移動した座標
	private Vector2 dragPoint;
	
	//フリック用タッチしている時間
	private double touchTime;
	
	//タッチした位置と移動した位置の差分ベクトル
	private Vector3 direction;
	
	//directionに入れる座標
	private double x;
	private double y;
	private double z;
	
	//回転速度
	private float rotationSpeed = 10000.0f;
	
    //Buttonコンポーネント
	Buttons button;

    //アニメーション
    Animator anim;

    //Stateクラス
    State state = new State();

    //更生力
    private float attack;

    //アクションカウント
    private int tapCount = 0;

    //オーディオソース
    private string[] audioList = new string[3] {"punch-swing", "jabpunch", "itemget"}; 
    private AudioClip[] audioSorce = new AudioClip[3];
    private AudioSource audio;

    //波動
    private GameObject hado;

    //BMIManager
    private BMIManager bmiManager;

    //攻撃判定オンオフ用コライダー
    SphereCollider jab;
    SphereCollider smash;

    //アニメーターステート保存用
    int[] stateHash = new int[4];


    //BMI外用
    public float bmi = 200f;

    //探知したエネミー格納用
    private Dictionary<GameObject, float> list = new Dictionary<GameObject, float>();
    //探知したエネミー
    Transform target;

	//ヒットエフェクト
	Hitefect hitefect;


    void Start () {
        //デバッグ用
        //bmi = 10f;

        moveOk = false;

        //BMIManager
        bmiManager = GetComponent<BMIManager>();
		//攻撃判定オフ
		button = GameObject.Find("Screen").GetComponent<Buttons>();

        //モーションをいじるため
        anim = GetComponent<Animator>();

        //オーディオソースコンポーネント
        audio = GetComponent<AudioSource>();
        //オーディオクリップをリソースフォルダから取得
        for(int i = 0; i < audioSorce.Length; i++)
        {
            audioSorce[i] = (AudioClip)Resources.Load("SEfects/" + audioList[i]);
        }

        //波動非表示
        hado = transform.GetChild(1).gameObject;
        hado.SetActive(false);

        //攻撃判定用
        jab = GameObject.FindWithTag("Jab").GetComponent<SphereCollider>();
        smash = GameObject.FindWithTag("Smash").GetComponent<SphereCollider>();
        jab.enabled = false;
        smash.enabled = false;

        //アニメーターステート
        //Stanby
        stateHash[0] = 17588480;
        //Jab1
        stateHash[1] = 1284765916;
        //Jab2
        stateHash[2] = -711284378;
        //Smash
        stateHash[3] = -355276023;

		hitefect = GetComponent<Hitefect>();
    }

    void Update () {
        if (state.getState() == GameState.Playing)
		{
            move();
		}

        //デバッグ用
        if (Input.GetKeyDown("p"))
        {
            bm = true;
            StartCoroutine(bmi200());
        }
        if (Input.GetKeyDown("o"))
        {
            bm = false;
        }
        if (Input.GetKeyDown("d"))
        {
            bmi = 0;
        }
	}

    //デバッグ用BMI回復
    bool bm = false;
    IEnumerator bmi200()
    {
        while(bm == true)
        {
            bmi = 200;
            yield return new WaitForSeconds(0.5f);
        }
    }

	//コントローラー状態
	public bool getFlick()
	{
		return flickOk;
	}
	
	//コントローラー本体
	public void move()
	{
		//タッチされた瞬間のみ
		if (Input.GetMouseButtonDown(0) && state.getState() == GameState.Playing)
		{
			//タッチされた座標を取得
			touch = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			touchTime = 0;
			//タッチされるたびにフリック判定を初期化
			flickOk = false;
			tapOk = false;
			moveOk = false;
		}
		if (button.getPushButton() == false && state.getState() == GameState.Playing)
		{
			//タッチされている間
			if (Input.GetMouseButton(0) && state.getState() == GameState.Playing)
			{
				//タップ判定
				tapOk = false;
				
				//タッチ後移動した座標
				dragPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

				//プレイヤーが移動するベクトル
				x = dragPoint.x - touch.x;
				y = 0;
				z = dragPoint.y - touch.y;
				
				//タッチされてる時間を計測
				touchTime += Time.deltaTime;
				
				//入力をVector3に変換/移動量を制限
				direction = new Vector3((float)x, (float)y, (float)z) / 1000;
				
				//フリック判定用
				Vector3 pointA = new Vector3(touch.x, 0, touch.y);
				Vector3 pointB = new Vector3(Mathf.Clamp(dragPoint.x, touch.x - 60, touch.x + 60), 0, Mathf.Clamp(dragPoint.y, touch.y - 60, touch.y + 60));
				//二点間の距離(float)
				float flickVector = Vector3.Distance(pointA, pointB);
				
				//フリックスピード
				double flickSpeed = flickVector / touchTime;
				
				//フリックスピードが800以上あればフリック
				if (flickSpeed > 800 && state.getState() == GameState.Playing)
				{
					//フリックであると判定する
					flickOk = true;
				}
				
				//タッチ位置と移動位置が同じなら移動
				else if (dragPoint != touch)
				{
					//移動判定オン
					moveOk = true;
                    //移動モーション
                    anim.SetBool("Move", true);
                    anim.SetTrigger("Move");

                    //入力ベクトルをQuaternionに変換
                    Quaternion to = Quaternion.LookRotation(direction);
					
					//キャラクターを向かせる
					transform.rotation = Quaternion.RotateTowards(transform.rotation, to, rotationSpeed * Time.deltaTime);

                    //反転用
                    if (reverse == true)
					{
						direction = new Vector3(-direction.x, 0, -direction.z);
					}

					//移動
					transform.Translate(direction.normalized * 0.1f * speed, Space.World);
				}
				//移動でもフリックでもなければ
				else if (touchTime < touchJdg)
				{
					flickOk = false;
					moveOk = false;
					tapOk = true;
				}
			}
            if (Input.GetMouseButtonUp(0))
            {
                anim.SetBool("Move", false);
                moveOk = false;
            }
        }

        //フリックアクション
        if (flickOk == true && state.getState() == GameState.Playing)
		{
			if (Input.GetMouseButtonUp(0) && state.getState() == GameState.Playing)
			{
				//print("Flick");
                anim.SetTrigger("Flick");
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
				
				//反転用
				if (reverse == true)
				{
					direction = new Vector3(-direction.x, 0, direction.z);
				}
				transform.Translate(direction * 50, Space.World);
                StartCoroutine(flick());
				flickOk = false;
			}
		}

        //タップアクション
		if(tapOk == true && state.getState() == GameState.Playing)
		{
			if (Input.GetMouseButtonUp(0) && state.getState() == GameState.Playing)
			{
				//print("Tap");
                tapCount++;
                anim.SetBool("Move", false);
                anim.SetTrigger("Attack");
                StartCoroutine(Hado());
                StartCoroutine(punch());
                tapOk = false;
            }
        }
    }

    //タップアクション
    IEnumerator punch()
    {
        //print(anim.GetCurrentAnimatorStateInfo(0).fullPathHash);
        if (stateHash[1] == anim.GetCurrentAnimatorStateInfo(0).fullPathHash || stateHash[2] == anim.GetCurrentAnimatorStateInfo(0).fullPathHash)
        {
            //print("Jab");
            jab.enabled = true;
            hado.tag = "Jab";
            transform.Translate(transform.forward / 10);
            yield return new WaitForSeconds(0.5f);
            jab.enabled = false;
            yield break;
        }
        else if(stateHash[3] == anim.GetCurrentAnimatorStateInfo(0).fullPathHash)
        {
            //print("Smash");
            smash.enabled = true;
            hado.tag = "Smash";
            transform.Translate(transform.forward / 10);
            tapCount = 0;
            yield return new WaitForSeconds(0.5f);
            smash.enabled = false;
            yield break;
        }
    }

    //タップ時波動エフェクトを出す
    IEnumerator Hado()
    {
        hado.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        hado.SetActive(false);
        yield break;
    }

    //フリックアクション
    IEnumerator flick()
    {
        yield break;
    }

    /*
    探知機: Sphere Collider, Center(0, 1, 0), Radius(3), Is Trigger(On)
    探知機に当たった物体を格納するコレクション
    Key  : 接触GameObject 
    Value: プレイヤーとの距離
    */
    void OnTriggerStay(Collider c)
    {
        float min = 10f;
        //print("OnTri: " + c);
        //Enemyタグがついたオブジェクトのみコレクションに格納
        if (c.tag == "Enemy" || c.tag == "Boss")
        {
            if (list.ContainsKey(c.gameObject) == false)
            {
                //コレクションに存在しない場合追加
                list.Add(c.gameObject, Vector3.Distance(transform.position, c.transform.position));
            }
            else
            {
                //既にコレクションに存在したらValueを更新
                list[c.gameObject] = Vector3.Distance(transform.position, c.transform.position);
            }

            //コレクションの中で最も近いGameObjectに向く
            foreach(var val in list)
            {
                min = val.Value;
                //プレイヤーに近い方に向く
                if (min >= val.Value)
                {
                    target = val.Key.gameObject.transform;
                    transform.LookAt(target);
                }
            }
        }

        //敵の攻撃にあったたら
        if (c.gameObject.tag == "Bullet")
        {
            print("HIt to Player: Bullet");
            bmi -= 5f;
			hitefect.EnemyattackHit();
            c.gameObject.SetActive(false);
        }
    }

    //タッチパッド向けスライド取得用
    public bool getMoveOk()
    {
        return moveOk;
    }

    //ターゲット取得用
    public Transform getTarget()
    {
        return target;
    }

    //ターゲットをコレクションから削除する
    public void removeList(GameObject g)
    {
        list.Remove(g);
    }

    //離れたらコレクションから削除
    void OnTriggerExit(Collider c)
    {
        if (list.ContainsKey(c.gameObject))
        {
            list.Remove(c.gameObject);
        }
    }
    //BMI取得
    public float getBMI()
    {
        return bmi;
    }
    //BMIセット
    public void setBMI(float f)
    {
        bmi = f;
    }
    //BMIに足す
    public void incBMI(float f)
    {
        bmi += f;
    }

    //更生力 外用
    private float jabAtk = 1f;
    private float smashAtk = 3f;
    //取得
    public float getJabAtk()
    {
        return jabAtk;
    }
    public float getSmashAtk()
    {
        return smashAtk;
    }
    //セット
    public void setJabAtk(float f)
    {
        jabAtk = f;
    }
    public void setSmashAtk(float f)
    {
        smashAtk = f;
    }
}
