using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StateSpaceSearch
{
    class SimpleStack : IHeap<int, State>
    {
        private readonly Stack<State> _innerStack = new Stack<State>();

        public void insert(int k, State v)
        {
            _innerStack.Push(v);
        }

        public State getMin()
        {
            return _innerStack.Peek();
        }

        public int getMinKey()
        {
            throw new NotImplementedException();
        }

        public State removeMin()
        {
            return _innerStack.Pop();
        }

        public bool remove(State v)
        {
            throw new NotImplementedException();
        }

        public bool change(State v, int newKey)
        {
            throw new NotImplementedException();
        }

        public int size()
        {
            return _innerStack.Count;
        }

        public void clear()
        {
            _innerStack.Clear();
        }

        public string getName()
        {
            throw new NotImplementedException();
        }
    }

    class SimpleQueue : IHeap<int, State>
    {
        readonly Queue<State> _innerQueue = new Queue<State>();

        public void insert(int k, State v)
        {
            _innerQueue.Enqueue(v);
        }

        public State getMin()
        {
            return _innerQueue.Peek();
        }

        public int getMinKey()
        {
            throw new NotImplementedException();
        }

        public State removeMin()
        {
            return _innerQueue.Dequeue();
        }

        public bool remove(State v)
        {
            throw new NotImplementedException();
        }

        public bool change(State v, int newKey)
        {
            throw new NotImplementedException();
        }

        public int size()
        {
            return _innerQueue.Count;
        }

        public void clear()
        {
            _innerQueue.Clear();
        }

        public string getName()
        {
            throw new NotImplementedException();
        }
    }

    // Depth First Search
    class DFS : SearchEngine
    {
        public DFS()
        {
            this.openNodes = new SimpleStack();
        }
    }

    // Breadth First Search
    class BFS : SearchEngine
    {
        public BFS()
        {
            this.openNodes = new SimpleQueue();
        }
    }

    // Depth Limited Search
    class DLS : DFS
    {
        private const int Limit = 700;

        protected override void addToOpenList(State s, int gValue, State pred)
        {
            if (gValue > Limit) return;
            base.addToOpenList(s, gValue, pred);
        }
    }

    // Iterative Deepening Search
    class IDS : DFS
    {
        private int _nextIterationDepthLimit;
        private int _currentDepthLimit;

        public override void search(State s)
        {
            result = null;
            for (_currentDepthLimit = 1; result == null; _currentDepthLimit = _nextIterationDepthLimit)
            {
                _nextIterationDepthLimit = Int32.MaxValue;

                printMessage("\n== Next Iteration ==", quiet);
                printMessage("Searching with depth limit: " + _currentDepthLimit, quiet);
                base.search(s);

                // No next depth available
                if (_nextIterationDepthLimit == Int32.MaxValue)
                {
                    printMessage("No vertices discovered above depth " + _currentDepthLimit + " - exitting.", quiet);
                    break;
                }
            }
        }

        protected override void addToOpenList(State s, int gValue, State pred)
        {
            // Append if less than allowed depth
            if (gValue <= _currentDepthLimit)
            {
                base.addToOpenList(s, gValue, pred);
            }

            // Over the depth limit => store the depth for next iteration
            else if (_nextIterationDepthLimit > gValue)
            {
                _nextIterationDepthLimit = gValue;
            }
        }
    }
}