using MyLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MyThreadPool;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static int TaskCount = 0;

        static void TestTask()
        {
            try
            {
                var taskNumber = Interlocked.Increment(ref TaskCount);
                WriteTaskNumber(taskNumber);

                //Thread.CurrentThread.Abort();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void WriteTaskNumber(int taskNumber)
        {
            for (int i = 0; i < 10000; ++i)
            {
                //Console.Write(" {0} ", taskNumber);
            }
            Thread.Sleep(2000);
        }

        static void Main(string[] args)
        {
            Logger logger = Logger.getInstance();
            var taskQueue = new PriorityThreadPool(4);
            for (int i = 0; i < 10; i++)
            {
                taskQueue.EnqueueTask(TestTask);
                logger.Info("Добавлена задача №" + i);
            }
            taskQueue.Close();
            Console.ReadLine();
        }
    }
}
