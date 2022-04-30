# BlazorObservers

## Introduction
Blazor not support resize events. 
This means that usually, a complicated JSInterop will be required, which adds a lot of boilerplate code to the project.

This library attempts to prevent this issue, by providing a manager, which creates and deletes [JavaScript ResizeObservers](https://developer.mozilla.org/en-US/docs/Web/API/ResizeObserver). 
This allows efficient and clean code to execute dotNET methods on element(s) resize.

## Usage

First add the ResizeObserverRegistrationService to the dependency injection.
```csharp
using BlazorObservers.ObserverLibrary.DI;

...

builder.Services.AddResizeObserverRegistrationService();
```

And add the using statements to the imports.razor
```razor
@using BlazorObservers.ObserverLibrary.Tasks
@using BlazorObservers.ObserverLibrary.Services
```

Then you can inject the ResizeObserverRegistrationService into your razor component.

Now register an element with the OnAfterREnderAsync method, and make sure the registration is removed on disposal.
```csharp

@inject ResizeObserverRegistrationService ResizeObserverRegistrationService
@implements IAsyncDisposable

<div @ref="targetElement" style="width: 100%; height: 100%; background-color: green;"></div>

private ElementReference targetElement { get; set; }
private ObserverTask? taskReference;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
        taskReference = await ResizeObserverRegistrationService.RegisterObserver(
            async () => Console.WriteLine("Hello resizable world"), 
            targetElement);
}

public async ValueTask DisposeAsync()
{
    if (taskReference is not null)
        await ResizeObserverRegistrationService.DeregisterObserver(taskReference);
}

```


## Future
Feature backlog:

- [ ] Make ResizeObserver callback parameters available
- [ ] Add more observers, such as [MutationObserver](https://developer.mozilla.org/en-US/docs/Web/API/MutationObserver) and [IntersectionObserver](https://developer.mozilla.org/en-US/docs/Web/API/IntersectionObserver)
- [ ] Unit testing
- [ ] Performance analysis
- [ ] Auto-package
