using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NAsync;
using NUnit.Framework;

#pragma warning disable 162

namespace NAsyncTests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public class ThenDelegateTest
    {
        #region Util

        public class Syncer
        {
            private int mCounter;

            public void Step(int step)
            {
                while (true)
                {
                    int expectedPreviousStep = step - 1;
                    int previousStep = Interlocked.CompareExchange(ref mCounter, step, expectedPreviousStep);
                    if (expectedPreviousStep == previousStep)
                        return;

                    Thread.Sleep(10);
                }

            }
        }

        public class Ex1 : Exception
        {

        }

        public class Ex2 : Exception
        {

        }

        public interface IAfter
        {
            void NextAction();
            void NextActionWithInput(int resultOfFirst);
            string NextFunction();
            string NextFunctionWithInput(int resultOfFirst);
            void CatchExceptionHandler(Exception e);
            int CatchExceptionHandlerWithOutput(Exception e);
        }

        #endregion

        #region Then: Task then Action
        [Test]
        public void Task_Then_Action__Normal()
        {
            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() => { /*do nothing*/})
                .Then(() => mock.Object.NextAction());
            
            task.Wait();

            mock.Verify(then => then.NextAction(), Times.Once);
        }

        [Test]
        public void Task_Then_Action__ExceptionInFirst()
        {
            var mock = new Mock<IAfter>();
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
        public void Task_Then_Action__FirstCancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            
            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                }, token)
                .Then(() => mock.Object.NextAction(), token);

            syncer.Step(2); 
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<TaskCanceledException>(e.InnerException);
                mock.Verify(then => then.NextAction(), Times.Never);
                Assert.Pass();
            }
        }

        [Test]
        public void Task_Then_Action__SecondCancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            
            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() =>
                {
                    // do nothing
                }, token)
                .Then(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                    mock.Object.NextAction();
                }, token);

            syncer.Step(2); 
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.NextAction(), Times.Never);
                Assert.Pass();
            }
        }

        #endregion

        #region Then: Task then FuncT2
        [Test]
        public void Task_Then_FuncT2__Normal()
        {
            var mock = new Mock<IAfter>();
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
        public void Task_Then_FuncT2__ExceptionInFirst()
        {
            var mock = new Mock<IAfter>();
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
                })
                .Then( () => mock.Object.NextFunction());

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
        public void Task_Then_FuncT2__ExceptionInSecond()
        {
            var thrownException = new Exception();
            Task task = SimpleTaskFactory.Run(() => { })
                .Then(() =>
                    {
                        throw thrownException;
                        return default(string);
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
        public void Task_Then_FuncT2__Cancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() =>
            {
                syncer.Step(1);
                syncer.Step(4);
                token.ThrowIfCancellationRequested();
            }, token)
                .Then(() => mock.Object.NextFunction(), token);

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<TaskCanceledException>(e.InnerException);
                mock.Verify(then => then.NextFunction(), Times.Never);
                Assert.Pass();
            }
        }

        [Test]
        public void Task_Then_FuncT2__SecondCancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() =>
            {
                // do nothing
            }, token)
                .Then(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                    mock.Object.NextFunction();
                }, token);

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.NextFunction(), Times.Never);
                Assert.Pass();
            }
        }


        #endregion

        #region Then: TaskT1 then ActionT1
        [Test]
        public void TaskT1_Then_ActionT1__Normal()
        {
            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() => 12)
                .Then( result => mock.Object.NextActionWithInput(result));

            task.Wait();
            
            mock.Verify(then => then.NextActionWithInput(12), Times.Once);
        }

        [Test]
        public void TaskT1_Then_ActionT1__ExceptionInFirst()
        {
            var mock = new Mock<IAfter>();
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                    {
                        throw thrownException;
                        return 12;
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
        public void TaskT1_Then_ActionT1__Cancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() =>
            {
                syncer.Step(1);
                syncer.Step(4);
                token.ThrowIfCancellationRequested();
                return 10;
            }, token)
                .Then(result => mock.Object.NextActionWithInput(result), token);

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<TaskCanceledException>(e.InnerException);
                mock.Verify(then => then.NextActionWithInput(It.IsAny<int>()), Times.Never);
                Assert.Pass();
            }
        }


        [Test]
        public void TaskT1_Then_ActionT1__SecondCancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() => 12, token)
                .Then(result =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                    mock.Object.NextActionWithInput(result);
                }, token);

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.NextFunction(), Times.Never);
                Assert.Pass();
            }
        }

        #endregion

        #region Then: TaskT1 then FuncT1T2
        [Test]
        public void TaskT1_Then_FuncT1T2__Normal()
        {
            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() => 12 )
                .Then(result => mock.Object.NextFunctionWithInput(result));

            task.Wait();

            mock.Verify(then => then.NextFunctionWithInput(12), Times.Once);
        }

        [Test]
        public void TaskT1_Then_FuncT1T2__ExceptionInFirst()
        {
            var mock = new Mock<IAfter>();
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
                    return 12;
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
        public void TaskT1_Then_FuncT1T2__ExceptionInSecond()
        {
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() => 12 )
                .Then(result =>
                {
                    throw thrownException;
                    return 12;
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
        public void TaskT1_Then_FuncT1T2__Cancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task<string> task = SimpleTaskFactory.Run(() =>
            {
                syncer.Step(1);
                syncer.Step(4);
                token.ThrowIfCancellationRequested();
                return 10;
            }, token)
                .Then(result => mock.Object.NextFunctionWithInput(result), token);

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<TaskCanceledException>(e.InnerException);
                mock.Verify(then => then.NextFunctionWithInput(It.IsAny<int>()), Times.Never);
                Assert.Pass();
            }
        }

        [Test]
        public void TaskT1_Then_FuncT1T2__SecondCancelled()
        {
            var source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var syncer = new Syncer();

            var mock = new Mock<IAfter>();
            Task task = SimpleTaskFactory.Run(() => 12, token)
                .Then(result =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                    mock.Object.NextFunctionWithInput(result);
                }, token);

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                // ReSharper disable once MethodSupportsCancellation
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.NextFunctionWithInput(It.IsAny<int>()), Times.Never);
                Assert.Pass();
            }
        }


        #endregion
    }
}
