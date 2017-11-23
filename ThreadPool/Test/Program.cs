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
            var taskNumber = Interlocked.Increment(ref TaskCount);
            if (TaskCount == 5)
            {
                throw new Exception();
            }
            Wait();
        }

        static void Wait()
        {
            Thread.Sleep(1000);
        }

        static void Main(string[] args)
        {
            Logger logger = Logger.getInstance();
            int id = 0;
            var taskPool = new PriorityThreadPool(4);
            for (int i = 0; i < 10; i++)
            {
                taskPool.AddTask(TestTask);
                logger.Info("Добавлена задача №" + id);
                ++id;
            }
            for (int i = 0; i < 40; i++)
            {
                taskPool.AddTask(TestTask, TaskPriority.HIGH);
                logger.Info("Добавлена задача №" + id);
                ++id;
            }
            for (int i = 0; i < 10; i++)
            {
                taskPool.AddTask(TestTask, TaskPriority.MIDDLE);
                logger.Info("Добавлена задача №" + id);
                ++id;
            }
            taskPool.Close();
            Console.WriteLine("=)");
            Console.ReadLine();
        }
    }
}
