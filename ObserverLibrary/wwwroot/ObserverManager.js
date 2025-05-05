
export class ObserverManager {

    /**
     * Generate a Guid v4 (RFC4122)
     * 
     * Adaptation of code from https://stackoverflow.com/a/2117523
     * @returns {string} new Guid
     */
    static #GetGuid() {
        if (crypto?.randomUUID != null) return crypto.randomUUID();
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }

    static ActiveObservers = {};

    /**
     * Register a resize observer
     * @param {string} id
     * @param {object} dotNetRef
     * @param {...Element} els
     * @returns {string[]} Generated ids for the tracked elements in the same order as the given elements 
     */
    static CreateNewResizeObserver(id, dotNetRef, ...els) {
        const callback = (entries, obsCallback) => {
            const dotNetArguments = [];
            for (const entry of entries) {
                dotNetArguments.push(this.#CreateResizeCallbackObject(entry, id))
            }
            dotNetRef.invokeMethodAsync("Execute", dotNetArguments);
        }
        const obs = new ResizeObserver(callback)
        const ids = [];

        for (const el of els) {
            const elementTrackingId = this.#GetGuid();
            el.setAttribute(this.#GetObserverTrackingAttributeName(id), elementTrackingId);
            ids.push(elementTrackingId);
            obs.observe(el);
        }
        this.ActiveObservers[id] = obs;
        return ids;
    }

    /**
     * Register an intersection observer
     * @param {string} id
     * @param {object} dotNetRef
     * @param {IntersectionObserverInit} options
     * @param {...Element} els
     * @returns {string[]} Generated ids for the tracked elements in the same order as the given elements 
     */
    static CreateNewIntersectionObserver(id, dotNetRef, options, ...els) {
        const callback = (entries, obsCallback) => {
            const dotNetArguments = [];
            for (const entry of entries) {
                dotNetArguments.push(this.#CreateIntersectionCallbackObject(entry, id))
            }
            dotNetRef.invokeMethodAsync("Execute", dotNetArguments);
        }
        const obs = new IntersectionObserver(callback, options)
        const ids = [];
        for (const el of els) {
            const elementTrackingId = this.#GetGuid();
            el.setAttribute(this.#GetObserverTrackingAttributeName(id), elementTrackingId);
            ids.push(elementTrackingId);
            obs.observe(el);
        }
        this.ActiveObservers[id] = obs;
        return ids;
    }


    /**
     * (Deprecated) Disconnect and delete a existing observer
     * Exists for backwards compatibility with the old API to avoid cache mismatch issues
     * @param {string} observerId
     * @returns
     */
    static RemoveResizeObserver(observerId) {
        return this.RemoveObserver(observerId);
    }

    /**
    * Disconnect and delete a existing observer
    * @param {string} observerId
    */
    static RemoveObserver(observerId) {
        if (!this.ActiveObservers[observerId]) return;
        this.ActiveObservers[observerId].disconnect();
        delete this.ActiveObservers[observerId];
    }

    /**
     * Add a new element to an existing observer
     * @param {string} observerId
     * @param {Element} element
     */
    static StartObserving(observerId, element) {

        if (!this.ActiveObservers[observerId]) return null;
        let obs = this.ActiveObservers[observerId];
        let elementTrackingId = this.#GetGuid();
        element.setAttribute(this.#GetObserverTrackingAttributeName(observerId), elementTrackingId);
        obs.observe(element);
        return elementTrackingId;
    }

    /**
     * Add a new element to an existing observer
     * @param {string} observerId
     * @param {Element} element
     */
    static StopObserving(observerId, element) {
        if (!this.ActiveObservers[observerId]) return false;
        let obs = this.ActiveObservers[observerId];
        obs.unobserve(element);
        element.removeAttribute(this.#GetObserverTrackingAttributeName(observerId));
        return true;
    }

    /**
     * Generate serializable resize object for DotNET
     * @param {ResizeObserverEntry} callbackEl
     * @param {string} observerId
     * @returns {object} Serialize object with all required info for dotNet
     */
    static #CreateResizeCallbackObject(callbackEl, observerId) {
        let result = {};
        result.borderBoxSize = this.#ConvertResizeObserverSizeObject(callbackEl.borderBoxSize[0]);
        result.contentBoxSize = this.#ConvertResizeObserverSizeObject(callbackEl.contentBoxSize[0]);
        result.contentRect = callbackEl.contentRect;
        result.targetTrackingId = callbackEl.target.getAttribute(this.#GetObserverTrackingAttributeName(observerId));
        return result;
    }

    /**
     * Generate serializable intersection object for DotNET
     * @param {IntersectionObserverEntry} callbackEl
     * @param {string} observerId
     * @returns {object} Serialize object with all required info for dotNet
     */
    static #CreateIntersectionCallbackObject(callbackEl, observerId) {
        return {
            time: callbackEl.time,
            rootBounds: callbackEl.rootBounds || null,
            boundingClientRect: callbackEl.boundingClientRect,
            intersectionRect: callbackEl.intersectionRect,
            isIntersecting: callbackEl.isIntersecting,
            intersectionRatio: callbackEl.intersectionRatio,
            targetTrackingId: callbackEl.target.getAttribute(this.#GetObserverTrackingAttributeName(observerId))
        };
    }

    /**
     * Get the attribute name used to track elements belonging to a specific observer
     * @param {string} observerId
     */
    static #GetObserverTrackingAttributeName(observerId) {
        return `ObserverElementTrackingId-${observerId}`;
    }



    /**
     * Convert a ResizeObserverSize object to a serializable object
     * @param {ResizeObserverSize} [input]
     * @return {object}
     */
    static #ConvertResizeObserverSizeObject(input) {
        let result = {};
        result.blockSize = input?.blockSize ?? 0;
        result.inlineSize = input?.inlineSize ?? 0;
        return result;
    }

}