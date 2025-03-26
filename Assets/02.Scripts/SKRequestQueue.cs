using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a queue of LLM requests to limit concurrent API calls
/// </summary>
public class SKRequestQueue : MonoBehaviour
{
    // Maximum concurrent requests to LLM API
    [SerializeField] private int _maxConcurrentRequests = 2;

    // Delay between starting consecutive requests in seconds
    [SerializeField] private float _requestDelaySeconds = 0.5f;

    // Queue of pending requests
    private Queue<RequestItem> _requestQueue = new Queue<RequestItem>();

    // Currently active requests count
    private int _activeRequestsCount = 0;

    // Structure to hold request info
    private class RequestItem
    {
        public Action ExecuteAction { get; set; }
        public Action<string> OnComplete { get; set; }
        public Action<string> OnError { get; set; }
    }

    private void Awake()
    {
        // Start the queue processor coroutine
        StartCoroutine(ProcessQueueCoroutine());
    }

    /// <summary>
    /// Add a request to the queue
    /// </summary>
    /// <param name="executeAction">Action to execute the request</param>
    /// <param name="onComplete">Callback when request completes successfully</param>
    /// <param name="onError">Callback when request fails</param>
    public void EnqueueRequest(Action executeAction, Action<string> onComplete, Action<string> onError = null)
    {
        if (executeAction == null)
        {
            Debug.LogError("SKRequestQueue: Cannot enqueue null execute action");
            return;
        }

        // Create new request item
        var request = new RequestItem
        {
            ExecuteAction = executeAction,
            OnComplete = onComplete,
            OnError = onError
        };

        // Add to queue
        _requestQueue.Enqueue(request);

        // Log queue status
        Debug.Log($"SKRequestQueue: Request added. Queue size: {_requestQueue.Count}, Active requests: {_activeRequestsCount}");
    }

    /// <summary>
    /// Process the request queue continuously
    /// </summary>
    private IEnumerator ProcessQueueCoroutine()
    {
        while (true)
        {
            // Check if we can process more requests
            if (_requestQueue.Count > 0 && _activeRequestsCount < _maxConcurrentRequests)
            {
                // Get next request
                RequestItem request = _requestQueue.Dequeue();

                // Increment active count
                _activeRequestsCount++;

                // Create wrapped callbacks to track completion
                Action<string> wrappedComplete = (result) => {
                    _activeRequestsCount--;
                    request.OnComplete?.Invoke(result);
                };

                Action<string> wrappedError = (error) => {
                    _activeRequestsCount--;
                    request.OnError?.Invoke(error);
                };

                // Start the request with wrapped callbacks
                try
                {
                    request.ExecuteAction.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"SKRequestQueue: Error executing request: {ex.Message}");
                    _activeRequestsCount--;
                    request.OnError?.Invoke($"Error starting request: {ex.Message}");
                }

                // Add delay between starting requests
                yield return new WaitForSeconds(_requestDelaySeconds);
            }
            else
            {
                // No requests to process, wait a frame
                yield return null;
            }
        }
    }

    /// <summary>
    /// Get the current queue status
    /// </summary>
    /// <returns>A tuple with queue size and active request count</returns>
    public (int QueueSize, int ActiveRequests) GetQueueStatus()
    {
        return (_requestQueue.Count, _activeRequestsCount);
    }

    /// <summary>
    /// Clear all pending requests in the queue
    /// </summary>
    /// <param name="notifyError">If true, notify error callbacks of cancellation</param>
    public void ClearQueue(bool notifyError = true)
    {
        int count = _requestQueue.Count;

        if (notifyError)
        {
            while (_requestQueue.Count > 0)
            {
                var request = _requestQueue.Dequeue();
                request.OnError?.Invoke("Request was cancelled (queue cleared)");
            }
        }
        else
        {
            _requestQueue.Clear();
        }

        Debug.Log($"SKRequestQueue: Cleared {count} pending requests");
    }
}