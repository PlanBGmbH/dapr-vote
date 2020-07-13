namespace Actors

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

// Based on https://github.com/dapr/dotnet-sdk/blob/master/docs/get-started-dapr-actor.md#update-startupcs
type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    member val Configuration : IConfiguration = null with get, set

    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddRouting() |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseHsts() |> ignore
