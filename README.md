# YmmExplorer

ゆっくりMovieMaker4(YMM4)用のプラグイン「エクスプローラー＋」です。
演出図形(2000種類)と、その立体版(演出3D)を含みます。

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
