/*
 * Author: Porter Squires
 */

using System.Collections.Generic;
using UnityEngine;

namespace BufferedInput
{
    public class InputDataBuffer
    {
        public bool debugMessages = false;
        // maintain a buffer of InputData pairs corresponding to the action masks
        private List<InputPair>[] actionBuffers;
        // maintain a cache of InputDatas in a similar fashion
        public InputData[] cachedInputs;

        // maintain a dictionary that maps directional actions to modify the direction of other inputs
        private Dictionary<ActionMask, ActionMask> directionalsForActions;
        // the default action if an input doesn't explicitly create its own
        private ActionMask defaultDirectional = ActionMask.NONE;
        // also, cache possible directionals
        public Vector2[] cachedDirectionals;

        public static bool GetMaskNumb(ActionMask mask, out int numb)
        {
            numb = -1;
            if (!InputData.IsControl(mask))
            {
                return false;
            }
            long temp = mask.Mask;
            while (temp > 0)
            {
                numb++;
                temp = temp >> 1;
            }
            return true;
        }

        public void InitializeBuffers(InputControlScheme controlScheme)
        {
            actionBuffers = new List<InputPair>[ActionMask.MAX_MASKS];
            List<List<InputPair>> buffers = new List<List<InputPair>>();
            foreach (ControlSchemeMap map in controlScheme.maps)
            {
                foreach (ControlSchemeAction action in map.actions)
                {
                    if (GetMaskNumb(action.actionMask, out int i) && i >= 0 && i < ActionMask.MAX_MASKS)
                    {
                        actionBuffers[i] = new List<InputPair>();
                    } else
                    {
                        Debug.LogError("Error while initializing buffers... failed to create buffer for control: " + 
                            action.Identifier.properName + ", ActionMask " + action.actionMask.name + " resulted in index of " + i);
                    }
                    if (action.Identifier.properName.Equals("Player/Move"))
                    {
                        defaultDirectional = action.actionMask;
                    }
                }
            }

            cachedInputs = new InputData[ActionMask.MAX_MASKS];
            for (int i = 0; i < cachedInputs.Length; i++)
                cachedInputs[i] = null;

            cachedDirectionals = new Vector2[ActionMask.MAX_MASKS];
            for (int i = 0; i < cachedDirectionals.Length; i++)
                cachedDirectionals[i] = Vector2.zero;

            directionalsForActions = new Dictionary<ActionMask, ActionMask>();
        }

        public bool TryGetBuffer(ActionMask mask, out List<InputPair> buffer)
        {
            if (GetMaskNumb(mask, out int i) && i >= 0 && i < actionBuffers.Length && actionBuffers[i] != null)
            {
                buffer = actionBuffers[i];
                return true;
            }
            Debug.LogWarning("Tried to access buffer with mask " + mask.name + " but failed with index " + i);
            buffer = null;
            return false;
        }

        public void CacheInput(InputData input)
        {
            if (GetMaskNumb(input.Mask, out int i))
            {
                cachedInputs[i] = input;
            } else
            {
                Debug.LogWarning("InputData for mask " + input.Mask.name + " isn't valid control.");
            }
        }

        public void ClearCachedInput(ActionMask mask)
        {
            if (GetMaskNumb(mask, out int i)) {
                cachedInputs[i] = null;
            } else
            {
                Debug.LogWarning("InputData for mask " + mask.name + " isn't valid control.");
            }
        }

        public bool TryGetCachedInput(ActionMask mask, out InputData input)
        {
            if (GetMaskNumb(mask, out int i))
            {
                if (cachedInputs[i] != null)
                {
                    input = cachedInputs[i];
                    return true;
                }
            }
            else
            {
                Debug.LogWarning("InputData for mask " + mask.name + " isn't valid control.");
            }
            input = null;
            return false;
        }

        public bool AcceptInput(InputData input, int bufferTime)
        {
            if (TryGetBuffer(input.Mask, out List<InputPair> buffer))
            {
                buffer.Add(new InputPair(input, bufferTime));
                if (debugMessages)
                    Debug.Log("Accepted input into buffer " + input.Mask.name + " with phase " + input.Phase.ToString() + " and direction " + input.Direction.current);
                return true;
            }
            return false;
        }

        public Vector2 GetCachedDirection()
        {
            if (defaultDirectional.Equals(ActionMask.NONE))
                return Vector2.zero;

            if (TryGetCachedInput(defaultDirectional, out InputData input))
            {
                return input.Direction.current;
            }
            return Vector2.zero;
        }

        public Vector2 GetCachedDirection(ActionMask mask)
        {
            if (directionalsForActions.TryGetValue(mask, out ActionMask directionMask))
            {
                if (TryGetCachedInput(directionMask, out InputData input))
                {
                    return input.Direction.current;
                }
            }
            return GetCachedDirection();
        }

        public Vector2 CurrentDirection()
        {
            if (defaultDirectional.Equals(ActionMask.NONE))
                return Vector2.zero;

            if (TryGetBuffer(defaultDirectional, out List<InputPair> buffer) && buffer.Count > 0)
            {
                InputPair latestEntry = buffer[buffer.Count - 1];
                if (latestEntry.input.IsHeld())
                {
                    return latestEntry.input.Direction.current;
                }
            }
            return Vector2.zero;
        }

