namespace BlazorObservers.ObserverLibrary.Tasks
{
    /// <summary>
    /// Represents a task that includes additional options for configuration.
    /// </summary>
    /// <typeparam name="TData">The type of data provided to callback delegate on observer trigger.</typeparam>
    /// <typeparam name="TOptions">The type of options used to configure the task.</typeparam>
    public abstract class OptionedTask<TData, TOptions> : ObserverTask<TData>
    {
        /// <summary>
        /// Gets the options used to configure the task.
        /// </summary>
        public TOptions Options { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionedTask{TData, TOptions}"/> class.
        /// </summary>
        /// <param name="taskFunc">The function to execute as the task.</param>
        /// <param name="options">The options used to configure the task.</param>
        private protected OptionedTask(Func<TData, ValueTask> taskFunc, TOptions options) : base(taskFunc)
        {
            Options = options;
        }
    }
}
