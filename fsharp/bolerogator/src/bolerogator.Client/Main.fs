module bolerogator.Client.Main

open System.Collections.Generic

open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

open Parameter

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/general">] General
    | [<EndPoint "/common">] Common
    | [<EndPoint "/dependent">] Dependent
    | [<EndPoint "/data">] Data

type ValidationMessagePart =
    | Text of string
    | Link of ValidationMessageLink

and ValidationMessageLink =
    {
        parameter: INamedParameter
        language: string option
        page: Page
    }

and ValidationMessage = ValidationMessagePart * (ValidationMessagePart list)

/// The Elmish application's model.
type Model =
    {
        page: Page
        configurationMetadatas: ConfigurationMetadata[] option
        configuration: Configuration option
        error: string option
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
        focusedFieldId: string option
    }

and Configuration =
    {
        configurationProjectName: string
        Languages: string list
        LanguageDependent: LanguageParameter list
        Parameters: IndependentParameter list
        ValidationMessages: ValidationMessage list
    }

and ConfigurationMetadata =
    {
        name: string
        languageIndependent: ParameterMetadata list
        languageDependent: ParameterMetadata list
    }

let initModel =
    {
        page = Home
        configurationMetadatas = None
        configuration = None
        error = None
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
        focusedFieldId = None
    }

/// Remote service definition.
type ConfigurationMetadataService =
    {
        /// Get the list of all configurationMetadatas in the collection.
        getConfigurationMetadatas: unit -> Async<ConfigurationMetadata[]>

        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }

    interface IRemoteService with
        member _.BasePath = "/configurationMetadatas"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | SetFieldFocus of INamedParameter * string Option * Page
    | GetConfigurationMetadatas
    | GotConfigurationMetadatas of ConfigurationMetadata[]
    | CreateNewConfiguration of ConfigurationMetadata
    | SetLanguageOrder of string list
    | SetProjectName of string
    | SetBool of string * string Option * bool
    | SetString of string * string Option * string
    | SetInt32 of string * string Option * int32
    | SetUsername of string
    | SetPassword of string
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError

let weakParsingOfErrorMessage
        (link: string -> ValidationMessagePart)
        (quote: string -> ValidationMessagePart)
        (msg: string)
        : ValidationMessage option =
    match seq {
              for s in msg.Split("<<") do
                  match s.Split(">>") with
                  | [| "" |] when s = "" -> ()
                  | [| "" |] -> failwith "error: empty param name"
                  | [| p; "" |] -> yield link p
                  | [| p; t |] ->
                      yield link p
                      yield quote t
                  | [| t |] -> yield quote t
                  | t -> System.String.Join(", ", Array.map (fun x -> x.ToString()) t)
                         |> failwithf "Error: unexpected %s"
          } |> List.ofSeq with
      | [] -> None
      | h::t -> Some (h, t)

let validateParameters
        (uniqueErrors: HashSet<ValidationMessage>)
        (env: IValidatableParameter list) =
    let dict = Map(seq { for p in env do yield (p.Name, p) })
    let evalRule r =
        let link name =
            let q = dict.[name]
            let tab = if q.Language.IsNone then Common else Dependent
            Link { parameter = q; language = q.Language; page = tab }
        r env
        |> weakParsingOfErrorMessage link ValidationMessagePart.Text
    seq {
        for param in env do
            for rule in param.ValidationRules do
                match evalRule rule with
                | None -> ()
                | Some x when uniqueErrors.Add(x) ->
                    yield x
                | Some _ -> ()
    } |> List.ofSeq

let renderValidationMessage (dispatch: Message -> unit) ((fst, tail): ValidationMessage): Node =
    li [] [
        forEach (fst::tail) <| function
        | Text s -> text s
        | Link { parameter = p; language = lg; page = pg } ->
            a [on.click <| fun _ -> SetFieldFocus (p, lg, pg) |> dispatch] [text p.UiName]]

let validateConfiguration (configuration: Configuration): Configuration =
    let errorsInGeneralTab =
        if System.String.IsNullOrWhiteSpace(configuration.configurationProjectName)
        then [(Link { parameter = NamedParameter("configuration-options-configuration-project-name",
                                                 "Configuration Project Name")
                      language = None
                      page = General },
               [Text " may not be blank."])]
        else []
    let castToValidatable x = x :> IValidatableParameter
    let validatableLanguageIndependent = List.map castToValidatable configuration.Parameters
    let validatableLanguageDependent idx languageName =
        configuration.LanguageDependent
        |> List.map (fun p -> new MinimalParameter(p, idx, languageName) :> IValidatableParameter)
    let validateWithUniqueErrors = HashSet<ValidationMessage>() |> validateParameters
    let envs: (IValidatableParameter list) list =
        match configuration.Languages with
        | [] -> [validatableLanguageIndependent]
        | languages ->
            List.indexed languages
            |> List.map (fun (idx, languageName) ->
                             List.concat [validatableLanguageDependent idx languageName
                                          validatableLanguageIndependent])
    { configuration
      with ValidationMessages = List.concat [errorsInGeneralTab
                                             List.map validateWithUniqueErrors envs |> List.concat] }

