# ニフクラ mobile backend Unity SDKについて

## 概要

ニフクラ mobile backend Unity SDKは、
モバイルアプリのバックエンド機能を提供するクラウドサービス
[ニフクラ mobile backend](https://mbaas.nifcloud.com)用のUnity SDKです。

- プッシュ通知
- データストア
- 会員管理
- ファイルストア
- SNS連携
- スクリプト

といった機能をアプリから利用することが可能です。

このSDKを利用する前に、ニフクラ mobile backendのアカウントを作成する必要があります。
[ニフクラ mobile backend](https://mbaas.nifcloud.com)のサービスサイトからアカウント登録を行ってください。

アカウント作成後のSDK導入手順については、
[クイックスタート](https://mbaas.nifcloud.com/doc/current/introduction/quickstart_unity.html)をご覧ください。

## 依存ライブラリ

プッシュ通知機能を利用する場合は、以下のライブラリが必要です。
(NCMB.package内部に含まれているので、別途用意する必要はありません。)

- Android Support Library
- Google Play Service SDK
- Firebase Service Library

## 動作環境

- Unity 2017.x~2019.x
- Android 7.x~9.x
- iOS 10.x〜13.x
(※2020年4月時点)

※ Windows Phone 等、他のプラットフォームはサポートしていません。

## テクニカルサポート窓口対応バージョン

テクニカルサポート窓口では、1年半以内にリリースされたSDKに対してのみサポート対応させていただきます。<br>
定期的なバージョンのアップデートにご協力ください。<br>
※なお、mobile backend にて大規模な改修が行われた際は、1年半以内のSDKであっても対応出来ない場合がございます。<br>
その際は[informationブログ](https://mbaas.nifcloud.com/info/)にてお知らせいたします。予めご了承ください。

- v4.0.1 ～ (※2020年1月時点)

## ライセンス

このSDKのライセンスはApache License Version 2.0、YamlDotNet（ https://github.com/aaubry/YamlDotNet ）に従います。

## 参考URL集

- [ニフクラ mobile backend](https://mbaas.nifcloud.com)
- [ドキュメント](https://mbaas.nifcloud.com/doc/current)
- [ユーザーコミュニティ](https://github.com/NIFCLOUD-mbaas/UserCommunity)
