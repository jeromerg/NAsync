using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Moq;
using NAsync;
using NUnit.Framework;
using TaskExtensions = NAsync.TaskExtensions;
#pragma warning disable 162

namespace NAsyncTests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public class ArgumentNullExceptionTest
    {
        [Test]
        public void AllArgumentNullExceptions()
        { 
            var task = new Task(() => {});

            try { TaskExtensions.Then(task, (Action)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(task, (Func<int>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(task, (Func<Task>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(task, (Func<Task<int>>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Catch(task, (Action<Exception>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Finally(task, (Action)null); Assert.Fail(); } catch(ArgumentNullException) { }

            var taskT = new Task<int>(() => 12);
            
            try { TaskExtensions.Then(taskT, (Action<int>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(taskT, (Func<int, string>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(taskT, (Func<int, Task>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(taskT, (Func<int, Task<int>>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Catch(taskT, (Func<Exception, int>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Finally(taskT, (Action)null); Assert.Fail(); } catch(ArgumentNullException) { }

            Action action = new Mock<Action>().Object;
            Func<int> funcT = new Mock<Func<int>>().Object;
            Func<Task> funcTask = new Mock<Func<Task>>().Object;
            Func<Task<string>> funcTaskT = new Mock<Func<Task<string>>>().Object;
            Action<Exception> actionEx = new Mock<Action<Exception>>().Object;

            try { TaskExtensions.Then((Task)null, action); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task)null, funcT); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task)null, funcTask); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task)null, funcTaskT); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Catch((Task)null, actionEx); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Finally((Task)null, action); Assert.Fail(); } catch(ArgumentNullException) { }

            Action<int> actionT = new Mock<Action<int>>().Object;
            Func<int, string> funcT1T2 = new Mock<Func<int, string>>().Object;
            Func<int, Task> funcTTask = new Mock<Func<int, Task>>().Object;
            Func<int, Task<string>> funcTTaskT = new Mock<Func<int, Task<string>>>().Object;
            Func<Exception, int> funcEx = new Mock<Func<Exception, int>>().Object;

            try { TaskExtensions.Then((Task<int>)null, actionT); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task<int>)null, funcT1T2); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task<int>)null, funcTTask); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task<int>)null, funcTTaskT); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Catch((Task<int>)null, funcEx); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Finally((Task<int>)null, action); Assert.Fail(); } catch(ArgumentNullException) { }
        }
    }
}