let tryCreateFreshConfiguration conf name =
    let uniqueNames = HashSet<string>()
    traverse (tryCreateLanguageIndependent uniqueNames) conf.languageIndependent
    |> Result.bind
        (fun languageIndependents ->
         let languages = ["English"; "Dutch"; "German"; "French"; "Spanish"; "Italian"]
         conf.languageDependent
         |> (List.length languages |> tryCreateLanguageDependent uniqueNames |> traverse)
         |> Result.map
             (fun languageDependents ->
              {
                  configurationProjectName = name
                  Languages = languages
                  Parameters = languageIndependents
                  LanguageDependent = languageDependents
                  ValidationMessages = []
              } |> validateConfiguration))

let findLanguage lang =
    List.findIndex (fun x -> x = lang)

let reshuffleLanguages
        (oldLangs: string list)
        (data: LanguageParameter list)
        (newLangs : string list)
        : LanguageParameter list =
    let indices = List.map (fun newLang -> findLanguage newLang oldLangs) newLangs
    let shuffle (xs: 'a array): 'a array =
        [| for idx in indices -> xs.[idx] |]
    let shuffleVD { Default = ds; Value = vs } = { Default = shuffle ds; Value = shuffle vs }
    let shuffleValuesAndDefaults = function
        | Ss ss -> shuffleVD ss |> Ss
        | Is is -> shuffleVD is |> Is
        | Bs bs -> shuffleVD bs |> Bs
    let shuffleParameter (p: LanguageParameter) =
        new LanguageParameter(
            p,
            shuffleValuesAndDefaults p.ValuesAndDefaults,
            p.Description,
            p.ValidationRules)
    List.map shuffleParameter data

let updateConfigurationInModel model (uc: Configuration -> Configuration): Model * Cmd<Message> =
    match model with
    | { configuration = Some configuration } ->
        { model with configuration = uc configuration |> validateConfiguration |> Some }
        , Cmd.none
    | { configuration = None } ->
        model
        , System.Exception("No configuration to update.") |> Error |> Cmd.ofMsg

let getInputId p l page =
    let name = (p :> INamedParameter).Name
    match (l, page) with
    | (None, Common) -> sprintf "input-common-%s" name
    | (None, General) -> sprintf "input-general-%s" name
    | (Some n, Dependent) -> sprintf "input-%s-%s" n name
    | _ -> failwithf "Can't compute id for %s, %s, %s" name (Option.defaultValue "None" l) (page.ToString())

let replaceIndependentParameters (model: Model) (name: string) (clone: IndependentParameter -> IndependentParameter) =
    updateConfigurationInModel model <| fun conf ->
        {conf with Parameters = replaceByCloneIfNameMatch name clone conf.Parameters }

let replaceLanguageParameters (model: Model) (language: string) (name: string) (clone: int -> LanguageParameter -> LanguageParameter) =
    updateConfigurationInModel model <| fun conf ->
        let idx = findLanguage language conf.Languages
        { conf with LanguageDependent = replaceByCloneIfNameMatch name (clone idx) conf.LanguageDependent }

let update remote focusAfterRendering message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetConfigurationMetadatas
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with Model.page = page }, Cmd.none
    | SetFieldFocus (name, someLanguage, page ) ->
        let id = getInputId name someLanguage page
        focusAfterRendering id
        { model with Model.page = page }, Cmd.none

    | GetConfigurationMetadatas ->
        let cmd = Cmd.OfAsync.either remote.getConfigurationMetadatas () GotConfigurationMetadatas Error
        { model with configurationMetadatas = None }, cmd
    | GotConfigurationMetadatas data ->
        { model with configurationMetadatas = Some data }, Cmd.none

    | CreateNewConfiguration conf ->
        let projectNamePrefix = match model.signedInAs with
                                | None -> ""
                                | Some userName -> sprintf "%s " userName
        match sprintf "%s%s" projectNamePrefix conf.name |> tryCreateFreshConfiguration conf with
        | Result.Ok configuration ->
            { model with configuration = Some configuration; error = None }
        | Result.Error message ->
            { model with error = Some message; configuration = None }
        , Cmd.none
    | SetLanguageOrder newLanguages ->
        updateConfigurationInModel model <| fun configuration ->
            let newDependents = reshuffleLanguages configuration.Languages
                                                   configuration.LanguageDependent
                                                   newLanguages
            { configuration with
                  Languages = newLanguages
                  LanguageDependent = newDependents }
    | SetProjectName newProjectName ->
        updateConfigurationInModel model <| fun configuration ->
            { configuration with configurationProjectName = newProjectName }
    | SetString (name, None, newValue) ->
        let clone p = IndependentParameter(p, newValue)
        replaceIndependentParameters model name clone
    | SetString (name, Some language, newValue) ->
        let clone idx p = LanguageParameter(p, idx, newValue)
        replaceLanguageParameters model language name clone
    | SetInt32 (name, None, newValue) ->
        let clone p = IndependentParameter(p, newValue)
        replaceIndependentParameters model name clone
    | SetInt32 (name, Some language, newValue) ->
        let clone idx p = LanguageParameter(p, idx, newValue)
        replaceLanguageParameters model language name clone
    | SetBool (name, None, newValue) ->
        let clone p = IndependentParameter(p, newValue)
        replaceIndependentParameters model name clone
    | SetBool (name, Some language, newValue) ->
        let clone idx p = LanguageParameter(p, idx, newValue)
        replaceLanguageParameters model language name clone

    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none
    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.username, model.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username }, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false; configuration = None }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let configurationProjectName = function
    | { configuration = Some { configurationProjectName = configurationProjectName } } ->
        if System.String.IsNullOrWhiteSpace(configurationProjectName)
        then "Blank project name."
        else let trimmed = configurationProjectName.Trim()
             if trimmed.Length > 17
             then trimmed.Substring(0, 17) |> sprintf "%s..."
             else trimmed
    | { configuration = None } -> "No Project yet."

