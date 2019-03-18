# Concurrency Extension for Entity Framework
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

## Features
- Conflict handling: "AddOrUpdate" / "Upsert" handler (OptiContext)
- Conflict handling: Dublicate keys / Double Adds (OptiContext)
- Ignore: Update, but deleted in the meantime (SaveChangesMode)
- Ignore: Two Adds resulting in Dublicate key -> LastWins


## Samples
### Sample: AddOrUpdate / Upsert - Optimistic Concurrency Handler
#### Situtation Sample 1:
- Context 1 selects the entity X
- Context 2 deletes the entity X
- Context 1 update the entity X
##### Result
Concurrency Exception will be thrown

#### Situation Sample 2:
- Context 1 selects the entity X
- Context 2 selects the entity X
- Context 2 updates entity X and SaveChanges
- Context 1 updates entity X and SaveChanges
##### Result
Concurrency Exception will be thrown because RowVersion is having the ConcurrencyCheck attribute and has changed in the meantime

#### Solution: Use OptiContext
```csharp
public void AddOrUpdate()
{
    var id = Guid.NewGuid();
    var initialRowVersion = Guid.NewGuid();
    var lastRowVersion = Guid.NewGuid();
    
    // Create a new OptiContext. 
    // - Pass a factory method to create a new DbContext
    var oc = new OptiContext<SqlDbContext, OptiEntity>(() => new SqlDbContext());

    // defines the method to select an entity
    oc.SelectFunc = (cx) =>
    {
        var i = cx.Optis.Find(id);
        return i;
    };

    // defines the method to add an entity, if select returns no entity
    oc.AddAction = (cx) =>
    {
        var oe = new OptiEntity();
        oe.Id = id;
        oe.Value = 0;
        oe.Created = oe.Updated = DateTime.Now;
        oe.RowVersion = initialRowVersion;
        cx.Optis.Add(oe);
        cx.SaveChanges();
    };

    // defines the method to update the entity, if select returns a entity
    oc.UpdateAction = (cx, oe) =>
    {
        // update the entity
        lastRowVersion = Guid.NewGuid();
        oe.Value++;
        oe.Created = oe.Updated = DateTime.Now;
        oe.RowVersion = lastRowVersion;
        cx.SaveChanges();
    };

    // initial add opti entity
    oc.Execute();
}
```
#### Explanation
Note1: The SelectFunc, AddAction and UpdateAction could be called multiple times if and until
- no ConcurrencyException occurs
- or MaxRetryLimit (Extensions.MaxRetryLimit) is reached (=> will throw a MaxRetryReachedException)
Note2: Constructor of the OptiContext needs a factory to create a new DbContext
- Everytime a ConcurrencyException occurs a new DbContext instance is created to reduce cache issues.

### Sample: How to ignore dublicate key errors
#### Situation 1
- Entity X is added
- Entity Y with the same primary key as X is added
##### Result
Entity Y SaveChanges will throw a Dublicate key error
#### Solution
Use SaveChangesMode.IgnoreEntityDublicateKey
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
        
        // ignore dublicate key errors. Entity will not be stored in the database! No exception will be thrown
        cx.SaveChanges(SaveChangesMode.IgnoreEntityDublicateKey);
    }
}
```
