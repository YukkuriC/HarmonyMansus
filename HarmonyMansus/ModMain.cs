using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Constants.Modding;
using SecretHistories.Entities;
using SecretHistories.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class EventFreezer : MonoBehaviour
{
    const float FREEZE_TIME = 11451.4f;
    const float AUTOADD_TIME = 999f;
    const float UPDATE_INTERVAL = 1;


    public static void Initialise(ISecretHistoriesMod mod)
    {
        new GameObject().AddComponent<EventFreezer>();
    }

    HashSet<Situation> freezed;
    float updateCounter;

    void Awake()
    {
        freezed = new HashSet<Situation>();
        updateCounter = 0;
    }

    void Update()
    {
        updateCounter += Time.deltaTime;
        if (updateCounter > UPDATE_INTERVAL)
        {
            updateCounter -= UPDATE_INTERVAL;
            freezed = new HashSet<Situation>(
                from s in Watchman.Get<HornedAxe>().GetRegisteredSituations()
                where s.TimeRemaining > AUTOADD_TIME
                select s
            );
        }

        var grabber = Watchman.Get<Meniscate>();
        if (grabber != null)
        {
            var openEvent = grabber.GetCurrentlyOpenSituation();
            if (openEvent != null)
            {
                if (Keyboard.current.numpadPlusKey.wasPressedThisFrame) AddFreeze(openEvent);
                if (Keyboard.current.numpadMinusKey.wasPressedThisFrame && freezed.Contains(openEvent)) RemoveFreeze(openEvent);
            }
        }

        foreach (var s in freezed)
        {
            if (s != null)
                SetTime(s, FREEZE_TIME);
        }
    }

    void AddFreeze(Situation s)
    {
        freezed.Add(s);
        try { s.OnChanged += AutoRemoveOnRetire; } catch { }
    }

    void RemoveFreeze(Situation s)
    {
        if (freezed.Contains(s))
            freezed.Remove(s);
        SetTime(s, s.Warmup);
        try { s.OnChanged -= AutoRemoveOnRetire; } catch { }
    }

    void AutoRemoveOnRetire(TokenPayloadChangedArgs args)
    {
        if (args.ChangeType != SecretHistories.Enums.PayloadChangeType.Retirement) return;
        RemoveFreeze(args.Payload as Situation);
    }

    void SetTime(Situation s, float t)
    {
        s.ReduceLifetimeBy(s.TimeRemaining - t);
    }
}