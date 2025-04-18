using System.Xml.Linq;
using BlazorObservers.ObserverLibrary.JsModels;
using BlazorObservers.ObserverLibrary.Services;
using BlazorObservers.ObserverLibrary.Tasks;
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
            // clear the elementIdMap before each test
            _elementIdMap.Clear();

            // Setup the JS module mock
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
                    var taskId = args[0]?.ToString();
                    var element = (ElementReference)args[1];

                    return new ValueTask<bool>(Task.FromResult(_elementIdMap.Remove(element)));
                });


            // Setup the JS runtime mock
            _jsRuntimeMock = new Mock<IJSRuntime>();
            _jsRuntimeMock
                .Setup(r => r.InvokeAsync<IJSObjectReference>(
                    "import",
                    It.Is<object[]>(args => args.Length == 1 && args[0]!.ToString() == "./_content/BlazorObservers/ObserverManager.js")))
                .Returns(new ValueTask<IJSObjectReference>(Task.FromResult(_jsModuleMock.Object)));

            // Create the service under test
            _service = new ResizeObserverService(_jsRuntimeMock.Object);
        }

        [Test]
        public async Task RegisterObserver_SynchronousFunction_ValidatesAndRegisters()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var onObserve = new Action<JsResizeObserverEntry[]>(entries => { });

            var task = await _service.RegisterObserver(onObserve, element);

            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.ConnectedElements.Count);

            _jsModuleMock.Verify(m =>
                m.InvokeAsync<string[]>(
                    "ObserverManager.CreateNewResizeObserver",
                    It.Is<object[]>(args => args.Contains(element))
                ),
                Times.Once);
        }

        [Test]
        public async Task RegisterObserver_AsyncFunction_ValidatesAndRegisters()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var onObserve = new Func<JsResizeObserverEntry[], Task>(entries => Task.CompletedTask);

            var task = await _service.RegisterObserver(onObserve, element);

            Assert.IsNotNull(task);
            Assert.AreEqual(1, task.ConnectedElements.Count);


            _jsModuleMock.Verify(m =>
                m.InvokeAsync<string[]>(
                    "ObserverManager.CreateNewResizeObserver",
                    It.Is<object[]>(args => args.Contains(element))
                ),
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

            Assert.IsTrue(result);
            Assert.AreEqual(2, task.ConnectedElements.Count);
            _jsModuleMock.Verify(m =>
                m.InvokeAsync<string?>(
                    "ObserverManager.StartObserving",
                    It.Is<object[]>(args =>
                        args.Length == 2 &&
                        args[0]!.ToString() == task.TaskId.ToString() &&
                        args[1].GetType() == typeof(ElementReference) && ((ElementReference)args[1]).Equals(element2)
                    )
                ),
                Times.Once);
        }

        [Test]
        public async Task StopObserving_ValidElement_RemovesFromObservedElements()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var onObserve = new Action<JsResizeObserverEntry[]>(entries => { });
            var task = await _service.RegisterObserver(onObserve, element);

            var result = await _service.StopObserving(task, element);

            Assert.IsTrue(result);
            Assert.AreEqual(0, task.ConnectedElements.Count);
            _jsModuleMock.Verify(m =>
                m.InvokeAsync<bool>(
                    "ObserverManager.StopObserving",
                    It.Is<object[]>(args =>
                        args.Length == 2 &&
                        args[0]!.ToString() == task.TaskId.ToString() &&
                        args[1].GetType() == typeof(ElementReference) && element.Equals((ElementReference)args[1])
                    )
                ),
                Times.Once);
        }

        [Test]
        public async Task DeregisterObserver_RemovesObserverAndElements()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var task = await _service.RegisterObserver(entries => { }, element);

            await _service.DeregisterObserver(task);

            Assert.AreEqual(0, task.ConnectedElements.Count);
        }

        [Test]
        public async Task DeregisterObserver_ById_RemovesObserverAndElements()
        {
            var element = new ElementReference(Guid.NewGuid().ToString());
            var task = await _service.RegisterObserver(entries => { }, element);

            await _service.DeregisterObserver(task.TaskId);

            Assert.AreEqual(0, task.ConnectedElements.Count);
        }

        [Test]
        public void RegisterObserver_ThrowsOnNullAction()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.RegisterObserver((Action<JsResizeObserverEntry[]>)null, new ElementReference()));
        }

        [Test]
        public void RegisterObserver_ThrowsOnNullElement()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.RegisterObserver(entries => Task.CompletedTask, null!));
        }

        [Test]
        public void StartObserving_ThrowsOnNullTask()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.StartObserving(null!, new ElementReference()));
        }

        [Test]
        public void StopObserving_ThrowsOnNullTask()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.StopObserving(null!, new ElementReference()));
        }
    }
}
