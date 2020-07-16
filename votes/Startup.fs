namespace Votes

open Dapr.Client
open Shared.Config
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type Startup private () =

    new(configuration: IConfiguration) as this =
        Startup()
        then this.Configuration <- configuration

    member this.ConfigureServices(services: IServiceCollection) =
        services
            .AddControllers()
            .AddDapr()
            .AddJsonOptions(fun options -> options.JsonSerializerOptions.Converters.Add(jsonConverter))
        |> ignore

        services.AddRouting() |> ignore
        services.AddSingleton(jsonSerializerOptions) |> ignore
        services.AddSingleton<DaprClient>(Dapr.client) |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseHsts() |> ignore

        app.UseRouting() |> ignore
        app.UseCloudEvents() |> ignore
        app.UseAuthorization() |> ignore
        app.UseEndpoints(fun endpoints ->
            endpoints.MapSubscribeHandler() |> ignore
            endpoints.MapControllers() |> ignore)
        |> ignore

    member val Configuration: IConfiguration = null with get, set
