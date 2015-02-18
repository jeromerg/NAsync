using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Moq;
using NAsync;
using NUnit.Framework;
using TaskExtensions = NAsync.TaskExtensions;

namespace NAsyncTests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public class TaskExtensionsTest
    {
        public interface IThen
        {
            void NextAction();
            void NextActionWithInput(int resultOfFirst);
            string NextFunction();
            string NextFunctionWithInput(int resultOfFirst);
        }

        [Test]
        public void AllArgumentNullExceptions()
        { 
            var task = new Task(() => {});

            try { TaskExtensions.Then(task, (Action)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(task, (Func<int>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(task, (Func<Task>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(task, (Func<Task<int>>)null); Assert.Fail(); } catch(ArgumentNullException) { }

            var taskT = new Task<int>(() => 12);
            
            try { TaskExtensions.Then(taskT, (Action<int>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(taskT, (Func<int, string>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(taskT, (Func<int, Task>)null); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then(taskT, (Func<int, Task<int>>)null); Assert.Fail(); } catch(ArgumentNullException) { }

            Action action = new Mock<Action>().Object;
            Func<int> funcT = new Mock<Func<int>>().Object;
            Func<Task> funcTask = new Mock<Func<Task>>().Object;
            Func<Task<string>> funcTaskT = new Mock<Func<Task<string>>>().Object;

            try { TaskExtensions.Then((Task)null, action); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task)null, funcT); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task)null, funcTask); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task)null, funcTaskT); Assert.Fail(); } catch(ArgumentNullException) { }

            Action<int> actionT = new Mock<Action<int>>().Object;
            Func<int, string> funcT1T2 = new Mock<Func<int, string>>().Object;
            Func<int, Task> funcTTask = new Mock<Func<int, Task>>().Object;
            Func<int, Task<string>> funcTTaskT = new Mock<Func<int, Task<string>>>().Object;

            try { TaskExtensions.Then((Task<int>)null, actionT); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task<int>)null, funcT1T2); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task<int>)null, funcTTask); Assert.Fail(); } catch(ArgumentNullException) { }
            try { TaskExtensions.Then((Task<int>)null, funcTTaskT); Assert.Fail(); } catch(ArgumentNullException) { }

        }

        [Test]
        public void Task_Then_Action__Normal()
        {
            var mock = new Mock<IThen>();
            Task task = SimpleTaskFactory.Run(() => { /*do nothing*/})
                .Then(() => mock.Object.NextAction());
            
            task.Wait();

            mock.Verify(then => then.NextAction(), Times.Once);
        }

        [Test]
        public void Task_Then_Action__ExceptionInFirst()
        {
            var mock = new Mock<IThen>();
            var thrownException = new Exception();
            Task task = SimpleTaskFactory.Run(() => { throw thrownException; })
                .Then(() => mock.Object.NextAction());

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                mock.Verify(then => then.NextAction(), Times.Never);
                Assert.Pass();                
            }

        }

        [Test]
        public void Task_Then_Action__ExceptionInSecond()
        {
            var thrownException = new Exception();
            Task task = SimpleTaskFactory.Run(() => { })
                .Then(() =>
                {
                    throw thrownException;
                } );

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                Assert.Pass();                
            }

        }

        [Test]
        public void TaskT1_Then_ActionT1__Normal()
        {
            var mock = new Mock<IThen>();
            Task task = SimpleTaskFactory.Run(() => 12)
                .Then( result => mock.Object.NextActionWithInput(result));

            task.Wait();
            
            mock.Verify(then => then.NextActionWithInput(12), Times.Once);
        }

        [Test]
        public void TaskT1_Then_ActionT1__ExceptionInFirst()
        {
            var mock = new Mock<IThen>();
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                    {
                        throw thrownException;
                        #pragma warning disable 162 // for purpose of test
                        return 12;
                        #pragma warning restore 162
                    })
                .Then(result => mock.Object.NextActionWithInput(result));

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                mock.Verify(then => then.NextActionWithInput(It.IsAny<int>()), Times.Never);
                Assert.Pass();
            }
            
        }

        [Test]
        public void TaskT1_Then_ActionT1__ExceptionInSecond()
        {
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() => 12 )
                .Then(result =>
                {
                    throw thrownException;
                });

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                Assert.Pass();
            }
            
        }

        [Test]
        public void Task_Then_FuncT2__Normal()
        {
            var mock = new Mock<IThen>();
            Task<string> task = SimpleTaskFactory.Run(() => { })
                .Then(() => mock.Object.NextFunction());

            task.Wait();

            mock.Verify(then => then.NextFunction(), Times.Once);
        }

        [Test]
        public void Task_Then_FuncT2__Normal_ResultBubbling()
        {
            Task<string> task = SimpleTaskFactory.Run(() => { })
                .Then(() => "Coucou");

            task.Wait();
            Assert.AreEqual("Coucou", task.Result);
        }

        [Test]
        public void Task_Then_FuncT2__Exception()
        {
            var mock = new Mock<IThen>();
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
#pragma warning disable 162 // for purpose of test
                    return 12;
#pragma warning restore 162
                })
                .Then(result => mock.Object.NextFunction());

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                mock.Verify(then => then.NextFunction(), Times.Never);
                Assert.Pass();
            }
        }


        [Test]
        public void TaskT1_Then_FuncT2__Normal()
        {
            var mock = new Mock<IThen>();
            Task task = SimpleTaskFactory.Run(() => 12 )
                .Then(result => mock.Object.NextFunctionWithInput(result));

            task.Wait();

            mock.Verify(then => then.NextFunctionWithInput(12), Times.Once);
        }

        [Test]
        public void TaskT1_Then_FuncT2__ExceptionInFirst()
        {
            var mock = new Mock<IThen>();
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
                    #pragma warning disable 162 // for compiler, to find signature
                    return 12;
                    #pragma warning restore 162
                })
                .Then(result => mock.Object.NextFunctionWithInput(result));

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                mock.Verify(then => then.NextFunctionWithInput(It.IsAny<int>()), Times.Never);
                Assert.Pass();
            }
        }

        [Test]
        public void TaskT1_Then_FuncT2__ExceptionInSecond()
        {
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() => 12 )
                .Then(result =>
                {
                    throw thrownException;
                });

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the exception");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException, e.InnerException);
                Assert.Pass();
            }
        }

        // TODO TEST : 
        // - FOR TASK-UNWRAPPING OVERLOADS OF THEN
        // - WITH SPECIFIC SCHEDULER   (moq-able?)
        // - WITH CANCELLATION TOKEN
        // - CATCH
        // - FINALLY
    }
}
