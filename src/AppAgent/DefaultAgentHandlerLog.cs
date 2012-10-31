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
    /// 为默认的Agent实现提供对日志输出的截获并输出到外部流
    /// <remarks>为简化使用而设计</remarks>
    /// </summary>
    public class DefaultAgentHandlerLog : LogWrapper
    {
        /// <summary>
        /// 初始化为默认的Agent实现提供对日志输出的截获并输出到外部流
        /// </summary>
        /// <param name="log"></param>
        public DefaultAgentHandlerLog(ILog log) : base(log) { }

        public override void Debug(object message)
        {
            base.Debug(message);
            this.Render(message);
        }
        public override void Debug(object message, Exception exception)
        {
            base.Debug(message, exception);
            this.Render(message);
        }
        public override void DebugFormat(string format, params object[] args)
        {
            base.DebugFormat(format, args);
            this.Render(string.Format(format, args));
        }
        public override void Info(object message)
        {
            base.Info(message);
            this.Render(message);
        }
        public override void Info(object message, Exception exception)
        {
            base.Info(message, exception);
            this.Render(message);
        }
        public override void InfoFormat(string format, params object[] args)
        {
            base.InfoFormat(format, args);
            this.Render(string.Format(format, args));
        }
        public override void Warn(object message)
        {
            base.Warn(message);
            this.Render(message);
        }
        public override void Warn(object message, Exception exception)
        {
            base.Warn(message, exception);
            this.Render(message);
        }
        public override void WarnFormat(string format, params object[] args)
        {
            base.WarnFormat(format, args);
            this.Render(string.Format(format, args));
        }
        public override void Error(object message)
        {
            base.Error(message);
            this.Render(message);
        }
        public override void Error(object message, Exception exception)
        {
            base.Error(message, exception);
            this.Render(message);
        }
        public override void ErrorFormat(string format, params object[] args)
        {
            base.ErrorFormat(format, args);
            this.Render(string.Format(format, args));
        }
        public override void Fatal(object message)
        {
            base.Fatal(message);
            this.Render(message);
        }
        public override void Fatal(object message, Exception exception)
        {
            base.Fatal(message, exception);
            this.Render(message);
        }
        public override void FatalFormat(string format, params object[] args)
        {
            base.FatalFormat(format, args);
            this.Render(string.Format(format, args));
        }

        private void Render(object message)
        {
            var w = Taobao.Infrastructure.AppAgents.DefaultAgent.GetWriter();
            if (w == null) return;
            w.WriteLine(message);
            w.Flush();
        }
    }
}