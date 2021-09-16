using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalBarController : MonoBehaviour
{
    private Color[] emissiveColors = {new Color(0.12743768f, 0.0f, 0.0f, 1.00f), new Color(0.1811643f, 0.0f, 0.0f, 1.00f)
    ,new Color(0.30946895f, 0.0f, 0.0f, 1.00f), new Color(0.4072403f, 0.0f, 0.0f, 1.00f), new Color(0.61720675f, 0.0f, 0.0f, 1.0f),
    new Color(0.72305536f, 0.0f, 0.0f, 1.00f), new Color(0.8631574f, 0.0f, 0.0f, 1.00f), new Color(1.0f, 0.0f, 0.0f, 1.0f),
    new Color(1.0f, 0.32314324f, 0.03071345f, 1.0f), new Color(1.0f, 0.5840786f, 0.0f, 1.0f), new Color(1.0f, 1.0f, 0.03071345f, 1.0f),
    new Color(1.0f, 1.0f, 0.2874409f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f)};
    private Color desiredColor;
    public MetalStruct metalBarStruct;
    public Rigidbody metalBarRB;
    //is the metal bar in the embers?
    public bool isHeating, inAir, inWater, isEmitting;
    //"dT/dt"
    private float instantTempChange, heatingTimer, initTemp, time, envTemp;
    //Reference to script that contains needed info on the forge such as temperature
    public ForgeController forgeController;
    //Reference to script that contains needed info on the forge cooler
    public QuenchingController quenchingController;
    //cooling Constant that is calculated upon instatiation and can be passed to cooling/heating equation parameters
    private float curCoolingConst;
    public Material mBMaterial;
    //when object is not being heated or cooled
    public void resetTimer(){
        heatingTimer = 0.0f;
    }
    // Start is called before the first frame update
    void Start(){
        //GameStats.ambientTemp = 27.0f;
        resetTimer();
        GameStats.ambientTemp = 22.0f;
        metalBarStruct.metalTemp = GameStats.ambientTemp;
        isHeating = false;
        inAir = true;
        inWater = false;
        //(Thermal conductivity * surface area)/(mass * specific heat * thickness)
        curCoolingConst = (metalBarStruct.thermConduct*metalBarStruct.surfaceArea)*(metalBarRB.mass*metalBarStruct.specificHeat*metalBarStruct.normalDepth);
        mBMaterial.DisableKeyword("_EMISSION");
        isEmitting = false;
    }

    void FixedUpdate(){
        time += Time.deltaTime;
        inAir = (!isHeating && !inWater);
        if(metalBarStruct.metalTemp < 550.0f){
            mBMaterial.DisableKeyword("_EMISSION");
            isEmitting = false;
            }
        else if(metalBarStruct.metalTemp >= 550.0f && !isEmitting){
                isEmitting = true;
            }
        if(isEmitting){
            mBMaterial.EnableKeyword("_EMISSION");
            if(metalBarStruct.metalTemp >= 550.0f && metalBarStruct.metalTemp < 630.0f)
                desiredColor = emissiveColors[0];
            else if(metalBarStruct.metalTemp >= 630.0f && metalBarStruct.metalTemp < 680.0f)
                desiredColor = emissiveColors[1];
            else if(metalBarStruct.metalTemp >= 680.0f && metalBarStruct.metalTemp < 740.0f)
                desiredColor = emissiveColors[2];
            else if(metalBarStruct.metalTemp >= 740.0f && metalBarStruct.metalTemp < 770.0f)
                desiredColor = emissiveColors[3];
            else if(metalBarStruct.metalTemp >= 770.0f && metalBarStruct.metalTemp < 800.0f)
                desiredColor = emissiveColors[4];
            else if(metalBarStruct.metalTemp >= 800.0f && metalBarStruct.metalTemp < 850.0f)
                desiredColor = emissiveColors[5];
            else if(metalBarStruct.metalTemp >= 850.0f && metalBarStruct.metalTemp < 900.0f)
                desiredColor = emissiveColors[6];
            else if(metalBarStruct.metalTemp >= 900.0f && metalBarStruct.metalTemp < 950.0f)
                desiredColor = emissiveColors[7];
            else if(metalBarStruct.metalTemp >= 950.0f && metalBarStruct.metalTemp < 1000.0f)
                desiredColor = emissiveColors[8];
            else if(metalBarStruct.metalTemp >= 1000.0f && metalBarStruct.metalTemp < 1100.0f)
                desiredColor = emissiveColors[9];
            else if(metalBarStruct.metalTemp >= 1100.0f && metalBarStruct.metalTemp < 1200.0f)
                desiredColor = emissiveColors[10];
            else if(metalBarStruct.metalTemp >= 1200.0f && metalBarStruct.metalTemp < 1300.0f)
                desiredColor = emissiveColors[11];
            else if(metalBarStruct.metalTemp >= 1300.0f)
                desiredColor = emissiveColors[12];
            mBMaterial.SetColor("_EmissionColor", desiredColor);
        }
        else if(!isEmitting)
            mBMaterial.DisableKeyword("_EMISSION");
        //In the act of heating metal bar
        if(isHeating){
            if(heatingTimer == 0.0f)
                initTemp = metalBarStruct.metalTemp;
            //Debug.Log(initTemp - GameStats.ambientTemp)
            heatingTimer += Time.deltaTime;
            if (time >= 1.00f){
                time = time - 1.00f;
                //Newtons Cooling Law for heating
                //T(t)= Ambient Temp - (Ambient Temp - Objects init temp) * e^(-k*t)
                metalBarStruct.metalTemp = NewtonsCoolingEquation.heatObj(envTemp, initTemp, curCoolingConst, heatingTimer, metalBarStruct.metalType);          
            }
            //Debug.Log(metalBarStruct.metalTemp);
        }
        else if(!isHeating && metalBarStruct.metalTemp > GameStats.ambientTemp){
            if(!inWater)
                envTemp = GameStats.ambientTemp;
            if(heatingTimer == 0.0f)
                initTemp = metalBarStruct.metalTemp;
            heatingTimer += Time.deltaTime;
            if (time >= 1.00f){
                time = time - 1.00f;
                //Newtons Cooling Law
                //T(t)= Ambient Temp + (Objects init temp - Ambient Temp ) * e^(-k*t)
                 metalBarStruct.metalTemp = NewtonsCoolingEquation.coolObj(envTemp, initTemp, curCoolingConst, heatingTimer, metalBarStruct.metalType);
            }
            //Debug.Log(metalBarStruct.metalTemp);
        }
        
    }

    void  OnCollisionEnter(Collision collision){
       isHeating = collision.collider.tag == "Embers";
       inWater = collision.collider.tag == "Water";
       if(isHeating)
           envTemp = forgeController.forgeTemp;
       else if(inWater)
            envTemp = quenchingController.getWaterTemp();


    }
}
