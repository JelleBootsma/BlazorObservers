// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.


export class ObserverManager {


    static ActiveResizeObservers = {};

    /**
     * Register a resize observer
     * @param {string} id
     * @param {object} dotNetRef
     * @param {...Element} els
     */
    static CreateNewResizeObserver(id, dotNetRef, ...els) {
        let callback = () => { dotNetRef.invokeMethodAsync("Execute"); }
        let obs = new ResizeObserver(callback)
        for (let el of els) {
            obs.observe(el);
        }
        this.ActiveResizeObservers[id] = obs;

    }

    /**
    * Disconnect and delete a resize observer
    * @param {string} observerId
    */
    static RemoveResizeObserver(observerId) {
        if (!this.ActiveResizeObservers[observerId]) return;
        this.ActiveResizeObservers[observerId].disconnect();
        delete this.ActiveResizeObservers[observerId];
    }

}