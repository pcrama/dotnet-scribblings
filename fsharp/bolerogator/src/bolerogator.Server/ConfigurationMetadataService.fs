namespace bolerogator.Server

open System
open System.IO
open System.Text.Json
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server
open bolerogator

type ConfigurationMetadataService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.ConfigurationMetadataService>()

    let configurationMetadatas =
        let json = Path.Combine(env.ContentRootPath, "data/configurationMetadatas.json") |> File.ReadAllText
        JsonSerializer.Deserialize<Client.Main.ConfigurationMetadata[]>(json)
        |> ResizeArray

    override _.Handler =
        {
            getConfigurationMetadatas = ctx.Authorize <| fun () -> async {
                return configurationMetadatas.ToArray()
            }

            signIn = fun (username, password) -> async {
                if password = "password" then
                    do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
                    return Some username
                else
                    return None
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            getUsername = ctx.Authorize <| fun () -> async {
                return ctx.HttpContext.User.Identity.Name
            }
        }
