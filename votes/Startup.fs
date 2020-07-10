namespace Votes

open System.Text.Json
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    member this.ConfigureServices(services: IServiceCollection) =
        let jsonSerializerOptions = JsonSerializerOptions()
        jsonSerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        jsonSerializerOptions.PropertyNameCaseInsensitive <- true

        services.AddControllers().AddDapr() |> ignore
        services.AddSingleton(jsonSerializerOptions) |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseHttpsRedirection() |> ignore
        app.UseRouting() |> ignore
        app.UseCloudEvents() |> ignore
        app.UseAuthorization() |> ignore
        app.UseEndpoints(fun endpoints ->
            endpoints.MapSubscribeHandler() |> ignore
            endpoints.MapControllers() |> ignore
        ) |> ignore

    member val Configuration : IConfiguration = null with get, set
