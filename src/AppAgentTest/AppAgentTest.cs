using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Taobao.Infrastructure.AppAgents;
using Taobao.Infrastructure.Services;

namespace Taobao.Infrastructure.Test.Infrastructure
{
    [TestClass]
    public class AppAgentTest
    {
        [TestMethod]
        public void NamedPipeTest()
        {
            var name = "agent"; //@"\\.\pipe\agent";
            var server = new NamedPipeServerStream(name
                , PipeDirection.InOut, 1
                , PipeTransmissionMode.Message
                , PipeOptions.Asynchronous
                , 4096
                , 4096);
            //server listen
            this.Listen(server, new StreamWriter(server));

            var msg = "";
            while ((msg += "1").Length < 40960) ;

            var i = 0;
            while (i++ < 10)
            {
                this.Write(new NamedPipeClientStream("localhost", name, PipeDirection.InOut), i + "=" + msg);
                //new Thread(o => this.Write(new NamedPipeClientStream(".", name, PipeDirection.InOut), o + "=" + msg)).Start(i);
            }

            Thread.Sleep(5000);
        }
        //server
        private void Listen(NamedPipeServerStream server, StreamWriter writer)
        {
            var buffer = new byte[4096];

            #region
            //server.BeginRead(buffer, 0, 4096, p =>
            //{
            //    Trace.WriteLine(p.IsCompleted);
            //    server.EndRead(p);
            //    var reader = new StreamReader(server);
            //    var temp = string.Empty;

            //    while (!string.IsNullOrEmpty((temp = reader.ReadLine())))
            //    {
            //        Trace.WriteLine("Server:from client " + temp);
            //        writer.WriteLine("echo:" + temp);
            //        writer.Flush();
            //        break;
            //    }
            //    server.Disconnect();
            //    Listen(server, writer);
            //}, null);
            #endregion

            server.BeginWaitForConnection(new AsyncCallback(o =>
            {
                var pipe = o.AsyncState as NamedPipeServerStream;
                pipe.EndWaitForConnection(o);

                var reader = new StreamReader(pipe);
                var result = reader.ReadLine();
                var text = string.Format("connected:receive from client {0}|{1}", result.Length, result);
                Trace.WriteLine(text);
                writer.WriteLine(result);
                writer.Flush();
                writer.WriteLine("End");
                writer.Flush();

                server.WaitForPipeDrain();
                server.Disconnect();
                Listen(pipe, writer);
            }), server);
        }
        //client
        private void Write(NamedPipeClientStream client, string msg)
        {
            client.Connect();
            var writer = new StreamWriter(client);
            writer.WriteLine(msg);
            writer.Flush();

            var reader = new StreamReader(client);
            string temp;
            while (!string.IsNullOrEmpty((temp = reader.ReadLine())))
                Trace.WriteLine(string.Format("client:read from server {0}|{1}", temp.Length, temp));
            client.Close();
        }

        [TestMethod]
        public void NamedPipeTimeout()
        {
            var client = new NamedPipeClientStream("localhost", "agent", PipeDirection.InOut);
            try
            {
                client.Connect(10);
            }
            catch (Exception e)
            {
                //throw e;
                Assert.AreEqual(typeof(TimeoutException), e.GetType());
            }

            //var writer = new StreamWriter(client);
            //var reader = new StreamReader(client);

            //try
            //{
            //    writer.WriteLine("");
            //    writer.Flush();
            //}
            //catch (Exception e)
            //{
            //    //throw e;
            //    //Assert.AreEqual(typeof(TimeoutException), e);
            //}
        }

        //默认实现测试
        [TestMethod]
        public void DefaultAppAgent()
        {
            var name = "Agent";
            var agent = new DefaultAgent(new TraceLoggerFactory()//DependencyResolver.Resolve<ILoggerFactory>()
                , "Master"
                , name, "test"
                , new DefaultHandle());

            agent.Run();

            var msg = "";
            while ((msg += "1").Length < 4099) ;

            var i = 0;
            while (i++ < 5)
            {
                Trace.WriteLine("发送消息，文本长度=" + msg.Length);
                Trace.WriteLine("返回：" + DefaultMaster.Send(null, ".", name, msg).Length);
                Thread.Sleep(1000);
            }
        }
        //[TestMethod]
        //public void DefaultAppAgent_Cache()
        //{
        //    //Trace.WriteLine("返回=" + DefaultMaster.Send("taobao-k2-dev", "CommonServiceNode", ""));
        //    //return;

