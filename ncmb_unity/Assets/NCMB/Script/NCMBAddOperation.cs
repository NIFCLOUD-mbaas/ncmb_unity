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
using System.Collections;
using System.Collections.Generic;

namespace NCMB.Internal
{
	//add関連の履歴操作を扱う
	internal class NCMBAddOperation : INCMBFieldOperation
	{

		ArrayList objects = new ArrayList ();

		public NCMBAddOperation (object values)
		{
			//List等
			if (values is IEnumerable) {
				IEnumerable newValues = (IEnumerable)values;
				IEnumerator obj = newValues.GetEnumerator ();
				while (obj.MoveNext()) {
					object val = (object)obj.Current;
					this.objects.Add (val);
				}
			} else {
				this.objects.Add (values);
			}
		}

		//AndroidのmaybeReferenceAndEncode注意
		public object Encode ()
		{
			Dictionary<string, object> dic = new Dictionary<string, object> ();
			dic.Add ("__op", "Add");
			dic.Add ("objects", NCMBUtility._maybeEncodeJSONObject (this.objects, true));
			return dic;
		}

		public INCMBFieldOperation MergeWithPrevious (INCMBFieldOperation previous)
		{
			if (previous == null) {
				return this;
			}

			if ((previous is NCMBDeleteOperation)) {
				return new NCMBSetOperation (this.objects);
			}

			if ((previous is NCMBSetOperation)) {
				object value = ((NCMBSetOperation)previous).getValue ();
				if ((value is IList)) {
					ArrayList result = new ArrayList ((IList)value);
					result.AddRange (this.objects);
					return new NCMBSetOperation (result);
				}
				throw new  InvalidOperationException ("You can only add an item to a List.");
			}

			if ((previous is NCMBAddOperation)) {
				ArrayList result = new ArrayList (((NCMBAddOperation)previous).objects);
				result.AddRange (this.objects);
				return new NCMBAddOperation (result);
			}
			throw new  InvalidOperationException ("Operation is invalid after previous operation.");
		}

		public object Apply (object oldValue, NCMBObject obj, string key)
		{
			if (oldValue == null) {
				return this.objects;
			}
			if ((oldValue is IList)) {
				ArrayList result = new ArrayList ((IList)oldValue);
				result.AddRange (this.objects);
				return result;
			}
			throw new  InvalidOperationException ("Operation is invalid after previous operation.");
		}

	}
}
