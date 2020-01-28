﻿/*******
 Copyright 2017-2020 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 **********/

using System;

namespace NCMB.Internal
{

	//インターフェイスのメソッド
	//今後オペレーション処理が追加された場合NCMBFieldOperationを継承する事
	internal interface INCMBFieldOperation
	{

		object Encode ();

		INCMBFieldOperation MergeWithPrevious (INCMBFieldOperation previous);

		object Apply (object oldValue, NCMBObject obj, string key);
	}

}
