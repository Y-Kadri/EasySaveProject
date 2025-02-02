using System;

namespace EasySave_Project.Service
{
    /// <summary>
    /// The abstract base class for backup job strategies.
    /// This class serves as a foundation for specific job types, such as 
    /// complete and differential backup strategies.
    /// </summary>
    public abstract class AJobStrategyService
    {
        /// <summary>
        /// Initializes a new instance of the AJob class.
        /// This constructor is protected to prevent direct instantiation of AJob.
        /// </summary>
        protected AJobStrategyService() { }
    }
}
