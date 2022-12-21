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
- [Quick Notes](#quick-notes)
- [Creating a new web api](#creating-a-new-web-api)
  - [Create new web api](#create-a-new-web-api)
  - [Understanding our new web api](#understanding-our-new-web-api)
  - [Run the project using swagger](#run-the-project-and-using-swagger)
  - [Understanding our domain](#understanding-our-domain)
  - [Creating domain models](#creating-domain-models)
  - [Install entity framework core nuget packages](#install-entity-framework-core-nuget-packages)
  - [Create DbContext so our domain objects can talk to the db](#create-dbcontext-so-our-domain-objects-can-talk-to-the-database)
  - [Create connection to DB](#create-connection-to-db)
  - [Injecting DbContext class](#injecting-dbcontext-class)
  - [Running EF Core Migrations](#running-ef-core-migrations)
  - [Seeding Data into database](#seeding-data-into-database)
- [Create new controller - Regions Controller](#create-new-controller---regions-controller)
  - [Understanding the repository pattern](#understanding-the-repository-pattern)
  - [Adding DTOs or Contracts](#adding-dtos-or-contracts)
  - [Making our controller asynchronous](#making-our-code-asynchronous)
- [Creating CRUD on region controller](#creating-crud-on-region-controller)
- [Creating Walks Controller and Implementing CRUD](#creating-walks-controller-and-implementing-crud)
  - [Navigation Properties](#navigation-properties)
- [Creating WalkDifficulty Controller and CRUD](#creating-walkdifficulty-controller-and-crud)
- [Validations in .NET Core Web API](#validations-in-aspnet-core-web-api)
  - [Validating Region Controller Manually](#validating-region-controller---addregionrequest-model)
  - [Fluent Validations](#fluent-validations)
- [Authentication and Authorisation in .NET Core Web API](#authentication-and-authorisation-in-net-core-web-api)
  - [Setting up Authorisation](#setting-up-authorisation)
  - [Creating Users and Static User repository](#creating-users-and-static-user-repository)
  - [Implementing and Testing Authorisation without Token](#implementing-and-testing-authorisation-without-token)
  - [Creating AuthController and Login Method](#creating-authcontroller-and-login-method)
  - [Creating token handler and Generate Token](#creating-token-handler-and-generate-token)
  - [Role Based Authorization](#role-based-authorization)
  - [Changing Swagger to add Authentication](#changing-swagger-to-add-authentication)
  - [Add EF Core and Add Users To Database](#add-ef-core-and-add-users-to-database)



sql server 2019 connection string: Server=localhost\MSSQLSERVER01;Database=master;Trusted_Connection=True;

---

### Quick Notes
#### Flow of creating API
1. Create project
2. Understand domain then create domain models (in models/domain file)
3. Install Entity framework core nuget packages => SqlServer + Tools (database mapper for .net, supports linq queries)
4. Create dbContext (in data file)
5. create connection to DB
6. in program.cs, add the dbContext dependency (so you can inject it)
7. Run EF Core migration commands
8. Optional: seed db
9. Start creating CRUD controllers (see below section)
10. Add in validations to the controller methods which need it - i.e. those that take a data such as a DTO as an argument
  - do this manually or use fluent validator 
11. Authorisation / authentication
12. for any changes to the db, need to change the domain models, then `Add-Migration` in package manager console, then `Update-Database` to use entityframework core to fix it up and apply. 

#### Flow of adding controllers 
looks something like this:
1. create controller, i.e. WalkDifficultyController.cs 
2. Add in the method you want to use, i.e. GetAllAsync with code that might not exist yet, i.e.
````c#
[HttpGet]
public async Task<IActionResult> GetAllAsync()
{
    var walkDifficulties = await walkDifficultyRepository.GetAllAsync();
    var walkDifficultiesDto = mapper.Map<List<Models.DTO.WalkDifficulty>>(walkDifficulties);

    return Ok(walkDifficultiesDto);
}
````
3. Add in any dependency injection to the controllers constructor, i.e. the mapper and the repository (Repository pattern)
  - in models/Profiles folder, create mapper i.e. `CreateMap<Domain.WalkDifficulty, DTO.WalkDifficulty>().ReverseMap();`
4. Fix up the pseudo code written in 2 that doesn't exist yet, i.e. create the repository 
  - create IWalkDifficultyRepository
    - add actions as you go, i.e. for above we need `Task<IEnumerable<WalkDifficulty>> GetAllAsync();`
    - in program.cs, add the scoped dependency: `builder.Services.AddScoped<IWalkDifficultyRepository, WalkDifficultyRepository>();`
  - create WalkDifficultyRepository, inherit from IWalkDifficultyRepository, implement missing members
    - dependency inject the db connection
    - create / fill in the GetAllAsync method
5. Repeat this for all CRUD actions

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

public class IndexModel : PageModel 
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

### Create New Controller - Regions Controller

In this section:
- create new controller
- GET all regions
- Understand repository pattern
- DTOs vs Domain models
- async code

#### Create regions controller
- right-click controllers folder, select add new controller, select MVC Controller- Empty and add 
  - name it 'RegionsController.cs' and add 
- decorate the class with the `[ApiController]` attribute
- decorate it with `[Route("Regions")]` to specify the route 
- create some dummy data to test the GET:
````c#
 [HttpGet]
        public IActionResult GetAllRegions()
        {
            var regions = new List<Region>()
            {
                new Region
                {
                    Id = Guid.NewGuid(),
                    Name = "Wellington",
                    Code = "WLG",
                    Area = 227755,
                    Lat = -1.88,
                    Long = 299.99,
                    Population = 200000,
                },
                new Region
                {
                    Id = Guid.NewGuid(),
                    Name = "Auckland",
                    Code = "AUCK",
                    Area = 227755,
                    Lat = -1.88,
                    Long = 299.99,
                    Population = 200000,
                },
            };

            return Ok(regions);
        }
````

#### Understanding the Repository Pattern
- No Repository:
  - Controller => DbContext => EntityFramework & Database
  - Bad practice, as code is not testable.
  - To change implementation of not using EntityFramework, then we will have to make changes to our controller 
- With Repository:
  - Controller => Repository (can have multiple) => DbContext => Entityframework & database 
  - We can create an IRepository interface, implement the repositories, which in turn call the database.
  - using dependency injection, we can inject the repository into the controller
  - Controller does not depend on how data is coming back from the database

#### Create Region Repository 
- Will first make IRepository then Repository
  - IRepository is the interface, which defines a contract 
  - A class that implements an interface must implement all members of the interface
- Create Repositories folder in the root
  - create IRegionRepository.cs with a "getAll" method, right click repositiroes and select to create new class, then choose interface
  - create RegionRepository.cs, make it implement IRegionRepository, and implement missing members.
  - we need to talk to the DB first, so we need a constructor to dependency inject the db like so:
  ````c#
    public class RegionRepository : IRegionRepository
    {
        private readonly NZWalksDbContext dbContext;

        public RegionRepository(NZWalksDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<Region> GetAll()
        {
            return dbContext.Regions.ToList();
        }
    }
  ````
  - in program.cs add `builder.Services.AddScoped<IRegionRepository, RegionRepository>();` => "whenever we ask for the IRegionRepository, give me the implementation for the RegionRepository"
    - because we have injected this, we can use this RegionRepository implementation into the regions controller
  - update RegionsController.cs to use this:
  ````c#
    [ApiController]
    [Route("Regions")]
    public class RegionsController : Controller
    {
        private readonly IRegionRepository regionRepository;

        public RegionsController(IRegionRepository regionRepository)
        {
            this.regionRepository = regionRepository;
        }

        [HttpGet]
        public IActionResult GetAllRegions()
        {
            return Ok(regionRepository.GetAll());
        }
    }
  ````

#### Adding DTOs or Contracts
- we are getting the regions domain models back in our swagger
- we are exposing our domain models - if we want to change them in the future, will we create breaking changes. so it is a bad practice. the clients of the API should never get breaking changes in the current version of the API they are using
- Add a new folder inside the Domain folder callted "DTO"
- Add a class to DTO and call it Region.cs as well, copy everything from the Doman Region into the DTO region.
- Now we are sending the DTO, and we are freed up to change the domain model as we see fit without breaking changes
````c#
[HttpGet]
public IActionResult GetAllRegions()
{
    var regions = regionRepository.GetAll();

    // return DTO regions
    var regionsDTO = new List<Models.DTO.Region>();
    regions.ToList().ForEach(domainRegion =>
    {
        var regionDTO = new Models.DTO.Region()
        {
            Id = domainRegion.Id,
            Name = domainRegion.Name,
            Code = domainRegion.Code,
            Area = domainRegion.Area,
            Lat = domainRegion.Lat,
            Long = domainRegion.Long,
            Population = domainRegion.Population,
        };

        regionsDTO.Add(regionDTO);
    });

    return Ok(regionsDTO);
}
````


#### Install and use Automapper
- the above code (the new Models.DTO.Region) has repeatable code - we can use automapper to do this for us
- click manage nuget packages and install "Automapper" and "Automapper.extensions.microsoft.dependencyInjection"
- next step is to create profiles for automapper 
- create a new folder called profiles in models 
  - in profiles create 'RegionsProfile.cs' and it inherits from Profile (from automapper)
  - in the below example, we're saying if the domain region had "RegionId" and the DTO we wanted to just call "Id", we could change it like so
  ````c#
      public RegionsProfile()
    {
        CreateMap<Domain.Region, DTO.Region>()
            .ForMember(dest => dest.Id, options => options.MapFrom(src => src.RegionId));
    }
  ````
  - because our ID's are the same in both, we don't need to do this - automapper will do it for us. 
  - we have a mapper here, but we need to inject this into our solution in program.cs: `builder.Services.AddAutoMapper(typeof(Program).Assembly);`
  - now in our controller we can use:
  ````c#
  [HttpGet]
  public IActionResult GetAllRegions()
  {
      var regions = regionRepository.GetAll();

      //// return DTO regions
      //var regionsDTO = new List<Models.DTO.Region>();
      //regions.ToList().ForEach(domainRegion =>
      //{
      //    var regionDTO = new Models.DTO.Region()
      //    {
      //        Id = domainRegion.Id,
      //        Name = domainRegion.Name,
      //        Code = domainRegion.Code,
      //        Area = domainRegion.Area,
      //        Lat = domainRegion.Lat,
      //        Long = domainRegion.Long,
      //        Population = domainRegion.Population,
      //    };

      //    regionsDTO.Add(regionDTO);
      //});

      var regionsDTO = mapper.Map<List<Models.DTO.Region>>(regions);


      return Ok(regionsDTO);
  }
  ````

#### Making our code Asynchronous 
- Website makes call to API, then makes another call, then gets data from 2nd call, then gets data from first call
- Can do multiple things at once 


- Make regionRepository asynchronous 
  - convert IRegionRepository `IEnumerable<Region> GetAll();` to `Task<IEnumerable<Region>> GetAllAsync();` => change name to getAllAsync to make it clear
  - convert RegionRepository
  ````c#
  public async Task<IEnumerable<Region>> GetAllAsync() // this becomes async and a task
  {
      return await dbContext.Regions.ToListAsync(); // change .ToList to ToListAsync and add the await.
  }
  ````
  - update the RegionsController:
  ````c#
        [HttpGet]
        public async Task<IActionResult> GetAllRegions() // becomes async and a task
        {
            var regions = await regionRepository.GetAllAsync(); // update method name and add await
            var regionsDTO = mapper.Map<List<Models.DTO.Region>>(regions);

            return Ok(regionsDTO);
        }
  ````

---

### Creating CRUD on Region Controller
Both in the repository and the controller

#### Creating Repository Methods 

GET by Region ID
- in IRegionRepository.cs add `Task<Region> GetAsync(Guid id);`
- in RegionRepository.cs add
````c#
        public async Task<Region> GetAsync(Guid id)
        {
            return await dbContext.Regions.FirstOrDefaultAsync(x => x.Id == id);
        }
````
- in RegionsController.cs 
````c#
[HttpGet]
[Route("{id:guid}")] // this makes it so in the route we're looking for /id . It also specifies that it has to be of type guid
public async Task<IActionResult> GetRegionAsync(Guid id)
{
    var region = await regionRepository.GetAsync(id);
    if (region == null)
    {
        return NotFound();
    }
    var regionDTO = mapper.Map<Models.DTO.Region>(region);

    return Ok(regionDTO);
}
````


POST AddRegion
- in IRegionRepository add `Task<Region> AddAsync(Region region);`
- in RegionRepository add 
````c#
public async Task<Region> AddAsync(Region region)
{
    region.Id = Guid.NewGuid();
    await dbContext.AddAsync(region);
    await dbContext.SaveChangesAsync();
    return region;
}
````
- in RegionsController.cs add
````c#
[HttpPost]
[ActionName("AddRegionAsync")]
public async Task<IActionResult> AddRegionAsync(Models.DTO.AddRegionRequest addRegionRequest)
{
    // Request(DTO) to domain model
    // var region = mapper.Map<Models.Domain.Region>(addRegionRequest);
    var region = new Models.Domain.Region()
    {
        Code = addRegionRequest.Code,
        Area = addRegionRequest.Area,
        Lat = addRegionRequest.Lat,
        Long = addRegionRequest.Long,
        Name = addRegionRequest.Name,
        Population = addRegionRequest.Population,
    };

    // pass details to repository
    region = await regionRepository.AddAsync(region);

    // convert back to DTO
    // var regionDTO = mapper.Map<Models.DTO.Region>(region);
    var regionDTO = new Models.DTO.Region
    {
        Id = region.Id,
        Code = region.Code,
        Area = region.Area,
        Lat = region.Lat,
        Long = region.Long,
        Name = region.Name,
        Population = region.Population,
    };

    return CreatedAtAction(nameof(AddRegionAsync), new { id = regionDTO.Id }, regionDTO);
}
````
  - we don't want to use Models.DTO.Region as the argument type because that will expose the ID field, and we want to set that in the API
  - so we create a new contract in Models => DTO => add new class 'AddRegionRequest.cs'
  - steps:
    - convert request(DTO) to domain model
    - pass details to Repository
    - convert back to DTO => we don't pass domain model, we pass DTO back to user

DELETE delete region
- iregionRepository `        Task<Region> DeleteAsync(Guid id);`
- RegionRepository ctrl + . over class name
````c#
public async Task<Region> DeleteAsync(Guid id)
{
    var region = await GetAsync(id);
    if (region == null)
    {
        return null;
    }

    // Delete the region
    dbContext.Regions.Remove(region);
    await dbContext.SaveChangesAsync();

    return region;
}
````
- in RegionsController
````c#
[HttpDelete]
[Route("{id:guid}")]
public async Task<IActionResult> DeleteRegionAsync(Guid id)
{
    var region = await regionRepository.DeleteAsync(id);
    if (region == null)
    {
        return NotFound();
    }

    var regionDTO = mapper.Map<Models.DTO.Region>(region);

    return Ok(regionDTO);
}
````

PUT updateRegion
- in IRegionRepository `        Task<Region> UpdateAsync(Guid id, Region updatedRegion);`
- in RegionRepository 
````c#
public async Task<Region> UpdateAsync(Guid id, Region updatedRegion)
{
    var existingRegion = await GetAsync(id);
    if (existingRegion == null)
    {
        return null;
    }

    existingRegion.Code = updatedRegion.Code;
    existingRegion.Name = updatedRegion.Name;
    existingRegion.Area = updatedRegion.Area;
    existingRegion.Lat = updatedRegion.Lat;
    existingRegion.Long = updatedRegion.Long;
    existingRegion.Population = updatedRegion.Population;
    await dbContext.SaveChangesAsync();

    return existingRegion;
}
````
- in RegionsController
````c#
[HttpPut]
[Route("{id:guid}")]
public async Task<IActionResult> UpdateRegionAsync(
  [FromRoute] Guid id,  // we decorate this with 'fromRoute' so its obvious that its coming from the route 
  [FromBody] Models.DTO.UpdateRegionRequest updatedRegion // and so its obvious its coming from the body
  )
{
    // Convert the DTO to domain model
    var region = new Models.Domain.Region()
    {
        Code = updatedRegion.Code,
        Area = updatedRegion.Area,
        Lat = updatedRegion.Lat,
        Long = updatedRegion.Long,
        Name = updatedRegion.Name,
        Population = updatedRegion.Population,
    };

    // Update Region using repository 
    var response = await regionRepository.UpdateAsync(id, region);

    // if null, then NotFound
    if (response == null)
    {
        return NotFound();
    }

    // if successful, convert domain back to DTO 
    var regionDTO = mapper.Map<Models.DTO.Region>(region);

    // return OK response
    return Ok(regionDTO);
}
````

---

### Creating Walks Controller and Implementing CRUD

try to DIY this: 
- create controller
- create IWalksRepository
- create WalksRepository
- implement CRUD on controller
- do one at a time 
- same apis:
  - GetAllWalksAsync
  - GetWalkAsync
  - AddWalkAsync
  - DeleteWalkAsync
  - UpdateWalkAsync

#### Navigation Properties
- In models.dto.walks:
````c#
    public class Walk
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Length { get; set; }
        public Guid RegionId { get; set; }
        public Guid WalkDifficultyId { get; set; }

        // Navigation Property
        public Region Region { get; set; }
        public WalkDifficulty WalkDifficulty { get; set; }
    }
````
- we need to tell entity framework to be able to fetch Region and WalkDifficulty (else they will be null)
  - navigation properties in entity framework provides a way to navigate an association between two entity types
  - Every object can have a navigation property for every relationship in which it participates 
-  in our case a walk can only have one region, and one walkDifficulty.
- In WalksRepository.cs:
````c#
public async Task<IEnumerable<Walk>> GetAllAsync()
{
    return await dbContext.Walks
        .Include(x => x.Region)
        .Include(x => x.WalkDifficulty)
        .ToListAsync();
}
````
- Now checking in swagger, our GetAllAsync (which is called in walksController 'GetAllWalksAsync') returns the Region and the WalkDifficulty as well. Great success


---

### Creating WalkDifficulty Controller and CRUD

#### Flow of adding controllers 
looks something like this:
1. create controller, i.e. WalkDifficultyController.cs 
2. Add in the method you want to use, i.e. GetAllAsync with code that might not exist yet, i.e.
````c#
[HttpGet]
public async Task<IActionResult> GetAllAsync()
{
    var walkDifficulties = await walkDifficultyRepository.GetAllAsync();
    var walkDifficultiesDto = mapper.Map<List<Models.DTO.WalkDifficulty>>(walkDifficulties);

    return Ok(walkDifficultiesDto);
}
````
3. Add in any dependency injection to the controllers constructor, i.e. the mapper and the repository (Repository pattern)
4. Fix up the pseudo code written in 2 that doesn't exist yet, i.e. create the repository 
  - create IWalkDifficultyRepository
    - add actions as you go, i.e. for above we need `Task<IEnumerable<WalkDifficulty>> GetAllAsync();`
    - in program.cs, add the scoped dependency: `builder.Services.AddScoped<IWalkDifficultyRepository, WalkDifficultyRepository>();`
  - create WalkDifficultyRepository, inherit from IWalkDifficultyRepository, implement missing members
    - dependency inject the db connection
    - create / fill in the GetAllAsync method
5. Repeat this for all CRUD actions
  - GetAllAsync (/)
  - GetAsync (/)
    - involves setting the decorator `[Route("{id:guid}")]` and getting it [FromRoute]
  - CreateAsync (/)
    - involves getting it [FromBody] and creating a DTO for `AddWalkDifficultyRequest`
  - UpdateAsync (/)
    - involves getting id fromRoute and `UpdateDifficultyRequest` fromBody
  - DeleteAsync (/)
    - involves setting the decorator `[Route("{id:guid}")]` and getting it [FromRoute]

---

### Validations in ASP.NET Core Web API

- clients can now consume our API to get this info
- we need to be cautious about what data they are sending to our API, thats where validations come into play 
  - we need to validate the data is correct and in the correct format 
  - we do this using 400 "bad request" status code. server cannot or will not process the request due to something that is percieved to be a client error. 
  - we need to do this so data base doesn't get junk data, and API doesn't throw random exceptions 

- What endpoints do we need to validate? i.e. looking at RegionController
  - we aren't passing any data to the `getAllRegionsAsync` data, so don't need to worry about this endpoint
  - `GetRegionAsync` is being passed a Guid. 
    - We are already protecting this using the guid validator we have in the route 
    - if we don't find anything in the DB, we are returning a not found 404. 
    - this endpoint doesnt need validations
  - `AddRegionAsync` takes an addRegionRequest argument, so we need to validate it
  - `DeleteRegionAsync` has a decorator validating that the argument is a guid already, no need to validate
  - `UpdateRegionAsync` takes an updateRegionRequest, so we need to validate it 

#### Validating Region Controller - AddRegionRequest Model 

- AddRegionAsync is taking an AddRegionRequest argument 
  - we expect the client to give the properties as expected in AddRegionRequest 
````c#
[HttpPost]
[ActionName("AddRegionAsync")]
public async Task<IActionResult> AddRegionAsync(Models.DTO.AddRegionRequest addRegionRequest)
{
    // validate the request (addRegionRequest) (we're adding this)
    if (!ValidateAddRegionAsync(addRegionRequest))
    {
        return BadRequest(ModelState); // sends the modelState errors, can see in Swagger
    }

    // Request(DTO) to domain model
    // pass details to repository
    // convert back to DTO

    return CreatedAtAction(nameof(AddRegionAsync), new { id = regionDTO.Id }, regionDTO);
}

// now in our same API class, add a private methods region and the validator: 
#region Private methods
private bool ValidateAddRegionAsync(Models.DTO.AddRegionRequest addRegionRequest)
{
    if (addRegionRequest == null)
    {
        ModelState.AddModelError(nameof(addRegionRequest),
            "Add Region Data is requiredcannot be null or empty or white space.");

        return false;
    }

    if (string.IsNullOrWhiteSpace(addRegionRequest.Code))
    {
        ModelState.AddModelError(nameof(addRegionRequest.Code), 
            $"{nameof(addRegionRequest.Code)} cannot be null or empty or white space.");
    }

    if (string.IsNullOrWhiteSpace(addRegionRequest.Name))
    {
        ModelState.AddModelError(nameof(addRegionRequest.Name),
            $"{nameof(addRegionRequest.Name)} cannot be null or empty or white space.");
    }

    if (addRegionRequest.Area <= 0)
    {
        ModelState.AddModelError(nameof(addRegionRequest.Area),
            $"{nameof(addRegionRequest.Area)} cannot be less than or equal to zero.");
    }

    if (addRegionRequest.Population < 0)
    {
        ModelState.AddModelError(nameof(addRegionRequest.Population),
            $"{nameof(addRegionRequest.Population)} cannot be less than zero.");
    }

    if (ModelState.ErrorCount > 0)
    {
        return false;
    }

    return true;
}

#endregion
````
- each property could have one or more validations, entirely up to us how much validation we want. 


#### Validating Region Controller - UpdateRegionRequest Model

- Basically the exact same as above.
- he duplicates the method (in case it grows to be different)

#### Validating Walks Controller 

- `GetAllWalksAsync` takes no arguments, no need to validate
- `GetWalksAsync` only takes a guid, and the decorator takes care of validating this 
- `AddWalkAsync` takes an addWalkRequest, we will need to validate:
  - add code here after:
````c#
// in the top of AddWalkAsync:
if (!await ValidateAddWalkAsync(addWalkRequest))
{
    return BadRequest(ModelState);
}

private async Task<bool> ValidateAddWalkAsync(Models.DTO.AddWalkRequest addWalkRequest)
{
  if (addWalkRequest == null)
  {
      ModelState.AddModelError(nameof(addWalkRequest), "Add Walk data cannot be null.");

      return false;
  }

  if (string.IsNullOrWhiteSpace(addWalkRequest.Name))
  {
      ModelState.AddModelError(nameof(addWalkRequest.Name),
          $"{nameof(addWalkRequest.Name)} cannot be null or empty or white space.");
  }

  if (addWalkRequest.Length <= 0)
  {
      ModelState.AddModelError(nameof(addWalkRequest.Length),
          $"{nameof(addWalkRequest.Length)} cannot be less than or equal to zero.");
  }

  // RegionId and WalkDifficultyId both have to be valid guids and have to exist
  // need to inject region repository and walkdifficulty repository to check these.
  var region = await regionRepository.GetAsync(addWalkRequest.RegionId);
  if (region == null)
  {
      ModelState.AddModelError(nameof(addWalkRequest.RegionId),
          $"{nameof(addWalkRequest.RegionId)} is an invalid region id.");
  }

  var walkDifficulty = await walkDifficultyRepository.GetAsync(addWalkRequest.WalkDifficultyId);
  if (walkDifficulty == null)
  {
      ModelState.AddModelError(nameof(addWalkRequest.WalkDifficultyId),
          $"{nameof(addWalkRequest.WalkDifficultyId)} is an invalid walk difficulty id.");
  }

  if (ModelState.ErrorCount > 0)
  {
      return false;
  }

  return true;
}

// func: 

````
- `DeleteWalkAsync` takes a guid, decorator validates this anyway
- `UpdateWalkAsync` takes a updateWalkRequest, we will need to validate:
  - code is basically the same as AddWalkAsync. 

#### Validating Walk Difficulty Controller 

- `GetAllAsync` doesn't take any arguments, thus needs no validation
- `GetAsync` only takes a Guid which is validated by the decorator, thus doesn't need validation
- `AddWalkDifficultyAsync` takes an AddWalkDifficultyRequest, thus DOES require validation **********
- `UpdateAsync` takes a UpdateWalkDifficultyRequest, thus DOES require validation *******************
- `DeleteASync` takes a Guid only which is validated by the decorator, thus doesn't need validation

#### Fluent Validations 

- Another way to validate our requests
- can use fluent validation instead... www.fluentvalidation.net 
- replicate our existing validations using fluent validation 

#### Setting up Fluent Validations 
- right click dependencies => manage nuget packages 
- install `FluentValidation`, `FluentValidation.AspNetCore`, `FluentValidation.DependencyInjectionExtensions`
- below AddSwaggerGen(); add:
````c#
builder.Services.AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<Program>());
````
- from here you just need to create a validators folder in the root, and add them for each DTO argument that the controllers api function takes. i.e.:
````c#
using FluentValidation;

namespace NZWalks.API.Validators
{
    public class AddRegionRequestValidator : AbstractValidator<Models.DTO.AddRegionRequest>
    {
        public AddRegionRequestValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Area).GreaterThan(0);
            RuleFor(x => x.Population).GreaterThanOrEqualTo(0);
        }
    }
}
````

**Adding Fluent Validations to the AddRegionAsync method in RegionController** 
- add a new folder "Validators"
  - create class inside "AddRegionRequestValidator.cs"
- when our request comes inside AddRegionAsync, it should automatically provoke fluentValidation, trigger it, then decide if it wants to sent a bad request or not. 
  - in AddRegionRequestValidator.cs:
    - need to inherit from AbstractValidator, and give it the type (in this case, `Models.DTO.AddRegionRequest`)
    - replicate our manual validator (code below)
- comment our manual validate checking function in AddRegionAsync
- now we have fluent validations on `Models.DTO.AddRegionRequest`, add the `[ApiController]` decorator on our controller, fluent Validations will use the ApiController tag to check if the model state is valid or invalid. 
  - if it fails, it will sent the ModelState to invalid and the execution of the endpoint will never go inside the AddRegionAsync function. 

**did the same thing for WalkDifficultyController, and WalksController**
- Note, for checking if regionId / walkDifficultyId exists for the walkController methods ... it is better to use the manual way
  - fluent validator can do it but it gets very complex talking to the DB etc. 
  - so a combo of fluent validator + manual ones (just only use the db checking validators in the manual one since fluent can do the rest)
  - using above method, fluent validation will be triggered first. so if there are errors there, it will 400 just with them before checking db. 

---

### Authentication and Authorisation in .NET Core Web API

- we have build our API and its ready to be consumed, but before we can make it public we need to understand authentication and authorization 

**Authentication** 

- What is authentication?
  - Authentication is the process of determining a users identity
  - by using Authentication, we check if we trust the user or client or not
  - i.e. can this web app talk to our API?
- Why do we need authentication? 
  - if we keep our API public, anyone with a browser would be able to get all the resources from our API and modify the data we store in our SQL server.
  - they could also delete all the data etc. 

**Authorisation**

- What is authorization? 
  - authorization is the process of determing whether a user has access to a resource
  - by using authorisation, we check if the user has read permissions or write permissions or other kind of roles
  - i.e. does this authenticated user get to delete others data, or only their own? 
- Why do we need authorization?
  - if we trust our react app front end, and in our react app we have two users: normal user (read access) and admin user (read and write access)
  - API will check the roles of the users, to see if they're allowed to only read or read and write 

#### Authentication Flow (JWT) - Json Web Token 

- JWT is an open standard that defines a compact and self-contained way for securely transmitting information between parties as a JSON object. This info can be verified and trusted because it is digitally signed, either using the HMAC algorithm or a public/private key pair using RSA or ECDSA. 
- users => come to website => login => website sends login info to API => api checks user / pass, if correct sends JWT token to website => website then uses JWT token to make calls to API => API checks if token is correct => API returns data to website 


#### Setting up Authentication

- add nuget packages: `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.IdentityModel.Tokens`, `System.IdentityModel.Tokens.Jwt`
- go to appsettings.json
  - below "connectionStrings", section add another "Jwt".
  - Add a key, which is to be kept secret. He just says "i will add a random string of letters which will be my secret"
    - if someone else has this key, they will be able to make JWT tokens for themselves and use the API => aka can't commit this. Can be per environment as well. 
  - Add an issuer and audience. Because this API is issuing this jwt token but also the audience for it, it will be the same for both. This value is the https location its hosted at.
  - Note: Will need to make all this JWT stuff in appsettings based on environment for it to work in non-dev environment 
- go to Program.cs, just before `var app = builder.Build();`, add the following: 
````c#
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true, // validate app issuing this token
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // this is the same as whats in appsettings.json
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    });
````
- just before `app.UseAuthorization();` add: `app.UseAuthentication();` so we make sure we check our app is authenticated before we check for authorization. thats the flow of the middleware  


#### Implementing and Testing Authorisation without Token

- to use authentication we need to put the decorator on our API controllers, under `[Route("someroute")]` add `[Authorize]`.
  - This is telling the client trying to access this controller that you need a valid token to access any of the resources in it.  
  - if we start the app and try to access any of the methods on that controller, we now get a 401 (unauthorized)
  - we check authorisation before we check validators etc 

NOTE:
- if you want to onlyu have some endpoints require authorisation, then you can remove the authorize decorator from the controller class and instead put it on the individual endpoints (or functions) within the controller. 
- he's changed it so only the bottom 3 methods "AddRegionAsync", "DeleteRegionAsync" and "UpdateRegionAsync" require authorisation. 

#### Creating Users and Static User repository

- to generate a token, we need users who have valid login id's and valid passwords. 
- start creating user domain model first (models => domain => user.cs)
- create 'User.cs' class:
````c#
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<string> Roles { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
````
- "for demonstration purposes we will not connect this to a database"
  - "but to show you a working example i will create a static list of 2 users to see how authentication works"
- in repositories creates IUserRepository interface 
````c#
public interface IUserRepository
{
    Task<bool> AuthenticateUserAsync(string username, string password);
}
````
- creates a new repository 'StaticUserRepository.cs' class
  - note this can be used for testing etc later. we can swap this out for a real DB later on.
````c#
    public class StaticUserRepository : IUserRepository
    {
        private List<User> Users = new List<User>()
        {
            new User()
            {
                Id = Guid.NewGuid(),
                Username= "Readonly",
                Email="Readonly@test.com",
                Password="password",
                Roles= new List<string> { "reader" },
                FirstName= "Readonly",
                LastName= "Readonly"
            },
            new User()
            {
                Id = Guid.NewGuid(),
                Username= "Admin",
                Email="Admin@test.com",
                Password="password",
                Roles= new List<string> { "reader", "writer" },
                FirstName= "Admin",
                LastName= "Admin"
            },
        };

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = Users.Find(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) &&
            x.Password == password);

            if (user !=null)
            {
                return true;
            }

            return false;
        }
    }
````
- time to inject this StaticUserRepository in Program.cs
  - because we're using a static user repository instance, we need to use "AddSingleton". it cannot be scoped, because we can only generate it once (due to the fact we're creating new guids)
  - `builder.Services.AddSingleton<IUserRepository, StaticUserRepository>();`


#### Creating AuthController and Login Method

- we will now have another controller API where we will ask users to log in. i.e. they send user name and password to an API 
- creates new controller `AuthController.cs`
- create the loginAsync method, which takes a LoginRequest (create a model/DTO for this):
````c#
public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }}
}
````
- the controller: 
````c#
[ApiController]
[Route("Auth")]
public class AuthController : Controller
{
    private readonly IUserRepository userRepository;

    public AuthController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync(Models.DTO.LoginRequest loginRequest)
    {
        // validate the incoming request (done using fluent validator)

        // check if user is authenticated 
        var isAuthenticated = await userRepository.AuthenticateUserAsync(loginRequest.Username, 
            loginRequest.Password);

        if (isAuthenticated)
        {
            // generate a JWT token and send it back 
        }

        return BadRequest("Username or password is incorrect.");

    }
}
````

#### Creating token handler and Generate Token 

- we want our controllers to be lean and minimalistic, so we will create a token handler repository 
- create a new interface `ITokenHandler.cs` in the repositories folder
````c#
public interface ITokenHandler
{
    Task<string> CreateTokenAsync(User user); // returns a task of string, i.e. the JWT token.
}
````
- create a new class `TokenHandler.cs` in the repositories folder
- we need to get the Key from app settings (the symmetric key) => this is what should be stored securely somehow.
- he did a lot here, not much to understand really in terms of theory. i think this is just "how its done" to generate a JWT token. 
````c#
public class TokenHandler : ITokenHandler
{
    private readonly IConfiguration config;

    public TokenHandler(IConfiguration config)
    {
        this.config = config;
    }

    public Task<string> CreateTokenAsync(User user)
    {
        // create claims 
        var claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
        claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
        claims.Add(new Claim(ClaimTypes.Email, user.Email));

        // loop into roles of users
        user.Roles.ForEach((role) =>
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            config["Jwt:Issuer"],
            config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(15), // 15 minutes till expiry
            signingCredentials: credentials);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
````
- need to add the dependency injection for ITokenHandler in Program.cs `builder.Services.AddScoped<ITokenHandler, NZWalks.API.Repositories.TokenHandler>();`
- now inject the dependency into the AuthController, and use it. We have changed the userRepository slightly so now it returns the user from AuthenticateUSerAsync:
````c#
[HttpPost]
[Route("login")]
public async Task<IActionResult> LoginAsync(Models.DTO.LoginRequest loginRequest)
{
    // validate the incoming request (done using fluent validator)

    // check if user is authenticated 
    var user = await userRepository.AuthenticateUserAsync(loginRequest.Username, 
        loginRequest.Password);

    if (user != null )
    {
        // generate a JWT token and send it back 
        var token = tokenHandler.CreateTokenAsync(user);
        return Ok(token);
    }

    return BadRequest("Username or password is incorrect.");

}
````
- can test it now in Swagger. 
- it should return a token, which you can paste into jwt.io website. you can see here the token has a payload, including name / surname / email / roles etc. 
- add `[authorize]` decorator to the `GetAllRegionsAsync` method
- now use postman, go to https://localhost:7201/regions using a GET request and it should say unauthorised. we need to add the token.
  - get a token using swagger 
  - in the headers tab in postman, add the key "Authorization" and the value as `bearer <paste token>` // Important **************************************************************
  - it now works!

#### Role Based Authorization
- currently with the authorised attribute, both the reader and the writer can access all the endpoints available (as long as theyre valid users)
- we need to use role-based authorisation 
- our first static user is readonly, with the 'reader' role only.
  - the 2nd user is 'readwrite' and has 'reader' and 'writer' roles.
- our first endpoint GetAllRegionsAsync and GetRegionAsync are just reads, but AddRegion, DeleteRegion, Update region will require writes. 
- .net provides us for this. we can call `[Authorize(Roles = "reader")]` with the brackets and give it the roles, or `[Authorize(Roles = "writer")]`
  - if the role has rights it will let it through, if not it will give a 403: forbidden.
  - 403 means you are authenticated but not authorised to access this resource
- we can now test this using swagger and postman using a combo of users / apis 

#### Adding Authentication and Authorisation to All API's
- all the other controllers should follow the same format. GET requests are 'reader' and PUT / POST / DELETE requests are 'writer'. 
- keep the AuthController login endpoint as not requiring an authorize decorator, because everyone needs to be able to access that. 

#### Changing Swagger to add Authentication 
- in program.cs find `builder.Services.AddSwaggerGen();`, we can now add options as an argument 
- change it to this:
````c#
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme()
    {
        Name = "JWT Authentication",
        Description = "Enter a valid JWT bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference()
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {securityScheme, new string[] {} }
    });
});
````
- now when you start the project and go to swagger, notice the "Authorize" button on the top right. Use the login endpoint to get a bearer, and then click the authorze button to put the token in. 
- the token should now give you the authorisation and authentication for whatever login roles you have associated with the user/pass you used. 

#### Add EF Core and Add Users To Database 

- lets replace `StaticUserRepository.cs` with an actual database user model
- we have a domain model User.cs but we will need to change some things
- create a Role.cs domain model first
````c#
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    // navigation Property 
    public List<User_Role> UserRoles { get; set; }
}
````
- create a domain model User_Role.cs as well, this will store info on how the users have roles and how many roles are to one user
````c#
public class User_Role
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } //maps to userId automatically 
    public User User { get; set; } // navigation property 
    public Guid RoleId { get; set; }
    public Role Role { get; set; } // navigation property
}
````
- update User.cs: 
````c#
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // navigation property
    public List<User_Role> UserRoles { get; set; }
}
````


- in NZWalksDbContext.cs:
  - tbh didn't really get what he was doing here. need to research this more
  - added OnModelCreating override, add DbSet's for user / role / user_role.
````c#
public class NZWalksDbContext : DbContext 
{
    public NZWalksDbContext(DbContextOptions<NZWalksDbContext> options): base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User_Role>()
            .HasOne(x => x.Role)
            .WithMany(y => y.UserRoles)
            .HasForeignKey(x => x.RoleId);

        modelBuilder.Entity<User_Role>()
            .HasOne(x => x.User)
            .WithMany(y => y.UserRoles)
            .HasForeignKey(x => x.UserId);
    }

    public DbSet<Region> Regions { get; set; } 
    public DbSet<Walk> Walks { get; set; }
    public DbSet<WalkDifficulty> WalkDifficulty { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User_Role> User_Roles { get; set; }
}
````
- tools => nugetpackage manager => open console 
- `add-migration "adding users"`
- build failed because we changed the roles in user.cs , just comment out whatever fails
- this time it generated the script for us. now we need to run `update-database`
- open sql and we've got users, user_roles, roles 
  - he now uses a SQL query to seed some data. Better idea for me is to create the endpoints?! 
  - might be better to seed some stuff, i.e. the roles ... only need to be seeded, users wont create these. 
  - run script first, then build out sign-up functionality after. 

- we won't be using static users anymore, so make a new repositry 'UserRepository.cs'
- back in user.cs he adds back in `public List<string> Roles { get; set; }`
  - uncomment the user.Roles throughout the code (in the tokenHandler.cs)
````c#
public class UserRepository : IUserRepository
{
    private readonly NZWalksDbContext dbContext;

    public UserRepository(NZWalksDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public async Task<User> AuthenticateUserAsync(string username, string password)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower() &&
        x.Password == password);

        if (user == null)
        {
            return null;
        }

        var userRoles = await dbContext.User_Roles.Where(x => x.UserId == user.Id).ToListAsync();

        if (userRoles.Any())
        {
            user.Roles = new List<string>();
            foreach (var userRole in userRoles)
            {
                var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == userRole.RoleId);
                if (role != null)
                {
                    user.Roles.Add(role.Name);
                }
            }
        }

        user.Password = null; // set the password to null as we don't want this floating around! 
        return user;
    }
}
````

- in program.cs change the singleton StaticUserRepository to: `builder.Services.AddScoped<IUserRepository, UserRepository>();`
  - this is how dependency injection allows us to change dependencies very quickly
- when we start the app to test swagger and try to login, we have an issue with the primary key. 
  - We have just added the Roles property to the domain model User.cs
  - need to add this to the User.cs class:
````c#
[NotMapped] // this attribute
public List<string> Roles { get; set; }
````
- now we can use swagger and get the jwt and it works! 


---

End of the course. 

Thoughts: 
- I would need an endpoint to create users in the auth controller
- I would give everyone read / write, so no roles required. However they will need to be only able to access their own resources, i.e. based on their UserId 
- Need to thoroughly plan the DB schemas. what tables do i need etc. 