let homePage model _dispatch =
    Main.Home()
        .Title(configurationProjectName model)
        .Elt()

let validationPageContent (dispatch: Message -> unit) (validationMessageParts: ValidationMessage list): Node =
    let (hiddenWhenValidationErrors, visibleWhenValidationErrors) =
        match validationMessageParts with
        | _::_ -> ("is-hidden", "")
        | [] -> ("", "is-hidden")
    Main.ParameterValidationResults()
        .HiddenWhenValidationErrors(hiddenWhenValidationErrors)
        .VisibleWhenValidationErrors(visibleWhenValidationErrors)
        .ValidationErrors(
            forEach validationMessageParts <| renderValidationMessage dispatch)
        .Elt()

let limitingToInt32OnChange (setInt32: int -> Message) (onChange: Message -> unit) (newValue: int): unit =
    if (System.Int32.MinValue <= newValue) && (newValue <= System.Int32.MaxValue)
    then newValue |> setInt32 |> onChange
    else System.Exception(sprintf "%d overflows." newValue) |> Error |> onChange

let renderParameterInput
        (p: MinimalParameter)
        (id: string)
        (setString: string -> Message)
        (setInt32: int -> Message)
        (setBool: bool -> Message)
        (onChange: Message -> unit) =
    let baseAttributes = [attr.id id
                          attr.name id]
    let fullAttributes =
        baseAttributes
        @ match (p :> IValidatableParameter).ValueAndDefault with
          | I { Value = i } ->
              [bind.input.int i (limitingToInt32OnChange setInt32 onChange)
               attr.``type`` "number"
               attr.``class`` "input"]
          | S { Value = s } ->
              [bind.input.string s (setString >> onChange)
               attr.``type`` "text"
               attr.``class`` "input"]
          | B { Value = b } ->
              [bind.``checked`` b (setBool >> onChange)
               attr.``type`` "checkbox"]
    input fullAttributes

let renderOnlyWithConfiguration renderConfiguration model dispatch =
    match model with
    | { configuration = None } -> text "Internal error."
    | { configuration = Some configuration } -> renderConfiguration configuration dispatch
    
let commonPage configuration dispatch =
    Main.Common()
        .IndependentParameters(
            forEach configuration.Parameters <| fun p ->
                let inputId = getInputId p None Common
                let named = p :> INamedParameter
                li [] [
                    label [attr.``for`` inputId] [text p.Description]
                    renderParameterInput (new MinimalParameter(p))
                                         inputId
                                         (fun v -> SetString (named.Name, None, v))
                                         (fun i -> SetInt32 (named.Name, None, i))
                                         (fun b -> SetBool (named.Name, None, b))
                                         dispatch])
        .ParameterValidationResults(
            validationPageContent dispatch configuration.ValidationMessages)
        .Elt()

