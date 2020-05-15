# ニフクラ mobile backend Unity SDKについて

## 概要

スマートフォンアプリ向けクラウド「ニフクラ mobile backend（https://mbaas.nifcloud.com/）」用の **** SDK です。
SDKを導入することで、以下の機能をアプリから利用することが可能です。

- [プッシュ通知](https://mbaas.nifcloud.com/doc/current/push/basic_usage_unity.html)
- [会員管理・認証](https://mbaas.nifcloud.com/doc/current/user/basic_usage_unity.html)
- [SNS連携](https://mbaas.nifcloud.com/doc/current/sns/facebook_unity.html)
- [データストア](https://mbaas.nifcloud.com/doc/current/datastore/basic_usage_unity.html)
- [ファイルストア](https://mbaas.nifcloud.com/doc/current/filestore/basic_usage_unity.html)
- [位置情報検索](https://mbaas.nifcloud.com/doc/current/geopoint/basic_usage_unity.html)
- [スクリプト](https://mbaas.nifcloud.com/doc/current/script/basic_usage_unity.html)

SDKを通じてAPIを利用するには、[ニフクラ mobile backend](https://mbaas.nifcloud.com)の会員登録が必要です。
SDK導入手順については、[クイックスタート](https://mbaas.nifcloud.com/doc/current/introduction/quickstart_unity.html)をご覧ください。

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
(※2020年5月時点)

※ Windows Phone 等、他のプラットフォームはサポートしていません。

## テクニカルサポート窓口対応バージョン

テクニカルサポート窓口では、1年半以内にリリースされたSDKに対してのみサポート対応させていただきます。<br>
定期的なバージョンのアップデートにご協力ください。<br>
※なお、mobile backend にて大規模な改修が行われた際は、1年半以内のSDKであっても対応出来ない場合がございます。<br>
その際は[informationブログ](https://mbaas.nifcloud.com/info/)にてお知らせいたします。予めご了承ください。

- v4.0.1 ～ (※2020年5月時点)

## ライセンス

このSDKのライセンスはApache License Version 2.0、YamlDotNet（ https://github.com/aaubry/YamlDotNet ）に従います。

## 参考URL集

- [ニフクラ mobile backend](https://mbaas.nifcloud.com)
- [ドキュメント](https://mbaas.nifcloud.com/doc/current)
- [ユーザーコミュニティ](https://github.com/NIFCLOUD-mbaas/UserCommunity)
