﻿namespace Bandog.Core.Domain

module CoreCommands =
    open System
    open DomainTypes
    open Bandog.Common

    type CommandMeta =
        { OperationId : Guid
          InitiatorId : UserId
          CommandId : Guid
          TimeStamp : DateTimeOffset }

    type Command<'Payload> =
        { Meta : CommandMeta
          Payload : 'Payload }

    type UpdateCommandPayload<'AggregateId, 'Update> =
        { AggregateId : 'AggregateId 
          Payload : 'Update NonEmptyList }

    type UpdateCommand<'AggregateId, 'Update> = Command<UpdateCommandPayload<'AggregateId, 'Update>>

    type AddUserCommandPayload =
        { Email : Email
          FullName : LetterString 
          Username : LetterAndDigitString option 
          BirthDate : BirthDate option }

    type AddUserCommand = Command<AddUserCommandPayload>

    type BasicUserProfileUpdate =
        | FullName of LetterString
        | PictureId of Guid option
        | BirthDate of BirthDate option
        | UserName of LetterAndDigitString

    type UpdateBasicUserProfileCommand = UpdateCommand<UserId, BasicUserProfileUpdate>

    type UserContactInfoUpdate =
        | Email of Email
        | Phone of PhoneNumber

    type UpdateContactInfoCommand = UpdateCommand<UserId, UserContactInfoUpdate>

    type AddAudioMetaCommand = Command<AudioMeta>

    type AudioMetaUpdate =
        | Duration of TimeSpan
        | Title of NonEmptyString
        | AddSkill of Skill
        | RemoveSkill of Skill
        | AddGenre of Genre
        | RemoveGenre of Genre

    type UpdateAudioMetaCommand = UpdateCommand<AudioId, AudioMetaUpdate>

    type AddMusicProfileCommandPayload =
        { Id : UserId 
          Skills : Map<Skill, SkillLevel>
          Degrees : Degree Set
          CollaborationStatus : CollaborationStatus }

    type AddMusicProfileCommand = Command<AddMusicProfileCommandPayload>

    type MusicProfileUpdate =
        | AddProfileSkill of Skill * SkillLevel
        | RemoveProfileSkill of Skill
        | CollaborationStatus of CollaborationStatus
        | AddDegree of Degree
        | RemoveDegree of Degree
        | AddAudio of AudioMeta
        | RemoveAudio of AudioId

    type UpdateMusicProfileCommand = UpdateCommand<UserId, MusicProfileUpdate>
