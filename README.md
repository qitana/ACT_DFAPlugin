# ACT_DFAPlugin

DutyFinder(コンテンツファインダー) のステータスをオーバーレイ表示する、[OverlayPlugin](https://github.com/hibiyasleep/OverlayPlugin) のアドオンです。 

マッチングのコードは[easly1989/ffxiv_act_dfassist](https://github.com/easly1989/ffxiv_act_dfassist)
で使用されているコードを参考にしています。

## 特徴
- OverlayPlugin のプラグインとして動作します。
- [FFXIV_ACT_Pugin](https://github.com/ravahn/FFXIV_ACT_Plugin) から展開済のデータを取得して処理するため、データ処理の負荷が軽微です。(ネットワークデータの処理を個別に行っていません)
- ルーレットでシャキった時点で、どのコンテンツにマッチしたのか分かります。
- コンテンツファインダーで申請直後から、何人待ちか分かります。
- 待ちが50番以降でも、人数が表示されます。
- 突入を押す前から、ロール毎のパーティ編成状況が分かります。
