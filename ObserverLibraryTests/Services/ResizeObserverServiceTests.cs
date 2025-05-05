using BlazorObservers.ObserverLibrary.JsModels;
using BlazorObservers.ObserverLibrary.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;

namespace BlazorObservers.ObserverLibrary.Tests.Services
{
    [TestFixture]
    public class ResizeObserverServiceTests
    {
        private Mock<IJSRuntime> _jsRuntimeMock = null!;
        private Mock<IJSObjectReference> _jsModuleMock = null!;
        private ResizeObserverService _service = null!;

        private readonly Dictionary<ElementReference, string> _elementIdMap = new();

        [SetUp]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly", Justification = "Mock Setup")]
        public void SetUp()
        {
            _elementIdMap.Clear();

            _jsModuleMock = new Mock<IJSObjectReference>();
            _jsModuleMock
                .Setup(m => m.InvokeAsync<string[]>(
                    "ObserverManager.CreateNewResizeObserver",
                    It.IsAny<object[]>()))
                .Returns((string _, object[] args) =>
                {
                    var elements = args.Skip(2).Cast<ElementReference>().ToArray();

                    List<string> ids = new List<string>(elements.Length);
                    foreach (var element in elements)
                    {
                        if (!_elementIdMap.TryGetValue(element, out var id))
                        {
                            id = Guid.NewGuid().ToString();
                            _elementIdMap[element] = id;
                            ids.Add(id);
                        }
                    }

                    return new ValueTask<string[]>(Task.FromResult(ids.ToArray()));
                });

            _jsModuleMock
                .Setup(m => m.InvokeAsync<string?>(
                    "ObserverManager.StartObserving",
                    It.IsAny<object[]>()))
                .Returns((string _, object[] args) =>
                {
                    var element = (ElementReference)args[1];

                    if (!_elementIdMap.TryGetValue(element, out var id))
                    {
                        id = Guid.NewGuid().ToString();
                        _elementIdMap[element] = id;
                    }

                    return new ValueTask<string?>(Task.FromResult<string?>(id));
                });

            _jsModuleMock
                .Setup(m => m.InvokeAsync<bool>(
                    "ObserverManager.StopObserving",
                    It.IsAny<object[]>()))
                .Returns((string _, object[] args) =>
                {
                    var element = (ElementReference)args[1];
                    return new ValueTask<bool>(Task.FromResult(_elementIdMap.Remove(element)));
                });

            _jsRuntimeMock = new Mock<IJSRuntime>();
            _jsRuntimeMock
                .Setup(r => r.InvokeAsync<IJSObjectReference>(
                    "import",
                    It.Is<object[]>(args => args.Length == 1 && args[0]!.ToString() == "./_content/BlazorObservers/ObserverManager.js")))
                .Returns(new ValueTask<IJSObjectReference>(Task.FromResult(_jsModuleMock.Object)));

            _service = new ResizeObserverService(_jsRuntimeMock.Object);
        }

        [Test]
        public async Task RegisterObserver_SynchronousFunction_ValidatesAndRegisters()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var onObserve = new Action<JsResizeObserverEntry[]>(entries => { });

            var task = await _service.RegisterObserver(onObserve, element);

            Assert.That(task, Is.Not.Null);
            Assert.That(task.ConnectedElements.Count, Is.EqualTo(1));

            _jsModuleMock.Verify(m =>
                m.InvokeAsync<string[]>(
                    "ObserverManager.CreateNewResizeObserver",
                    It.Is<object[]>(args => args.Contains(element))),
                Times.Once);
        }

        [Test]
        public async Task RegisterObserver_AsyncFunction_ValidatesAndRegisters()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var onObserve = new Func<JsResizeObserverEntry[], Task>(entries => Task.CompletedTask);

            var task = await _service.RegisterObserver(onObserve, element);

            Assert.That(task, Is.Not.Null);
            Assert.That(task.ConnectedElements.Count, Is.EqualTo(1));

            _jsModuleMock.Verify(m =>
                m.InvokeAsync<string[]>(
                    "ObserverManager.CreateNewResizeObserver",
                    It.Is<object[]>(args => args.Contains(element))),
                Times.Once);
        }

        [Test]
        public async Task StartObserving_ValidElement_AddsToObservedElements()
        {
            var element1 = new ElementReference(Guid.NewGuid().ToString());
            var element2 = new ElementReference(Guid.NewGuid().ToString());
            var onObserve = new Action<JsResizeObserverEntry[]>(entries => { });
            var task = await _service.RegisterObserver(onObserve, element1);

            var result = await _service.StartObserving(task, element2);

            Assert.That(result, Is.True);
            Assert.That(task.ConnectedElements.Count, Is.EqualTo(2));

            _jsModuleMock.Verify(m =>
                m.InvokeAsync<string?>(
                    "ObserverManager.StartObserving",
                    It.Is<object[]>(args =>
                        args.Length == 2 &&
                        args[0]!.ToString() == task.TaskId.ToString() &&
                        args[1].GetType() == typeof(ElementReference) && ((ElementReference)args[1]).Equals(element2))),
                Times.Once);
        }

        [Test]
        public async Task StopObserving_ValidElement_RemovesFromObservedElements()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var onObserve = new Action<JsResizeObserverEntry[]>(entries => { });
            var task = await _service.RegisterObserver(onObserve, element);

            var result = await _service.StopObserving(task, element);

            Assert.That(result, Is.True);
            Assert.That(task.ConnectedElements.Count, Is.EqualTo(0));

            _jsModuleMock.Verify(m =>
                m.InvokeAsync<bool>(
                    "ObserverManager.StopObserving",
                    It.Is<object[]>(args =>
                        args.Length == 2 &&
                        args[0]!.ToString() == task.TaskId.ToString() &&
                        args[1].GetType() == typeof(ElementReference) && element.Equals((ElementReference)args[1]))),
                Times.Once);
        }

        [Test]
        public async Task DeregisterObserver_RemovesObserverAndElements()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var task = await _service.RegisterObserver(entries => { }, element);

            await _service.DeregisterObserver(task);

            Assert.That(task.ConnectedElements.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DeregisterObserver_ById_RemovesObserverAndElements()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var task = await _service.RegisterObserver(entries => { }, element);

            await _service.DeregisterObserver(task.TaskId);

            Assert.That(task.ConnectedElements.Count, Is.EqualTo(0));
        }

        [Test]
        public void RegisterObserver_ThrowsOnNullAction()
        {
            Assert.That(
                async () => await _service.RegisterObserver((Action<JsResizeObserverEntry[]>)null!, new ElementReference()),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void RegisterObserver_ThrowsOnNullElement()
        {
            Assert.That(
                async () => await _service.RegisterObserver(entries => Task.CompletedTask, null!),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void StartObserving_ThrowsOnNullTask()
        {
            Assert.That(
                async () => await _service.StartObserving(null!, new ElementReference()),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void StopObserving_ThrowsOnNullTask()
        {
            Assert.That(
                async () => await _service.StopObserving(null!, new ElementReference()),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}
