using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorObservers.ObserverLibrary.Models
{
    internal readonly struct ElementRegistration<TTask, TEntry> where TTask : ObserverTask<TEntry[]>
    {
        public readonly ElementReference ElementReference;
        public readonly TTask TaskReference;

        public ElementRegistration(ElementReference elementReference, TTask taskReference)
        {
            ElementReference = elementReference;
            TaskReference = taskReference;
        }
    }
}
