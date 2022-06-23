using Assets.Scripts.Application.Infrastructure.Events;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Constants.Modding;
using SecretHistories.Entities;
using SecretHistories.Logic;
using SecretHistories.Spheres;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    HashSet<ITokenPayload> freezed;
    float updateCounter;
    TokenDetailsWindow tokenInfo;
    FieldInfo window_stack, stack_shadow;
    PropertyInfo stack_element;

    void Awake()
    {
        window_stack = typeof(TokenDetailsWindow).GetField("_stack", BindingFlags.Instance | BindingFlags.NonPublic);
        stack_shadow = typeof(ElementStack).GetField("_timeshadow", BindingFlags.Instance | BindingFlags.NonPublic);
        stack_element = typeof(ElementStack).GetProperty("Element", BindingFlags.Instance | BindingFlags.NonPublic);
        freezed = new HashSet<ITokenPayload>();
        updateCounter = 0;
    }

    void Update()
    {
        updateCounter += Time.deltaTime;
        if (updateCounter > UPDATE_INTERVAL)
        {
            updateCounter -= UPDATE_INTERVAL;
            freezed = new HashSet<ITokenPayload>(
                from s in getPayloads()
                where getTimeRemaining(s) > AUTOADD_TIME
                select s
            );
            if (tokenInfo == null)
                tokenInfo = FindObjectOfType<TokenDetailsWindow>();
        }
        // update situations
        var grabber = Watchman.Get<Meniscate>();
        if (grabber != null)
        {
            var openEvent = grabber.GetCurrentlyOpenSituation();
            if (openEvent != null)
            {
                if (keyAddPressed()) AddFreeze(openEvent);
                if (keyRemovePressed() && freezed.Contains(openEvent)) RemoveFreeze(openEvent);
            }
        }
        // update element stacks
        if (tokenInfo != null)
        {
            var stack = (ElementStack)window_stack.GetValue(tokenInfo);
            if (stack != null)
            {
                if (keyAddPressed()) AddFreeze(stack);
                if (keyRemovePressed() && freezed.Contains(stack)) RemoveFreeze(stack);
            }
        }
        foreach (var s in freezed)
        {
            if (s != null && s.GetToken() != null)
                SetTime(s, FREEZE_TIME);
        }
    }

    #region ITERATOR
    IEnumerable<ITokenPayload> getPayloads()
    {
        foreach (var sphere in FucineRoot.Get().Spheres)
            foreach (var p in getPayloadsIn(sphere))
                yield return p;
    }
    IEnumerable<ITokenPayload> getPayloadsIn(Sphere sphere)
    {
        foreach (var token in sphere.Tokens)
        {
            var payload = token.Payload;
            if (payload is ElementStack || payload is Situation)
                yield return payload;

            foreach (var s in payload.GetSpheres())
                foreach (var p in getPayloadsIn(s))
                    yield return p;
        }
    }
    #endregion

    #region INFO
    float getTimeRemaining(ITokenPayload p)
    {
        if (p is Situation) return (p as Situation).TimeRemaining;
        if (p is ElementStack) return (p as ElementStack).LifetimeRemaining;
        return 0;
    }
    float getMaxTime(ITokenPayload p)
    {
        if (p is Situation) return (p as Situation).Warmup;
        if (p is ElementStack) return (stack_element.GetValue(p) as Element).Lifetime;
        return 0;
    }
    #endregion

    #region FREEZER
    void AddFreeze(ITokenPayload p)
    {
        freezed.Add(p);
        try { p.OnChanged += AutoRemoveOnRetire; } catch { }
    }

    void RemoveFreeze(ITokenPayload p)
    {
        if (freezed.Contains(p))
            freezed.Remove(p);
        SetTime(p, getMaxTime(p));
        try { p.OnChanged -= AutoRemoveOnRetire; } catch { }
    }

    void AutoRemoveOnRetire(TokenPayloadChangedArgs args)
    {
        if (args.ChangeType != SecretHistories.Enums.PayloadChangeType.Retirement) return;
        try { RemoveFreeze(args.Payload); } catch { }
    }

    void SetTime(ITokenPayload p, float t)
    {
        if (p is ElementStack)
        {
            var timer = (Timeshadow)stack_shadow.GetValue(p);
            timer.SpendTime(getTimeRemaining(p) - t);
            return;
        }
        p.GetToken().ExecuteHeartbeat(getTimeRemaining(p) - t, 0);
    }
    #endregion

    #region HELPERS
    bool keyAddPressed()
    {
        return Keyboard.current.numpadPlusKey.wasPressedThisFrame;
    }
    bool keyRemovePressed()
    {
        return Keyboard.current.numpadMinusKey.wasPressedThisFrame;
    }
    #endregion
}