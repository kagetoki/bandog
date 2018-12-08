namespace Bandog.Core.Domain

module internal CoreDomainImpl =
    open DomainTypes
    open CoreCommands

    let applyMusicProfileUpdate (profile : MusicProfile) update =
        match update with
        | AddProfileSkill (skill, level) -> { profile with Skills = profile.Skills.Add (skill, level) }
        | RemoveProfileSkill skill -> { profile with Skills = profile.Skills.Remove skill }
        | AddAudio audio -> { profile with Audios = profile.Audios.Add (audio.Id, audio) }
        | RemoveAudio audioId -> { profile with Audios = profile.Audios.Remove audioId }
        | AddDegree degree -> { profile with Degrees = profile.Degrees.Add degree }
        | RemoveDegree degree -> { profile with Degrees = profile.Degrees.Remove degree }
        | CollaborationStatus status -> { profile with CollaborationStatus = status }

    let applyMusicProfileUpdates profile updates = Seq.fold applyMusicProfileUpdate profile updates

    let applyBasicProfileUpdate (profile: BasicUserInfo) update =
        match update with
        | FullName name -> { profile with FullName = name }
        | UserName name -> { profile with Username = name }
        | BirthDate bd -> { profile with BirthDate = bd }
        | PictureId id -> { profile with PictureId = id }

    let applyBasicProfileUpdates profile updates = Seq.fold applyBasicProfileUpdate profile updates

    let applyAudioMetaUpdate (audio: AudioMeta) update =
        match update with
        | AddSkill skill -> { audio with AppliedSkills = audio.AppliedSkills.Add skill }
        | RemoveSkill skill -> { audio with AppliedSkills = audio.AppliedSkills.Remove skill }
        | AddGenre genre -> { audio with Genre = audio.Genre.Add genre }
        | RemoveGenre genre -> { audio with Genre = audio.Genre.Remove genre }
        | Duration d -> { audio with Duration = d }
        | Title title -> { audio with Title = title }

    let applyAudioMetaUpdates audio updates = Seq.fold applyAudioMetaUpdate audio updates
