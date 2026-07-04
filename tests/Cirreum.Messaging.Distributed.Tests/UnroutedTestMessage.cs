namespace Cirreum.Messaging.Distributed.Tests;

using Cirreum.Messaging;

/// <summary>
/// Test message with no target attribute — must default to topic routing.
/// </summary>
[MessageVersion("tests.unrouted", "2.1")]
public sealed record UnroutedTestMessage(int Number) : DistributedMessage;