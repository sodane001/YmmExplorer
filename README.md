# YmmExplorer

ゆっくりMovieMaker4(YMM4)用のプラグイン「エクスプローラー＋」です。
演出図形(3400種類・すべてに専用3Dモデル)、演出エフェクト(映像・音声 各800種類)、テキストのテンプレート(8000件・タグで検索)などを含みます。

## ダウンロード・入れ方

[Releases](https://github.com/sodane001/YmmExplorer/releases) から `YmmExplorer_(版).ymme` をダウンロードしてください。
YMM4 を閉じた状態で .ymme を開く(ダブルクリック、または YMM4 の画面にドラッグ)と、プラグインが入ります。
手で入れる時は、.ymme を .zip に名前を変えて開き、中の `YmmExplorer` フォルダーを YMM4 の `user\plugin` フォルダーに入れてください。

## 利用規約

使う前に [利用規約](利用規約.txt) をお読みください(入れた時点で同意したものとします)。

- 作者　: そだねー
- 連絡先: https://x.com/JA7n5XIM8L29006 (DM へ)

ゆっくりMovieMaker4 の公式プラグインではありません。YMM4 の作者様・配布元とは関係ありません。

## ビルド

```
cd YmmExplorer
dotnet build
```

- YMM4 の場所は `YmmExplorer.csproj` の `YMM4DirPath` で指定します。
- 3Dプレビュー(YMM43D)との橋渡しは、YMM43D が入っている時だけ作られます。
  YMM43D.dll は参照するだけで、このリポジトリにも出力にも含めません(再配布禁止のため)。

## 同梱プラグイン

`YmmExplorer/Bundled/` にある DLL は、同じ作者の別プラグインです。
midi・五線譜プラグインは、同じフォルダーの利用規約に従います。
