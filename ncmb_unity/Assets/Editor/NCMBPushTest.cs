using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NCMB;

public class NCMBPushTest
{
	[TestFixtureSetUp]
	public void Init ()
	{
		NCMBTestSettings.Initialize ();
	}

	/**
     * - 内容：絞り込み条件が正しく設定されている事を確認する
     * - 結果：絞り込み条件が正しく取得できる事
     */
	[Test]
	public void searchConditionTest ()
	{
		// 絞り込み条件
		Dictionary<string, IDictionary> searchCondition = new Dictionary<string, IDictionary> () { { "score", new Dictionary<string, int> () {
					{ "$gte", 1000 },
					{ "$lte", 3000 }
				}
			}
		};
			
		NCMBPush push = new NCMBPush ();
		push.SearchCondition = searchCondition;

		Assert.AreEqual (1000, ((IDictionary)((IDictionary)push.SearchCondition) ["score"]) ["$gte"]);
		Assert.AreEqual (3000, ((IDictionary)((IDictionary)push.SearchCondition) ["score"]) ["$lte"]);
	}
}
