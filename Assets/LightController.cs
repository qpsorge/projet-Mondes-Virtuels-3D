using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightController : MonoBehaviour
{
    public Text m_text;
    public const float LIGHT_FREQ = 24f;
    float start_time;
    private Light global_light;

    public static List<Light> house_light = new List<Light>();
    public static List<GameObject> agents = new List<GameObject>();

    public static int nb_agent_work;
    public static int nb_agent_home;
    public static int nb_agent_total;


    void Start()
    {
        nb_agent_work = 0;
        nb_agent_home = 0;
        start_time = Time.realtimeSinceStartup;
        global_light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        float time = LIGHT_FREQ/2+Time.time;
        Debug.Log(time);
        global_light.intensity = 0.4f + 1f - Mathf.Abs( time%(LIGHT_FREQ) - LIGHT_FREQ / 2) / (LIGHT_FREQ/2);
        foreach(Light l in house_light)
        {
            l.intensity = 30*(1 - global_light.intensity);
        }

        string[] day_time = new string[] { "Matin", "Midi", "Après-midi", "Soir", "Nuit" };
        int period = -1;
        if (LIGHT_FREQ / 4 < time % LIGHT_FREQ && time % LIGHT_FREQ < LIGHT_FREQ/2.5f)
            period = 0;
        else if (LIGHT_FREQ / 2.5f < time % LIGHT_FREQ && time % LIGHT_FREQ < LIGHT_FREQ / 1.8f)
            period = 1;
        else if (LIGHT_FREQ / 1.8f < time % LIGHT_FREQ && time % LIGHT_FREQ < LIGHT_FREQ / 1.3f)
            period = 2;
        else if (LIGHT_FREQ / 1.3f < time % LIGHT_FREQ && time % LIGHT_FREQ < LIGHT_FREQ / 1.1f)
            period = 3;
        else if (LIGHT_FREQ / 1.1f < time % LIGHT_FREQ || time % LIGHT_FREQ < LIGHT_FREQ / 4)
            period = 4;

        nb_agent_work = 0;
        nb_agent_home = 0;
        foreach (GameObject agent in agents)
        {
            if (agent.GetComponent<move>().state == "immeuble")
                nb_agent_work++;
            else if (agent.GetComponent<move>().state == "home")
                nb_agent_home++;
        }

        m_text.text =  "Journée " + ((int)(time / LIGHT_FREQ)).ToString() + " - " + day_time[period] + "\n";
        m_text.text += "Total : "+nb_agent_total.ToString()+"\n"+"Working : "+nb_agent_work.ToString()+"\n"+"Free : "+nb_agent_home.ToString();

    }
}
