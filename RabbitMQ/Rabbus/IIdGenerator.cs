namespace Rabbus
{
    /// <summary>
    /// Generates unique identifiers for outgoing messages
    /// </summary>
    public interface IIdGenerator
    {
        /// <summary>
        /// Generates the next id to assign to a message being published
        /// </summary>
        /// <returns>The generated unique identifier</returns>
        RabbusGuid Next();
    }
}