# AspireTodoApp

![Unit tests](https://github.com/Aristocrab/AspireTodoApp/actions/workflows/dotnet.yml/badge.svg)

## How to run
Requires Docker to be running
```
cd AspireTodoApp.AppHost
dotnet run
```

## Technologies used  
- .NET Aspire (with Docker)
- ASP.NET Core
- EF Core
- MongoDB.Driver
- Serilog
- HotChocolate (GraphQL)
- Redis
- Scalar (API docs)
- Angular

## Screenshots

### Angular
You can switch between REST and GraphQL, as well as between MongoDB and Postgres
![](/img/angular.png)

### Aspire resources
![](/img/aspire-resources.png)

### Aspire traces
![](/img/aspire-traces.png)

## Aspire resource diagram
```mermaid
flowchart LR
angular(angular)
apiservice(apiservice)
cache(redis)
mongo(mongo)
mongodb(mongodb)
postgres(postgres)
postgresdb(postgresdb)

angular  -->  apiservice 
apiservice  -->  cache 
apiservice  -->  mongodb 
apiservice  -->  postgresdb 
mongodb  -->  mongo 
postgresdb  -->  postgres 
```
