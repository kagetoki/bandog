namespace Bandog.Core.Domain

open System
open Bandog.Common
open Microsoft.FSharp.Reflection
open Bandog.Core.Domain.ValueObjectsDto
open CoreCommands
open DomainTypes
open CoreCommandDto

module Validation =

    let tryParseEmptyDUCase<'DU> str =
        if FSharpType.IsUnion typeof<'DU> |> not then None
        else
        match str with
        | null | "" -> None
        | str ->
            FSharpType.GetUnionCases typeof<'DU>
            |> Array.tryFind (fun c -> c.Name =~ str && (c.GetFields() |> Array.isEmpty))
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

    let validateArray validate array =
        array
        |> Array.map validate
        |> Result.ofArray

    let validateSkill (dto: SkillDto) =
        if dto.Skill |> isWhiteSpaceString then InvalidInput "skill can't be empty" |> Error
        else
        match dto.SkillKind, dto.InstrumentKind with
        | SkillKind.Instrument, (null | "") ->
            tryParseEmptyDUCase<Instrument> dto.Skill
            |> Result.ofOption (InvalidInput "unknown instrument")
            |> Result.map Instrument
        | SkillKind.Instrument, (("electric"|"acoustic") as kind) ->
            result {
                let! kind = tryParseEmptyDUCase<InstrumentKind> kind |> Result.ofOption (InvalidInput "")
                let! instrument =
                    match dto.Skill.ToLower() with
                    | "guitar" -> Guitar |> Ok
                    | "bass" -> Bass |> Ok
                    | _ -> InvalidInput "unknown skill" |> Error
                return instrument kind |> Instrument
            }
        | SkillKind.MusicMaking, (null | "") ->
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
 
    let validateGenre (dto:GenreDto) =
        match dto.Genre, dto.GenreKind with
        | ((null|""), _) -> InvalidInput "empty genre" |> Error
        | "classic", (null | "") -> Ok Classic
        | "ambient", (null | "") -> Ok Ambient
        | "folk", (null | "") -> Ok Folk
        | "pop", (null | "") -> Ok Pop
        | "blues", (null | "") -> Ok Blues
        | "soul", (null | "") -> Ok Soul
        | "rock", rockKind ->
            tryParseEmptyDUCase<RockKind> rockKind |> Rock |> Ok
        | "jazz", jazzKind ->
            tryParseEmptyDUCase<JazzKind> jazzKind |> Jazz |> Ok
        | "metal", metalKind ->
            tryParseEmptyDUCase<MetalKind> metalKind |> Metal |> Ok
        | "hiphop", hipHopKind ->
            tryParseEmptyDUCase<HipHopKind> hipHopKind |> HipHop |> Ok
        | "electro", electroKind ->
            tryParseEmptyDUCase<ElectroKind> electroKind |> Electro |> Ok
        | _ -> InvalidInput "unknown genre" |> Error

    let validateSkills = validateArray validateSkill

    let validateGenres = validateArray validateGenre

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
            let! appliedSkills = validateSkills audioDto.AppliedSkills
            let! appliedSkills = appliedSkills |> NonEmptySet.ofSeq |> Result.ofOption (ListIsEmpty "skills")
            let! genres = validateGenres audioDto.Genre
            let! genres = genres |> NonEmptySet.ofSeq |> Result.ofOption (ListIsEmpty "genres")
            return
                { Meta = meta
                  Payload =
                   { Id = audioDto.Id
                     UserId = UserId audioDto.UserId
                     Title = title
                     AppliedSkills = appliedSkills
                     Genre = genres
                     Duration = audioDto.Duration } }
        }

    let validateUpdateAudioMetaCommand (dto: UpdateAudioMetaCommandDto)
        : Result<UpdateAudioMetaCommand, ValidationError> =
        result {
            let! meta = validateMeta dto.Meta
            let validateUpdateArray validate update = Array.map (validate >> Result.map update)
            let validateGenresUpdates = validateUpdateArray validateGenre
            let validateSkillUpdates = validateUpdateArray validateSkill
            let payload = dto.Payload
            let! updates =
                [
                    if payload.Duration.HasValue then
                        yield Duration payload.Duration.Value |> Ok
                    if notNull payload.Title then
                        yield payload.Title |> NonEmptyString.create |> Result.map Title
                    if notNull payload.AddGenre then
                        yield! payload.AddGenre
                            |> validateGenresUpdates AddGenre
                    if notNull payload.RemoveGenre then
                        yield! payload.RemoveGenre |> validateGenresUpdates RemoveGenre
                    if notNull payload.AddSkill then
                        yield! payload.AddSkill |> validateSkillUpdates AddSkill
                    if notNull payload.RemoveSkill then
                        yield! payload.RemoveSkill |> validateSkillUpdates RemoveSkill
                ] |> Result.ofList
            let! updates = updates |> NonEmptyList.ofList |> Result.ofOption (ListIsEmpty "audio updates")
            return
                { Meta = meta
                  Payload =
                  { AggregateId = payload.Id
                    Payload = updates } }
        }
