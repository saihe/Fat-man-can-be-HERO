﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace GameSystems{

   enum GameState{
        Playing,
        NotPlaying,
        Pausing,
        StageClear,
        GameOver
    };

    enum StageName
    {
        Stage1,
        Stage2,
        Stage3
    };


    //ステート・ステージ確認用
    public class DebugSystem: MonoBehaviour
    {
        public void OnGUI()
        {
            if (Input.GetKeyDown("i"))
            {
                print("S: NowState/ C: CurrentStage/ D: ClearData");
            }
            if (Input.GetKeyDown("s"))
            {
                State state = new State();
                print(state.getState());
            }
            if (Input.GetKeyDown("c"))
            {
                ScenChanger c = new ScenChanger();
                print(c.getStageName());
            }
            if (Input.GetKeyDown("d"))
            {
                ClearedStage d = new ClearedStage();
                foreach(var val in d.getClearedStages())
                {
                    print(val);
                }
            }
        }
    }

    //ゲームステート
    class State
    {
        private static GameState NowState;

        public GameState getState()
        {
            return NowState;
        }

        public void setState(GameState e)
        {
            NowState = e;
        }

    }

    //シーンチェンジャー
    class ScenChanger
    {
        State state = new State();

        ClearedStage cs = new ClearedStage();

        //選択ステージ
        private static StageName currentStage;

        //取得用
        public StageName getStageName()
        {
            return currentStage;
        }
        //セット用
        public void setStage(StageName e)
        {
            currentStage = e;
        }

        //タイトル画面へ
        public void toTitle()
        {
            state.setState(GameState.NotPlaying);
            Application.LoadLevel("Title");
        }

        //ステージセレクト画面へ
        public void toStageSelect()
        {
            state.setState(GameState.NotPlaying);
            Application.LoadLevel("StageSelect");
        }

        //LoadSceneへ
        public void toLoading()
        {
            Application.LoadLevel("LoadScene");
        }

        //リザルトへ
        public void toResult()
        {
            if(currentStage == StageName.Stage3 && state.getState() == GameState.StageClear)
            {
                Application.LoadLevel("GameClear");
            }
            else
            {
                Application.LoadLevel("Result");
            }
        }
    };

    //ステージクリア情報
    class ClearedStage
    {
        //保存したクリア情報を取得しゲーム内に保存しておく配列
        private static Dictionary<StageName, int> clearedStages = new Dictionary<StageName, int>();

        //初期化
        public void clearData()
        {
            //端末にデフォルトの値を保存
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt(StageName.Stage1.ToString(), 0);
            PlayerPrefs.SetInt(StageName.Stage2.ToString(), 0);
            PlayerPrefs.SetInt(StageName.Stage3.ToString(), 0);
        }

        public void defaultData()
        {
            if (PlayerPrefs.HasKey(StageName.Stage1.ToString()) == false)
            {
                //端末にデフォルトの値を保存
                PlayerPrefs.SetInt(StageName.Stage1.ToString(), 0);
                PlayerPrefs.SetInt(StageName.Stage2.ToString(), 0);
                PlayerPrefs.SetInt(StageName.Stage3.ToString(), 0);
                clearedStages[StageName.Stage1] = PlayerPrefs.GetInt(StageName.Stage1.ToString(), 0);
                clearedStages[StageName.Stage2] = PlayerPrefs.GetInt(StageName.Stage2.ToString(), 0);
                clearedStages[StageName.Stage3] = PlayerPrefs.GetInt(StageName.Stage3.ToString(), 0);
                //Debug.Log("端末に保存した");
            }
            else
            {
                //Debug.Log("すでに端末に保存されてる");
            }
        }

        //端末にクリア情報を保存する
        public void setCleared(StageName s, int i)
        {
            //i == 1 でクリア
            PlayerPrefs.SetInt(s.ToString(), i);
        }

        //端末からクリア情報を取得する
        public void getCleared()
        {
            if (PlayerPrefs.HasKey(StageName.Stage1.ToString()) == true)
            {
                //端末に保存された値を取得
                PlayerPrefs.GetInt(StageName.Stage1.ToString(), 0);
                PlayerPrefs.GetInt(StageName.Stage2.ToString(), 0);
                PlayerPrefs.GetInt(StageName.Stage3.ToString(), 0);
                //保存した情報をコレクションに格納
                clearedStages[StageName.Stage1] = PlayerPrefs.GetInt(StageName.Stage1.ToString(), 0);
                clearedStages[StageName.Stage2] = PlayerPrefs.GetInt(StageName.Stage2.ToString(), 0);
                clearedStages[StageName.Stage3] = PlayerPrefs.GetInt(StageName.Stage3.ToString(), 0);
            }
            else
            {
                //Debug.Log("まだ端末に保存されてない");
            }
        }

        //コレクション取得用
        public Dictionary<StageName, int> getClearedStages()
        {
            return clearedStages;
        }
    };
};