/*
    Copyright (c) Alibaba.  All rights reserved. - http://www.alibaba-inc.com/

	Licensed under the Apache License, Version 2.0 (the "License");

	you may not use this file except in compliance with the License.

	You may obtain a copy of the License at
 
		 http://www.apache.org/licenses/LICENSE-2.0
 
	Unless required by applicable law or agreed to in writing, 

	software distributed under the License is distributed on an "AS IS" BASIS, 

	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

	See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Taobao.Infrastructure.AppAgents
{
    /// <summary>
    /// 消息处理器
    /// <remarks>仅为默认的agent实现提供</remarks>
    /// </summary>
    public interface IMessageHandle
    {
        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="msg">消息文本</param>
        /// <param name="writer">当前可用的Agent Writer</param>
        void Handle(string msg, System.IO.StreamWriter writer);
    }
}