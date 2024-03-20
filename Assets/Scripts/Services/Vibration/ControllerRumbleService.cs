using Service;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;

namespace Client.Services.Vibration
{
    public enum RumblePattern
    {
        Constant,
        Pulse,
        Linear
    }

    public class ControllerRumbleService : IVibrationService, IService
    {
        private VibrationDataBase m_vibrationData;
        public VibrationDataBase CurrentActiveVibration => m_vibrationData;

        bool m_isMotorActive = false;

        public RumblePattern RumblePattern => m_vibrationData.Pattern;

        public bool IsRumblingNow()
        {
            return m_isMotorActive;
        }

        public void SetDuration(float duration)
        {
            throw new System.NotImplementedException();
        }

        public void StartRumble(RumblePattern pattern)
        {
            throw new System.NotImplementedException();
        }

        public void StartRumble(VibrationDataBase data)
        {
            throw new System.NotImplementedException();
        }

        public void StopRumble()
        {
            throw new System.NotImplementedException();
        }

        public void StopAllRumble()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown()
        {
            StopAllRumble();
        }

        public void RegisterServiceType(ServiceType serviceType, IService svc)
        {
            throw new System.NotImplementedException();
        }

        // Start is called before the first frame update
        /*void Start()
        {
            //tmp test
            //StartPulseRumble(GetComponent<PlayerInput>(), rDuration);
            //StartPulseRumbleAll(3f);
        }

        public void StartRumble(PlayerInput pInput, RumblePattern pattern = RumblePattern.CONSTANT, float duration = 0.5f, float pulseTime = 0.3f, float pulseWait = 0.2f)
        {
            if (!pInput)
                return;

            switch(pattern)
            {
                case RumblePattern.CONSTANT:
                    StartConstantRumble(pInput, duration);
                    break;
                case RumblePattern.PULSE:
                    StartPulseRumble(pInput, duration, pulseTime, pulseWait);
                    break;
                case RumblePattern.LINEAR:
                    StartLinearRumble(pInput, duration);
                    break;
                default:
                    break;
            }
        }

        public void StartConstantRumble(PlayerInput pInput, float duration = 0.2f)
        {
            if (!pInput)
              return;

            rPattern = RumblePattern.CONSTANT;
            rDuration = duration;

            Gamepad pad = GetGamepad(pInput);

            if (pad != null)
                StartCoroutine(RumbleFor(duration, pad));
        }

        public void StartLinearRumble(PlayerInput pInput, float duration = 0.2f)
        {
            if (!pInput)
                return;

            rPattern = RumblePattern.LINEAR;
            rDuration = duration;

            Gamepad pad = GetGamepad(pInput);

            if (pad != null)
                StartCoroutine(RumbleFor(duration, pad));
        }

        public void StartPulseRumble(PlayerInput pInput, float totalDuration = 0.5f, float pulseDuration = 0.2f, float pulseWait = 0.2f)
        {
            if (!pInput)
                return;

            rPattern = RumblePattern.PULSE;
            rDuration = totalDuration;
            this.pulseDuration = pulseDuration;
            this.pulseWaitBetweenDuration = pulseWait;

            Gamepad pad = GetGamepad(pInput);

            if(pad != null)
                StartCoroutine(RumbleFor(totalDuration, pad));
        }

        public void StartRumbleAll(RumblePattern pattern = RumblePattern.CONSTANT, float duration = 0.5f, float pulseTime = 0.3f)
        {
            //GameManager.Instance.
        }

        public void StartConstantRumbleAll(float duration = 1f)
        {
            rPattern = RumblePattern.CONSTANT;
            rDuration = duration;

            StartCoroutine(RumbleAll(duration));
        }

        public void StartLinearRumbleAll(float duration = 1f)
        {
            rPattern = RumblePattern.LINEAR;
            rDuration = duration;

            StartCoroutine(RumbleAll(duration));
        }

        public void StartPulseRumbleAll(float duration = 1f)
        {
            rPattern = RumblePattern.PULSE;
            rDuration = duration;

            StartCoroutine(RumbleAll(duration));
        }

        public void StopRumble(int pIndex)
        {
            Gamepad pad = GetGamepad(pIndex);
            StopRumble(pad);
        }

        public void StopRumble(Gamepad pad)
        {
            pad.SetMotorSpeeds(0f, 0f);
        }

        public void StopRumbleAll()
        {
            foreach (Gamepad pad in Gamepad.all)
            {
                StopRumble(pad);
            }
        }

        Gamepad GetGamepad(int pIndex = 0)
        {
            if (pIndex < 0)
                return null;

            return Gamepad.all[pIndex];
        }

        Gamepad GetGamepad(PlayerInput p)
        {
            foreach(var g in Gamepad.all)
            {
                foreach(var device in p.devices)
                {
                    print(device);
                    if (device.deviceId == g.deviceId)
                        return g;
                }
            }

            return null;
        }

        IEnumerator RumbleFor(float seconds, Gamepad pad)
        {
            //tmp
            if (pad == null)
            {
                print("No pad found");
                yield return null;
            }

            float currTime = 0f;

            switch (rPattern)
            {
                case RumblePattern.CONSTANT:
                    pad.SetMotorSpeeds(lowFreq, highFreq);
                    yield return new WaitForSeconds(seconds);
                    break;
                case RumblePattern.PULSE:
                    isMotorActive = true;

                    //total duration
                    while (currTime <= seconds)
                    {
                        //vibrate
                        if (isMotorActive)
                        {
                            pad.SetMotorSpeeds(lowFreq, highFreq);
                            yield return new WaitForSeconds(pulseDuration);
                            isMotorActive = false;
                            currTime += pulseDuration;
                        }
                        else
                        {
                            //stop
                            StopRumble(pad);

                            yield return new WaitForSeconds(pulseWaitBetweenDuration);
                            isMotorActive = true;
                            currTime += pulseWaitBetweenDuration;
                        }

                        yield return new WaitForEndOfFrame();
                    }
                    break;
                case RumblePattern.LINEAR:
                    while (currTime <= seconds)
                    {
                        pad.SetMotorSpeeds(lowFreq, highFreq);
                        lowFreq += lowStep * Time.deltaTime;
                        highFreq += highStep * Time.deltaTime;
                        currTime += Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                    break;
                default:
                    break;
            }

            StopRumble(pad);
            yield return null;
        }

        IEnumerator RumbleAll(float seconds)
        {
            float currTime = 0f;

            switch (rPattern)
            {
                case RumblePattern.CONSTANT:
                    foreach (Gamepad pad in Gamepad.all)
                        pad.SetMotorSpeeds(lowFreq, highFreq);

                    yield return new WaitForSeconds(seconds);
                    break;
                case RumblePattern.PULSE:
                    isMotorActive = true;

                    //total duration
                    while (currTime <= seconds)
                    {
                        //vibrate
                        if (isMotorActive)
                        {
                            foreach (Gamepad pad in Gamepad.all)
                                pad.SetMotorSpeeds(lowFreq, highFreq);

                            yield return new WaitForSeconds(pulseDuration);
                            isMotorActive = false;
                            currTime += pulseDuration;
                        }
                        else
                        {
                            //stop
                            foreach (Gamepad pad in Gamepad.all)
                                StopRumble(pad);

                            yield return new WaitForSeconds(pulseWaitBetweenDuration);
                            isMotorActive = true;
                            currTime += pulseWaitBetweenDuration;
                        }

                        yield return new WaitForEndOfFrame();
                    }
                    break;
                case RumblePattern.LINEAR:
                    while (currTime <= seconds)
                    {
                        foreach (Gamepad pad in Gamepad.all)
                            pad.SetMotorSpeeds(lowFreq, highFreq);

                        lowFreq += lowStep * Time.deltaTime;
                        highFreq += highStep * Time.deltaTime;
                        currTime += Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                    break;
                default:
                    break;
            }

            StopRumbleAll();
        }*/
    }

}