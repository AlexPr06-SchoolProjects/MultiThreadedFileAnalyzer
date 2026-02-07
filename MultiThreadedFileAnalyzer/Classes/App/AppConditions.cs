using MultiThreadedFileAnalyzer.Interfaces;

namespace MultiThreadedFileAnalyzer.Classes.App
{

    class Conditon : ICondition
    {
        public string Name { get; set; } = "Undefined_condition";
        private readonly List<Func<bool>> _conditions;
        public Conditon() {
            _conditions = new List<Func<bool>>();
        }

        public Conditon(string name)
        {
            Name = name;
        }

        public Conditon(string name, List<Func<bool>> conditions) : this(name)
        {
            _conditions = conditions;
        }

        public void Add(Func<bool> func) => _conditions.Add(func);

        public bool IsApplied()
        {
            for (int i = 0; i < _conditions.Count; i++)
            { 
                if (!_conditions[i]())
                    return false;
            }

            return _conditions.Count > 0;
        }
    }

    internal class AppLoopConditions
    {
        private readonly List<ICondition> _conditions;

        public AppLoopConditions() 
        {
            _conditions = new List<ICondition>();
        }

        public void Add(ICondition condition) =>  _conditions.Add(condition);

        public void AddSome(List<ICondition> conditions)
        {
            foreach(var condition in conditions)
                _conditions.Add(condition);
        }
        public void Clear() => _conditions.Clear();

        public bool AreApplied()
        {
            if (_conditions is null)
                return true;
            foreach (var condition in _conditions)
                if (!condition.IsApplied())
                    return false;
            return true;
        }
    }
}
