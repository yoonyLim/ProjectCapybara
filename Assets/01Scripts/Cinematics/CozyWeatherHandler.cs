using DistantLands.Cozy;
using DistantLands.Cozy.Data;
using UnityEngine;

public class CozyWeatherHandler : MonoBehaviour
{
    [SerializeField] private WeatherProfile weatherProfile;
    // [SerializeField] private AmbienceProfile ambienceProfile;

    public void ChangeToGrandmaWeather()
    {
        if (weatherProfile)
        {
            // weather module
            CozyWeather.instance.weatherModule.ecosystem.SetWeather(weatherProfile);
            
            // time module
            CozyTimeModule timeModule = CozyWeather.instance.timeModule;
            timeModule.currentTime = new MeridiemTime(6, 00);
            
            // disable wind
            CozyWeather.instance.windModule.enabled = false;
        }
    }

    public void DisableCozyWeather()
    {
        CozyWeather.instance.gameObject.SetActive(false);
    }
}
