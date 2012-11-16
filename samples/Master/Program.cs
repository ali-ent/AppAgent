using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taobao.Infrastructure.AppAgents;

namespace Master
{
    class Program
    {
        static void Main(string[] args)
        {
            new DefaultMaster(new DefaultAgentHandlerLog(new Log())
                , (msg, writer) => writer.WriteLine("received:" + msg)).Run();

            Console.ReadKey();
        }

        class Log : Taobao.Infrastructure.ILog
        {
            public bool IsDebugEnabled
            {
                get { throw new NotImplementedException(); }
            }

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

            public void Debug(object message)
            {
                Console.WriteLine(message);
            }

            public void DebugFormat(string format, params object[] args)
            {
                Console.WriteLine(string.Format(format, args));
            }

            public void Debug(object message, Exception exception)
            {
                Console.WriteLine(message);
            }

            public void Info(object message)
            {
                Console.WriteLine(message);
            }

            public void InfoFormat(string format, params object[] args)
            {
                Console.WriteLine(string.Format(format, args));
            }

            public void Info(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void Warn(object message)
            {
                Console.WriteLine(message);
            }

            public void WarnFormat(string format, params object[] args)
            {
                Console.WriteLine(string.Format(format, args));
            }

            public void Warn(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void Error(object message)
            {
                Console.WriteLine(message);
            }

            public void ErrorFormat(string format, params object[] args)
            {
                Console.WriteLine(string.Format(format, args));
            }

            public void Error(object message, Exception exception)
            {
                throw new NotImplementedException();
            }

            public void Fatal(object message)
            {
                Console.WriteLine(message);
            }

            public void FatalFormat(string format, params object[] args)
            {
                Console.WriteLine(string.Format(format, args));
            }

            public void Fatal(object message, Exception exception)
            {
                throw new NotImplementedException();
            }
        }
    }
}