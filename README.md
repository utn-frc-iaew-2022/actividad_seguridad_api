
 # Actividad Seguridad API - Desarrollo Autenticación y Autorización de TodoAPI

## Autenticación 
1. Para que la API tenga autenticación agregar el siguiente código al archivo Program.cs antes de la linea builder.Build():

```csharp
//Configuracion para Validar Autenticacion.
builder.Services.AddAuthentication(options  =>
{
	options.DefaultAuthenticateScheme  =  JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme  =  JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options  =>
{
	// Agregar URL de su cuenta de Auth0 de cada uno.
	options.Authority  =  "https://xyz.auth0.com/";
	// Agregar audiencia de la API.
	options.Audience  =  "https://api.example.com";
});

var  app  =  builder.Build();
```

2. En el archivo **TodoItemsController.cs** agregar un decorado **[Authorize]** para indicar que operaciones deben validar autenticación y cuales no. Si lo definimos a nivel de Controller aplica a todas las operaciones de ese controlador:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        ...
```

3. Probar desde Postman.
 
a. Compilar e Iniciar TodoAPI. 

b. Abrir Postman y generar un access_token. 

c. Agregar un nuevo request en Postman para GET /api/todoitems.

d. Agregar un header **Authorization** de la siguente forma:
	- Authorization: Bearer [access_token]
	
e. Ejecutar el request y esperar la respuesta 200 OK. En caso de tener una respuesta 401   Unauthorized. 



## Autorización
1. Crear una carpeta de nombre **Authorization** en la raiz del proyecto con VSCode.
2. Crear el archivo **HasScopeHandler.cs** con el siguiente código: 
```csharp
// HasScopeHandler.cs

using Microsoft.AspNetCore.Authorization;

public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
    {
        // If user does not have the scope claim, get out of here
        if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
            return Task.CompletedTask;

        // Split the scopes string into an array
        var scopes = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == requirement.Issuer).Value.Split(' ');

        // Succeed if the scope array contains the required scope
        if (scopes.Any(s => s == requirement.Scope))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

3. Crear el archivo **HasScopeRequirement.cs** con el siguiente código: 

```csharp
// HasScopeRequirement.cs

using Microsoft.AspNetCore.Authorization;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Issuer { get; }
    public string Scope { get; }

    public HasScopeRequirement(string scope, string issuer)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}
```
4. Para que TodoAPI autorice se debe agregar el siguiente codigo al archivo **Program.cs ** antes de la linea builder.Build():
```csharp
//Configuracion para Validar Autorización. 
builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("read:todoitems", policy => policy.Requirements.Add(new HasScopeRequirement("read:todoitems", "https://dev-utn-frc-iaew.auth0.com/")));
        options.AddPolicy("write:todoitems", policy => policy.Requirements.Add(new HasScopeRequirement("write:todoitems", "https://dev-utn-frc-iaew.auth0.com/")));
    });
    
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
```

5. En **Program.cs ** agregar la siguente linea despues de **app.UseAuthentication();**:
```csharp
//Habilitamos Autenticacion.
app.UseAuthentication();

//Habilitamos Autorización. 
app.UseAuthorization();

```

6. Agregar validaciones de scope **read:todoitems** en operaciones GET con el decorado **[Authorize]**:
```csharp
....
    // GET: api/TodoItems/5
        [HttpGet("{id}")]
        [Authorize("read:todoitems")]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
....

```
7. Agregar validaciones de scope **write:todoitems** en operaciones POST/PUT/DELETE con el decorado **[Authorize]**::
```csharp
....
        [HttpPost]
        [Authorize("write:todoitems")]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
....

```
