# ニフティクラウド mobile backend Unity SDKについて

## 概要

ニフティクラウド mobile backend Unity SDKは、
モバイルアプリのバックエンド機能を提供するクラウドサービス
[ニフティクラウド mobile backend](http://mb.cloud.nifty.com)用のUnity SDKです。

- プッシュ通知
- データストア
- 会員管理

といった機能をアプリから利用することが可能です。

このSDKを利用する前に、ニフティクラウドmobile backendのアカウントを作成する必要があります。
[ニフティクラウド mobile backend](http://mb.cloud.nifty.com)のサービスサイトからアカウント登録を行ってください。

アカウント作成後のSDK導入手順については、
[クイックスタート](http://mb.cloud.nifty.com/doc/current/introduction/quickstart_unity.html)をご覧ください。

## 依存ライブラリ

プッシュ通知機能を利用する場合は、以下のライブラリが必要です。
(NCMB.package内部に含まれているので、別途用意する必要はありません。)

- Android Support Library
- Google Play Serivce SDK

## 動作環境

- Unity 5.x
- Android 5.x
- iOS 7.x〜10.x

※ Windows Phone 等、他のプラットフォームはサポートしていません。

## ライセンス

このSDKのライセンスはApache License Version 2.0、YamlDotNet（https://github.com/aaubry/YamlDotNet） に従います。

## 参考URL集

- [ニフティクラウド mobile backend](http://mb.cloud.nifty.com)
- [ドキュメント](http://mb.cloud.nifty.com/doc/current)
- [ユーザーコミュニティ](https://github.com/NIFTYCloud-mbaas/UserCommunity)
