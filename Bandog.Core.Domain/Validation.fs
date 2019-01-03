namespace Bandog.Core.Domain

module Validation =
    open System
    open Bandog.Common
    open Microsoft.FSharp.Reflection
    open Bandog.Core.Domain.ValueObjectsDto
    open CoreCommands
    open DomainTypes
    open CoreCommandDto

    let tryParseEmptyDUCase<'DU> str =
        if FSharpType.IsUnion typeof<'DU> |> not then None
        else
        match str with
        | null | "" -> None
        | str ->
            FSharpType.GetUnionCases typeof<'DU>
            |> Array.tryFind (fun c -> c.Name =~ str)
            |> Option.map (fun case -> FSharpValue.MakeUnion(case, [||]) :?> 'DU)

    let validateLocation (dto : LocationDto) =
        result {
            let! country = tryParseEmptyDUCase<Country> dto.Country |> Result.ofOption (InvalidInput "Unknown country")
            let! disctrict = NonEmptyString.create dto.Disctict
            let! city = NonEmptyString.create dto.City
            return 
                { Country = country
                  StateOrDistrict = disctrict
                  City = city }
        }

    let validateSkill (dto: SkillDto) =
        if dto.Skill |> isWhiteSpaceString then InvalidInput "skill can't be empty" |> Error
        else
        match dto.SkillKind, dto.InstrumentKind with
        | SkillKind.Instrument, kind when isEmptyString kind ->
            tryParseEmptyDUCase<Instrument> dto.Skill
            |> Result.ofOption (InvalidInput "unknown instrument")
            |> Result.map Instrument
        | SkillKind.Instrument, "electric" ->
            match dto.Skill.ToLower() with
            | "guitar" -> Guitar Electric |> Instrument |> Ok
            | "bass" -> Bass Electric |> Instrument |> Ok
            | _ -> InvalidInput "skill" |> Error
        | SkillKind.Instrument, "acoustic" ->
            match dto.Skill.ToLower() with
            | "guitar" -> Guitar Acoustic |> Instrument |> Ok
            | "bass" -> Bass Acoustic |> Instrument |> Ok
            | _ -> InvalidInput "skill" |> Error
        | SkillKind.MusicMaking, kind when isEmptyString kind ->
            tryParseEmptyDUCase<MusicMakingSkill> dto.Skill
            |> Result.ofOption (InvalidInput "music making skill")
            |> Result.map MusicMakingSkill
        | _ -> InvalidInput "unknown skill" |> Error

    let validateMeta metaDto =
        result {
            let! userId = UserId.tryParse metaDto.InitiatorId |> Result.ofOption (InvalidInput "userId")
            return 
                { CommandMeta.InitiatorId = userId
                  OperationId = metaDto.OperationId
                  CommandId = metaDto.CommandId
                  TimeStamp = metaDto.TimeStamp}
        }
        

    let validateAddBasicProfileCommand currentDate (dto: AddUserCommandDto)
            : Result<AddUserCommand, ValidationError> =
        result {
            let! meta = validateMeta dto.Meta
            let! fullName = dto.Payload.FullName |> LetterString.create
            let! username =
                match dto.Payload.Username with
                | null | "" -> Ok None
                | str -> LetterAndDigitString.create str |> Result.map Some
            let! email = dto.Payload.Email |> Email.create
            let birthdate =
                Option.ofNullable dto.Payload.BirthDate
                |> Option.bind (BirthDate.tryCreate currentDate)
            return
                { AddUserCommand.Meta = meta
                  Payload =
                    { AddUserCommandPayload.Email = email
                      FullName = fullName
                      Username = username
                      BirthDate = birthdate } }
        }

    let validateUpdateBasucProfileCommand currentDate (dto: BasicProfileUpdateCommandDto)
            : Result<UpdateBasicUserProfileCommand, ValidationError>=
        result {
            let cmd = dto.Payload
            let! meta = validateMeta dto.Meta
            let! updates =
                [
                    if cmd.BirthDate.HasValue then
                        yield BirthDate.create currentDate cmd.BirthDate.Value |> Result.map (Some >> BirthDate)
                    if notNull dto.Payload.FullName then
                        yield LetterString.create cmd.FullName |> Result.map FullName
                    if notNull dto.Payload.Username then
                        yield LetterAndDigitString.create cmd.FullName |> Result.map UserName
                    if cmd.PictureId.HasValue then
                        yield Some cmd.PictureId.Value |> BasicUserProfileUpdate.PictureId |> Ok
                ] |> Result.ofList
            let! updates = NonEmptyList.ofList updates |> Result.ofOption (InvalidInput "empty update list")
            return
                { UpdateBasicUserProfileCommand.Meta = meta
                  Payload = 
                    { AggregateId = UserId cmd.Id
                      Payload = updates } }
        }

    let validateAddAudioMetaCommand (dto: AddAudioMetaCommandDto)
        : Result<AddAudioMetaCommand, ValidationError> =
        result {
            let! meta = validateMeta dto.Meta
            let audioDto = dto.Payload
            let! title = audioDto.Title |> NonEmptyString.create
            let! appliedSkills =
                    audioDto.AppliedSkills
                    |> Array.map validateSkill
                    |> Result.ofArray
            return ()
        }
