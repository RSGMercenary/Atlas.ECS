﻿using System;

namespace Atlas.Core.Objects.Update;

public class Updater<T> : IUpdater<T>, IDisposable where T : IUpdater<T>
{
	#region Events
	public event Action<T, bool> IsUpdatingChanged;
	public event Action<T, TimeStep, TimeStep> TimeStepChanged;
	#endregion

	#region Fields
	private readonly T instance;
	private bool isUpdating = false;
	private TimeStep timeStep = TimeStep.None;
	#endregion

	public Updater(T instance)
	{
		this.instance = instance;
	}

	public void Dispose()
	{
		IsUpdatingChanged = null;
		isUpdating = false;

		TimeStepChanged = null;
		timeStep = TimeStep.None;
	}

	public bool IsUpdating
	{
		get => isUpdating;
		set
		{
			if(isUpdating == value)
				return;
			isUpdating = value;
			IsUpdatingChanged?.Invoke(instance, value);
		}
	}

	public void Assert()
	{
		if(isUpdating)
			throw new InvalidOperationException("Update is already running, and can't be run again.");
	}

	public TimeStep TimeStep
	{
		get => timeStep;
		set
		{
			if(timeStep == value)
				return;
			var previous = timeStep;
			timeStep = value;
			TimeStepChanged?.Invoke(instance, value, previous);
		}
	}
}