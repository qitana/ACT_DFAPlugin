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
      DutyStatus: '',
      Roulette: {
        Code: 0,
        Name: '',
      },
      Instance: {
        Code: 0,
        Name: '',
      },
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

      if(e.detail.DFAData.State == "MATCHED") {
        lastMatched = Date.now();
      }

      this.stats.DutyStatus = e.detail.DFAData.State;
      if (e.detail.DFAData.State == "IDLE" && Date.now() - lastMatched > 5000) {
        this.stats.Hide = true;
      } else {
        this.stats.Hide = false;
      }

      this.stats.Roulette.Code = e.detail.DFAData.RouletteCode;
      if (Number(e.detail.DFAData.RouletteCode) > 0) {
        this.stats.Roulette.Name = dic.roulettes[e.detail.DFAData.RouletteCode];
      } else {
        this.stats.Roulette.Name = '';
      }

      this.stats.Instance.Code = e.detail.DFAData.Code;
      if (Number(e.detail.DFAData.Code) > 0) {
        this.stats.Instance.Name = dic.instances[e.detail.DFAData.Code].name;
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

