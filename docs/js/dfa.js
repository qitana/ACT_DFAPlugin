'use strict'

// ダンジョンのリストを取得します
const getDungeonData = new Promise((resolve, reject) => {
  let xmlHttpRequest = new XMLHttpRequest();
  xmlHttpRequest.onloadend = function () {
    if (this.readyState == 4 && this.status == 200) {
      if (this.response) {
        resolve(this.response)
      } else {
        reject();
      }
    }
    else {
      reject();
    }
  }
  xmlHttpRequest.open('GET', 'data/dungeon.json', true);
  xmlHttpRequest.responseType = 'json';
  xmlHttpRequest.send(null);
});

// ルーレットのリストを取得します
const getRouletteData = new Promise((resolve, reject) => {
  let xmlHttpRequest = new XMLHttpRequest();
  xmlHttpRequest.onloadend = function () {
    if (this.readyState == 4 && this.status == 200) {
      if (this.response) {
        resolve(this.response)
      } else {
        reject();
      }
    }
    else {
      reject();
    }
  }
  xmlHttpRequest.open('GET', 'data/roulette.json', true);
  xmlHttpRequest.responseType = 'json';
  xmlHttpRequest.send(null);
});

// 読み(ふりがな)のリストを取得します
const getPhoneticData = new Promise((resolve, reject) => {
  let xmlHttpRequest = new XMLHttpRequest();
  xmlHttpRequest.onloadend = function () {
    if (this.readyState == 4 && this.status == 200) {
      if (this.response) {
        resolve(this.response)
      } else {
        reject();
      }
    }
    else {
      reject();
    }
  }
  xmlHttpRequest.open('GET', 'data/phonetic.json', true);
  xmlHttpRequest.responseType = 'json';
  xmlHttpRequest.send(null);
});


// UIの各言語表記
let localeStrings = {
  'English': {
    status: 'Status',
    minutes: 'mins',
    seconds: 'secs',
    roulette: 'Roulette',
    dungeon: 'Dungeon',
    remainings: 'Remainings',
    numberOfPeople: 'people',
    numberOfPeopleWaiting: 'people waiting',
    avgWaitTime: 'Avg. Wait time',
    expectedWaitTime: 'EWT',
    partyStatus: "Party Status",
  },
  'Japanese': {
    status: 'ステータス',
    minutes: '分',
    seconds: '秒',
    roulette: 'ルーレット',
    dungeon: 'ダンジョン',
    remainings: '残り',
    numberOfPeople: '人',
    numberOfPeopleWaiting: '人待ち',
    avgWaitTime: '平均待機時間',
    expectedWaitTime: '推定待ち時間',
    partyStatus: "構築状況",
  },
};

var localeDungeons = {};
var localeRoulettes = {};
var localePhonetics = {};

// Main

// Vue 初期化
var dfa = new Vue({
  el: '#vue',
  data: {
    updated: false,
    locked: true,
    collapsed: false,
    status: {
      Hide: true,
    },
    strings: {},
    dungeons: {},
    roulettes: {},
    phonetics: {},
  },
  mounted: function () {
    this.$nextTick(function () {
      document.addEventListener('onOverlayStateUpdate', this.updateState);

      // 必要なリソースを取得
      Promise.all([getDungeonData, getRouletteData, getPhoneticData])
        .then(values => {
          localeDungeons = values[0]
          localeRoulettes = values[1]
          localePhonetics = values[2]
          // getLanguage で言語を取得して設定
          window.callOverlayHandler({ call: 'getLanguage' }).then((msg) => {
            if (msg.language in localeStrings) {
              this.strings = localeStrings[msg.language];
              this.dungeons = localeDungeons[msg.language];
              this.roulettes = localeRoulettes[msg.language];
              this.phonetics = localePhonetics[msg.language];
            }
            else {
              this.strings = localeStrings['English'];
              this.dungeons = localeDungeons['English'];
              this.roulettes = localeRoulettes['English'];
              this.phonetics = localePhonetics['English'];
            }

            window.addOverlayListener('onDFAStatusUpdateEvent', this.update);
            window.startOverlayEvents();
          });
        })
        .catch(error => {
          console.log(error);
        });
    });
  },
  destroyed: function () {
    this.$nextTick(function () {
      // EventListener を停止
      window.removeOverlayListener('onDFAStatusUpdateEvent', this.update);
      document.removeEventListener('onOverlayStateUpdate', this.updateState);
    });
  },
  methods: {
    // DFA Status のアップデート
    update: function (updateMessage) {
      if (updateMessage.type && updateMessage.type == "onDFAStatusUpdateEvent") {
        let newStatus = JSON.parse(updateMessage.detail.statusJson)

        // init QueueStarted
        if (this.status.QueueStarted) {
          newStatus.QueueStarted = this.status.QueueStarted
        } else {
          newStatus.QueueStarted = null;
        }

        // init lastMatched
        if (this.status.lastMatched) {
          newStatus.lastMatched = this.status.lastMatched
        } else {
          newStatus.lastMatched = new Date(2000, 0, 1, 0, 0, 0);
        }

        // Idle
        if (newStatus.MatchingStateString == "IDLE") {
          newStatus.IsIdle = true;
        } else {
          newStatus.IsIdle = false;
        }

        // Queued
        if (newStatus.MatchingStateString == "QUEUED") {
          if (this.status.MatchingStateString == "IDLE") {
            newStatus.QueueStarted = new Date();
          }
          // Calc EWT
          newStatus.ExpectedWaitTimeSeconds = ((newStatus.WaitTime * 60 * 1000) - (Date.now() - newStatus.QueueStarted)) / 1000;
          if (newStatus.ExpectedWaitTimeSeconds <= 0) {
            newStatus.ExpectedWaitTimeSeconds = 0;
          }
          newStatus.IsQueued = true;
        } else {
          newStatus.ExpectedWaitTimeSeconds = 0;
          newStatus.IsQueued = false;
        }

        // Matched
        if (newStatus.MatchingStateString == "MATCHED") {
          if (this.status.IsMatched == false) {
            let text = this.dungeons[newStatus.DungeonCode]
            for (var key in this.phonetics) {
              text = text.replace(key, this.phonetics[key]);
            }
            window.callOverlayHandler({ call: 'DFATTS', text: text })
          }
          newStatus.IsMatched = true;
          newStatus.lastMatched = Date.now();
        } else {
          newStatus.IsMatched = false;
        }

        // Roulette
        if (Number(newStatus.RouletteCode) > 0) {
          newStatus.IsRoulette = true;
        } else {
          newStatus.IsRoulette = false;
        }

        // 残りの人数計算
        newStatus.Remains = (newStatus.TankMax - newStatus.Tank) + (newStatus.HealerMax - newStatus.Healer) + (newStatus.DpsMax - newStatus.Dps)

        if (newStatus.MatchingStateString == "IDLE" && Date.now() - newStatus.lastMatched < 12000) {
          // keep last status
        } else if (newStatus.MatchingStateString == "IDLE") {
          newStatus.Hide = true;
          this.status = newStatus;
        } else {
          newStatus.Hide = false;
          this.status = newStatus;
        }
      }

      this.updated = true;
    },
    // Overlayの設定アップデート
    updateState: function (e) {
      this.locked = e.detail.isLocked;
    },
  },
});

