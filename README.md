# skyway-gateway-template

主にSkyway-WebRTC-Gatewayを紹介するためのリポジトリです。  
GatewayServer, GatewayClientはAPIの使い方を示したものです。  
いずれも内部的にはC#でRESTクライアントを用いています。  
js-clientはGatewayClientと同じ内容をJavaScriptで書いたものです。比較に使ってください。  
  
映像配信にはGStreamerを用いています。  
Windowsの場合は[ココ](https://gstreamer.freedesktop.org/download/)の"MinGW ~~ runtime installer" みたいなのをインストールするだけです。インストールオプションは"complete"を選んでおいてください。  
Linuxの場合は[コッチ](https://qiita.com/kurun_pan/items/f7896d52c1a3fcc947b0)です。  
  
プログラムの処理自体は見たまんまです。特に説明することはありません。  
最初のネゴシエーションだけが肝ですが，以下の手順を踏めば大丈夫です。  

## 起動順
* GatewayServer。内部でSkyway-GatewayとGStreamerの配信が実行されます。
* GatewayClient or js-client。

## 依存パッケージ
* [Husty.SkywayGateway](https://github.com/husty530/Husty-public/tree/master/Lib/cs/Husty.SkywayGateway)
* [RestSharp](https://restsharp.dev/) ... Hustyの方に含まれるので間接的に勝手に入ってきます。  
* OpenCvSharp ... clientでネットワークストリームを受け取ってます。これを使わずに生のGStreamerやffmpegでも受けることができます。
  
終了時にPeerの解除に失敗すると同じ名前のPeerが使えなくなります。  
その場合はGatewayごと(JavaScriptも使ってる場合は相手の通信ごと)再起動する必要があります。  

### おまけ ... OpenCvSharpのGStreamerビルド
基本的に[ココ](https://qiita.com/satorimon/items/8ab24eb54ab9e64bdf7c)にある手順を踏む。  
ただしちょっと情報が古くて，OpenCvSharpExtern→Properties→Linker→Input→AdditionalDependenciesにgstaudio-1.0.libの追加が必要。  
tesseractが要らなければ面倒なだけなのでスルーしてもいい。  
