namespace Bandog.Core.Domain

module DomainTypes =
    open System

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

    [<Struct>]
    type Ability =
        | Composition
        | Mastering
        | Recording
        | Arrangement
        | TextWriting

    [<Struct>]
    type Skill =
        | Instrument of instrument:Instrument
        | Ability of ability:Ability

    type SkillLevel =
        | Novice
        | Apprentice
        | Adept
        | Mage
        | ArchMage


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
    type BasicGenre =
        | Jazz
        | Blues
        | Rock
        | PowerMetal
        | DeathMetal
        | BlackMetal
        | HeavyMetal
        | NuMetal
        | MelodicMetal
        | Classic
        | Soul
        | Ambient
        | Pop
        | HipHop
        | Core
    and 
        [<Struct>]
        GenreAttributes =
        | Progressive
        | Neo
        | Fusion
        | Experimental

    and Genre = BasicGenre * GenreAttributes option

    type SchoolName = NonEmptyString

    type Discipline = NonEmptyString

    type Degree =
        | Bachelor of SchoolName * Discipline
        | Master of SchoolName * Discipline
        | PhD of SchoolName * Discipline

    type AudioMeta =
        { Id            : AudioId
          UserId        : UserId
          Title         : NonEmptyString
          Genre         : Genre NonEmptyList
          AppliedSkills : Skill NonEmptyList
          Duration      : TimeSpan }

    type BasicUserInfo =
        { Id        : UserId
          FullName  : LetterString
          Username  : LetterAndDigitString
          PictureId : PictureId
          BirthDate : BirthDate option }

    type UserContactInfo =
        { UserInfo : BasicUserInfo
          Email : Email
          Phone : PhoneNumber option }

    type MusicProfile =
        { UserInfo            : BasicUserInfo
          Skills              : Map<Skill, SkillLevel>
          Degrees             : Degree Set
          Audios              : Map<AudioId, AudioMeta>
          CollaborationStatus : CollaborationStatus }
