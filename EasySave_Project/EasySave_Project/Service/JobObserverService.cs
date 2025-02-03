using EasySave_Project.Model;
using System;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Abstract class representing an observer for <see cref="JobModel"/>.
    /// Implements the <see cref="IObserver{T}"/> interface to observe changes in a JobModel.
    /// </summary>
    public abstract class JobObserverService : IObserver<JobModel>
    {
        /// <summary>
        /// Method called when a change is detected in a <see cref="JobModel"/>.
        /// This method must be implemented in derived classes.
        /// </summary>
        /// <param name="job">The instance of <see cref="JobModel"/> that has been modified.</param>
        public abstract void OnNext(JobModel job);

        /// <summary>
        /// Method called when the observable sequence completes.
        /// This implementation is optional and can be overridden if needed.
        /// </summary>
        public virtual void OnCompleted()
        {
            // Optional
        }

        /// <summary>
        /// Method called when an error occurs in the observable sequence.
        /// This implementation is optional and can be overridden if needed.
        /// </summary>
        /// <param name="error">The exception describing the error.</param>
        public virtual void OnError(Exception error)
        {
            // Optional
        }
    }
}
