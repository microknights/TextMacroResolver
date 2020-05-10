# TextMacroResolver
A common thing in any IT solution is to present text to a customer. The text is typical written by a non-tech person, and usually maintained in such way that it can be changed dynamically.
The challenge here is that within the text there is reference to data specific information like personal information, different kind of numbers and so on.

So a common solution to this is to define macros, that will be substituted with values before the text is presented.

## Text with macros

```
Hi {{user.name.first}}

Aliquam tristique neque nec turpis viverra rhoncus.
Nulla facilisi.
Vivamus euismod fermentum fermentum.
Nulla lacinia nec lacus et lobortis.
Maecenas facilisis augue erat, non rhoncus sapien vestibulum id.
Pellentesque arcu ante, placerat nec turpis nec, interdum suscipit orci.
Vivamus at nisi nec ante eleifend tristique facilisis dignissim elit.
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.
Integer sed faucibus lorem, non ultrices velit. Proin lacinia aliquet libero, vitae dapibus orci lobortis sed.
Nam consectetur elit ut vestibulum pretium. Aliquam posuere egestas justo et semper.
In euismod ligula non imperdiet porta. In hac habitasse platea dictumst.
Sed tempus ex id gravida blandit. Nulla facilisi.

Regards {{advisor.name}}
```

## The macro pattern
The macro pattern is configurable, since the text can be of many different kinds. By default the macro starts with `{{` and ends with `}}`.

This can be changed with `TextMacroResolverOptions` that have two properties; `MacroNamePrefix` and `MacroNamePostfix`.

### Text format patterns
Since a macro is resolved using the same format pattern, it is possible within the macro definition to define an explicit text format.

So if you have a macro `{{user.account.amount}}` that is resolved normally using text format `N0`, you can within the macro override it by doing this `{{user.account.amount:N2}}`.

Also `:` can be replaced in the macro pattern. In `TextMacroResolverOptions` with the property `MacroNameTextFormatSeperator`.

## The macro value resolving
Each macro must have a resolver, so the macro string can be replaced with the resolved value.

```
[Macro("user.name.first", Description: "", Synchronize: true)]
public class UserNameFirstMacroValueResolver : MacroValueResolver
{
    private UserDbContext _userDbContext;
    private MacroContext _MacroValueContext;
    public UserNameFirstMacroValueResolver( MacroValueContext macroContext, UserDbContext userDbContext )
    {
      _macroContext = macroContext;
      _userDbContext = userDbContext;
    }
    
    public override async Task<MacroValueResult> Resolve(string originalMacroName, string textFormat)
    {
        var user = await _dbContext.Users.FindAsync(_macroContext.UserId);
        return new MacroValueResult(originalMacroName, user.GetFirstName(_macroContext.CultureInfo), textFormat));
    }
}
```

### Type declartion
You macro value resolver must either inherit abstract class `MacroValueResolver`, or interface `IMacroValueResolver`.
Also the attribute `[Macro(.....)]` must be set on the class with a name.

### Using dependency injection.
Each `MacroValueResolver` will be resolved using Dependency Injection, and therefor they can individually get what is required to resolve the value through the constructor. 
All `MacroValueResolver` must be registered at `Scope` level, and will be done if `services.AddTextMacroResolver(....)` is used in your startup.

### Using context state
Since many `MacroValueResolver` must resolve data that depends on a context, or application state. It is possible to register a specialized `MacroValueContext` type. 
You can specialize (inherit) `MacroValueContext` with your own properties, or make a complete different type. The context type, also registered at `Scope` level in the DI, can then have its properties set just before the actual macro value resolving.

### Using DbContext synchronized
The `TextMacroResolver` is resolving macros in a task async manner, meaning that they will be executed in parallel. 
This is good for performance, if there is a lot of resource depended macros, but also bad since some resources do not allow parallel work - like Entity Framework Core.
To solve this non-parallel-execution there is a `Synchronize` flag. Setting `Synchronize` to true, means it will be executed one at time with all others synchronized `MacroValueResolver`.

## Startup
Now lets see how a `Startup` could look like.

```
.AddTextMacroResolver<MyMacroValueContext>( options =>
{
	options.AssembliesWithMacroValueResolverTypes = new[] {Assembly.GetExecutingAssembly()};
})
```
one or more assemblies can be specified, and they will be scanned and the found `MacroValueResolver` types will be registered for DI usage.

or
```
.AddTextMacroResolver<MyMacroValueContext>( options =>
{
	options.MacroValueResolverTypes = new[] {typeof(UserNameFirstMacroValueResolver),.... };
})
```
where one or more types can be specified, and the will be registered for DI usage.

_the `TextMacroResolverOptions` is registered as Singleton._

## Execution
With all `MacroValueResolver` registered, it is time to do some macro stuff:

### supported macro names
```
var list = textMacroResolver.GetResolverMacroNames()
```
gives a list of all registered `MacroValueResolver` macro names.

### individual macro resolving
this can be used to call:
```
var result = await textMacroResolver.ResolveMacro( macroName );
```
which returns a result with the value, or an exception if `result.IsResolved == false`.


### full text macro resolving
doing and complete text replacement, call:
```
// Set context related info.
var context = serviceProvider.GetRequiredService<MyMacroValueContext>();
context.UserId = 42;
// Replace macros
var result = await textMacroResolver.ResolveText( textWithMacros );
```
if `result.IsResolved == true` then all macros are replaced successfully.

### just extract macro names from text.
```
var list = textMacroResolver.ExtractMacros( textWithMacros );
```
_also look at the UnitTest project, where you can see it in action._


# NuGet Package
`PM> Install-Package MicroKnights.TextMacroResolver`