let dependentPage (configuration: Configuration) (dispatch: Message -> unit) =
    let enumLanguages = List.indexed configuration.Languages
    let makeRow (p: LanguageParameter) =
        let firstCell = th [] [text p.Description]
        let makeCell (lngIdx, language) =
            let name = (p :> INamedParameter).Name
            let input =
                renderParameterInput (new MinimalParameter(p, lngIdx, language))
                                     (getInputId p (Some language) Dependent)
                                     (fun s -> SetString (name, Some language, s))
                                     (fun i -> SetInt32 (name, Some language, i))
                                     (fun b -> SetBool (name, Some language, b))
                                     dispatch
            td [] [input]
        tr [] [firstCell
               forEach enumLanguages makeCell]
    Main.Dependent()
        .Header(
            forEach (" "::configuration.Languages) <| fun s -> th [] [text s])
        .Body(
            forEach configuration.LanguageDependent makeRow)
        .ParameterValidationResults(
            validationPageContent dispatch configuration.ValidationMessages)
        .Elt()

let generalPage
        ({ Configuration.Languages = languageNames
           configurationProjectName = configurationProjectName
           ValidationMessages = validationMessages })
        dispatch =
    let removeIdx idx xs =
        List.indexed xs
        |> List.filter (fun (otherIdx, _) -> otherIdx <> idx)
        |> List.map snd
    let languages =
        let languageCount = List.length languageNames
        let mayNotRemove = languageCount < 2
        forEach (List.indexed languageNames) <| fun (idx, langName) ->
            let mayGoUp = idx > 0
            let mayGoDown = idx < languageCount - 1
            li [attr.``class`` "level"] [
                div [attr.``class`` "buttons"] [
                    button [yield on.click <| fun _ -> if mayGoUp
                                                       then langName::removeIdx idx languageNames
                                                            |> SetLanguageOrder
                                                            |> dispatch
                                                       else ()
                            yield attr.``classes`` ["button"]
                            if not mayGoUp then yield attr.disabled "disabled"]
                           [text "To top"]
                    button [yield on.click <| fun _ -> if mayGoDown
                                                       then removeIdx idx languageNames @ [langName]
                                                            |> SetLanguageOrder
                                                            |> dispatch
                                                       else ()
                            yield attr.``classes`` ["button"]
                            if not mayGoDown then yield attr.disabled "disabled"]
                           [text "To bottom"]]
                text langName
                button [yield on.click <| fun _ -> match removeIdx idx languageNames with
                                                   | [] -> ()
                                                   | lessLanguages -> 
                                                        SetLanguageOrder lessLanguages
                                                        |> dispatch
                        yield attr.classes ["button"; "is-danger"]
                        if mayNotRemove then yield attr.disabled "disabled"]
                       [text "Remove"]]
    Main.General()
        .ConfigurationProjectName(configurationProjectName, SetProjectName >> dispatch)
        .Languages(languages)
        .ParameterValidationResults(validationPageContent dispatch validationMessages)
        .Elt()

let dataPage model (username: string) dispatch =
    Main.Data()
        .Reload(fun _ -> dispatch GetConfigurationMetadatas)
        .Username(username)
        .SignOut(fun _ -> dispatch SendSignOut)
        .Rows(cond model.configurationMetadatas <| function
            | None ->
                Main.EmptyData().Elt()
            | Some data ->
                forEach data <| fun conf ->
                    tr [] [
                        td []
                           [a [on.click <| fun _ -> CreateNewConfiguration conf |> dispatch]
                              [text conf.name]]
                        td [] [textf "%d parameters" conf.languageIndependent.Length]
                        td [] [textf "%d parameters" conf.languageDependent.Length]
                    ])
        .Elt()

let signInPage model dispatch =
    Main.SignIn()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty
            | true ->
                Main.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Data "New configuration"
            cond model.configuration <| function
            | None -> empty
            | Some _ ->
                forEach
                    [(General, configurationProjectName model)
                     (Common, "Common Parameters")
                     (Dependent, "Language Dependent Parameters")]
                    <| fun (page, menuName) ->
                        menuItem model page menuName
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | General -> renderOnlyWithConfiguration generalPage model dispatch
            | Common -> renderOnlyWithConfiguration commonPage model dispatch
            | Dependent -> renderOnlyWithConfiguration dependentPage model dispatch
            | Data ->
                cond model.signedInAs <| function
                | Some username -> dataPage model username dispatch
                | None -> signInPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    let mutable focusedFieldId = None

    override this.OnAfterRenderAsync(firstRender) =
        match focusedFieldId with
        | None -> base.OnAfterRenderAsync(firstRender)
        | Some id ->
            let baseCall = base.OnAfterRenderAsync(firstRender)
            task {
                do! baseCall
                do! this.JSRuntime.InvokeAsync("MyJsLib.focusById", [| id |])
                focusedFieldId <- None
            }

    override this.Program =
        let configurationMetadataService = this.Remote<ConfigurationMetadataService>()
        let focusAfterRendering id =
            focusedFieldId <- Some id
        let update = update configurationMetadataService focusAfterRendering
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
