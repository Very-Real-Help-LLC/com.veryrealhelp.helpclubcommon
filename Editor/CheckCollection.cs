using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VeryRealHelp.HelpClubCommon.Editor
{
    public class CheckCollection : IEnumerable<CheckCollection.Check>
    {
        public class Check
        {
            public string label;
            public string invalidMessage;
            public Func<bool> test;
            public Action fix;

            public Check(
                string label,
                string invalidMessage,
                Func<bool> test,
                Action fix = null
            )
            {
                this.label = label;
                this.invalidMessage = invalidMessage;
                this.test = test;
                this.fix = fix;
            }

            public bool HasFix => fix != null;
            public bool Test() => test();
            public void Fix() => fix.Invoke();
        }

        public class Result
        {
            public Check check;
            public bool passed;
            public bool autoFixed;
        }

        private IEnumerable<Check> checks;
        private Func<IEnumerable<Check>> checkEnumerator;
        public IEnumerable<Check> Checks { get => checkEnumerator != null ? checkEnumerator() : checks; }

        public CheckCollection(Func<IEnumerable<Check>> checkEnumerator) => this.checkEnumerator = checkEnumerator;

        public CheckCollection(params Check[] checks) => this.checks = checks;

        public CheckCollection Concat(params CheckCollection[] others) => new CheckCollection(Checks.Concat(others.SelectMany(x => x.Checks)).ToArray());

        public IEnumerator<Check> GetEnumerator()
        {
            foreach (var check in Checks)
                yield return check;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<Result> GetResults(bool applyAutoFixes = false)
        {
            return Checks.Select(check => {
                var passed = check.Test();
                bool autoFixed = false;
                if (!passed && applyAutoFixes && check.HasFix) {
                    check.Fix();
                    passed = check.Test();
                    autoFixed = true;
                }
                return new Result()
                {
                    check = check,
                    passed = passed,
                    autoFixed = autoFixed
                };
            });
        }
    }

}