        //    var name = "Agent";
        //    var agent = new DefaultAgent(new TraceLoggerFactory()
        //        , "Master"
        //        , name
        //        , "test"
        //        , new DefaultHandle());

        //    var msg = "";
        //    var i = 0;
        //    while (i++ < 8000) System.Web.HttpRuntime.Cache.Add("c_" + i, i, null
        //        , System.Web.Caching.Cache.NoAbsoluteExpiration
        //        , System.Web.Caching.Cache.NoSlidingExpiration
        //        , System.Web.Caching.CacheItemPriority.Default
        //        , null);
        //    //add cache
        //    System.Web.HttpRuntime.Cache["cache"] = DateTime.Now;
        //    System.Web.HttpRuntime.Cache["key"] = DateTime.Now;
        //    System.Web.HttpRuntime.Cache["key1"] = DateTime.Now;
        //    System.Web.HttpRuntime.Cache["key2"] = DateTime.Now;
        //    Trace.WriteLine("caches prepared");
        //    agent.Run();

        //    var log = new TraceLog();
        //    Trace.WriteLine("\n返回：" + DefaultMaster.Send(log, ".", name, ""));
        //    Trace.WriteLine("\n返回：" + DefaultMaster.Send(log, ".", name, "cache"));
        //    Trace.WriteLine("\n返回：" + DefaultMaster.Send(log, ".", name, "cache summary"));
        //    Trace.WriteLine("\n返回：" + DefaultMaster.Send(log, ".", name, "cache detail"));
        //    Trace.WriteLine("\n返回：" + DefaultMaster.Send(log, ".", name, "cache detail c"));
        //    Trace.WriteLine("\n返回：" + DefaultMaster.Send(log, ".", name, "cache clear key"));

        //    Assert.IsNotNull(System.Web.HttpRuntime.Cache["cache"]);
        //    Assert.IsNull(System.Web.HttpRuntime.Cache["key"]);
        //    Assert.IsNull(System.Web.HttpRuntime.Cache["key1"]);
        //    Assert.IsNull(System.Web.HttpRuntime.Cache["key2"]);
        //}

        [TestMethod]
        public void Master()
        {
            var f = new TraceLoggerFactory();
            var master = new DefaultMaster(f, (m, w) =>
            {
                try
                {
                    throw new Exception();
                }
                catch (Exception e)
                {
                    w.WriteLine(e.Message);
                    w.Flush();
                }
            });
            master.Run();
            var agent = new DefaultAgent(f, ".", "agent", "test", new DefaultHandle());
            agent.Run();

            DefaultMaster.Send(new TraceLog(), ".", DefaultMaster.Name, "");
            Thread.Sleep(10000);
            Trace.WriteLine("关闭agent");
            agent.Stop();
            Thread.Sleep(5000);
            Trace.WriteLine("打开agent");
            agent.Run();
            Thread.Sleep(10000);
            Trace.WriteLine("关闭master");
            master.Stop();
            Thread.Sleep(5000);
            Trace.WriteLine("打开master");
            master.Run();
            Thread.Sleep(10000);
            master.Broadcast("cache summary");
            Thread.Sleep(10000);
        }

        class TraceLoggerFactory : ILoggerFactory
        {

            public ILog Create(string name)
            {
                return new TraceLog();
            }

            public ILog Create(Type type)
            {
                return new TraceLog();
            }
        }
        class TraceLog : ILog
        {
            #region ILog Members

            public bool IsDebugEnabled
            {
                get { throw new NotImplementedException(); }
            }

            public void Info(object message)
            {
                throw new NotImplementedException();
            }

            public void InfoFormat(string format, params object[] args)
            {
                Trace.WriteLine(string.Format(format, args));
            }

            public void Info(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void Error(object message)
            {
                throw new NotImplementedException();
            }

            public void ErrorFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Error(object message, Exception exception)
            {
                Trace.WriteLine(message,"Error");
            }

            public void Warn(object message)
            {
                throw new NotImplementedException();
            }

            public void WarnFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Warn(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void Debug(object message)
            {
                Trace.WriteLine("Trace:" + message);
            }

            public void DebugFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Debug(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void Fatal(object message)
            {
                throw new NotImplementedException();
            }

            public void FatalFormat(string format, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void Fatal(object message, Exception exception)
            {
                Trace.WriteLine(message, "Fatal");
            }

            #endregion

            #region ILog Members


            public bool IsErrorEnabled
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsFatalEnabled
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsInfoEnabled
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsWarnEnabled
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }
    }
}