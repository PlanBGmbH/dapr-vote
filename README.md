# [WIP] A voting application based on dapr

Note: This example may not follow best practices. It's a playground for me, to get comfortable with the .NET platform
and dapr. This may change in the feature. Please don't hesitate to share your suggestions via GitHub issues.

An example application that uses different [dapr] concepts like [Actors], [Service-to-Service Invocation],
[State Management] and [Publish and Subscribe].

The application consists of four services:

| Service           | Description                     | Language   |
| ----------------- | ------------------------------- | ---------- |
| `actors`          | Serves the actors               | F#         |
| `votes`           | Handles the voting              | F#         |
| `notifications`   | Allows to subscribe for updates | C#         |
| `frontend`        | The react frontend application  | TypeScript |

[dapr]: https://dapr.io/
[Actors]: https://github.com/dapr/docs/blob/master/concepts/actors/README.md
[Service-to-Service Invocation]: https://github.com/dapr/docs/blob/master/concepts/service-invocation/README.md
[State Management]: https://github.com/dapr/docs/blob/master/concepts/state-management/README.md
[Publish and Subscribe]: https://github.com/dapr/docs/blob/master/concepts/publish-subscribe-messaging/README.md


## Run the actors service

```
cd actors
dapr run --app-id actors --app-port 3000 dotnet run
```

## Run the votes service

```
cd actors
dapr run --app-id votes dotnet run
```
