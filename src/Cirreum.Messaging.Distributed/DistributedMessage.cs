namespace Cirreum.Messaging;

using Cirreum.Conductor;
using System.Text.Json.Serialization;

/// <summary>
/// Base record for messages distributed to external systems via Cirreum's distributed-
/// messaging channel. Inherits <see cref="INotification"/> (from <c>Cirreum.Kernel</c>)
/// so distributed messages flow through Conductor's publisher pipeline — apps publish
/// once via <c>IPublisher.PublishAsync(myMessage)</c> and the framework fans out to all
/// in-process handlers AND to the configured <see cref="IDistributedTransportPublisher{TBase}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Required attributes:</b> Every concrete subtype must be decorated with
/// <c>[MessageVersion(identifier, version)]</c> (from <c>Cirreum.Kernel</c>) to participate
/// in versioning and registry discovery. Optionally also decorate with
/// <see cref="DistributedMessageTargetAttribute"/> to specify within-channel routing
/// (queue vs topic); omitting it defaults to <see cref="MessageTarget.Topic"/>.
/// </para>
/// <para>
/// <b>Schema immutability:</b> Once a <c>(identifier, version)</c> pair is deployed, the
/// shape of the record must not change. Author a new record type with an incremented
/// version for any schema evolution. The version registry tracks both for the duration
/// of the migration window.
/// </para>
/// <para>
/// <b>Channel:</b> The channel this message belongs to is <c>DistributedMessage</c>
/// itself (the <c>TBase</c>). Apps that want a separate channel — e.g., for auth events,
/// telemetry, or domain-specific isolation — define their own base type or marker
/// interface, decorate with <c>[MessageVersion]</c>, and register a parallel
/// <c>IDistributedTransportPublisher&lt;TTheirBase&gt;</c> with its own transport
/// configuration.
/// </para>
/// </remarks>
public abstract record DistributedMessage : INotification {

	/// <summary>
	/// Whether this specific message instance prefers background (batched, asynchronous)
	/// delivery or immediate (synchronous) delivery. When <see langword="null"/>, the
	/// channel's default from <c>BackgroundDeliveryOptions.UseBackgroundDeliveryByDefault</c>
	/// applies.
	/// </summary>
	[JsonIgnore]
	public virtual bool? UseBackgroundDelivery { get; set; } = null;

	/// <summary>
	/// Priority for this message when using background delivery. Synchronous delivery
	/// bypasses prioritization.
	/// </summary>
	[JsonIgnore]
	public virtual DistributedMessagePriority Priority { get; set; } = DistributedMessagePriority.Standard;

}
