## .net 6 web API

Topics to be covered:
- this tutorial uses visual studio 2022 + .net 6 + SQL server 2019 
- understand principles of REST
- create and understand a new .net web api project
- learn and use entity framework to talk to SQL server 
- use JWT authentication
- api controller
- repository pattern
- domain models and DTOs
- use automapper to map these 
- using asyncc programming
- add validations to our API using 'fluent validations' 
- add authentication and authorization 


Contents 
- [Creating a new web api](#creating-a-new-web-api)


sql server 2019 connection string: Server=localhost\MSSQLSERVER01;Database=master;Trusted_Connection=True;

---

### Creating a new web api

#### Create a new web api 
- open visual studio
- click create a new project 
- select 'asp.net core web api'
- puts project name as 'NZWalks.API' and solution name as 'NZWalks'
- change location to where you have started the repo
- for framework, check .net 6
- click create

#### Understanding our new web api
- the entry point to an asp.net core application is the program.cs file 
- appsettings.json is used to store configuration settings such as db connecting strings, application global scope variables etc 
- controllers file will hold the APIs in the form of API controllers
  - to handle requests, the web api uses controllers
  - controllers derivce from the ControllerBase class.
  - will have methods like get / post / put / delete / patch (CRUD)

#### Run the project and using swagger
- click on the triangle (one of them is debug mode the other is without debugging)
- it opens swagger and shows the weatherforecast api 
- you can 'try it out' using swagger. or use it in the browser or postman 

#### Understanding our domain
- we'll have a one to many relationship where one region can have many walks. walks will also have a walkdifficulty, so 3 tables (regions, walks, walkDifficulty)

#### Creating domain models 
- creates a models file at the root 
- creates a domain file inside models
  - within that creates a class 'Region.cs' and starts creating props (can type prop+tab to generate one quicker)
  - right click on domain and create a 'Walk.cs' class
  - does the same for 'WalkDifficulty.cs'
- time to add navigation properties for entity framework
  - in region.cs adds `public IEnumerable<Walk> Walks { get; set; }`
  - in Walk.cs adds `public Region Region { get; set; } public WalkDifficulty WalkDifficulty { get; set; }`

#### Install Entity Framework Core Nuget Packages
- entity framework core is a modern object database mapper for .net 
- supports linq queries, change tracking, updates and schema migrations 
- works with many db's including sql server
- he right-clicks "dependencies" and opens "manage nuGet packages"
  - click browse tab
  - search for Microsoft.Entityframeworkcore.SqlServer + install
  - search for Microsoft.Entityframeworkcore.tools + install


#### Create DBContext (so our domain objects can talk to the database)
- a dbContext instance represents a session with the database 
- can be used to query and save instances your entities 
- creates a new folder at root level "Data"
  - creates NZWalksDbContext.cs class 
    - this class needs to inherit from the 'DbContext' class `public class NZWalksDbContext : DbContext ` (should add import auto)
    - create constructor using 'ctor + tab' shortcut
      - `public NZWalksDbContext(DbContextOptions<NZWalksDbContext> options): base(options) {}`
    - add properties: `public DbSet<Region> Regions { get; set; }` => this tells entityframework "please create a regions table for us if it doesnt exist in the database" 
    - do same as above for walks and walk difficulty.

#### Create connection to DB
- open appsettings.json
- create a connectionStrings section
- it takes multiple keyvalue pairs, the first key is the name of the connection string (this can be whatever, we used 'NZWalks') and the value of this is your connection string to the database.
  - the 'server' is the name of your server, specified when you created it in sql server
  - the 'database' is the name of your db, you can specify what you want here
  - you have to specify trusted_connection=true
  - ````
    "ConnectionStrings": {
    "NZWalks": "server=localhost\\MSSQLSERVER01;database=NZWalksDb;Trusted_Connection=true;TrustServerCertificate=True"
  }
  ````
  - note, i had add this to the above string: TrustServerCertificate=True (he didnt do that in the lecture)
  - when we run entityframework core in a later video it comes here and creates a db for you if it doesn't exist already. 


#### Understanding dependency injection
- dependency injection is a software design pattern, a technique for achieving 'Inversion of Control' between classes and their dependencies
- the dependencies of a class are injected into them, rather than the class directly creating them.


example of not using dependency injection:
````c#
public class MyDependency
{
  public void WriteMessage(string message)
  {
    Console.WriteLine($"my dependency write message called. message: {message}")
  }
}

public clas IndexModel : PageModel 
{
  private readonly MyDependency _dependency = new MyDependency(); // here the dependency is created

  public void OnGet()
  {
    _dependency.WriteMessage("IndexModel.onGet")
  }
}
````

example of using dependency injection
````c#
public interface IMyDependency 
{
  void WriteMessage(string message);
}

public class MyDependency : IMyDependency 
{
  public void WriteMessage(string message)
  {
    Console.WriteLine($"my dependency write message called. message: {message}")
  }
}

// in program.cs 
builder.Services.AddScoped<IMyDependency, MyDependency>();

public class Index2Model : PageModel 
{
  private readonly IMyDependency _myDependency; 
  public Index2Model(IMyDependency myDependency) // it searches the program.cs services and adds it here
  {
    _myDependency = myDependency;
  }

  public void OnGet()
  {
    _myDependency.WriteMessage("Index2Model.OnGet")
  }
}

````

#### Injecting DbContext class
````c#
// in program.cs
builder.Services.AddDbContext<NZWalksDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("NZWalks"));
});
````

#### Running EF Core Migrations 
- if we look in SQL server management studio, we have our server name and the database file doesn't have a database called NZWalks 
- we can create one using entity framework core migrations 
- open tools in visual studio => nuget package manager => package manager console 
  - run add migration command `Add-Migration`, give it the name "initial migration"
  - in the solutions explorer there is now a migrations folder, check the class "....initial migration.cs"
    - it has an up and down function. it is how entity framework to create new tables etc. 
  - because we have just created the migration but not executed it, we shouldnt see the db yet in SQL management studio
  - next command is `Update-Database`
  - we should be able to open SQL management studio => databases and see "NZWalksDb". we can look into tables and dbo.Walks for example, right click select top 1000, and it will be empty because we haven't seeded data yet. 

#### Seeding Data into database 
- He's created a script to use in microsoft sql server management studio (NZWalksDbSeed.sql)
- copy paste it into a query, and hit execute 



---

add this thing to the backend doc 