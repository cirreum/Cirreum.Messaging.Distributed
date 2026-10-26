namespace Cirreum.Messaging;

/// <summary>
/// The subject on whose behalf a distributed message was published, as asserted by the
/// producer.
/// </summary>
/// <remarks>
/// Carried on <see cref="DistributedMessageEnvelope.Subject"/>. The values are the producer's
/// assertion and are verified only by the trust between producer and consumer on the
/// channel; a consumer decides whether to honour them.
/// </remarks>
/// <param name="Id">The subject's identifier.</param>
/// <param name="Name">The subject's name.</param>
/// <param name="Scheme">The authentication scheme that established the subject on the producer.</param>
/// <param name="Roles">The subject's roles on the producer.</param>
public sealed record DistributedMessageSubject(
	string Id,
	string Name,
	string Scheme,
	IReadOnlyList<string> Roles);
