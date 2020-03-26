using System;
using System.Linq;

namespace VeryRealHelp.HelpClubCommon.Editor
{

    public class CheckCollection
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
        }

        public Check[] checks;
        public Check[] invalidCache;

        public CheckCollection(params Check[] checks) => this.checks = checks;

        public Check[] UpdateInvalidCache() => invalidCache = checks.Where(c => !c.test()).ToArray();

    }

}
