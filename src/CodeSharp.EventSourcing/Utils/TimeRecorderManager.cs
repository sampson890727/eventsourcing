//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个实用类，用于提供性能测试支持
    /// </summary>
    public class TimeRecorderManager
    {
        private static TimeRecorderManager _instance = new TimeRecorderManager();
        private Dictionary<string, TimeRecorder> _timeRecorderDictionary = new Dictionary<string, TimeRecorder>();

        private TimeRecorderManager() { }

        public static TimeRecorderManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public TimeRecorder GetTimeRecorder(string timeRecorderName)
        {
            return GetTimeRecorder(timeRecorderName, false);
        }
        public TimeRecorder GetTimeRecorder(string timeRecorderName, bool reset)
        {
            if (!_timeRecorderDictionary.ContainsKey(timeRecorderName))
            {
                _timeRecorderDictionary.Add(timeRecorderName, new TimeRecorder(timeRecorderName));
            }
            var recorder = _timeRecorderDictionary[timeRecorderName];

            if (reset)
            {
                recorder.Reset();
            }

            return recorder;
        }
    }
    public class TimeRecorder
    {
        #region Private Members

        private List<Action> _actionList;
        private Stopwatch _stopWatch;

        #endregion

        #region Constructors

        public TimeRecorder(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
            _actionList = new List<Action>();
            _stopWatch = new Stopwatch();
        }

        #endregion

        #region Public Properties

        public string Name { get; private set; }

        #endregion

        #region Public Methods

        public void Reset()
        {
            _stopWatch.Stop();
            _stopWatch.Reset();
            _actionList.Clear();
        }
        public Action BeginAction(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            return new Action(this, description);
        }
        public string GenerateReport()
        {
            StringBuilder reportBuilder = new StringBuilder();

            reportBuilder.AppendLine(Environment.NewLine);
            reportBuilder.AppendLine("------------------------------------------------------------------------------------------------------------------------------------");

            reportBuilder.AppendLine(string.Format("TimeRecorder Name:{0}  Total Action Times:{1}ms", Name, (GetTotalTicks() / 10000).ToString()));
            reportBuilder.AppendLine("Action Time Details:");
            reportBuilder.AppendLine(GenerateTreeReport());

            reportBuilder.AppendLine("------------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);

            return reportBuilder.ToString();
        }

        #endregion

        #region Internal Methods

        internal void AddCompletedAction(Action action)
        {
            if (action != null && action.IsCompleted)
            {
                _actionList.Add(action);
            }
        }
        internal double GetCurrentTicks()
        {
            _stopWatch.Stop();
            double currentTicks = (double)_stopWatch.Elapsed.Ticks;
            _stopWatch.Start();
            return currentTicks;
        }

        #endregion

        #region Private Methods

        private string GenerateTreeReport()
        {
            string totalString = string.Empty;
            string leftSpace = "";
            string unitIndentString = "    ";
            List<string> actionTimeStrings = new List<string>();
            List<Action> topLevelActions = null;

            topLevelActions = GetTopLevelActions();

            foreach (Action action in topLevelActions)
            {
                action.TreeNodeDeepLevel = 1;
            }

            foreach (Action action in topLevelActions)
            {
                BuildChildActionTree(action);
            }

            foreach (Action action in topLevelActions)
            {
                GenerateActionTimeStrings(action, leftSpace, unitIndentString, actionTimeStrings);
                totalString += string.Join(Environment.NewLine, actionTimeStrings.ToArray());
                if (topLevelActions.IndexOf(action) < topLevelActions.Count() - 1)
                {
                    totalString += Environment.NewLine;
                }
                actionTimeStrings.Clear();
            }

            return totalString;
        }
        private void BuildChildActionTree(Action parentAction)
        {
            List<Action> childActions = GetChildActions(parentAction);
            foreach (Action childAction in childActions)
            {
                childAction.TreeNodeDeepLevel = parentAction.TreeNodeDeepLevel + 1;
                childAction.ParentAction = parentAction;
                parentAction.ChildActions.Add(childAction);
                BuildChildActionTree(childAction);
            }
        }
        private double GetTotalTicks()
        {
            if (_actionList.Count == 0)
            {
                return 0D;
            }

            double total = 0;
            foreach (Action action in GetTopLevelActions())
            {
                total = total + action.TotalTicks;
            }
            return total;
        }
        private bool IsTopLevelAction(Action action)
        {
            if (action == null)
            {
                return false;
            }
            foreach (Action a in _actionList)
            {
                if (a.Id == action.Id)
                {
                    continue;
                }
                if (a.StartTicks < action.StartTicks && a.EndTicks > action.EndTicks)
                {
                    return false;
                }
            }
            return true;
        }
        private List<Action> GetTopLevelActions()
        {
            List<Action> topLevelActions = new List<Action>();
            foreach (Action action in _actionList)
            {
                if (IsTopLevelAction(action))
                {
                    topLevelActions.Add(action);
                }
            }
            return topLevelActions;
        }
        private Action GetDirectParent(Action action)
        {
            if (action == null)
            {
                return null;
            }
            foreach (Action a in _actionList)
            {
                if (action.Id == a.Id)
                {
                    continue;
                }
                if (a.StartTicks < action.StartTicks && a.EndTicks > action.EndTicks)
                {
                    return a;
                }
            }
            return null;
        }
        private List<Action> GetChildActions(Action parentAction)
        {
            if (parentAction == null)
            {
                return new List<Action>();
            }
            List<Action> childActions = new List<Action>();
            foreach (Action action in _actionList)
            {
                if (action.Id == parentAction.Id)
                {
                    continue;
                }
                if (action.StartTicks > parentAction.StartTicks && action.EndTicks < parentAction.EndTicks)
                {
                    Action directParent = GetDirectParent(action);
                    if (directParent != null && directParent.Id == parentAction.Id)
                    {
                        childActions.Add(action);
                    }
                }
            }
            return childActions;
        }
        private void GenerateActionTimeStrings(Action action, string leftSpace, string unitIndentString, List<string> actionTimeStrings)
        {
            string actionTimeStringFormat = "{0}{1}({2})  {3}  {4}  {5}";
            string actionTimeLeftSpaceString = leftSpace;
            for (int i = 0; i <= action.TreeNodeDeepLevel - 1; i++)
            {
                actionTimeLeftSpaceString += unitIndentString;
            }

            actionTimeStrings.Add(string.Format(actionTimeStringFormat, new object[] { actionTimeLeftSpaceString, (action.TotalTicks / 10000).ToString() + "ms", GetTimePercent(action), action.Description, action.StartTime.ToString() + ":" + action.StartTime.Millisecond.ToString(), action.EndTime.ToString() + ":" + action.EndTime.Millisecond.ToString() }));

            foreach (Action childAction in action.ChildActions)
            {
                GenerateActionTimeStrings(childAction, leftSpace, unitIndentString, actionTimeStrings);
            }
        }
        private string GetTimePercent(Action action)
        {
            if (action.TreeNodeDeepLevel == 1)
            {
                var totalTicks = GetTotalTicks();
                if (totalTicks == 0D)
                {
                    return "0.00%";
                }
                else
                {
                    return (action.TotalTicks / totalTicks).ToString("##.##%");
                }
            }
            else if (action.TreeNodeDeepLevel >= 2)
            {
                if (action.ParentAction.TotalTicks == 0)
                {
                    return "0.00%";
                }
                else
                {
                    return (action.TotalTicks / action.ParentAction.TotalTicks).ToString("##.##%");
                }
            }
            return "0.00%";
        }

        #endregion
    }
    public class Action
    {
        #region Constructors

        public Action(TimeRecorder timeRecorder, string description)
        {
            if (timeRecorder == null)
            {
                throw new ArgumentNullException("timeRecorder");
            }

            Id = Guid.NewGuid().ToString();
            TimeRecorder = timeRecorder;
            StartTicks = TimeRecorder.GetCurrentTicks();
            StartTime = DateTime.Now;
            Description = description;
            IsCompleted = false;
            ChildActions = new List<Action>();
        }

        #endregion

        #region Public Properties

        public TimeRecorder TimeRecorder { get; private set; }
        public string Id { get; private set; }
        public Action ParentAction { get; set; }
        public List<Action> ChildActions { get; set; }
        public int TreeNodeDeepLevel { get; set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public string Description { get; private set; }
        public double StartTicks { get; private set; }
        public double EndTicks { get; private set; }
        public double TotalTicks
        {
            get { return EndTicks - StartTicks; }
        }
        public bool IsCompleted { get; private set; }

        #endregion

        #region Public Methods

        public void Complete()
        {
            EndTicks = TimeRecorder.GetCurrentTicks();
            EndTime = DateTime.Now;
            IsCompleted = true;
            TimeRecorder.AddCompletedAction(this);
        }

        #endregion
    }
}