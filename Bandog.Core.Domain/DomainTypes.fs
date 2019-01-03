namespace Bandog.Core.Domain

module DomainTypes =
    open System
    open Bandog.Common

    [<Struct>]
    type UserId = 
        | System
        | UserId of Guid
        | Anonymous
        with
        static member tryParse str =
            match str with
            | "" | null -> None
            | str when str.ToLower() = "system" -> Some System
            | str ->
                match Guid.TryParse str with
                | (true, guid) when guid = Guid.Empty -> Some Anonymous
                | (true, guid) -> UserId guid |> Some
                | _ -> None

    type PictureId = Guid

    type AudioId = Guid

    [<Struct>]
    type InstrumentKind =
        | Electric
        | Acoustic

    [<Struct>]
    type Instrument =
        | Guitar of guitar: InstrumentKind
        | Keyboard
        | Drums
        | Bass of bass: InstrumentKind
        | Violin
        | Cello
        | Flute
        | Clarinet
        | Sax
        | Trombone
        | Vocals

    [<Struct>]
    type MusicMakingSkill =
        | Composition
        | Mastering
        | Recording
        | Arrangement
        | TextWriting
        | Improvising

    [<Struct>]
    type Skill =
        | Instrument of instrument:Instrument
        | MusicMakingSkill of musicMakingSkill:MusicMakingSkill

    [<Struct>]
    type SkillLevel =
        | Novice
        | Apprentice
        | Adept
        | Maester
        | GrandMaester

    type AvailabilitySchedule =
        | Weekend of AvailabilityHours
        | WeekDay of AvailabilityHours
        | AnyDay of AvailabilityHours
    and AvailabilityHours =
        | Morning
        | Noon
        | Evening
        | AnyTime

    type CollaborationStatus =
        | Unavailable
        | OneTime of AvailabilitySchedule
        | LongTerm of AvailabilitySchedule
        | OpenToAnyCollaboration

    [<Struct>]
    type MetalKind =
        | Progressive | Nu | Heavy | Melodic | Death | Black | Thrash | Speed | Experimental

    [<Struct>]
    type JazzKind =
        | Bebop | Hardbop | Fusion | Latin | Dark | Swing

    [<Struct>]
    type HipHopKind =
        | Rap | RnB | Trip

    [<Struct>]
    type ElectroKind =
        | Trance | House | Techno

    [<Struct>]
    type RockKind =
        | Hard | Britpop | Indie | Alternative | Post | Punk | Grunge | Psychedelic | Surf

    [<Struct>]
    type Genre =
        | Jazz of jazz: JazzKind option
        | Blues
        | Rock
        | Metal of metal: MetalKind option
        | Classic
        | Soul
        | Ambient
        | Folk
        | Pop
        | HipHop of hiphop: HipHopKind option
        | Core
        | Electro of electro: ElectroKind option

    type SchoolName = NonEmptyString

    type Discipline = NonEmptyString

    type Degree =
        | Bachelor of SchoolName * Discipline
        | Master of SchoolName * Discipline
        | PhD of SchoolName * Discipline

    type Location =
        { Country : Country
          StateOrDistrict : NonEmptyString
          City : NonEmptyString }

    type AudioMeta =
        { Id            : AudioId
          UserId        : UserId
          Title         : NonEmptyString
          Genre         : Genre NonEmptySet
          AppliedSkills : Skill NonEmptySet
          Duration      : TimeSpan }

    type BasicUserInfo =
        { Id        : UserId
          FullName  : LetterString
          Username  : LetterAndDigitString
          PictureId : PictureId option
          Location  : Location
          Joined    : DateTimeOffset
          BirthDate : BirthDate option }

    type UserContactInfo =
        { UserInfo : BasicUserInfo
          Email : Email
          Phone : PhoneNumber option }

    type MusicProfile =
        { UserInfo            : BasicUserInfo
          Skills              : Map<Skill, SkillLevel>
          Degrees             : Degree Set
          Genres              : Genre Set
          Audios              : Map<AudioId, AudioMeta>
          CollaborationStatus : CollaborationStatus }
