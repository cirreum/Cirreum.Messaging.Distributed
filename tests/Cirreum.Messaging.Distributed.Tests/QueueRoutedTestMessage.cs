namespace Cirreum.Messaging.Distributed.Tests;

using Cirreum.Messaging;

/// <summary>
/// Queue-routed test message. Public so the Kernel's exported-type assembly scan
/// discovers it during registry initialization.
/// </summary>
[MessageVersion("tests.queue-routed", "1.0")]
[DistributedMessageTarget(MessageTarget.Queue)]
public sealed record QueueRoutedTestMessage(string Payload) : DistributedMessage;