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
using System.IO;

namespace Taobao.Infrastructure.AppAgents
{
    /// <summary>
    /// 实现了默认的进程消息处理
    /// </summary>
    public class DefaultHandle : IMessageHandle
    {
        #region IMessageHandle Members
        public void Handle(string msg, StreamWriter writer)
        {
            var args = (msg ?? "").Split(' ');
            //简易实现常规app管理命令
            //本地cache清理
            //clear cachekey 
            if (string.IsNullOrEmpty(args[0]))
                writer.WriteLine(this.Description);
            //else if (args[0].Equals("cache", StringComparison.InvariantCultureIgnoreCase))
            //    this.LocalCache(writer, args);
            else
                //处理其他消息
                this.HandleOther(msg, writer);

            writer.Flush();
        }
        #endregion

        protected virtual string Description { get { return "请提交有效的App管理命令，如：cache clear cachekey或cache summary"; } }
        protected virtual void HandleOther(string msg, StreamWriter writer) { }

        /* 开源版本取消此支持以简化依赖，由外部按需添加实现
        private void LocalCache(StreamWriter writer, params string[] args)
        {
            //cache clear|summary|detail key
            var action = args != null && args.Length > 1 ? args[1] : null;
            var key = args != null && args.Length > 2 ? args[2] : null;

            if (string.IsNullOrEmpty(action))
            {
                writer.WriteLine("cache管理支持操作：clear|summary|detail [key]");
                return;
            }
            if (action != "summary" && string.IsNullOrEmpty(key))
            {
                writer.WriteLine("clear|detail操作需要指定要清理的缓存键名称，支持模糊匹配");
                return;
            }
            if (action.Equals("summary"))
            {
                writer.WriteLine(string.Format("当前缓存总数={0}，NHibernate-Cache数={1}"
                    , System.Web.HttpRuntime.Cache.Count
                    , this.FindCache("nhibernate").Count));
                return;
            }
            if (action.Equals("detail"))
            {
                this.FindCache(key).ForEach(o =>
                {
                    writer.WriteLine(o);
                    writer.Flush();
                });
                return;
            }

            //clear
            var list = this.FindCache(key);
            //从缓存清除
            this.FindCache(key).ForEach(o => System.Web.HttpRuntime.Cache.Remove(o));
            writer.WriteLine(string.Format("清理了{0}个键名包含有“{1}”的缓存", list.Count, key));
        }
        private List<string> FindCache(string key)
        {
            var list = new List<string>();
            var e = System.Web.HttpRuntime.Cache.GetEnumerator();
            while (e.MoveNext())
                if (e.Key.ToString().ToLower().IndexOf(key) >= 0)
                    list.Add(e.Key.ToString());
            return list;
        }
         */
    }
}
