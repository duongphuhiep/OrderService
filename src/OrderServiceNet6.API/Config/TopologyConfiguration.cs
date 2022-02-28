namespace OrderService.API.Config
{
    /// <summary>
    /// https://masstransit-project.com/advanced/middleware/killswitch.html
    /// </summary>
    public class KillSwitchConfiguration
    {
        /// <summary>
        /// The time window for tracking exceptions
        /// </summary>
        public int? TrackingPeriod { get; set; } = 3600 * 1000;
        /// <summary>
        /// The number of messages that must be consumed before the kill switch activates.
        /// </summary>
        public int? ActivationThreshold { get; set; } = 1000;
        /// <summary>
        /// The percentage of failed messages that triggers the kill switch
        /// </summary>
        public int? TripThreshold { get; set; } = 90;
        /// <summary>
        /// The wait time (mili-second) before restarting the receive endpoint
        /// </summary>
        public long? RestartTimeout { get; set; } = 5 * 60 * 1000;
    }

    /// <summary>
    /// https://masstransit-project.com/advanced/middleware/circuit-breaker.html
    /// </summary>
    public class CircuitBreakerConfiguration
    {
        /// <summary>
        /// The window of time before the success/failure counts are reset to zero. This is typically set to around one minute, but can be as high as necessary. More than ten seems really strange to me.
        /// </summary>
        public int? TrackingPeriod { get; set; } = 30000;
        /// <summary>
        /// This is a percentage, and is based on the ratio of successful to failed attempts. When set to 15, if the ratio exceeds 15%, the circuit breaker opens and remains open until the ResetInterval expires.
        /// </summary>
        public int? TripThreshold { get; set; } = 10;
        /// <summary>
        /// This is the number of messages that must reach the circuit breaker in a tracking period before the circuit breaker can trip. If set to 10, the trip threshold is not evaluated until at least 10 messages have been received.
        /// </summary>
        public int? ActiveThreshold { get; set; } = 10;
        /// <summary>
        /// The period of time between the circuit breaker trip and the first attempt to close the circuit breaker. Messages that reach the circuit breaker during the open period will immediately fail with the same exception that tripped the circuit breaker.
        /// </summary>
        public int? ResetInterval { get; set; } = 1000;
    }

    public class TopologyConfiguration
    {
        /// <summary>
        /// Time to keep messages in the error queues
        /// </summary>
        public long? ErrorsTtl { get; set; } = 60 * 1000;
        /// <summary>
        /// Time to keep messages in the skipped queues
        /// </summary>
        public long? DeadLettersTtl { get; set; } = 3600 * 1000 * 24;

        /// <summary>
        /// A Kill Switch is used to prevent failing consumers from moving all the messages from the input queue to the error queue. By monitoring message consumption and tracking message successes and failures, a Kill Switch stops the receive endpoint when a trip threshold has been reached.
        /// Typically, consumer exceptions are transient issues and suspending consumption until a later time when the transient issue may have been resolved.
        /// </summary>
        public KillSwitchConfiguration KillSwitch { get; set; }
        /// <summary>
        /// A circuit breaker is used to protect resources (remote, local, or otherwise) from being overloaded when in a failure state. For example, a remote web site may be unavailable and calling that web site in a message consumer takes 30-60 seconds to time out. By continuing to call the failing service, the service may be unable to recover. A circuit breaker detects the repeated failures and trips, preventing further calls to the service and giving it time to recover. Once the reset interval expires, calls are slowly allowed back to the service. If it is still failing, the breaker remains open, and the timeout interval resets. Once the service returns to healthy, calls flow normally as the breaker closes.
        /// </summary>
        public CircuitBreakerConfiguration CircuitBreaker { get; set; }

    }
}