        public Vector2 CurrentDirection(ActionMask mask)
        {
            if (directionalsForActions.TryGetValue(mask, out ActionMask directionMask))
            {
                if (TryGetBuffer(directionMask, out List<InputPair> buffer) && buffer.Count > 0)
                {
                    InputPair latestEntry = buffer[buffer.Count - 1];
                    if (latestEntry.input.IsHeld())
                    {
                        return latestEntry.input.Direction.current;
                    }
                }
            }
            return CurrentDirection();
        }

        public bool TryGetCurrentDirection(ActionMask mask, out Vector2 direction)
        {
            if (directionalsForActions.TryGetValue(mask, out ActionMask directionMask))
            {
                if (TryGetBuffer(directionMask, out List<InputPair> buffer) && buffer.Count > 0)
                {
                    InputPair latestEntry = buffer[buffer.Count - 1];
                    if (latestEntry.input.IsHeld())
                    {
                        direction = latestEntry.input.Direction.current;
                        return true;
                    }
                }
            }
            direction = Vector2.zero;
            return false;
        }

        public bool CacheDirectional()
        {
            if (defaultDirectional.Equals(ActionMask.NONE))
                return false;

            if (TryGetBuffer(defaultDirectional, out List<InputPair> buffer) && buffer.Count > 0)
            {
                InputPair latestEntry = buffer[buffer.Count - 1];
                if (latestEntry.input.IsHeld())
                {
                    CacheInput(latestEntry.input);
                    return true;
                }
            }

            return false;
        }

        public bool CacheDirectional(ActionMask mask)
        {
            if (directionalsForActions.TryGetValue(mask, out ActionMask directionMask)) {
                if (TryGetBuffer(directionMask, out List<InputPair> buffer) && buffer.Count > 0)
                {
                    InputPair latestEntry = buffer[buffer.Count - 1];
                    if (latestEntry.input.IsHeld())
                    {
                        CacheInput(latestEntry.input);
                        return true;
                    }
                }
            }
            return CacheDirectional();
        }

        public void SetDirectional(ActionMask action, ActionMask directional)
        {
            if (!directionalsForActions.ContainsKey(action))
            {
                directionalsForActions.Add(action, directional);
            } else
            {
                directionalsForActions.Remove(action);
                directionalsForActions.Add(action, directional);
            }
        }

        public void RemoveDirectional(ActionMask action)
        {
            if (directionalsForActions.ContainsKey(action))
            {
                directionalsForActions.Remove(action);
            }
        }

        public void SetDirectional(ActionMask directional)
        {
            defaultDirectional = directional;
        }
        
        public void CleanBuffer(ActionMask mask)
        {
            if (TryGetBuffer(mask, out List<InputPair> buffer))
            {
                CleanBuffer(buffer, mask);
            }
        }

        public void CleanBuffer(List<InputPair> buffer, ActionMask mask)
        {
            if (debugMessages)
                Debug.Log("Start - cleaning buffer of " + mask.name + ". Current buffer size: " + buffer.Count);

            for (int i = 0; i < buffer.Count - 1; i++)
            {
                InputPair current = buffer[i];
                InputPair next = buffer[i + 1];
                if (current.input.IsCombinable())
                {
                    if (debugMessages)
                        Debug.Log("Merging " + current.input.ToString() + " ~with~ " + next.input.ToString());
                    InputPair combined = InputData.CombineInputPairs(current, next);
                    buffer[i] = combined;
                    buffer.RemoveAt(i + 1);
                    if (debugMessages)
                        Debug.Log("Combined inputs in buffer: " + mask.name + ", new Count: " + buffer.Count);
                }
            }

            if (debugMessages)
                Debug.Log("Done - cleaning buffer. Current buffer size: " + buffer.Count);
        }

        public void MaintainBuffer(List<InputPair> buffer, ActionMask mask, bool canExpire)
        {
            // Is this needed? Probably
            CleanBuffer(buffer, mask);

            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                InputPair current = buffer[i];

                // remove if input is interrupted and released
                if (current.input.IsInterrupted() && current.input.IsReleased())
                {
                    if (debugMessages)
                        Debug.Log("Removing interrupted input from buffer: " + mask.name);
                    buffer.RemoveAt(i);
                    continue;
                }

                // if latest value is held down, update its directional
                if (i == buffer.Count - 1 && current.input.IsHeld() && TryGetCurrentDirection(mask, out Vector2 curDir))
                    current.input.UpdateDirection(curDir);

                // increment the frames held number
                bool incrementedFrames = current.input.TryIncrementInputFrame();
                // decrease time left in buffer if released and canExpire
                if (current.input.IsReleased() && canExpire)
                    current.duration--;

                // if the current input has expired, remove it
                if (current.duration <= 0)
                {
                    if (debugMessages)
                        Debug.Log("Removing expired input from buffer: " + mask.name);
                    buffer.RemoveAt(i);
                }
            }
        }

        public bool TryPopNextInput(List<InputPair> buffer, out InputData input)
        {
            if (buffer.Count > 0)
            {
                input = buffer[0].input;
                if (input.IsReleased())
                {
                    buffer.RemoveAt(0);
                    if (debugMessages)
                        Debug.Log("Popped released input from buffer. New buffer size: " + buffer.Count);
                }
                return true;
            }
            input = null;
            return false;
        }

        public bool PeekNextInput(List<InputPair> buffer, out InputData input)
        {
            if (buffer.Count > 0)
            {
                input = buffer[0].input;
                return true;
            }
            input = null;
            return false;
        }
    }

}