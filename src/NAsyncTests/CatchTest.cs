using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
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
    public class CatchTest
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

        #region Catch on Task
        [Test]
        public void Task_Catch__Return()
        {
            var mock = new Mock<IAfter>();
            var thrownException = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
                })
                .Catch<Exception>(exception => mock.Object.CatchExceptionHandler(exception));

            task.Wait();

            mock.Verify(then => then.CatchExceptionHandler(thrownException), Times.Once);

        }

        [Test]
        public void Task_Catch__ThrowAgain()
        {
            var thrownException1 = new Exception();
            var thrownException2 = new Exception();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException1;
                })
                .Catch<Exception>(exception => { throw thrownException2; });

            try
            {
                task.Wait();
                Assert.Fail("thrownException2 not propagated");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException2, e.InnerException);
                Assert.Pass();
            }
        }

        [Test]
        public void Task_Catch__ExceptionDoesntMatch()
        {
            var mock = new Mock<IAfter>();

            var thrownException = new Ex1();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
                })
                .Catch<Ex2>(exception => mock.Object.CatchExceptionHandler(exception));

            try
            {
                task.Wait();
                Assert.Fail("thrownException not propagated");
            }
            catch (AggregateException e)
            {
                mock.Verify(then => then.CatchExceptionHandler(It.IsAny<Exception>()), Times.Never);
                Assert.AreEqual(thrownException, e.InnerException);
                Assert.Pass();
            }
        }

        [Test]
        public void Task_2Catch__FirstDoesntMatch_SecondMatches()
        {
            var mock1 = new Mock<IAfter>();
            var mock2 = new Mock<IAfter>();

            var ex1 = new Ex1();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw ex1;
                })
                .Catch<Ex2>(exception => mock1.Object.CatchExceptionHandler(exception))
                .Catch<Ex1>(exception => mock2.Object.CatchExceptionHandler(exception));

            task.Wait();
            
            mock1.Verify(then => then.CatchExceptionHandler(It.IsAny<Exception>()), Times.Never);
            mock2.Verify(then => then.CatchExceptionHandler(ex1), Times.Once);
            Assert.Pass();            
        }

        [Test]
        public void Task_2Catch__FirstMatches_SecondDoesntMatch()
        {
            var mock1 = new Mock<IAfter>();
            var mock2 = new Mock<IAfter>();

            var ex1 = new Ex1();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw ex1;
                })
                .Catch<Ex1>(exception => mock1.Object.CatchExceptionHandler(exception))
                .Catch<Ex2>(exception => mock2.Object.CatchExceptionHandler(exception));

            task.Wait();

            mock1.Verify(then => then.CatchExceptionHandler(ex1), Times.Once);
            mock2.Verify(then => then.CatchExceptionHandler(It.IsAny<Exception>()), Times.Never);
            Assert.Pass();
        }

        [Test]
        public void Task_2Catch__FirstMatchesAndRethrow_SecondMatches()
        {
            var mock1 = new Mock<IAfter>();
            var mock2 = new Mock<IAfter>();

            var ex1 = new Ex1();
            var ex2 = new Ex2();

            mock1.Setup(then => then.CatchExceptionHandler(ex1)).Throws(ex2);

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    throw ex1;
                })
                .Catch<Ex1>(exception => mock1.Object.CatchExceptionHandler(exception))
                .Catch<Ex2>(exception => mock2.Object.CatchExceptionHandler(exception));

            task.Wait();
            mock1.Verify(then => then.CatchExceptionHandler(ex1), Times.Once);
            mock2.Verify(then => then.CatchExceptionHandler(ex2), Times.Once);
            Assert.Pass();
        }

        [Test]
        public void Task_CatchSpecificException__FirstCancelled()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var syncer = new Syncer();

            var mock = new Mock<IAfter>();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                }, token)
                .Catch<Ex1>(exception => mock.Object.CatchExceptionHandler(exception));

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.CatchExceptionHandler(It.IsAny<Exception>()), Times.Never);
                Assert.Pass();
            }

        }

        [Test]
        public void Task_CatchAllException__FirstCancelled()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var syncer = new Syncer();

            var mock = new Mock<IAfter>();

            Task task = SimpleTaskFactory
                .Run(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                }, token)
                .Catch<Exception>(exception => mock.Object.CatchExceptionHandler(exception));

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                // REMARK!!! The Catch doesn't cancel the OperationCanceledException!
                mock.Verify(then => then.CatchExceptionHandlerWithOutput(It.IsAny<Exception>()), Times.Never);
                Assert.Pass();
            }
        }


        #endregion

        #region Catch on TaskT
        [Test]
        public void TaskT_Catch__Return()
        {
            var mock = new Mock<IAfter>();
            var thrownException = new Exception();

            mock.Setup(o => o.CatchExceptionHandlerWithOutput(thrownException)).Returns(123);

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
                    return default(int);
                })
                .Catch<int, Exception>(exception => mock.Object.CatchExceptionHandlerWithOutput(exception));

            task.Wait();

            mock.Verify(then => then.CatchExceptionHandlerWithOutput(thrownException), Times.Once);
            Assert.AreEqual(123, task.Result);
        }


        [Test]
        public void TaskT_Catch__ThrowAgain()
        {
            var mock = new Mock<IAfter>();

            var thrownException1 = new Exception();
            var thrownException2 = new Exception();

            mock.Setup(o => o.CatchExceptionHandlerWithOutput(thrownException1)).Throws(thrownException2);

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException1;
                    return default(int);
                })
                .Catch<int, Exception>(exception => { throw thrownException2; });

            try
            {
                task.Wait();
                Assert.Fail("thrownException2 not propagated");
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(thrownException2, e.InnerException);
                Assert.Pass();
            }
        }

        [Test]
        public void TaskT_Catch__ExceptionDoesntMatch()
        {
            var mock = new Mock<IAfter>();

            var thrownException = new Ex1();

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    throw thrownException;
                    return 12;
                })
                .Catch<int, Ex2>(exception => mock.Object.CatchExceptionHandlerWithOutput(exception));

            try
            {
                task.Wait();
                Assert.Fail("thrownException not propagated");
            }
            catch (AggregateException e)
            {
                mock.Verify(then => then.CatchExceptionHandlerWithOutput(It.IsAny<Exception>()), Times.Never);
                Assert.AreEqual(thrownException, e.InnerException);
                Assert.Pass();
            }
        }

        [Test]
        public void TaskT_2Catch__FirstDoesntMatch_SecondMatches()
        {
            var mock1 = new Mock<IAfter>();
            var mock2 = new Mock<IAfter>();

            var ex1 = new Ex1();

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    throw ex1;
                    return default(int);
                })
                .Catch<int, Ex2>(exception => mock1.Object.CatchExceptionHandlerWithOutput(exception))
                .Catch<int, Ex1>(exception => mock2.Object.CatchExceptionHandlerWithOutput(exception));

            task.Wait();

            mock1.Verify(then => then.CatchExceptionHandlerWithOutput(It.IsAny<Exception>()), Times.Never);
            mock2.Verify(then => then.CatchExceptionHandlerWithOutput(ex1), Times.Once);
            Assert.Pass();
        }

        [Test]
        public void TaskT_2Catch__FirstMatches_SecondDoesntMatch()
        {
            var mock1 = new Mock<IAfter>();
            var mock2 = new Mock<IAfter>();

            var ex1 = new Ex1();

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    throw ex1; 
                    return default(int);
                })
                .Catch<int, Ex1>(exception => mock1.Object.CatchExceptionHandlerWithOutput(exception))
                .Catch<int, Ex2>(exception => mock2.Object.CatchExceptionHandlerWithOutput(exception));

            task.Wait();

            mock1.Verify(then => then.CatchExceptionHandlerWithOutput(ex1), Times.Once);
            mock2.Verify(then => then.CatchExceptionHandlerWithOutput(It.IsAny<Exception>()), Times.Never);
            Assert.Pass();
        }

        [Test]
        public void TaskT_2Catch__FirstMatchesAndRethrow_SecondMatches()
        {
            var mock1 = new Mock<IAfter>();
            var mock2 = new Mock<IAfter>();

            var ex1 = new Ex1();
            var ex2 = new Ex2();

            mock1.Setup(then => then.CatchExceptionHandlerWithOutput(ex1)).Throws(ex2);

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    throw ex1;
                    return default(int);
                })
                .Catch<int, Ex1>(exception => mock1.Object.CatchExceptionHandlerWithOutput(exception))
                .Catch<int, Ex2>(exception => mock2.Object.CatchExceptionHandlerWithOutput(exception));

            task.Wait();
            mock1.Verify(then => then.CatchExceptionHandlerWithOutput(ex1), Times.Once);
            mock2.Verify(then => then.CatchExceptionHandlerWithOutput(ex2), Times.Once);
            Assert.Pass();
        }

        [Test]
        public void TaskT_CatchSpecificException__FirstCancelled()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var syncer = new Syncer();

            var mock = new Mock<IAfter>();

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                    return 12; // T
                }, token)
                .Catch<int, Ex1>(exception => mock.Object.CatchExceptionHandlerWithOutput(exception));

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                mock.Verify(then => then.CatchExceptionHandlerWithOutput(It.IsAny<Exception>()), Times.Never);
                Assert.Pass();
            }

        }

        [Test]
        public void TaskT_CatchAllException__FirstCancelled()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var syncer = new Syncer();

            var mock = new Mock<IAfter>();

            Task<int> task = SimpleTaskFactory
                .Run(() =>
                {
                    syncer.Step(1);
                    syncer.Step(4);
                    token.ThrowIfCancellationRequested();
                    return 12; // T
                }, token)
                .Catch<int, Exception>(exception => mock.Object.CatchExceptionHandlerWithOutput(exception));

            syncer.Step(2);
            source.Cancel();
            syncer.Step(3);

            try
            {
                task.Wait();
                Assert.Fail("task must bubble up the TaskCanceledException");
            }
            catch (AggregateException e)
            {
                Assert.IsInstanceOf<OperationCanceledException>(e.InnerException);
                // REMARK!!! The Catch doesn't cancel the OperationCanceledException!
                mock.Verify(then => then.CatchExceptionHandlerWithOutput(It.IsAny<Exception>()), Times.Never);
                Assert.Pass();
            }


        }

        #endregion

    }
}
