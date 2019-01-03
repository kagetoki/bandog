namespace Bandog.Core.Domain

open System
open System.Collections.Generic
open Bandog.Common

module ValueObjectsDto =

    [<CLIMutable>]
    type LocationDto = 
        { Country  : string
          Disctict : string
          City     : string }

    type DegreeLevel = | Bachelor = 0 | Master = 1 | PhD = 2

    [<CLIMutable>]
    type DegreeDto =
        { SchoolName : string
          Discipline : string
          DegreeLevel : DegreeLevel }

    [<CLIMutable>]
    type GenreDto =
        { Genre : string
          GenreKind : string }

    type SkillKind = Instrument = 0 | MusicMaking = 1

    [<CLIMutable>]
    type SkillDto =
        { Skill : string
          InstrumentKind : string
          SkillKind : SkillKind }

    type AvailabilityHoursDto = Morning = 0 | Noon = 1 | Evening = 2 | Any = 3
    type AvailabilityDaysDto = Weekday = 0 | Weekend = 1 | Any = 2

    [<CLIMutable>]
    type CollaborationStatusDto =
        { Hours : AvailabilityHoursDto nil
          Days : AvailabilityDaysDto nil
          IsOpenToAnyCollaboration : bool
          IsUnavailable : bool }

module CoreQueryDto =
    open ValueObjectsDto

    [<CLIMutable>]
    type BasicUserInfoDto =
        { Id        : Guid
          FullName  : string
          Username  : string
          PictureId : Guid nil
          Location  : LocationDto
          Joined    : DateTimeOffset
          BirthDate : DateTimeOffset nil }

    [<CLIMutable>]
    type AudioMetaDto =
        { Id            : Guid
          UserId        : Guid
          Title         : string
          Genre         : GenreDto []
          AppliedSkills : SkillDto []
          Duration      : TimeSpan }

    [<CLIMutable>]
    type MusicProfileDto =
        { UserInfo            : BasicUserInfoDto
          Skills              : Dictionary<SkillDto, int>
          Degrees             : DegreeDto[]
          Genres              : GenreDto []
          Audios              : Dictionary<Guid, AudioMetaDto>
          CollaborationStatus : CollaborationStatusDto }

module CoreCommandDto =
    open ValueObjectsDto

    [<CLIMutable>]
    type CommandMetaDto =
        { OperationId : Guid
          InitiatorId : string
          CommandId : Guid
          TimeStamp : DateTimeOffset }

    [<CLIMutable>]
    type CommandDto<'Payload> =
        { Meta : CommandMetaDto
          Payload : 'Payload }

    [<CLIMutable>]
    type AddUserCommandPayloadDto =
        { Email : string
          FullName : string 
          Username : string
          BirthDate : DateTimeOffset nil }

    type AddUserCommandDto = CommandDto<AddUserCommandPayloadDto>

    type UserCreateCommandPayloadDto =
        { Email : string
          FullName : string 
          Username : string 
          BirthDate : DateTimeOffset nil }

    type UserCreateCommandDto = CommandDto<UserCreateCommandPayloadDto>

    type UpdateBasicProfilePayloadDto =
        { Id : Guid
          FullName : string
          PictureId : Guid nil
          BirthDate : DateTimeOffset nil
          Username : string }

    type BasicProfileUpdateCommandDto = CommandDto<UpdateBasicProfilePayloadDto>

    type AddAudioMetaCommandDto = CommandDto<CoreQueryDto.AudioMetaDto>

    type AudioMetaUpdatePayloadDto =
        { Duration : TimeSpan
          Title : string
          AddSkill : SkillDto
          RemoveSkill : SkillDto 
          AddGenre : GenreDto
          RemoveGenre : GenreDto }

    type UpdateAudoMetaCommandDto = CommandDto<int>
