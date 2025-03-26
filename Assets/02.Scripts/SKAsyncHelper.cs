using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Helper class to convert asynchronous Tasks to Unity Coroutines
/// </summary>
public static class SKAsyncHelper
{
    /// <summary>
    /// Execute an async Task within a Coroutine
    /// </summary>
    /// <typeparam name="T">Return type of the task</typeparam>
    /// <param name="task">Task to execute</param>
    /// <param name="onSuccess">Callback with result on success</param>
    /// <param name="onError">Callback with exception on error</param>
    /// <returns>Coroutine enumerator</returns>
    public static IEnumerator RunTaskAsCoroutine<T>(Task<T> task, Action<T> onSuccess, Action<Exception> onError = null)
    {
        if (task == null)
        {
            onError?.Invoke(new ArgumentNullException(nameof(task)));
            yield break;
        }

        // Wait for task to complete
        while (!task.IsCompleted)
        {
            yield return null;
        }

        // Handle result based on task status
        if (task.IsFaulted)
        {
            Debug.LogError($"Task failed with exception: {task.Exception}");
            onError?.Invoke(task.Exception);
        }
        else if (task.IsCanceled)
        {
            Debug.LogWarning("Task was cancelled");
            onError?.Invoke(new TaskCanceledException(task));
        }
        else
        {
            onSuccess?.Invoke(task.Result);
        }
    }

    /// <summary>
    /// Execute an async Task with no return value within a Coroutine
    /// </summary>
    /// <param name="task">Task to execute</param>
    /// <param name="onComplete">Callback when completed successfully</param>
    /// <param name="onError">Callback with exception on error</param>
    /// <returns>Coroutine enumerator</returns>
    public static IEnumerator RunTaskAsCoroutine(Task task, Action onComplete, Action<Exception> onError = null)
    {
        if (task == null)
        {
            onError?.Invoke(new ArgumentNullException(nameof(task)));
            yield break;
        }

        // Wait for task to complete
        while (!task.IsCompleted)
        {
            yield return null;
        }

        // Handle result based on task status
        if (task.IsFaulted)
        {
            Debug.LogError($"Task failed with exception: {task.Exception}");
            onError?.Invoke(task.Exception);
        }
        else if (task.IsCanceled)
        {
            Debug.LogWarning("Task was cancelled");
            onError?.Invoke(new TaskCanceledException(task));
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Run asynchronous function wrapped in a Coroutine
    /// </summary>
    /// <typeparam name="T">Return type of the function</typeparam>
    /// <param name="asyncFunc">Async function to execute</param>
    /// <param name="onSuccess">Callback with result on success</param>
    /// <param name="onError">Callback with exception on error</param>
    /// <returns>Coroutine enumerator</returns>
    public static IEnumerator RunAsyncFuncAsCoroutine<T>(Func<Task<T>> asyncFunc, Action<T> onSuccess, Action<Exception> onError = null)
    {
        if (asyncFunc == null)
        {
            onError?.Invoke(new ArgumentNullException(nameof(asyncFunc)));
            yield break;
        }

        Task<T> task = null;

        try
        {
            task = asyncFunc();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start async function: {ex.Message}");
            onError?.Invoke(ex);
            yield break;
        }

        // Use existing helper to handle the task
        yield return RunTaskAsCoroutine(task, onSuccess, onError);
    }
}