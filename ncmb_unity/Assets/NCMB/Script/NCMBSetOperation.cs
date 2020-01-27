/*******
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
	//add操作の履歴管理を扱う
	internal class NCMBSetOperation : INCMBFieldOperation
	{

		public object Value;

		public NCMBSetOperation (object value)
		{
			this.Value = value;
		}

		public object getValue ()
		{
			return this.Value;
		}

		public object Encode ()
		{
			//エンコードを行う
			return NCMBUtility._maybeEncodeJSONObject (this.Value, true);
		}

		public INCMBFieldOperation MergeWithPrevious (INCMBFieldOperation previous)
		{
			return this;
		}

		public object Apply (object oldValue, NCMBObject obj, string key)
		{
			return this.Value;
		}

	}
}
