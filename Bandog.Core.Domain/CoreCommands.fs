namespace Bandog.Core.Domain

module CoreCommands =
    open System
    open DomainTypes

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

    type UserCreateCommandPayload =
        { Email : Email
          FullName : LetterString 
          Username : LetterAndDigitString option 
          BirthDate : BirthDate option }

    type UserCreateCommand = Command<UserCreateCommandPayload>

    type BasicUserProfileUpdate =
        | FullName of LetterString
        | PictureId of Guid
        | BirthDate of BirthDate
        | UserName of LetterAndDigitString

    type UpdateBasicUserProfileCommand = UpdateCommand<UserId, BasicUserProfileUpdate>

    type UserContactInfoUpdate =
        | Email of Email
        | Phone of PhoneNumber

    type UpdateContactInfoCommand = UpdateCommand<UserId, UserContactInfoUpdate>

    type AddAudioMetaCommand = AudioMeta

    type AudioMetaUpdate =
        | Duration of TimeSpan
        | Title of NonEmptyString
        | AddSkill of Skill
        | RemoveSkill of Skill
        | AddGenre of Genre
        | RemoveGenre of Genre

    type UpdateAudioMetaCommand = UpdateCommand<AudioId, AudioMetaUpdate>

    type AddMusicProfileCommand =
        { Id : UserId 
          Skills : Map<Skill, SkillLevel>
          Degrees : Degree Set
          CollaborationStatus : CollaborationStatus }

    type MusicProfileUpdate =
        | AddProfileSkill of Skill * SkillLevel
        | RemoveProfileSkill of Skill
        | CollaborationStatus of CollaborationStatus
        | AddDegree of Degree
        | RemoveDegree of Degree
        | AddAudio of AudioMeta
        | RemoveAudio of AudioId

    type UpdateMusicProfileCommand = UpdateCommand<UserId, MusicProfileUpdate>
