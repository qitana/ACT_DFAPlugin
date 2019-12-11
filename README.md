# ACT_DFAPlugin

DutyFinder(コンテンツファインダー) のステータスをオーバーレイ表示する、[ngld/OverlayPlugin](https://github.com/ngld/OverlayPlugin) のアドオンです。  
マッチングのコードは[easly1989/ffxiv_act_dfassist](https://github.com/easly1989/ffxiv_act_dfassist)
で使用されているコードを参考にしています。

## 注意事項
**v2.x からは ngld/OverlayPlugin でないと動きません！hibiyasleep/OverlayPlugin では動きません！  
また、ngld/OverlayPlugin と hibiyasleep/OverlayPlugin は併用できません！**

- kagerou等の今までのオーバーレイは ngld/OverlayPlugin でも動作します。
- hibiyasleep/OverlayPlugin のアドオンは ngld/OverlayPlugin では動きません。

## できること
- ルーレットでシャキった時点で、どのコンテンツにマッチしたのか分かります。
- シャキった際にコンテンツ名をTTSで通知できます。
- ネットワークデータで待ち人数/更新時間が更新(1分間隔)され次第、表示が更新されます。    
  ゲームクライアントでは、データ到着から30秒後に1個前のデータとの中間値を表示しています。  
  (おそらく変化を緩やかにするためだと思われます。)
- 待ちが50番以降でも、実際の待ち人数が表示されます。
- あと何分でシャキりそうか、残り時間が表示されます。(開始時間と待ち時間から計算しています。)
- シャキった後、突入を押す前でもロール毎のパーティ編成状況が分かります。

## 特徴
- 実体は ngld/OverlayPlugin の EventSource プラグインです。  
  これを導入することで、オーバーレイで DutyFinder (コンテンツファインダー) のデータが扱えるようになります。
- [FFXIV_ACT_Pugin](https://github.com/ravahn/FFXIV_ACT_Plugin) が取得したネットワークデータを使用して処理しています。  
- パッチ毎に変更されるOpcodeに対応するため、Opcodeの設定を外部ファイルにしているので、  
  大きな変更がされない限りは、プラグインをアップグレードせずに使用可能です。  
  最新のOpcode設定ファイルが更新されれば、起動時に自動で読み込まれます。
- Opcode を確認するための Trace 機能があります。
- 主に日本語の難読なダンジョン名称にTTSが対応できないことが多いため、  
  TTS補助用の読み仮名変換を外部ファイルで設定しています。  
  このデータも、起動時に自動で最新のファイルが読み込まれます。  
  おかしな読みが発声した場合は PR/Issue で教えて頂けると助かります。


## インストール方法
1. [ngld/OverlayPlugin](https://github.com/ngld/OverlayPlugin) をACTのプラグインとして導入します。
2. Release から DFAPlugin をダウンロードし、任意のフォルダに展開します。  
   他のプラグインと同じフォルダに入れないよう、別フォルダを作って配置することをおすすめします。
3. DFAPlugin を ACTのプラグインとして導入します。
4. DFAPlugin を OverlayPlugin に認識させるため、一度ACTを再起動します。
5. OverlayPlugin の設定画面に DFA EventSource が増えていることを確認します。
6. OverlayPlugin で オーバーレイを追加します。種類は `MiniParse` を選びます。
7. 設定画面でURLを `https://qitana.github.io/ACT_DFAPlugin/dfa.html` に設定します。
8. 設置位置や大きさを調整し、位置が決まったらリサイズ/変更が出来ないようにロックします。  
   オーバーレイは見えなくなりますが、コンテンツファインダーを使用した際だけ表示されるようになります。

## FAQ

### 日本語環境なのに表示やTTSが英語になる
ACT_FFXIV_Plugin の言語設定が日本語になってるか確認して下さい。

### コンテンツファインダーを使用していない時は隠したい
「移動とサイズを制限する」のチェックを入れて下さい。  

### オーバーレイが見えないので位置が決められない
レイアウト用に表示したいときは 「移動とサイズを制限する」 のチェックを外して下さい。  
このチェックを外すと、全ての項目が表示されます。  
レイアウトが定まったら 移動とサイズを制限する」のチェックを入れて下さい。
