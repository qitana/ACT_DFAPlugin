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
    llsWaitTime: 'EWT by DFA',
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
    avgWaitTime: '待機時間',
    expectedWaitTime: '予測待ち時間',
    partyStatus: "構築状況",
    llsWaitTime: 'DFAによる予測',
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
    waitQueueData: [],
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
            this.waitQueueData = []
          }
          // Calc EWT
          if (newStatus.WaitTime > 0) {
            newStatus.ExpectedWaitTimeSeconds = ((newStatus.WaitTime * 60 * 1000) - (Date.now() - newStatus.QueueStarted)) / 1000;
            if (newStatus.ExpectedWaitTimeSeconds <= 0) {
              newStatus.ExpectedWaitTimeSeconds = 0;
            }
          } else {
            newStatus.ExpectedWaitTimeSeconds = 30 * 60;
          }
          // calc LLS
          if (newStatus.WaitList > 0 && newStatus.WaitList != this.status.WaitList) {
            let qData = [Date.now(), newStatus.WaitList]
            this.waitQueueData.push(qData);
          }
          if (this.waitQueueData.length > 1) {
            var lls = leastSquare(this.waitQueueData);
            newStatus.LLSWaitTimeSeconds = ((-1 * (lls.b / lls.a)) - Date.now()) / 1000;
            if (newStatus.LLSWaitTimeSeconds <= 0) {
              newStatus.LLSWaitTimeSeconds = 0.0001;
            }
          } else {
            newStatus.LLSWaitTimeSeconds = NaN
          }

          newStatus.IsQueued = true;
        } else {
          newStatus.ExpectedWaitTimeSeconds = 0;
          newStatus.LLSWaitTimeSeconds = NaN
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

        // 互換用
        if (!newStatus.PartyStateString) {
          newStatus.PartyStateString = "NORMAL"
          if (newStatus.TankMax > 24 || newStatus.HealerMax > 24 || newStatus.DpsMax > 24 ||
            newStatus.Tank > 24 || newStatus.Healer > 24 || newStatus.Dps > 24) {
            newStatus.TankMax = 0;
            newStatus.HealerMax = 0;
            newStatus.DpsMax = 0;
            newStatus.Tank = 0;
            newStatus.Healer = 0;
            newStatus.Dps = 0;
          }
        }


        if (newStatus.IsMatched) {
          if (newStatus.PartyStateString == "NORMAL") {
            newStatus.Remains = (newStatus.TankMax - newStatus.Tank) + (newStatus.HealerMax - newStatus.Healer) + (newStatus.DpsMax - newStatus.Dps)
          } else if (newStatus.PartyStateString == "ROLEFREE") {
            newStatus.Remains = newStatus.NonRoleMax - newStatus.NonRole;
          } else {
            newStatus.Remains = "??"
          }
        } else {
          newStatus.Remains = (newStatus.TankMax - newStatus.Tank) + (newStatus.HealerMax - newStatus.Healer) + (newStatus.DpsMax - newStatus.Dps)
        }

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


function sum(data, func) {
  var val = 0;
  for (var i = 0; i < data.length; i++) {
    val += func(data[i]);
  }
  return val;
}

function leastSquare(data) {
  var N = data.length;
  var sumX = sum(data, function (item) {
    return item[0];
  });
  var sumX2 = sum(data, function (item) {
    return item[0] * item[0];
  });
  var sumY = sum(data, function (item) {
    return item[1];
  });
  var sumXY = sum(data, function (item) {
    return item[0] * item[1];
  });

  var a, b;
  var denominator = (N * sumX2) - Math.pow(sumX, 2);
  var molecule1 = (N * sumXY) - (sumX * sumY);
  a = molecule1 / denominator;
  var molecule2 = (sumX2 * sumY) - (sumXY * sumX);
  b = molecule2 / denominator;

  return {
    a: a,
    b: b
  };
}
