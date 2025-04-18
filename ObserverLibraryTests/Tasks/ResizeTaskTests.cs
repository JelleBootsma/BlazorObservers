using BlazorObservers.ObserverLibrary.JsModels;
using BlazorObservers.ObserverLibrary.Tasks;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;

namespace BlazorObservers.ObserverLibrary.Tests.Tasks
{
    [TestFixture]
    public class ResizeTaskTests
    {
        private List<JsResizeObserverEntry[]> _executedData;
        private ResizeTask _resizeTask;

        [SetUp]
        public void SetUp()
        {
            _executedData = new List<JsResizeObserverEntry[]>();
            _resizeTask = new ResizeTask(entries =>
            {
                _executedData.Add(entries);
                return ValueTask.CompletedTask;
            });
        }

        [Test]
        public async Task Execute_WithValidTrackingId_SetsTargetElement()
        {
            // Arrange
            var element = new ElementReference(Guid.NewGuid().ToString());
            var trackingId = Guid.NewGuid();
            _resizeTask.ConnectedElements[trackingId] = element;

            var entries = new[]
            {
                new JsResizeObserverEntry { TargetElementTrackingId = trackingId.ToString() }
            };

            // Act
            await _resizeTask.Execute(entries);

            // Assert
            Assert.AreEqual(1, _executedData.Count);
            Assert.AreEqual(element, entries[0].TargetElement);
        }

        [Test]
        public async Task Execute_WithInvalidTrackingId_DoesNotSetTargetElement()
        {
            // Arrange
            var entries = new[]
            {
                new JsResizeObserverEntry { TargetElementTrackingId = "invalid-guid" }
            };

            // Act
            await _resizeTask.Execute(entries);

            // Assert
            Assert.AreEqual(1, _executedData.Count);
            Assert.IsNull(entries[0].TargetElement);
        }

        [Test]
        public async Task Execute_WhenPaused_DoesNotExecute()
        {
            // Arrange
            _resizeTask.HaltTaskTriggering();

            var entries = new[]
            {
                new JsResizeObserverEntry { TargetElementTrackingId = Guid.NewGuid().ToString() }
            };

            // Act
            await _resizeTask.Execute(entries);

            // Assert
            Assert.IsEmpty(_executedData);
        }

        [Test]
        public async Task Execute_WhenOnlyTriggerLast_DebouncesExecution()
        {
            // Arrange
            _resizeTask.OnlyTriggerLast(100); // 100 ms delay

            var entries1 = new[] {
                new JsResizeObserverEntry { TargetElementTrackingId = Guid.NewGuid().ToString() }
            };

            var entries2 = new[] {
                new JsResizeObserverEntry { TargetElementTrackingId = Guid.NewGuid().ToString() }
            };

            // Act
            var valueTask1 = _resizeTask.Execute(entries1);
            var valueTask2 = _resizeTask.Execute(entries2);

            // Wait long enough to trigger debounce
            await Task.Delay(200);

            await valueTask1;
            await valueTask2;

            // Assert
            Assert.AreEqual(1, _executedData.Count);
            Assert.AreEqual(entries2, _executedData[0]); // Only latest should execute
        }

        [Test]
        public async Task ResumeTaskTriggering_AllowsExecution()
        {
            // Arrange
            _resizeTask.HaltTaskTriggering();
            _resizeTask.ResumeTaskTriggering();

            var entries = new[] {
                new JsResizeObserverEntry { TargetElementTrackingId = Guid.NewGuid().ToString() }
            };

            // Act
            await _resizeTask.Execute(entries);

            // Assert
            Assert.AreEqual(1, _executedData.Count);
        }
    }
}
