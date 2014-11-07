using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SudokuGUI
{
    public class ProcessingInstance<Result>
    {
        public ProcessingInstance(Func<Result> act)
        {
            Action = act;
            Worker = new Thread(new ThreadStart(Run));
            Used = false;
        }

        public event Action Completed;
        public event Action Failed;

        public String ErrorMessage
        {
            get;
            private set;
        }

        public Result ReturnValue
        {
            get;
            set;
        }

        private Func<Result> Action
        {
            get;
            set;
        }

        private Thread Worker
        {
            get;
            set;
        }

        private bool Used
        {
            get;
            set;
        }

        public void Start()
        {
            if (!Used)
            {
                Worker.Start();
                Used = true;
            }
            else
            {
                throw new Exception("ProcessingInstance can't be used multiple times");
            }
        }

        public void Cancel()
        {
            if (Worker.IsAlive)
            {
                Worker.Abort();
            }
        }

        private void Run()
        {
            try
            {
                ReturnValue = Action();
                if (Completed != null)
                {
                    Completed();
                }
            }
            catch (ThreadAbortException)
            {
                ErrorMessage = "Operation cancelled";
                if (Failed != null)
                {
                    Failed();
                }
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                if (Failed != null)
                {
                    Failed();
                }
            }

        }
    }
}
