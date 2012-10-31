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
    /// 应用管理Agent
    /// <remarks>
    /// 用于对应用进行进程级别管理，提供IPC功能等
    /// 主要场景：本地缓存管理，配置管理，应用心跳，应用shell管理功能
    /// </remarks>
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// 获取Agent节点名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 获取指向的Master节点
        /// </summary>
        string Master { get; }
        /// <summary>
        /// 启动
        /// </summary>
        void Run();
        /// <summary>
        /// 停止
        /// </summary>
        void Stop();
        /// <summary>
        /// 向当前网络环境广播消息
        /// </summary>
        /// <param name="message"></param>
        void Broadcast(string message);
    }
}