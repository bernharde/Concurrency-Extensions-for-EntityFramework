 Concurrency Extension for Entity Framework
The following extensions can be used to dial with concurrency in Entity Framework. At the moment there exists three different implementations and the according nuget packages

## Be.EntityFramework.SqlServer.Extensions
- for EntityFramework
- .NET Framework
- https://www.nuget.org/packages/Be.EntityFramework.SqlServer.Extensions/

## Be.EntityFramworkCore.SqlServer.Extensions
- for Microsoft.EntityFrameworkCore
- .NET Core 2.x
- https://www.nuget.org/packages/Be.EntityFramworkCore.SqlServer.Extensions/

## Be.ManagedDataAccess.EntityFramwork.Extensions
- for Oracle.ManagedDataAccess.EntityFramework
- .NET Framework
- https://www.nuget.org/packages/Be.ManagedDataAccess.EntityFramwork.Extensions/

Ignore options
Concurrency problem: Update
Concurrency problem: Dublicate Key
- If you add a entity with a primary key, 
- and another user created an entity with the same primary key
-> Dublicate key error could occur

Sample: How to ignore dublicate key errors
```csharp
public void IgnoreEntityDublicateKeyError()
{
    var id = Guid.NewGuid();

    // create first entity with id (primary key)
    using (var cx = LastWins.CreateContext())
    {
        var lw = new LastWinsEntity();
        lw.Id = id;
        lw.Name = "v1";
        lw.Created = lw.Updated = DateTime.Now;
        cx.LastWins.Add(lw);
        cx.SaveChanges(); // save first one
    }

    // create second entity with same id (primary key) 
    // -> Dublicate key error will be ignored, 
    // -> second entity will not be added to db
    using (var cx = LastWins.CreateContext())
    {
        var lw2 = new LastWinsEntity();
        lw2.Id = id;
        lw2.Name = "v1";
        lw2.Created = lw2.Updated = DateTime.Now;
        cx.LastWins.Add(lw2);
        cx.SaveChanges(SaveChangesMode.IgnoreEntityDublicateKey);
    }
}
```
