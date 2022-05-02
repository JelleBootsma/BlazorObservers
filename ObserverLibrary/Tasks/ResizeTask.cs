using BlazorObservers.ObserverLibrary.JsModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorObservers.ObserverLibrary.Tasks
{
    public class ResizeTask : ObserverTask<JsResizeObserverEntry[]>
    {
        internal Dictionary<Guid, ElementReference> ConnectedElementes { get; set; }
        public ResizeTask(Func<JsResizeObserverEntry[], Task> taskFunc) : base(taskFunc)
        {
            ConnectedElementes = new Dictionary<Guid, ElementReference>();
        }

        public override Task Execute(JsResizeObserverEntry[] jsData)
        {
            foreach (var dataElement in jsData)
            {
                if (Guid.TryParse(dataElement.TargetElementTrackingId, out Guid trackingId) &&
                    ConnectedElementes.ContainsKey(trackingId))
                {
                    dataElement.TargetElement = ConnectedElementes[trackingId];
                }
            }
            return base.Execute(jsData);
        }
    }

   

}
