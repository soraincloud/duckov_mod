#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

internal class StylePropertyAnimationSystem : IStylePropertyAnimationSystem
{
	[Flags]
	private enum TransitionState
	{
		None = 0,
		Running = 1,
		Started = 2,
		Ended = 4,
		Canceled = 8
	}

	private struct AnimationDataSet<TTimingData, TStyleData>
	{
		private const int InitialSize = 2;

		public VisualElement[] elements;

		public StylePropertyId[] properties;

		public TTimingData[] timing;

		public TStyleData[] style;

		public int count;

		private Dictionary<ElementPropertyPair, int> indices;

		private int capacity
		{
			get
			{
				return elements.Length;
			}
			set
			{
				Array.Resize(ref elements, value);
				Array.Resize(ref properties, value);
				Array.Resize(ref timing, value);
				Array.Resize(ref style, value);
			}
		}

		private void LocalInit()
		{
			elements = new VisualElement[2];
			properties = new StylePropertyId[2];
			timing = new TTimingData[2];
			style = new TStyleData[2];
			indices = new Dictionary<ElementPropertyPair, int>(ElementPropertyPair.Comparer);
		}

		public static AnimationDataSet<TTimingData, TStyleData> Create()
		{
			AnimationDataSet<TTimingData, TStyleData> result = default(AnimationDataSet<TTimingData, TStyleData>);
			result.LocalInit();
			return result;
		}

		public bool IndexOf(VisualElement ve, StylePropertyId prop, out int index)
		{
			return indices.TryGetValue(new ElementPropertyPair(ve, prop), out index);
		}

		public void Add(VisualElement owner, StylePropertyId prop, TTimingData timingData, TStyleData styleData)
		{
			if (count >= capacity)
			{
				capacity *= 2;
			}
			int num = count++;
			elements[num] = owner;
			properties[num] = prop;
			timing[num] = timingData;
			style[num] = styleData;
			indices.Add(new ElementPropertyPair(owner, prop), num);
		}

		public void Remove(int cancelledIndex)
		{
			int num = --count;
			indices.Remove(new ElementPropertyPair(elements[cancelledIndex], properties[cancelledIndex]));
			if (cancelledIndex != num)
			{
				VisualElement element = (elements[cancelledIndex] = elements[num]);
				StylePropertyId property = (properties[cancelledIndex] = properties[num]);
				timing[cancelledIndex] = timing[num];
				style[cancelledIndex] = style[num];
				indices[new ElementPropertyPair(element, property)] = cancelledIndex;
			}
			elements[num] = null;
			properties[num] = StylePropertyId.Unknown;
			timing[num] = default(TTimingData);
			style[num] = default(TStyleData);
		}

		public void Replace(int index, TTimingData timingData, TStyleData styleData)
		{
			timing[index] = timingData;
			style[index] = styleData;
		}

		public void RemoveAll(VisualElement ve)
		{
			int num = count;
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				if (elements[num2] == ve)
				{
					Remove(num2);
				}
			}
		}

		public void RemoveAll()
		{
			capacity = 2;
			int length = Mathf.Min(count, capacity);
			Array.Clear(elements, 0, length);
			Array.Clear(properties, 0, length);
			Array.Clear(timing, 0, length);
			Array.Clear(style, 0, length);
			count = 0;
			indices.Clear();
		}

