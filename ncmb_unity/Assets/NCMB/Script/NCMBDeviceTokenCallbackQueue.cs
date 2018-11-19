/*******
 Copyright 2017-2018 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
 
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
using NCMB.Internal;
using UnityEngine;
using System.Collections.Generic;

namespace NCMB
{
    public class NCMBDeviceTokenCallbackQueue
    {
        private static NCMBDeviceTokenCallbackQueue instance;

        Queue<NCMBGetCallback<String>> queue;
        public static NCMBDeviceTokenCallbackQueue GetInstance()
        {
            if (instance == null)
            {
                instance = new NCMBDeviceTokenCallbackQueue();
            }
            return instance;
        }

        public Boolean isDuringSaveInstallation()
        {
            return (queue != null);
        }

        void beginSaveInstallation()
        {
            if (queue == null)
            {
                queue = new Queue<NCMBGetCallback<string>>();
            }
        }
        public void addQueue(NCMBGetCallback<string> callback)
        {
            beginSaveInstallation();
            queue.Enqueue(callback);
        }

        public void execQueue(String token, NCMBException e)
        {
            if (queue == null)
            {
                return;
            }
            while (queue.Count > 0)
            {
                NCMBGetCallback<string> callback = queue.Dequeue();
                callback(token, e);
            }

            endSaveInstallation();
        }
        void endSaveInstallation()
        {
            queue = null;
        }
    }
}