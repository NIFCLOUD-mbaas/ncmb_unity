/*******
 Copyright 2014 NIFTY Corporation All Rights Reserved.
 
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
//using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace NCMB.Internal
{
	internal class Platform
	{
		//internal static int ApiTimeout;
		private static readonly ReaderWriterLockSlim QueueLock;
		private static readonly Queue<Action> Queue;
		static Platform ()
		{
			//WWWRequestLooper.ApiTimeout = 30;
			Platform.QueueLock = new ReaderWriterLockSlim ();
			Platform.Queue = new Queue<Action> ();
		}

		//メインスレッドで実行する処理をキューに追加
		internal static void RunOnMainThread (Action action)
		{
			//ロック中なら
			if (Platform.QueueLock.IsWriteLockHeld) {
				Platform.Queue.Enqueue (action);
				return;
			}
			Platform.QueueLock.EnterWriteLock ();//追加中、他のスレッドから書き込まれないようロック
			try {
				Platform.Queue.Enqueue (action);
			} finally {
				Platform.QueueLock.ExitWriteLock ();//確実にロック解除
			}
		}


		//メインスレッドでキューの状態を監視し、追加されれば実行する
		//[DebuggerHidden]
		internal static IEnumerator RunLoop ()
		{
			while (true) {
				Platform.QueueLock.EnterUpgradeableReadLock ();//別のロックにアップグレード可能な状態で読み取りロック
				try {
					int i = Platform.Queue.Count;
					if (i > 0) {
						Platform.QueueLock.EnterWriteLock (); //更新の必要があれば、書き込みロックにアップグレード
						try {
							while (i > 0) {
								try {
									Platform.Queue.Dequeue () ();//追加
								} catch (Exception ex) {
									Debug.LogException (ex);
								}
								i--;
							}
						} finally {
							Platform.QueueLock.ExitWriteLock ();//書込ロック解除
						}
					}
				} finally {
					Platform.QueueLock.ExitUpgradeableReadLock ();//アップグレード可能な読み取りロック解除
				}
				yield return null;
			}
		}
	}
}
