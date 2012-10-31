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
using System.IO.Pipes;
using System.IO;
using Taobao.Infrastructure.Services;
using System.Timers;

namespace Taobao.Infrastructure.AppAgents
{
    /// <summary>
    /// 默认的简易Master实现
    /// <remarks>
    /// 基于NamePipe和MailSlot实现通信
    /// 支持对注册到此Master的AppAgent的管理以及消息广播功能
    /// PS:检查时间
    /// </remarks>
    /// </summary>
    public class DefaultMaster : DefaultAgent
    {
        /// <summary>
        /// 默认的Master名称
        /// </summary>
        public new static readonly string Name = "Master";
        /// <summary>
        /// 向agent发送的心跳命令文本
        /// </summary>
        public static readonly string HeartbeatCmd = "heartbeat";
        private List<Agent> _agents;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="factory"></param>
        public DefaultMaster(ILoggerFactory factory) : this(factory, null) { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="handle">定义命令处理，PS:避免writeline()空内容会导致pipe broken</param>
        public DefaultMaster(ILoggerFactory factory, Action<string, StreamWriter> handle)
            : base(factory, Name, Name, "Master", new DefaultHandle())
        {
            this._agents = new List<Agent>();
            this._handle = new MasterMessageHandle(this, handle);
        }

        //子节点验证
        protected override void Heartbeat()
        {
            this._timer = new Timer(INTERVAL * 2);
            this._timer.Elapsed += (s, e) =>
            {
                if (this._timer_actived) return;

                lock (this._timer_lock)
                    if (!this._timer_actived)
                        this._timer_actived = true;
                    else
                        return;

                try
                {
                    #region 遍历节点验证
                    for (var i = 0; i < this._agents.Count; i++)
                    {
                        var o = this._agents[i];
                        try
                        {
                            DefaultMaster.Send(this._log, o.Server, o.Name, HeartbeatCmd, 20, 500, 500);
                        }
                        catch (Exception ex)
                        {
                            lock (this._agents)
                                this._agents.Remove(o);
                            i--;
                            this._log.Warn(string.Format("移除节点{0}|{1}", o.Server, o.Name), ex);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    this._log.Warn(ex);
                }
            };
            this._timer.Start();
        }
        public override void Stop()
        {
            base.Stop();

            if (this._agents != null)
            {
                lock (this._agents)
                    this._agents.Clear();
            }
        }

        /// <summary>
        /// 获取所有注册到此Master的Agent信息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Agent> GetAgents()
        {
            return this._agents.AsEnumerable();
        }
        /// <summary>
        /// 向agent节点广播消息
        /// </summary>
        /// <param name="message"></param>
        public override void Broadcast(string message)
        {
            this._agents.ForEach(o =>
            {
                try
                {
                    this._log.InfoFormat("向节点{0}|{1}发送消息{2}，返回：{3}"
                        , o.Server
                        , o.Name
                        , message
                        , DefaultMaster.Send(this._log
                        , o.Server
                        , o.Name
                        , message));
                }
                catch (Exception e)
                {
                    this._log.Error(string.Format("向节点{0}|{1}发送消息时异常", o.Server, o.Name), e);
                }
            });
        }

        private void Register(Agent agent)
        {
            if (string.IsNullOrEmpty(agent.Name) || string.IsNullOrEmpty(agent.Server)) return;
            if (this._agents.Contains(agent)) return;

            lock (this._agents)
            {
                if (this._agents.Contains(agent)) return;
                this._agents.Add(agent);
            }
            this._log.DebugFormat("注册了节点{0}|{1}|{2}", agent.Name, agent.Server, agent.Description);
        }

        /// <summary>
        /// 向指定Agent节点发送消息 Block
        /// </summary>
        /// <param name="log">输出程序，可为其实现console输出或流输出</param>
        /// <param name="server">目标主机</param>
        /// <param name="agent">目标节点名</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static string Send(ILog log, string server, string agent, string message)
        {
            return Send(log, server, agent, message, null, null, null);
        }
        /// <summary>
        /// 向指定Agent节点发送消息
        /// </summary>
        /// <param name="log">输出程序，可为其实现console输出或流输出</param>
        /// <param name="server">目标主机</param>
        /// <param name="agent">目标节点名</param>
        /// <param name="message">消息</param>
        /// <param name="connectTimeout">连接超时设置</param>
        /// <param name="readTimeout">读超时</param>
        /// <param name="writeTimeout">写超时</param>
        /// <returns></returns>
        public static string Send(ILog log, string server, string agent, string message, int? connectTimeout, int? readTimeout, int? writeTimeout)
        {
            var client = new NamedPipeClientStream(server, agent, PipeDirection.InOut);
            //if (readTimeout.HasValue)
            //    client.ReadTimeout = readTimeout.Value;
            //if (writeTimeout.HasValue)
            //    client.WriteTimeout = writeTimeout.Value;

            if (connectTimeout.HasValue)
                client.Connect(connectTimeout.Value);
            else
                client.Connect();

            var writer = new StreamWriter(client);
            var reader = new StreamReader(client);

            try
            {
                writer.WriteLine(message);
                writer.Flush();

                string result = string.Empty, temp;
                while (!string.IsNullOrEmpty((temp = reader.ReadLine())))
                {
                    result += (temp + "\n");
                    if (log != null) log.Debug(temp);
                }
                return result.Trim();
            }
            catch (Exception e)
            {
                throw e;

            }
            finally
            {
                //会造成System.IO.IOException: Pipe is broken.
                //由于server调用完成就会主动关闭
                //if (writer != null)
                //    writer.Close();
                writer = null;
                if (reader != null)
                    reader.Close();
                if (client != null)
                    client.Close();
            }
        }
        
        /// <summary>
        /// Agent节点信息
        /// <remarks>作为心跳信息内容</remarks>
        /// </summary>
        public class Agent
        {
            /// <summary>
            /// 服务器名称
            /// </summary>
            public string Server { get; set; }
            /// <summary>
            /// 节点名称
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 节点描述
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 节点运行路径
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// 返回形如：server|name|description的心跳消息文本
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("{0}|{1}|{2}|{3}", this.Server, this.Name, this.Description, this.Path);
            }
            public override bool Equals(object obj)
            {
                var agent = obj as Agent;
                return agent != null
                    && this.Name.Equals(agent.Name, StringComparison.InvariantCultureIgnoreCase)
                    && this.Server.Equals(agent.Server, StringComparison.InvariantCultureIgnoreCase);
            }
            /// <summary>
            /// 从消息文本创建对象
            /// </summary>
            /// <param name="msg"></param>
            /// <returns></returns>
            public static Agent FromMessage(string msg)
            {
                var temp = msg.Split('|');
                return temp.Length < 2
                    ? null
                    : new Agent()
                    {
                        //必须
                        Server = temp[0],
                        Name = temp[1],
                        //可选
                        Description = temp.Length < 3 ? string.Empty : temp[2],
                        Path = temp.Length < 4 ? string.Empty : temp[3],
                    };
            }
        }
        /// <summary>
        /// 提供Master节点消息处理功能
        /// </summary>
        public class MasterMessageHandle : IMessageHandle
        {
            private DefaultMaster _master;
            private Action<string, StreamWriter> _handle;
            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="master"></param>
            /// <param name="handle">允许设置额外的命令扩展委托</param>
            public MasterMessageHandle(DefaultMaster master, Action<string, StreamWriter> handle)
            {
                this._master = master;
                this._handle = handle ?? new Action<string, StreamWriter>((m, w) => { });
            }

            #region IMessageHandle Members

            public void Handle(string msg, StreamWriter writer)
            {
                var agent = Agent.FromMessage(msg);
                
                if (agent == null)
                {
                    this._handle(msg, writer);
                    return;
                }

                this._master.Register(agent);
                writer.WriteLine("ok");
                writer.Flush();
            }

            #endregion
        }
    }
}