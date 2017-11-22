using MyLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyThreadPool
{
    public class PriorityThreadPool
    {
        private const int MIN_THREAD_COUNT = 2;
        private List<Task> listOfTasks;
        private List<TaskItem> listOfTaskItem;
        private int activeTaskCount;
        private int maxThreadCount;
        private int TaskCount = 0;
        private Logger logger = Logger.getInstance(true);
        private int taskNumber = 0;
        private bool isClosed;

        public PriorityThreadPool(int maxThreadCount = MIN_THREAD_COUNT)
        {
            if (maxThreadCount < MIN_THREAD_COUNT)
            {
                throw new ArgumentException("Incorrect number of threads");
            }

            listOfTaskItem = new List<TaskItem>();
            listOfTasks = new List<Task>();
            this.maxThreadCount = maxThreadCount;

            for (int i = 0; i < MIN_THREAD_COUNT; i++)
            {
                Interlocked.Increment(ref activeTaskCount);
                var task = new Task(DoThreadWork, TaskCreationOptions.LongRunning);
                listOfTasks.Add(task);
                logger.Info("Создана задача №" + i);
                task.Start();

            }
        }

        public void WaitAll()
        {
            foreach (Task t in listOfTasks)
                t.Wait();
        }

        public int ThreadCount
        {
            get { return listOfTasks.Count; }
        }

        private TaskItem SelectTask()
        {

            lock (listOfTaskItem)
            {
                if (listOfTaskItem.Count == 0)
                    return null;// throw new ArgumentException();

                var waitingTasks = listOfTaskItem.Where(t => !t.isRunned);
                var highTasks = listOfTaskItem.Where(t => t.getPriority() == TaskPriority.HIGH);
                var middleTasks = listOfTaskItem.Where(t => t.getPriority() == TaskPriority.MIDDLE);

                if (highTasks.Count() > 0)
                {
                    logger.Info("Взята задача c id=" + highTasks.First().id);
                    return highTasks.First();
                }
                else
                {
                    if (middleTasks.Count() > 0)
                    {
                        logger.Info("Взята задача c id=" + middleTasks.First().id);
                        return middleTasks.First();
                    }
                    else
                    {
                        var lowTasks = waitingTasks.Where(t => t.getPriority() == TaskPriority.LOW);
                        if (lowTasks != null)
                        {
                            logger.Info("Взята задача c id=" + lowTasks.FirstOrDefault().id);
                        }
                        return lowTasks.FirstOrDefault();
                    }
                }
            }
        }

        public void Close()
        {
            lock (listOfTaskItem)
            {
                isClosed = true;
                Monitor.PulseAll(listOfTaskItem);
            }

            lock (this)
            {
                while (activeTaskCount > 0)
                    Monitor.Wait(this);
            }

            WaitAll();
        }

        private void tryAddNewTaskInPool()
        {
            if (activeTaskCount < maxThreadCount)
            {
                Interlocked.Increment(ref activeTaskCount);
                var task = new Task(DoThreadWork, TaskCreationOptions.LongRunning);
                listOfTasks.Add(task);
                logger.Info("Создана задача №" + activeTaskCount);
                task.Start();
            }
        }

        public void EnqueueTask(TaskDelegate function)
        {
            lock (listOfTaskItem)
            {
                if (!isClosed)
                {
                    TaskItem task = new TaskItem();
                    task.setFunction(function);
                    task.id = taskNumber;
                    taskNumber = Interlocked.Increment(ref TaskCount);
                    task.isRunned = false;
                    listOfTaskItem.Add(task);
                    if (listOfTaskItem.Count > activeTaskCount)
                    {
                        tryAddNewTaskInPool();
                    }
                    //listOfTaskItem.Enqueue(task);
                    Monitor.Pulse(listOfTaskItem);
                }
                else
                    throw new ObjectDisposedException(function.ToString());
            }
        }

        private TaskDelegate DequeueTask()
        {
            lock (listOfTaskItem)
            {
                while (listOfTaskItem.Count == 0 && !isClosed)
                    Monitor.Wait(listOfTaskItem);
                TaskDelegate t = null;
                if (listOfTaskItem.Count > 0)
                {
                    TaskItem task = SelectTask();
                    t = task.getFunction();
                    task.isRunned = true;
                    listOfTaskItem.Remove(task);

                }
                return t;
            }
        }

        private void DoThreadWork()
        {
            TaskDelegate task;
            do
            {
                task = DequeueTask();
                try
                {
                    if (task != null)
                        task();
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            } while (task != null);
            lock (this)
            {
                activeTaskCount--;
                if (activeTaskCount == 0)
                    Monitor.Pulse(this);
            }
        }
    }
}
