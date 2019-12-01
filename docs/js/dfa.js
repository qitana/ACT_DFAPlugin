'use strict'

const getDungeonData = new Promise((resolve, reject) => {
  let xmlHttpRequest = new XMLHttpRequest();
  xmlHttpRequest.onloadend  = function () {
    if (this.readyState == 4 && this.status == 200) {
      if (this.response) {
        resolve(this.response)
      } else {
        reject();
      }
    }
    else{
      reject();
    }
  }
  xmlHttpRequest.open('GET', 'data/dungeon.json', true);
  xmlHttpRequest.responseType = 'json';
  xmlHttpRequest.send(null);
});


const getRouletteData = new Promise((resolve, reject) => {
  let xmlHttpRequest = new XMLHttpRequest();
  xmlHttpRequest.onloadend  = function () {
    if (this.readyState == 4 && this.status == 200) {
      if (this.response) {
        resolve(this.response)
      } else {
        reject();
      }
    }
    else{
      reject();
    }
  }
  xmlHttpRequest.open('GET', 'data/roulette.json', true);
  xmlHttpRequest.responseType = 'json';
  xmlHttpRequest.send(null);
});

const getPhoneticData = new Promise((resolve, reject) => {
  let xmlHttpRequest = new XMLHttpRequest();
  xmlHttpRequest.onloadend  = function () {
    if (this.readyState == 4 && this.status == 200) {
      if (this.response) {
        resolve(this.response)
      } else {
        reject();
      }
    }
    else{
      reject();
    }
  }
  xmlHttpRequest.open('GET', 'data/phonetic.json', true);
  xmlHttpRequest.responseType = 'json';
  xmlHttpRequest.send(null);
});


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
    expectedWaitTime: '推定残り待ち時間',
    partyStatus: "パーティ構築状況",
  },
};

var localeDungeons = {};
var localeRoulettes = {};
var localePhonetics = {};

Promise.all([getDungeonData, getRouletteData, getPhoneticData])
  .then(values => {
    localeDungeons = values[0]
    localeRoulettes = values[1]
    localePhonetics = values[2]
  }).then(() => {
    var dfa = new Vue({
      el: '#dfa',
      data: {
        updated: false,
        locked: true,
        collapsed: false,
        hide: false,
        status: {
          Hide: true,
        },
        strings: {},
        dungeons: {},
        roulettes: {},
        phonetics: {},
      },
      attached: function () {
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
          document.addEventListener('onOverlayStateUpdate', this.updateState);
          window.startOverlayEvents();
        });
      },
      detached: function () {
        window.removeOverlayListener('onDFAStatusUpdateEvent', this.update);
        document.removeEventListener('onOverlayStateUpdate', this.updateState);
      },
      methods: {
        update: function (updateMessage) {
          if (updateMessage.type && updateMessage.type == "onDFAStatusUpdateEvent") {
            let newStatus = JSON.parse(updateMessage.detail.statusJson)

            if (this.status.QueueStarted) {
              newStatus.QueueStarted = this.status.QueueStarted
            } else {
              newStatus.QueueStarted = null;
            }

            if (this.status.lastMatched) {
              newStatus.lastMatched = this.status.lastMatched
            } else {
              newStatus.lastMatched = new Date(2000, 0, 1, 0, 0, 0);
            }


            if (newStatus.MatchingStateString == "IDLE") {
            } else {
            }

            if (newStatus.MatchingStateString == "QUEUED") {
              if (this.status.MatchingStateString != "QUEUED") {
                newStatus.QueueStarted = new Date();
              }

              newStatus.ExpectedWaitTimeSeconds = ((newStatus.WaitTime * 60 * 1000) - (Date.now() - newStatus.QueueStarted)) / 1000;
              if(newStatus.ExpectedWaitTimeSeconds <= 0) {
                newStatus.ExpectedWaitTimeSeconds = 0;
              }
              newStatus.IsQueued = true;
            } else {
              newStatus.ExpectedWaitTimeSeconds = 0;
              newStatus.IsQueued = false;
            }

            if (newStatus.MatchingStateString == "MATCHED") {
              if (this.status.IsMatched == false) {
                console.log(newStatus.DungeonCode);
                let text = this.dungeons[newStatus.DungeonCode]
                for (var key in this.phonetics) {
                  text = text.replace(key, this.phonetics[key]);
                }
                console.log(text);
                window.callOverlayHandler({ call: 'DFATTS', text: text })
              }
              newStatus.IsMatched = true;
              newStatus.lastMatched = Date.now();
            } else {
              newStatus.IsMatched = false;
            }

            if (Number(newStatus.RouletteCode) > 0) {
              newStatus.IsRoulette = true;
            } else {
              newStatus.IsRoulette = false;
            }

            if (newStatus.StartQueueingTime) {
            }

            newStatus.Remains = (newStatus.TankMax - newStatus.Tank) + (newStatus.HealerMax - newStatus.Healer) + (newStatus.DpsMax - newStatus.Dps)

            if (newStatus.MatchingStateString == "IDLE" && Date.now() - newStatus.lastMatched < 10000) {
              // keep showing..
            } else if (newStatus.MatchingStateString == "IDLE") {
              newStatus.Hide = true;
              this.status = newStatus;
            } else {
              newStatus.Hide = false;
              this.status = newStatus;
            }
          }

          this.updated = true;
          if (this.hide)
            document.getElementById('dfa').style.visibility = 'hidden';
          else
            document.getElementById('dfa').style.visibility = 'visible';
        },
        updateState: function (e) {
          this.locked = e.detail.isLocked;
        },
        toggleCollapse: function () {
          this.collapsed = !this.collapsed;
        },
        toTimeString: function (time) {
          let totalSeconds = Math.floor(time);
          let minutes = Math.floor(totalSeconds / 60);
          let seconds = totalSeconds % 60;
          let str = '';
          if (minutes > 0)
            str = minutes + 'm';

          str += seconds + 's';
          return str;
        },
      },
    });
  })
  .catch(error=> {
    console.log(error);
  });

