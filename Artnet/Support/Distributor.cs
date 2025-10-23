namespace Artnet.Support;

/// <summary>
/// A utility class for pre-allocating some resources and firing off
/// one or multiple workers to use those resources whenever one becomes
/// available.
/// This is useful to avoid allocations and the delays that entails,
/// during runtime while still being able to run a somewhat arbitrary
/// amount of workers.
/// Note that the Distributor does not deal with unmanaged resources;
/// IDisposable implementations will not have their `Dispose` called.
/// Resources should be emptied/cleaned up before or after use to avoid
/// contamination. The Distributor will not do that.
/// </summary>
/// <param name="capacity">The amount of resources to allocate</param>
/// <param name="factory">Callback to generate a new resource</param>
/// <typeparam name="TResource">Resource to allocate & distribute</typeparam>
public class Distributor<TResource>(int capacity, Func<TResource> factory)
{
    /// <summary>
    /// Flag indicating whether the resource distributor is currently handing out
    /// resources & accepting returns of used ones.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Prevents the Distributor from being started and stopped at the same time,
    /// to avoid broken object state.
    /// </summary>
    private readonly object StartStopLock = new();

    /// <summary>
    /// Resources that are not currently in use by any worker.
    /// </summary>
    private readonly ConcurrentQueue<TResource> Resources = new();

    /// <summary>
    /// This will be set once the Resources Queue has changed; likely after
    /// an item was returned, or the Distributor was stopped.
    /// </summary>
    private readonly EventWaitHandle ChangeInAvailability = new(true, EventResetMode.AutoReset);

    /// <summary>
    /// Right before the start, may be used by the dependent component
    /// to ensure auxiliary shared resources are set up and ready to work.
    /// </summary>
    public event EventHandler? BeforeStart;
    /// <summary>
    /// Right after the end, may be used to deconstruct dependent components
    /// </summary>
    public event EventHandler? AfterStop;
    /// <summary>
    /// Once a resource becomes available, work may be done by handlers of this
    /// event. Handlers of this Event should not block too much, and are expected to
    /// use ReturnResource after they're done doing work.
    /// </summary>
    public event EventHandler<TResource>? ResourceAvailable;

    /// <summary>
    /// Prepare some resources, and start distributing resources to workers.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the Distributor was already started.</exception>
    public void Start()
    {
        lock (StartStopLock)
        {
            if (IsRunning)
                throw new InvalidOperationException("Cannot distribute twice.");
            IsRunning = true;
            BeforeStart?.Invoke(this, EventArgs.Empty);
            while (Resources.Count < capacity)
                Resources.Enqueue(factory());
            ChangeInAvailability.Set();
            Task.Run(BeginDistribution);
        }
    }

    /// <summary>
    /// Clear out all resources and stop Distributing them to workers.
    /// Workers still may hold some Resources for another bit, before
    /// returning them.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the Distributor was already stopped</exception>
    public void Stop()
    {
        lock (StartStopLock)
        {
            if (!IsRunning)
                throw new InvalidOperationException("Cannot stop twice.");
            try
            {
                IsRunning = false;
                Resources.Clear();
                ChangeInAvailability.Set();
                Resources.Clear();
            }
            finally
            {
                AfterStop?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Each Distributor instance has at most one of these running in the thread other than
    /// the starting thread. 
    /// </summary>
    private void BeginDistribution()
    {
        while (IsRunning)
            OccupyAllResources();
    }

    /// <summary>
    /// Blocks until Resources become available, and starts handing out resources to workers.
    /// When out of resources, will set ChangeInAvailability to Block, again.
    /// </summary>
    private void OccupyAllResources()
    {
        ChangeInAvailability.WaitOne();
        while (Resources.TryDequeue(out var resource))
        {
            ResourceAvailable?.Invoke(this, resource);
        }
    }

    /// <summary>
    /// Workers are expected to return their resources here after they're done.
    /// If they don't, the Distributor will halt. It is then recommended to call this
    /// from a try/finally.
    /// This does nothing for a Distributor that isn't running (anymore).
    /// </summary>
    /// <param name="resource">Resource to return. Will do nothing when null.</param>
    public void ReturnResource(TResource? resource)
    {
        if (!IsRunning || resource is null) return;
        lock (StartStopLock)
        {
            Resources.Enqueue(resource);
            ChangeInAvailability.Set();
        }
    }
}