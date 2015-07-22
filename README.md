Status:

[![Build status](https://ci.appveyor.com/api/projects/status/4sfb5vb121qcktmi/branch/master?svg=true)](https://ci.appveyor.com/project/jeromerg/nasync/branch/master)

Current Unit Test Code Coverage (2015-07-22): 86%

NAsync
======

Lightweight Extension to C# Task: Add `Then(...)`, `Catch<E>(...)`, and `Finally(...)` methods, to easily chain tasks together and handle errors. 

It provides very similar syntax to `async`/`await`. As such, it is the ideal solution for .Net < 4.5 projects, that want to be prepared to a future upgrade to `async`/`await`. Remark: in 4.0 projects, you can already use the `async`/`await` feature by adding the `Microsoft.Bcl.Async` dependency. 



The implementation is inspired by:

- Stephen Toub's blog (Microsoft): [http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx](http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx)

- javascript kriskowal `$q` promise: [https://github.com/kriskowal/q](https://github.com/kriskowal/q)

- java `jdeferred` framework [https://github.com/jdeferred/jdeferred](https://github.com/jdeferred/jdeferred)


Installation
------------

### Nuget

      Install-Package NAsync
      
see https://www.nuget.org/packages/NAsync/

### Manually

Just add the single file `TaskExtensions.cs` to your solution. 

Usage
-----

Typical usage looks like:

```C#
bool isProcessing;

public Task InitAsync() { ... }
public Task<int> GetValueAsync() { ... }

public void OnUserAction() {		
	isProcessing = true;
    InitAsync()
		.Then(() => GetValueAsync())                                 // chain with another Task 
		.Then(value => Console.WriteLine("The Value is " + value))   // chain with callback action/func
		.Catch<Exception>( e => Console.WriteLine("Error!! " + e))
		.Finally( () => isProcessing = false);		
}
```

Remark: you would write with the `async`/`await` syntax:

```C#
bool isProcessing;

public async Task InitAsync() { ... }
public async Task<int> GetValueAsync() { ... }

public void OnUserAction() {		
	isProcessing = true;
    
	try 
	{ 
	    await InitAsync();
		int value = await GetValueAsync();
		Console.WriteLine("The value is " + value));
	}
	catch(Exception e) 
	{
		Console.WriteLine("Error!! " + e);
	}
	finally 
	{
		isProcessing = false;
	}
}
```

There are 4 control flow statements:

- `.Then(...)`
    - Used to chain tasks, in the positive case, where the first task finishes properly
    - The callback `...` is typically an Action or a task factory. In the first case, the action is wrapped into a task.
- `.Catch<E>(...)` 
    - Used to handle exception
- `.OnCancelled(...)`
    - Used to handle cancelled task
- `.Finally(...)`
    - Used to execute a callback in any case

All signatures have the following optional argument `[CanBeNull] TaskScheduler taskScheduler = null`. If a TaskScheduler is provided, then it is used to call the callback `...`. If the TaskScheduler is omitted, then the original TaskScheduler of the original thread is used. If the original Thread is not bound to a TaskScheduler, then the callback is called on the same thread as the finishing task (similar behavior to async/await).

All `.Then(...)`extension methods have an overload where you can provide a CancellationToken, to enable cancelling the chaining, even if the first Task didn't handled the token.

Remaining Todos
---------------

- Implement overload of `.OnCancelled(...)` and `.Catch<E>(...)` accepting a CancellationToken
- Unit Tests for `.OnCancelled(...)`
- Unit Tests to ensure that exceptions are systematically observed 
