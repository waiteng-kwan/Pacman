using System;
using System.Collections;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Utils
{
    public class BaseProcess
    {
        private Action EOnDone = null;
        public string CommandIdentifier { get; private set; }
        public int CommandIndex { get; private set; }

        public BaseProcess(Action onDone = null)
        {
            EOnDone = onDone;
        }

        protected virtual void Execute()
        {
            //content here
            //the overarching manager will call Execute
        }

        protected virtual void NotifyProcessCompleted()
        {
            EOnDone?.Invoke();
        }

        protected virtual void Register(string identifier, Action onDone = null)
        {
            identifier = CommandIdentifier;
            EOnDone = onDone;
        }
    }
}