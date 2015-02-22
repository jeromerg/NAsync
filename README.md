NAsync
======

Lightweight Extension to C# Task: Add `Then(...)`, `Catch<E>(...)`, and `Finally(...)` methods, to easily chain tasks together and handle errors. 

It provides very similar syntax to `async`/`await`. As such, it is the ideal solution for .Net < 4.5 projects, that want to be prepared to a future upgrade to `async`/`await`. Remark: in 4.0 projects, you can already use the `async`/`await` feature by adding the `Microsoft.Bcl.Async` dependency. 



The implementation is inspired by:

- Stephen Toub's blog (Microsoft): [http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx](http://blogs.msdn.com/b/pfxteam/archive/2010/11/21/10094564.aspx)

- javascript kriskowal `$q` promise: [https://github.com/kriskowal/q](https://github.com/kriskowal/q)

- java `jdeferred` framework [https://github.com/jdeferred/jdeferred](https://github.com/jdeferred/jdeferred)


Usage
-----

Add the single file `TaskExtensions.cs` to your solution.It includes all the `Then(...)`, `Catch<E>(...)`, and `Finally(...)` extension methods.

From now on, you can use them as following:

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