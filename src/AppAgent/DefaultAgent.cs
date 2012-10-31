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
using System.Threading;

namespace Taobao.Infrastructure.AppAgents
{
    /// <summary>
    /// 为windows平台的应用默认的Agent实现
    /// <remarks>
    /// 基于NamePipe（IPC）和MailSlot（To Master）实现通信
    /// 仅用于app管理以及进程通信，并非为高性能通信设计
    /// PS：
    /// NamePipe仅使用FIFO方式收发消息
    /// INTERVAL=5000ms
    /// BUFFER_SIZE=4096
    /// 
    /// 当前的Agent实现设计并没有良好设计抽象，需需要支持多种协议则需考虑对writer进行抽象：）
    /// </remarks>
    /// </summary> 
    public class DefaultAgent : IAgent
    {
        public static readonly double INTERVAL = 5000;
        public static readonly int BUFFER_SIZE = 4096;
        public static readonly string SLOT_WRITER = "DefaultAgent_Writer";
        protected string _master;
        protected string _name;
        protected string _description;
        protected NamedPipeServerStream _server;
        protected StreamWriter _writer;
        protected StreamReader _reader;
        protected ILog _log;
        protected System.Timers.Timer _timer;
        protected object _timer_lock = new object();
        protected bool _timer_actived;
        protected IMessageHandle _handle;
        /// <summary>
        /// 初始化agent
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="master">指定master server，若为空则忽略</param>
        /// <param name="name">agent节点名</param>
        /// <param name="description">agent节点描述</param>
        /// <param name="handle"></param>
        public DefaultAgent(ILoggerFactory factory
            , string master
            , string name
            , string description
            , IMessageHandle handle)
        {
            if (string.IsNullOrEmpty(name)
                || handle == null
                || factory == null)
                throw new InvalidOperationException("name|handle|factory均不能为空");

            this._name = name;
            this._description = description;
            this._master = master;
            this._handle = handle;
            this._log = factory.Create(this.GetType());

            if (!string.IsNullOrEmpty(this._description)
                && this._description.Length > 100)
                throw new InvalidOperationException("description不能超过100个字符");
        }

        #region IAgent Members
        public string Name
        {
            get { return this._name; }
        }
        public string Master
        {
            get { return this._master + "|" + DefaultMaster.Name; }
        }
        /// <summary>
        /// 启动节点
        /// </summary>
        public void Run()
        {
            //初始化server pipe
            this._server = new NamedPipeServerStream(this._name
                , PipeDirection.InOut
                , 1
                , PipeTransmissionMode.Byte
                , PipeOptions.Asynchronous
                , BUFFER_SIZE
                , BUFFER_SIZE);
            this._reader = new StreamReader(this._server);
            this._writer = new StreamWriter(this._server);
            //开始侦听
            this.Wait();
            //TODO:向Master注册
            if (!string.IsNullOrEmpty(this._master))
                Heartbeat();

            this._log.InfoFormat(@"启动默认的AppAgent实现，NamedPipe=\\.\PIPE\{0} | {1}"
                , this._name
                , string.IsNullOrEmpty(this._master)
                ? string.Empty
                : string.Format(@"Master=\\{0}\PIPE\{1}", this._master, DefaultMaster.Name));
        }
        /// <summary>
        /// 停止节点
        /// </summary>
        public virtual void Stop()
        {
            if (this._server != null)
                this._server.Close();
            if (this._timer != null)
                this._timer.Stop();
        }
        public virtual void Broadcast(string message)
        {

        }
        #endregion

        private void Wait()
        {
            try { this.DoWait(); }
            catch (Exception e)
            {
                if (!(e is IOException))
                {
                    this._log.Fatal("发生意外，停止AppAgent", e);
                    return;
                }
                //尝试重启
                try
                {
                    this._log.Warn("由于异常而重启AppAgent节点", e);
                    this.Stop();
                    this.Run();
                }
                catch (Exception ex)
                {
                    this._log.Fatal("发生意外，停止AppAgent", ex);
                }
            }
        }
        private void DoWait()
        {
            this._server.BeginWaitForConnection(o =>
            {
                var pipe = o.AsyncState as NamedPipeServerStream;

                try
                {
                    pipe.EndWaitForConnection(o);
                    //接收消息
                    var msg = this._reader.ReadLine();

                    if (msg != DefaultMaster.HeartbeatCmd)
                        this._log.InfoFormat("接收到消息：{0}", msg);

                    //将writer放于当前线程slot，允许以上下文方式扩展对handle内部信息定向到外部流上
                    Thread.SetData(Thread.GetNamedDataSlot(SLOT_WRITER), this._writer);
                    //处理消息
                    this._handle.Handle(msg, this._writer);
                    Thread.SetData(Thread.GetNamedDataSlot(SLOT_WRITER), null);

                    pipe.WaitForPipeDrain();
                }
                catch (Exception e)
                {
                    this._log.Error("AppAgent监听发生错误", e);
                }
                finally
                {
                    var flag = true;
                    try
                    {
                        if (pipe.IsConnected)
                            pipe.Disconnect();
                    }
                    catch (Exception e)
                    {
                        flag = false;
                        this._log.Fatal("AppAgent监听发生严重错误，停止工作", e);
                    }
                    if (flag)
                        this.Wait();
                }
            }, this._server);
        }

        protected virtual void Heartbeat()
        {
            this._timer = new System.Timers.Timer(INTERVAL);
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
                    DefaultMaster.Send(this._log
                        , this._master
                        , DefaultMaster.Name
                        , new DefaultMaster.Agent()
                        {
                            Server = Environment.MachineName,
                            Name = this._name,
                            Description = this._description,
                            Path = AppDomain.CurrentDomain.BaseDirectory
                        }.ToString()
                        , 100
                        , 500
                        , 500);
                }
                catch (Exception ex)
                {
                    this._log.Warn(string.Format("向Master={0}|{1}发送心跳时异常", this._master, DefaultMaster.Name), ex);
                }
                finally
                {
                    this._timer_actived = false;
                }
            };
            this._timer.Start();
        }

        /// <summary>
        /// 获取当前可用的TextWriter，若没有则返回Null
        /// </summary>
        /// <returns></returns>
        public static TextWriter GetWriter()
        {
            return Thread.GetData(Thread.GetNamedDataSlot(SLOT_WRITER)) as TextWriter;
        }
    }
}