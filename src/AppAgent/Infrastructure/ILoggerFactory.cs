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

namespace Taobao.Infrastructure.Services
{
    /// <summary> 
    /// 提供日志记录器的创建
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// 创建Log
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILog Create(string name);
        /// <summary>
        /// 创建Log
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ILog Create(Type type);
    }
}
