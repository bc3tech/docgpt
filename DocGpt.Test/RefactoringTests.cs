
namespace DocGpt.Test
{
    using DocGpt.Options;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using System.Threading.Tasks;

    using RefactorCS = CSharpCodeRefactoringVerifier<DocGptRefactoringProvider>;

    [TestClass]
    [Ignore("Manual activation required; needs live API execution")]
    public class RefactoringTests : RefactorCS.Test
    {
        [ClassInitialize]
        public static void ClassInit()
        {
            //DocGptOptions.Instance.Endpoint = new("https://api.openai.com");
            //DocGptOptions.Instance.ApiKey = "TODO";
            //DocGptOptions.Instance.ModelDeploymentName = "gpt-3.5-turbo-16k";
        }

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task Refactor_Private_Member_Variable()
        {
            string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    /// <summary></summary>
    class MyClass
    {
        $$private int _numberOfUsers = 100;
    }
}";

            string fixd = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    /// <summary></summary>
    class MyClass
    {
        /// <summary>
        /// Represents the number of users. The default value is 100.
        /// </summary>
        private int _numberOfUsers = 100;
    }
}";

            await RefactorCS.VerifyRefactoringAsync(test, fixd);
        }

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task Refactor_Private_Member_Variable_Trigger_on_Name()
        {
            string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    /// <summary></summary>
    class MyClass
    {
        private int _n$$umberOfUsers = 100;
    }
}";

            string fixd = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    /// <summary></summary>
    class MyClass
    {
        /// <summary>
        /// Represents the number of users. The default value is 100.
        /// </summary>
        private int _numberOfUsers = 100;
    }
}";

            await RefactorCS.VerifyRefactoringAsync(test, fixd);
        }
    }
}
