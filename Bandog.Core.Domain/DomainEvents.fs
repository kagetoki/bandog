namespace Bandog.Core.Domain

module DomainEvents =
    open System
    open DomainTypes
    open CoreCommands

    type DomainEvent<'T> =
        { TimeStamp : DateTimeOffset
          InitiatorId : UserId
          Payload : 'T }

    type EntityUpdates<'Id, 'Update> =
        { EntityId : 'Id
          Updates : 'Update NonEmptyList }

    type UserCreated = DomainEvent<BasicUserInfo>

    type BasicUserInfoUpdated = DomainEvent<EntityUpdates<UserId, BasicUserProfileUpdate>>

    type AudioMetaCreated = DomainEvent<AudioMeta>

    type AudioMetaUpdated = DomainEvent<EntityUpdates<AudioId, AudioMetaUpdate>>