		public void GetActivePropertiesForElement(VisualElement ve, List<StylePropertyId> outProperties)
		{
			int num = count;
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				if (elements[num2] == ve)
				{
					outProperties.Add(properties[num2]);
				}
			}
		}
	}

	private struct ElementPropertyPair
	{
		private class EqualityComparer : IEqualityComparer<ElementPropertyPair>
		{
			public bool Equals(ElementPropertyPair x, ElementPropertyPair y)
			{
				return x.element == y.element && x.property == y.property;
			}

			public int GetHashCode(ElementPropertyPair obj)
			{
				return (obj.element.GetHashCode() * 397) ^ (int)obj.property;
			}
		}

		public static readonly IEqualityComparer<ElementPropertyPair> Comparer = new EqualityComparer();

		public readonly VisualElement element;

		public readonly StylePropertyId property;

		public ElementPropertyPair(VisualElement element, StylePropertyId property)
		{
			this.element = element;
			this.property = property;
		}
	}

	private abstract class Values
	{
		public abstract void CancelAllAnimations();

		public abstract void CancelAllAnimations(VisualElement ve);

		public abstract void CancelAnimation(VisualElement ve, StylePropertyId id);

		public abstract bool HasRunningAnimation(VisualElement ve, StylePropertyId id);

		public abstract void UpdateAnimation(VisualElement ve, StylePropertyId id);

		public abstract void GetAllAnimations(VisualElement ve, List<StylePropertyId> outPropertyIds);

		public abstract void Update(long currentTimeMs);

		protected abstract void UpdateValues();

		protected abstract void UpdateComputedStyle();

		protected abstract void UpdateComputedStyle(int i);
	}

	private abstract class Values<T> : Values
	{
		private class TransitionEventsFrameState
		{
			private static readonly UnityEngine.Pool.ObjectPool<Queue<EventBase>> k_EventQueuePool = new UnityEngine.Pool.ObjectPool<Queue<EventBase>>(() => new Queue<EventBase>(4));

			public readonly Dictionary<ElementPropertyPair, TransitionState> elementPropertyStateDelta = new Dictionary<ElementPropertyPair, TransitionState>(ElementPropertyPair.Comparer);

			public readonly Dictionary<ElementPropertyPair, Queue<EventBase>> elementPropertyQueuedEvents = new Dictionary<ElementPropertyPair, Queue<EventBase>>(ElementPropertyPair.Comparer);

			public IPanel panel;

			private int m_ChangesCount;

			public static Queue<EventBase> GetPooledQueue()
			{
				return k_EventQueuePool.Get();
			}

			public void RegisterChange()
			{
				m_ChangesCount++;
			}

			public void UnregisterChange()
			{
				m_ChangesCount--;
			}

			public bool StateChanged()
			{
				return m_ChangesCount > 0;
			}

			public void Clear()
			{
				foreach (KeyValuePair<ElementPropertyPair, Queue<EventBase>> elementPropertyQueuedEvent in elementPropertyQueuedEvents)
				{
					elementPropertyQueuedEvent.Value.Clear();
					k_EventQueuePool.Release(elementPropertyQueuedEvent.Value);
				}
				elementPropertyQueuedEvents.Clear();
				elementPropertyStateDelta.Clear();
				panel = null;
				m_ChangesCount = 0;
			}
		}

		public struct TimingData
		{
			public long startTimeMs;

			public int durationMs;

			public Func<float, float> easingCurve;

			public float easedProgress;

			public float reversingShorteningFactor;

			public bool isStarted;

			public int delayMs;
		}

		public struct StyleData
		{
			public T startValue;

			public T endValue;

			public T reversingAdjustedStartValue;

			public T currentValue;
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct EmptyData
		{
			public static EmptyData Default = default(EmptyData);
		}

		private long m_CurrentTimeMs = 0L;

		private TransitionEventsFrameState m_CurrentFrameEventsState = new TransitionEventsFrameState();

		private TransitionEventsFrameState m_NextFrameEventsState = new TransitionEventsFrameState();

		public AnimationDataSet<TimingData, StyleData> running;

		public AnimationDataSet<EmptyData, T> completed;

		public bool isEmpty => running.count + completed.count == 0;

		public abstract Func<T, T, bool> SameFunc { get; }

		protected virtual bool ConvertUnits(VisualElement owner, StylePropertyId prop, ref T a, ref T b)
		{
			return true;
		}

		protected Values()
		{
			running = AnimationDataSet<TimingData, StyleData>.Create();
			completed = AnimationDataSet<EmptyData, T>.Create();
			m_CurrentTimeMs = Panel.TimeSinceStartupMs();
		}

		private void SwapFrameStates()
		{
			TransitionEventsFrameState currentFrameEventsState = m_CurrentFrameEventsState;
			m_CurrentFrameEventsState = m_NextFrameEventsState;
			m_NextFrameEventsState = currentFrameEventsState;
		}

		private void QueueEvent(EventBase evt, ElementPropertyPair epp)
		{
			evt.target = epp.element;
			if (!m_NextFrameEventsState.elementPropertyQueuedEvents.TryGetValue(epp, out var value))
			{
				value = TransitionEventsFrameState.GetPooledQueue();
				m_NextFrameEventsState.elementPropertyQueuedEvents.Add(epp, value);
			}
			value.Enqueue(evt);
			if (m_NextFrameEventsState.panel == null)
			{
				m_NextFrameEventsState.panel = epp.element.panel;
			}
			m_NextFrameEventsState.RegisterChange();
		}

		private void ClearEventQueue(ElementPropertyPair epp)
		{
			if (m_NextFrameEventsState.elementPropertyQueuedEvents.TryGetValue(epp, out var value))
			{
				while (value.Count > 0)
				{
					value.Dequeue().Dispose();
					m_NextFrameEventsState.UnregisterChange();
				}
			}
		}

		private void QueueTransitionRunEvent(VisualElement ve, int runningIndex)
		{
			if (ve.HasParentEventCallbacksOrDefaultActions(EventCategory.StyleTransition))
			{
				StylePropertyId stylePropertyId = running.properties[runningIndex];
				ElementPropertyPair elementPropertyPair = new ElementPropertyPair(ve, stylePropertyId);
				if (m_NextFrameEventsState.elementPropertyStateDelta.TryGetValue(elementPropertyPair, out var value))
				{
					m_NextFrameEventsState.elementPropertyStateDelta[elementPropertyPair] = value | TransitionState.Running;
				}
				else
				{
					m_NextFrameEventsState.elementPropertyStateDelta.Add(elementPropertyPair, TransitionState.Running);
				}
				ref TimingData reference = ref running.timing[runningIndex];
				int num = ((reference.delayMs < 0) ? Mathf.Min(Mathf.Max(-reference.delayMs, 0), reference.durationMs) : 0);
				TransitionRunEvent pooled = TransitionEventBase<TransitionRunEvent>.GetPooled(new StylePropertyName(stylePropertyId), (float)num / 1000f);
				QueueEvent(pooled, elementPropertyPair);
			}
		}

		private void QueueTransitionStartEvent(VisualElement ve, int runningIndex)
		{
			if (ve.HasParentEventCallbacksOrDefaultActions(EventCategory.StyleTransition))
			{
				StylePropertyId stylePropertyId = running.properties[runningIndex];
				ElementPropertyPair elementPropertyPair = new ElementPropertyPair(ve, stylePropertyId);
				if (m_NextFrameEventsState.elementPropertyStateDelta.TryGetValue(elementPropertyPair, out var value))
				{
					m_NextFrameEventsState.elementPropertyStateDelta[elementPropertyPair] = value | TransitionState.Started;
				}
				else
				{
					m_NextFrameEventsState.elementPropertyStateDelta.Add(elementPropertyPair, TransitionState.Started);
				}
				ref TimingData reference = ref running.timing[runningIndex];
				int num = ((reference.delayMs < 0) ? Mathf.Min(Mathf.Max(-reference.delayMs, 0), reference.durationMs) : 0);
				TransitionStartEvent pooled = TransitionEventBase<TransitionStartEvent>.GetPooled(new StylePropertyName(stylePropertyId), (float)num / 1000f);
				QueueEvent(pooled, elementPropertyPair);
			}
		}

		private void QueueTransitionEndEvent(VisualElement ve, int runningIndex)
		{
			if (ve.HasParentEventCallbacksOrDefaultActions(EventCategory.StyleTransition))
			{
				StylePropertyId stylePropertyId = running.properties[runningIndex];
				ElementPropertyPair elementPropertyPair = new ElementPropertyPair(ve, stylePropertyId);
				if (m_NextFrameEventsState.elementPropertyStateDelta.TryGetValue(elementPropertyPair, out var value))
				{
					m_NextFrameEventsState.elementPropertyStateDelta[elementPropertyPair] = value | TransitionState.Ended;
				}
				else
				{
					m_NextFrameEventsState.elementPropertyStateDelta.Add(elementPropertyPair, TransitionState.Ended);
				}
				ref TimingData reference = ref running.timing[runningIndex];
				TransitionEndEvent pooled = TransitionEventBase<TransitionEndEvent>.GetPooled(new StylePropertyName(stylePropertyId), (float)reference.durationMs / 1000f);
				QueueEvent(pooled, elementPropertyPair);
			}
		}

		private void QueueTransitionCancelEvent(VisualElement ve, int runningIndex, long panelElapsedMs)
		{
			if (!ve.HasParentEventCallbacksOrDefaultActions(EventCategory.StyleTransition))
			{
				return;
			}
			StylePropertyId stylePropertyId = running.properties[runningIndex];
			ElementPropertyPair elementPropertyPair = new ElementPropertyPair(ve, stylePropertyId);
			bool flag;
			if (m_NextFrameEventsState.elementPropertyStateDelta.TryGetValue(elementPropertyPair, out var value))
			{
				if (value == TransitionState.None || (value & TransitionState.Canceled) == TransitionState.Canceled)
				{
					m_NextFrameEventsState.elementPropertyStateDelta[elementPropertyPair] = TransitionState.Canceled;
					ClearEventQueue(elementPropertyPair);
					flag = true;
				}
				else
				{
					m_NextFrameEventsState.elementPropertyStateDelta[elementPropertyPair] = TransitionState.None;
					ClearEventQueue(elementPropertyPair);
					flag = false;
				}
			}
			else
			{
				m_NextFrameEventsState.elementPropertyStateDelta.Add(elementPropertyPair, TransitionState.Canceled);
				flag = true;
			}
			if (flag)
			{
				ref TimingData reference = ref running.timing[runningIndex];
				long num = (reference.isStarted ? (panelElapsedMs - reference.startTimeMs) : 0);
				if (reference.delayMs < 0)
				{
					num = -reference.delayMs + num;
				}
				TransitionCancelEvent pooled = TransitionEventBase<TransitionCancelEvent>.GetPooled(new StylePropertyName(stylePropertyId), (float)num / 1000f);
				QueueEvent(pooled, elementPropertyPair);
			}
		}

		private void SendTransitionCancelEvent(VisualElement ve, int runningIndex, long panelElapsedMs)
		{
			if (!ve.HasParentEventCallbacksOrDefaultActions(EventBase<TransitionCancelEvent>.EventCategory))
			{
				return;
			}
			ref TimingData reference = ref running.timing[runningIndex];
			StylePropertyId stylePropertyId = running.properties[runningIndex];
			long num = (reference.isStarted ? (panelElapsedMs - reference.startTimeMs) : 0);
			if (reference.delayMs < 0)
			{
				num = -reference.delayMs + num;
			}
			using TransitionCancelEvent transitionCancelEvent = TransitionEventBase<TransitionCancelEvent>.GetPooled(new StylePropertyName(stylePropertyId), (float)num / 1000f);
			transitionCancelEvent.target = ve;
			ve.SendEvent(transitionCancelEvent);
		}

		public sealed override void CancelAllAnimations()
		{
			int count = running.count;
			if (count > 0)
			{
				using (new EventDispatcherGate(running.elements[0].panel.dispatcher))
				{
					for (int i = 0; i < count; i++)
					{
						VisualElement visualElement = running.elements[i];
						SendTransitionCancelEvent(visualElement, i, m_CurrentTimeMs);
						ForceComputedStyleEndValue(i);
						visualElement.styleAnimation.runningAnimationCount--;
					}
				}
				running.RemoveAll();
			}
			int count2 = completed.count;
			for (int j = 0; j < count2; j++)
			{
				VisualElement visualElement2 = completed.elements[j];
				visualElement2.styleAnimation.completedAnimationCount--;
			}
			completed.RemoveAll();
		}

		public sealed override void CancelAllAnimations(VisualElement ve)
		{
			int count = running.count;
			if (count > 0)
			{
				using (new EventDispatcherGate(running.elements[0].panel.dispatcher))
				{
					for (int i = 0; i < count; i++)
					{
						if (running.elements[i] == ve)
						{
							SendTransitionCancelEvent(ve, i, m_CurrentTimeMs);
							ForceComputedStyleEndValue(i);
							running.elements[i].styleAnimation.runningAnimationCount--;
						}
					}
				}
			}
			running.RemoveAll(ve);
			int count2 = completed.count;
			for (int j = 0; j < count2; j++)
			{
				if (completed.elements[j] == ve)
				{
					completed.elements[j].styleAnimation.completedAnimationCount--;
				}
			}
			completed.RemoveAll(ve);
		}

		public sealed override void CancelAnimation(VisualElement ve, StylePropertyId id)
		{
			if (running.IndexOf(ve, id, out var index))
			{
				QueueTransitionCancelEvent(ve, index, m_CurrentTimeMs);
				ForceComputedStyleEndValue(index);
				running.Remove(index);
				ve.styleAnimation.runningAnimationCount--;
			}
			if (completed.IndexOf(ve, id, out var index2))
			{
				completed.Remove(index2);
				ve.styleAnimation.completedAnimationCount--;
			}
		}

		public sealed override bool HasRunningAnimation(VisualElement ve, StylePropertyId id)
		{
			int index;
			return running.IndexOf(ve, id, out index);
		}

		public sealed override void UpdateAnimation(VisualElement ve, StylePropertyId id)
		{
			if (running.IndexOf(ve, id, out var index))
			{
				UpdateComputedStyle(index);
			}
		}

		public sealed override void GetAllAnimations(VisualElement ve, List<StylePropertyId> outPropertyIds)
		{
			running.GetActivePropertiesForElement(ve, outPropertyIds);
			completed.GetActivePropertiesForElement(ve, outPropertyIds);
		}

		private float ComputeReversingShorteningFactor(int oldIndex)
		{
			ref TimingData reference = ref running.timing[oldIndex];
			return Mathf.Clamp01(Mathf.Abs(1f - (1f - reference.easedProgress) * reference.reversingShorteningFactor));
		}

		private int ComputeReversingDuration(int newTransitionDurationMs, float newReversingShorteningFactor)
		{
			return Mathf.RoundToInt((float)newTransitionDurationMs * newReversingShorteningFactor);
		}

		private int ComputeReversingDelay(int delayMs, float newReversingShorteningFactor)
		{
			return (delayMs < 0) ? Mathf.RoundToInt((float)delayMs * newReversingShorteningFactor) : delayMs;
		}

		public bool StartTransition(VisualElement owner, StylePropertyId prop, T startValue, T endValue, int durationMs, int delayMs, Func<float, float> easingCurve, long currentTimeMs)
		{
			long startTimeMs = currentTimeMs + delayMs;
			TimingData timingData = new TimingData
			{
				startTimeMs = startTimeMs,
				durationMs = durationMs,
				easingCurve = easingCurve,
				reversingShorteningFactor = 1f,
				delayMs = delayMs
			};
			StyleData styleData = new StyleData
			{
				startValue = startValue,
				endValue = endValue,
				currentValue = startValue,
				reversingAdjustedStartValue = startValue
			};
			int num = Mathf.Max(0, durationMs) + delayMs;
			if (!ConvertUnits(owner, prop, ref styleData.startValue, ref styleData.endValue))
			{
				return false;
			}
			if (completed.IndexOf(owner, prop, out var index))
			{
				if (SameFunc(endValue, completed.style[index]))
				{
					return false;
				}
				if (num <= 0)
				{
					return false;
				}
				completed.Remove(index);
				owner.styleAnimation.completedAnimationCount--;
			}
			if (running.IndexOf(owner, prop, out var index2))
			{
				if (SameFunc(endValue, running.style[index2].endValue))
				{
					return false;
				}
				if (SameFunc(endValue, running.style[index2].currentValue))
				{
					QueueTransitionCancelEvent(owner, index2, currentTimeMs);
					running.Remove(index2);
					owner.styleAnimation.runningAnimationCount--;
					return false;
				}
				if (num <= 0)
				{
					QueueTransitionCancelEvent(owner, index2, currentTimeMs);
					running.Remove(index2);
					owner.styleAnimation.runningAnimationCount--;
					return false;
				}
				styleData.startValue = running.style[index2].currentValue;
				if (!ConvertUnits(owner, prop, ref styleData.startValue, ref styleData.endValue))
				{
					QueueTransitionCancelEvent(owner, index2, currentTimeMs);
					running.Remove(index2);
					owner.styleAnimation.runningAnimationCount--;
					return false;
				}
				styleData.currentValue = styleData.startValue;
				if (SameFunc(endValue, running.style[index2].reversingAdjustedStartValue))
				{
					float newReversingShorteningFactor = (timingData.reversingShorteningFactor = ComputeReversingShorteningFactor(index2));
					timingData.startTimeMs = currentTimeMs + ComputeReversingDelay(delayMs, newReversingShorteningFactor);
					timingData.durationMs = ComputeReversingDuration(durationMs, newReversingShorteningFactor);
					styleData.reversingAdjustedStartValue = running.style[index2].endValue;
				}
				running.timing[index2].isStarted = false;
				QueueTransitionCancelEvent(owner, index2, currentTimeMs);
				QueueTransitionRunEvent(owner, index2);
				running.Replace(index2, timingData, styleData);
				return true;
			}
			if (num <= 0)
			{
				return false;
			}
			if (SameFunc(startValue, endValue))
			{
				return false;
			}
			running.Add(owner, prop, timingData, styleData);
			owner.styleAnimation.runningAnimationCount++;
			QueueTransitionRunEvent(owner, running.count - 1);
			return true;
		}

		private void ForceComputedStyleEndValue(int runningIndex)
		{
			ref StyleData reference = ref running.style[runningIndex];
			reference.currentValue = reference.endValue;
			UpdateComputedStyle(runningIndex);
		}

		public sealed override void Update(long currentTimeMs)
		{
			m_CurrentTimeMs = currentTimeMs;
			UpdateProgress(currentTimeMs);
			UpdateValues();
			UpdateComputedStyle();
			if (m_NextFrameEventsState.StateChanged())
			{
				ProcessEventQueue();
			}
		}

		private void ProcessEventQueue()
		{
			SwapFrameStates();
			EventDispatcher d = m_CurrentFrameEventsState.panel?.dispatcher;
			using (new EventDispatcherGate(d))
			{
				foreach (KeyValuePair<ElementPropertyPair, Queue<EventBase>> elementPropertyQueuedEvent in m_CurrentFrameEventsState.elementPropertyQueuedEvents)
				{
					ElementPropertyPair key = elementPropertyQueuedEvent.Key;
					Queue<EventBase> value = elementPropertyQueuedEvent.Value;
					VisualElement element = elementPropertyQueuedEvent.Key.element;
					while (value.Count > 0)
					{
						EventBase eventBase = value.Dequeue();
						element.SendEvent(eventBase);
						eventBase.Dispose();
					}
				}
				m_CurrentFrameEventsState.Clear();
			}
		}

		private void UpdateProgress(long currentTimeMs)
		{
			int num = running.count;
			if (num <= 0)
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				ref TimingData reference = ref running.timing[i];
				if (currentTimeMs < reference.startTimeMs)
				{
					reference.easedProgress = 0f;
				}
				else if (currentTimeMs >= reference.startTimeMs + reference.durationMs)
				{
					ref StyleData reference2 = ref running.style[i];
					ref VisualElement reference3 = ref running.elements[i];
					reference2.currentValue = reference2.endValue;
					UpdateComputedStyle(i);
					completed.Add(reference3, running.properties[i], EmptyData.Default, reference2.endValue);
					reference3.styleAnimation.runningAnimationCount--;
					reference3.styleAnimation.completedAnimationCount++;
					QueueTransitionEndEvent(reference3, i);
					running.Remove(i);
					i--;
					num--;
				}
				else
				{
					if (!reference.isStarted)
					{
						reference.isStarted = true;
						QueueTransitionStartEvent(running.elements[i], i);
					}
					float arg = (float)(currentTimeMs - reference.startTimeMs) / (float)reference.durationMs;
					reference.easedProgress = reference.easingCurve(arg);
				}
			}
		}
	}

	private class ValuesFloat : Values<float>
	{
		public override Func<float, float, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(float a, float b)
		{
			return Mathf.Approximately(a, b);
		}

		private static float Lerp(float a, float b, float t)
		{
			return Mathf.LerpUnclamped(a, b, t);
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesInt : Values<int>
	{
		public override Func<int, int, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(int a, int b)
		{
			return a == b;
		}

		private static int Lerp(int a, int b, float t)
		{
			return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, t));
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesLength : Values<Length>
	{
		public override Func<Length, Length, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(Length a, Length b)
		{
			return a.unit == b.unit && Mathf.Approximately(a.value, b.value);
		}

		protected sealed override bool ConvertUnits(VisualElement owner, StylePropertyId prop, ref Length a, ref Length b)
		{
			return owner.TryConvertLengthUnits(prop, ref a, ref b);
		}

		internal static Length Lerp(Length a, Length b, float t)
		{
			return new Length(Mathf.LerpUnclamped(a.value, b.value, t), b.unit);
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesColor : Values<Color>
	{
		public override Func<Color, Color, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(Color c, Color d)
		{
			return Mathf.Approximately(c.r, d.r) && Mathf.Approximately(c.g, d.g) && Mathf.Approximately(c.b, d.b) && Mathf.Approximately(c.a, d.a);
		}

		private static Color Lerp(Color a, Color b, float t)
		{
			return Color.LerpUnclamped(a, b, t);
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private abstract class ValuesDiscrete<T> : Values<T>
	{
		public override Func<T, T, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(T a, T b)
		{
			return EqualityComparer<T>.Default.Equals(a, b);
		}

		private static T Lerp(T a, T b, float t)
		{
			return (t < 0.5f) ? a : b;
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}
	}

	private class ValuesEnum : ValuesDiscrete<int>
	{
		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesBackground : ValuesDiscrete<Background>
	{
		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesFontDefinition : ValuesDiscrete<FontDefinition>
	{
		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesFont : ValuesDiscrete<Font>
	{
		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesTextShadow : Values<TextShadow>
	{
		public override Func<TextShadow, TextShadow, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(TextShadow a, TextShadow b)
		{
			return a == b;
		}

		private static TextShadow Lerp(TextShadow a, TextShadow b, float t)
		{
			return TextShadow.LerpUnclamped(a, b, t);
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesScale : Values<Scale>
	{
		public override Func<Scale, Scale, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(Scale a, Scale b)
		{
			return a == b;
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}

		private static Scale Lerp(Scale a, Scale b, float t)
		{
			return new Scale(Vector3.LerpUnclamped(a.value, b.value, t));
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}
	}

	private class ValuesRotate : Values<Rotate>
	{
		public override Func<Rotate, Rotate, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(Rotate a, Rotate b)
		{
			return a == b;
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}

		private static Rotate Lerp(Rotate a, Rotate b, float t)
		{
			return new Rotate(Mathf.LerpUnclamped(a.angle.ToDegrees(), b.angle.ToDegrees(), t));
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}
	}

	private class ValuesTranslate : Values<Translate>
	{
		public override Func<Translate, Translate, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(Translate a, Translate b)
		{
			return a == b;
		}

		protected sealed override bool ConvertUnits(VisualElement owner, StylePropertyId prop, ref Translate a, ref Translate b)
		{
			return owner.TryConvertTranslateUnits(ref a, ref b);
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}

		private static Translate Lerp(Translate a, Translate b, float t)
		{
			return new Translate(ValuesLength.Lerp(a.x, b.x, t), ValuesLength.Lerp(a.y, b.y, t), Mathf.Lerp(a.z, b.z, t));
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}
	}

	private class ValuesTransformOrigin : Values<TransformOrigin>
	{
		public override Func<TransformOrigin, TransformOrigin, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(TransformOrigin a, TransformOrigin b)
		{
			return a == b;
		}

		protected sealed override bool ConvertUnits(VisualElement owner, StylePropertyId prop, ref TransformOrigin a, ref TransformOrigin b)
		{
			return owner.TryConvertTransformOriginUnits(ref a, ref b);
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}

		private static TransformOrigin Lerp(TransformOrigin a, TransformOrigin b, float t)
		{
			return new TransformOrigin(ValuesLength.Lerp(a.x, b.x, t), ValuesLength.Lerp(a.y, b.y, t), Mathf.Lerp(a.z, b.z, t));
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}
	}

	private class ValuesBackgroundPosition : ValuesDiscrete<BackgroundPosition>
	{
		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesBackgroundRepeat : ValuesDiscrete<BackgroundRepeat>
	{
		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}
	}

	private class ValuesBackgroundSize : Values<BackgroundSize>
	{
		public override Func<BackgroundSize, BackgroundSize, bool> SameFunc { get; } = IsSame;

		private static bool IsSame(BackgroundSize a, BackgroundSize b)
		{
			return a == b;
		}

		protected sealed override bool ConvertUnits(VisualElement owner, StylePropertyId prop, ref BackgroundSize a, ref BackgroundSize b)
		{
			return owner.TryConvertBackgroundSizeUnits(ref a, ref b);
		}

		protected sealed override void UpdateComputedStyle()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
			}
		}

		protected sealed override void UpdateComputedStyle(int i)
		{
			running.elements[i].computedStyle.ApplyPropertyAnimation(running.elements[i], running.properties[i], running.style[i].currentValue);
		}

		private static BackgroundSize Lerp(BackgroundSize a, BackgroundSize b, float t)
		{
			return new BackgroundSize(ValuesLength.Lerp(a.x, b.x, t), ValuesLength.Lerp(a.y, b.y, t));
		}

		protected sealed override void UpdateValues()
		{
			int count = running.count;
			for (int i = 0; i < count; i++)
			{
				ref TimingData reference = ref running.timing[i];
				ref StyleData reference2 = ref running.style[i];
				reference2.currentValue = Lerp(reference2.startValue, reference2.endValue, reference.easedProgress);
			}
		}
	}

	private long m_CurrentTimeMs = 0L;

	private ValuesFloat m_Floats;

	private ValuesInt m_Ints;

	private ValuesLength m_Lengths;

	private ValuesColor m_Colors;

	private ValuesEnum m_Enums;

	private ValuesBackground m_Backgrounds;

	private ValuesFontDefinition m_FontDefinitions;

	private ValuesFont m_Fonts;

	private ValuesTextShadow m_TextShadows;

	private ValuesScale m_Scale;

	private ValuesRotate m_Rotate;

	private ValuesTranslate m_Translate;

	private ValuesTransformOrigin m_TransformOrigin;

	private ValuesBackgroundPosition m_BackgroundPosition;

	private ValuesBackgroundRepeat m_BackgroundRepeat;

	private ValuesBackgroundSize m_BackgroundSize;

	private readonly List<Values> m_AllValues = new List<Values>();

	private readonly Dictionary<StylePropertyId, Values> m_PropertyToValues = new Dictionary<StylePropertyId, Values>();

	public StylePropertyAnimationSystem()
	{
		m_CurrentTimeMs = Panel.TimeSinceStartupMs();
	}

	private T GetOrCreate<T>(ref T values) where T : new()
	{
		T val = values;
		return (val != null) ? val : (values = new T());
	}

	private bool StartTransition<T>(VisualElement owner, StylePropertyId prop, T startValue, T endValue, int durationMs, int delayMs, Func<float, float> easingCurve, Values<T> values)
	{
		m_PropertyToValues[prop] = values;
		bool result = values.StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, CurrentTimeMs());
		UpdateTracking(values);
		return result;
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, float startValue, float endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Floats));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, int startValue, int endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Ints));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, Length startValue, Length endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Lengths));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, Color startValue, Color endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Colors));
	}

	public bool StartAnimationEnum(VisualElement owner, StylePropertyId prop, int startValue, int endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Enums));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, Background startValue, Background endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Backgrounds));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, FontDefinition startValue, FontDefinition endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_FontDefinitions));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, Font startValue, Font endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Fonts));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, TextShadow startValue, TextShadow endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_TextShadows));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, Scale startValue, Scale endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Scale));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, Rotate startValue, Rotate endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Rotate));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, Translate startValue, Translate endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_Translate));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, TransformOrigin startValue, TransformOrigin endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_TransformOrigin));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, BackgroundPosition startValue, BackgroundPosition endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_BackgroundPosition));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, BackgroundRepeat startValue, BackgroundRepeat endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_BackgroundRepeat));
	}

	public bool StartTransition(VisualElement owner, StylePropertyId prop, BackgroundSize startValue, BackgroundSize endValue, int durationMs, int delayMs, [NotNull] Func<float, float> easingCurve)
	{
		return StartTransition(owner, prop, startValue, endValue, durationMs, delayMs, easingCurve, GetOrCreate(ref m_BackgroundSize));
	}

	public void CancelAllAnimations()
	{
		foreach (Values allValue in m_AllValues)
		{
			allValue.CancelAllAnimations();
		}
	}

	public void CancelAllAnimations(VisualElement owner)
	{
		foreach (Values allValue in m_AllValues)
		{
			allValue.CancelAllAnimations(owner);
		}
		Assert.AreEqual(0, owner.styleAnimation.runningAnimationCount);
		Assert.AreEqual(0, owner.styleAnimation.completedAnimationCount);
	}

	public void CancelAnimation(VisualElement owner, StylePropertyId id)
	{
		if (m_PropertyToValues.TryGetValue(id, out var value))
		{
			value.CancelAnimation(owner, id);
		}
	}

	public bool HasRunningAnimation(VisualElement owner, StylePropertyId id)
	{
		Values value;
		return m_PropertyToValues.TryGetValue(id, out value) && value.HasRunningAnimation(owner, id);
	}

	public void UpdateAnimation(VisualElement owner, StylePropertyId id)
	{
		if (m_PropertyToValues.TryGetValue(id, out var value))
		{
			value.UpdateAnimation(owner, id);
		}
	}

	public void GetAllAnimations(VisualElement owner, List<StylePropertyId> propertyIds)
	{
		foreach (Values allValue in m_AllValues)
		{
			allValue.GetAllAnimations(owner, propertyIds);
		}
	}

	private void UpdateTracking<T>(Values<T> values)
	{
		if (!values.isEmpty && !m_AllValues.Contains(values))
		{
			m_AllValues.Add(values);
		}
	}

	private long CurrentTimeMs()
	{
		return m_CurrentTimeMs;
	}

	public void Update()
	{
		m_CurrentTimeMs = Panel.TimeSinceStartupMs();
		int count = m_AllValues.Count;
		for (int i = 0; i < count; i++)
		{
			m_AllValues[i].Update(m_CurrentTimeMs);
		}
	}
}
