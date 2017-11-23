using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThreadPool
{

    public class TaskItem
    {
        private TaskDelegate function;
        private TaskPriority priority;
        public int id { get; set; }

        public TaskItem(TaskDelegate task = null, TaskPriority priority =TaskPriority.LOW)
        {
            this.function = task;
            this.priority = priority;
        }

        public TaskPriority getPriority()
        {
            return priority;
        }

        public void setPriority(TaskPriority priority)
        {
            this.priority = priority;
        }

        public TaskDelegate getFunction()
        {
            return function;
        }

        public void setFunction(TaskDelegate function)
        {
            this.function = function;
        }

    }
}
