/*******
 Copyright 2017-2021 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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
using System.Collections.Generic;

namespace NCMB.Internal
{
	//Increment操作の履歴操作を扱う
	internal class NCMBIncrementOperation : INCMBFieldOperation
	{

		private object amount;

		public NCMBIncrementOperation (object amount)
		{
			this.amount = amount;
		}

		public object Encode ()
		{
			Dictionary<string, object> dic = new Dictionary<string, object> ();
			dic.Add ("__op", "Increment");
			dic.Add ("amount", this.amount);
			return dic;
		}

		public INCMBFieldOperation MergeWithPrevious (INCMBFieldOperation previous)
		{
			if (previous == null) {
				return this;
			}

			if ((previous is NCMBDeleteOperation)) {
				return new NCMBSetOperation (this.amount);
			}

			if ((previous is NCMBSetOperation)) {
				object value = ((NCMBSetOperation)previous).getValue ();
				if (value is string || value == null) {
					throw new  InvalidOperationException ("You cannot increment a non-number.");
				}
				return new NCMBSetOperation (NCMBObject._addNumbers (value, this.amount));
			}

			if ((previous is NCMBIncrementOperation)) {
				object oldAmount = (((NCMBIncrementOperation)previous).amount);
				return new NCMBIncrementOperation (NCMBObject._addNumbers (oldAmount, this.amount));
			}
			throw new  InvalidOperationException ("Operation is invalid after previous operation.");
		}

		public object Apply (object oldValue, NCMBObject obj, string key)
		{
			if (oldValue == null) {
				return this.amount;
			}
			if (oldValue is string || oldValue == null) {
				throw new  InvalidOperationException ("You cannot increment a non-number.");

			}
			return NCMBObject._addNumbers (oldValue, this.amount);
		}

	}
}
