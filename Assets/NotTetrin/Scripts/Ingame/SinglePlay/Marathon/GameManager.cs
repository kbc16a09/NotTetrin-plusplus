﻿using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NotTetrin.Constants;
using NotTetrin.SceneManagement;
using NotTetrin.Ingame.Marathon;
using NotTetrin.Utility.Physics2D;

namespace NotTetrin.Ingame.SinglePlay.Marathon {
    public class GameManager : SceneBase {
        [SerializeField] private Director director;
        [SerializeField] private BGMManager bgmManager;
        [SerializeField] private IngameSfxManager sfxManager;
        [SerializeField] private MinoManager minoManager;
        [SerializeField] private Score score;
        [SerializeField] private HighScore highScore;
        [SerializeField] private Ranking ranking;
        [SerializeField] private GroupManager groupManager;
        [SerializeField] private LevelManager levelManager;

        protected override void OnSceneReady(object sender, EventArgs args) {
            base.OnSceneReady(sender, args);
            minoManager.HitMino += onHitMino;
            loadRanking();
            gamestart();
        }

        protected override void Awake() {
            base.Awake();
            var creator = director.CollidersField.GetComponent<TileCreator>();
            creator.Create();
        }

        private void Update() {
            if (Input.GetButtonDown(@"Escape")) {
                SceneController.Instance.LoadScene(SceneName.Title, 0.7f);
            }
        }

        private void reset() {
            CancelInvoke("gamestart");
            sfxManager.Stop(IngameSfxType.GameOver);
            score.Reset();
            minoManager.Reset();
            levelManager.Reset();
        }

        private void gamestart() {
            reset();
            director.Floor.SetActive(true);
            bgmManager.RandomPlay();
            sfxManager.Play(IngameSfxType.GameStart);
            minoManager.Next();
        }

        private void gameover() {
            director.Floor.SetActive(false);
            bgmManager.Stop();
            sfxManager.Play(IngameSfxType.GameOver);
            
            // TODO: 本番は常にセーブ
            var updated = highScore.UpdateValue();
            if (updated) {
                saveRanking();
            }
            Invoke("gamestart", 9.0f);
        }

        private void loadRanking() {
            ranking.Fetch(RankingType.MarathonMode);
        }

        private void saveRanking() {
            var name = PlayerPrefs.GetString(PlayerPrefsKey.PlayerName);
            var score = highScore.Value;
            var ranker = new Ranker(name, score);
            ranking.Save(RankingType.MarathonMode, ranker);
        }

        private void onHitMino(object sender, EventArgs args) {
            // 天井に当たったらゲームオーバー
            if (director.Ceiling.IsHit) {
                minoManager.Release();
                gameover();
            } else {
                score.Increase(200 + (50 * levelManager.getLevel()));
                groupManager.DeleteMino();
                minoManager.Next();
            }
        }
    }
}