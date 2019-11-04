var lastMatched = 0;
// データ処理
var DFAStatus = new Vue({
  el: '#DFAStatus',
  data: {
    updated: false,
    locked: false,
    collapsed: false,
    DFAData: {},
    stats: {
      Hide: true,
      IsQueued: true,
      IsMatched: true,
      IsRoulette: true,
      DutyStatus: '',
      Roulette: {
        Code: 0,
        Name: '',
      },
      Instance: {
        Code: 0,
        Name: '',
      },
      Wait: {
        List: 0,
        Time: 0,
      },
      QueuedParty: {
        Tank: 0,
        TankMax: 0,
        Healer: 0,
        HealerMax: 0,
        Dps: 0,
        DpsMax: 0,
        Remains: 0,
      },
      MatchedParty: {
        Tank: 0,
        TankMax: 0,
        Healer: 0,
        HealerMax: 0,
        Dps: 0,
        DpsMax: 0,
        Remains: 0,
      }

    },
  },
  attached: function () {
    document.addEventListener('onOverlayDataUpdate', this.update);
    document.addEventListener('onOverlayStateUpdate', this.updateState);
  },
  detached: function () {
    document.removeEventListener('onOverlayStateUpdate', this.updateState);
    document.removeEventListener('onOverlayDataUpdate', this.update);
  },
  methods: {
    update: function (e) {
      this.updated = true;
      this.DFAData = e.detail.DFAData;

      if (e.detail.DFAData.State == "MATCHED") {
        this.stats.IsMatched = true;
        lastMatched = Date.now();
      } else {
        this.stats.IsMatched = false;
      }

      if (e.detail.DFAData.State == "QUEUED") {
        this.stats.IsQueued = true;
      } else {
        this.stats.IsQueued = false;
      }

      this.stats.Wait.List = e.detail.DFAData.WaitList;
      this.stats.Wait.Time = e.detail.DFAData.WaitTime;

      this.stats.QueuedParty.Tank = e.detail.DFAData.QueuedPartyStatus.Tank;
      this.stats.QueuedParty.Healer = e.detail.DFAData.QueuedPartyStatus.Healer;
      this.stats.QueuedParty.Dps = e.detail.DFAData.QueuedPartyStatus.Dps;
      this.stats.QueuedParty.TankMax = e.detail.DFAData.QueuedPartyStatus.TankMax;
      this.stats.QueuedParty.HealerMax = e.detail.DFAData.QueuedPartyStatus.HealerMax;
      this.stats.QueuedParty.DpsMax = e.detail.DFAData.QueuedPartyStatus.DpsMax;
      this.stats.QueuedParty.Remains = (this.stats.QueuedParty.TankMax + this.stats.QueuedParty.HealerMax + this.stats.QueuedParty.DpsMax) - (this.stats.QueuedParty.Tank + this.stats.QueuedParty.Healer + this.stats.QueuedParty.Dps)
      if (this.stats.QueuedParty.Remains < 0) this.stats.QueuedParty.Remains = 0;

      this.stats.MatchedParty.Tank = e.detail.DFAData.MatchedPartyStatus.Tank;
      this.stats.MatchedParty.Healer = e.detail.DFAData.MatchedPartyStatus.Healer;
      this.stats.MatchedParty.Dps = e.detail.DFAData.MatchedPartyStatus.Dps;
      this.stats.MatchedParty.TankMax = e.detail.DFAData.MatchedPartyStatus.TankMax;
      this.stats.MatchedParty.HealerMax = e.detail.DFAData.MatchedPartyStatus.HealerMax;
      this.stats.MatchedParty.DpsMax = e.detail.DFAData.MatchedPartyStatus.DpsMax;
      this.stats.MatchedParty.Remains = (this.stats.MatchedParty.TankMax + this.stats.MatchedParty.HealerMax + this.stats.MatchedParty.DpsMax) - (this.stats.MatchedParty.Tank + this.stats.MatchedParty.Healer + this.stats.MatchedParty.Dps)
      if (this.stats.MatchedParty.Remains < 0) this.stats.MatchedParty.Remains = 0;


      this.stats.DutyStatus = e.detail.DFAData.State;
      if (e.detail.DFAData.State == "IDLE" && Date.now() - lastMatched > 10000) {
        this.stats.Hide = true;
      } else if (e.detail.DFAData.State == "MATCHED" && Date.now() - lastMatched > 65000) {
        this.stats.Hide = true;
      } else {
        this.stats.Hide = false;
      }

      this.stats.Roulette.Code = e.detail.DFAData.RouletteCode;
      if (Number(e.detail.DFAData.RouletteCode) > 0) {
        this.stats.IsRoulette = true;
        this.stats.Roulette.Name = dic.roulettes[e.detail.DFAData.RouletteCode];
        if (!this.stats.Roulette.Name) {
          this.stats.Roulette.Name = 'Unknown Roulette (' + e.detail.DFAData.RouletteCode + ')';
        }
      } else {
        this.stats.IsRoulette = false;
        this.stats.Roulette.Name = '';
      }

      this.stats.Instance.Code = e.detail.DFAData.Code;
      if (Number(e.detail.DFAData.Code) > 0) {
        this.stats.Instance.Name = dic.instances[e.detail.DFAData.Code].name;
        if (!this.stats.Instance.Name) {
          this.stats.Instance.Name = 'Unknown Instance (' + e.detail.DFAData.Code + ')';
        }
      } else {
        this.stats.Instance.Name = '';
      }
    },
    updateState: function (e) {
      this.locked = e.detail.isLocked;
    },
    toggleCollapse: function () {
      this.collapsed = !this.collapsed;
    }
  }
});

