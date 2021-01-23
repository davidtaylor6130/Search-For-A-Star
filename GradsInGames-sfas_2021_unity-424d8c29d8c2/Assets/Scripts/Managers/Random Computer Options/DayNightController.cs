﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

//- Specifying What Random Class To Use -//
using Random = UnityEngine.Random;

[System.Serializable]
public struct TextureRenderers
{
    public GameObject ParentObject;
    public TextMeshProUGUI OnScreenText;
    public Camera Camera;
    public Game StoryManager;
};

[System.Serializable]
public struct ActiveComputerInfo
{
    public int ComputerIndex;
    public List<int> EventIndex;
    public List<bool> EventCompleated;
    public int RenderTextureIndex;
    public GameObject Screen;
};

public class DayNightController : MonoBehaviour
{
    [Header("Generator Links")]
    public EmailGenerator EmailGen;
    public EventGenerator EventGen;
    public NameGenerator NameGen;

    [Header("PlayerChoiceController")]
    public PlayerChoiceController PlayerChoiceTracker;

    [Header("Settings")]
    public int AmountOfComputersActive;
    public int EventsPerSystem;
    public int EmailsPerSystem;
    
    [Tooltip("What Shows On Computers That Arnt Selected")]
    public RenderTexture DeActiveRenderTexture;

    [Header("Computer Selection")]
    public GameObject[] Computers;
    public List<int> ActiveComputers;

    [Header("Camera To Texture Systems")]
    public GameObject[] CameraToTexture;
    public RenderTexture[] RenderTextures;
    private TextureRenderers[] CamToTex;

    [Header("Final Output")]
    public ActiveComputerInfo[] FormattedComputers;

    // Start is called before the first frame update
    void Start()
    {
        EventGen.CustomStart(AmountOfComputersActive);
        EmailGen.CustomStart(AmountOfComputersActive);
        NameGen.CustomStart(AmountOfComputersActive);

        //- Get Refrances to Camera To Texture Components -//
        GetCameraToTextureInfo();

        //- Calculate Days Computer's and events -//
        RefreshComputers();
    }

    [ContextMenu("Get Camera To Texture Info")]
    void GetCameraToTextureInfo()
    {
        // Create New data array
        CamToTex = new TextureRenderers[CameraToTexture.Length];

        //- Loop though all Camera To Textures -//
        for (int i = 0; i < CameraToTexture.Length; i++)
        {
            //- Save Required Info Into Easier to understand Format -//
            CamToTex[i].ParentObject = CameraToTexture[i];
            CamToTex[i].OnScreenText = CameraToTexture[i].GetComponentInChildren<TextMeshProUGUI>();
            CamToTex[i].Camera = CameraToTexture[i].GetComponentInChildren<Camera>();
            CamToTex[i].StoryManager = CameraToTexture[i].GetComponent<Game>();
        }
    }

    [ContextMenu("Refresh Computers")]
    void RefreshComputers()
    {
        //- Clears All Info About What is selected -//
        ResetDay();

        //- Generate and format Info For Each Computer -//
        FormatComputersToActivate();

        //- Asigned Correct Data To Relevent Systems -//
        AsignDataToComputers();
    }

    void AsignDataToComputers()
    {
        for (int i = 0; i < FormattedComputers.Length; i++)
        {
            Material tempMat = new Material(Shader.Find("Standard"));
            tempMat.mainTexture = RenderTextures[FormattedComputers[i].RenderTextureIndex];
            FormattedComputers[i].Screen.GetComponent<Renderer>().material = tempMat;
        }
    }

    void FormatComputersToActivate()
    {
        //- Generate Required Information -//
        for (int i = 0; i < AmountOfComputersActive; i++)
        { 
            //- PcInfo -//
            // What Computer To Active
            FormattedComputers[i].ComputerIndex = GetRandomComputer();

            // Select Amount Of Events
            for (int j = 0; j < EventsPerSystem; j++)
                FormattedComputers[i].EventIndex.Add(EventGen.GetRandomEvent());

            // Link Render Text System
            FormattedComputers[i].RenderTextureIndex = i;

            // Loops Through all childern of Computers
            foreach (Transform Child in Computers[FormattedComputers[i].ComputerIndex].GetComponentsInChildren<Transform>())
            {
                //- if screen then save ref and move on-//
                if (Child.name == "Screen")
                {
                    FormattedComputers[i].Screen = Child.gameObject;
                    break;
                }
            }

            //- Set refrance to each screens Game script -//
            Computers[FormattedComputers[i].ComputerIndex].GetComponent<ElectronicInteractable>().SetGameRef(CamToTex[i].ParentObject.GetComponent<Game>());

            //- Clear Active Events -//
            EventGen.ClearRandomSelectedMemory();
            EmailGen.ClearRandomSelectionMemory();
        }
    }

    int GetRandomComputer()
    {
        int randomSelection = 0;
        do
        {
            randomSelection = Random.Range(0, Computers.Length);
        }
        while (ActiveComputers.Contains(randomSelection));
        ActiveComputers.Add(randomSelection);

        return randomSelection;
    }

    [ContextMenu("Reset Day")]
    void ResetDay()
    {
        PlayerChoiceTracker.ResetDay();
        EventGen.ResetDay();
        EmailGen.ResetDay();

        //- Deactivate all screens -//
        for (int i = 0; i < Computers.Length; i++)
        {
            foreach (Transform Child in Computers[i].GetComponentsInChildren<Transform>())
            {
                //- if screen then save ref and move on-//
                if (Child.name == "Screen")
                {
                    Material Mat = new Material(Shader.Find("Standard"));
                    Mat.mainTexture = DeActiveRenderTexture;
                    Child.gameObject.GetComponent<Renderer>().material = Mat; 
                    break;
                }
            }
        }

        //- Create New Array -//
        FormattedComputers = new ActiveComputerInfo[AmountOfComputersActive];

        //- Create New List -//
        for (int i = 0; i < AmountOfComputersActive; i++)
        {
            FormattedComputers[i].EventIndex = new List<int>();
            FormattedComputers[i].EventCompleated = new List<bool>();
            
            //- fill Eventcompleated list -//
            for (int j = 0; j < EventsPerSystem; j++)
                FormattedComputers[i].EventCompleated.Add(false);
        }

        //- CLear Active Computers -//
        ActiveComputers.Clear();
    }

    public string GetEmail(string UnformattedInput/*int TitleOrMainBody, int PcIndex, int EmailIndex*/)
    {
        int[] ProcessedInputs = ProcessIntInput(UnformattedInput, 3, ',');
        return EmailGen.GetEmailInfo(ProcessedInputs[0], ProcessedInputs[1], ProcessedInputs[2]);
    }

    public string GetAction(string UnformattedInput/*int TitleOrMainBody, int PcIndex, int ActionIndex*/)
    {
        //- Process and save input's -//
        int[] ProcessedInputs = ProcessIntInput(UnformattedInput, 3, ',');
        
        //- temp string var -//
        string output = "";

        //- if title of event -//
        if (ProcessedInputs[0] == 0)
        {
            if (FormattedComputers[ProcessedInputs[1] - 1].EventCompleated[ProcessedInputs[2] - 1] == true)
            {
                output = "Compleated Event";
            }
            else
            {
                output = EventGen.Events[FormattedComputers[ProcessedInputs[1] - 1].EventIndex[ProcessedInputs[2] - 1]].NameOfEvent;
            }
        }
        //- if main body of text -//
        else if (ProcessedInputs[0] == 1)
        {
            if (FormattedComputers[ProcessedInputs[1] - 1].EventCompleated[ProcessedInputs[2] - 1] == true)
            {
                output = "Compleated Event \n Press esc to return to main menu";
            }
            else
            {
                //- Send Output Data -//
                output = EventGen.Events[FormattedComputers[ProcessedInputs[1] - 1].EventIndex[ProcessedInputs[2] - 1]].OutputOfEvent;

                //- increase or decrease carma ammount by developer set ammount -//
                PlayerChoiceTracker.CarmaAmount += EventGen.Events[FormattedComputers[ProcessedInputs[1] - 1].EventIndex[ProcessedInputs[2] - 1]].CarmaCost;
                
                //- Update Noticeable Ammount -//
                PlayerChoiceTracker.NoticeablenessChange(EventGen.Events[FormattedComputers[ProcessedInputs[1] - 1].EventIndex[ProcessedInputs[2] - 1]].NoticableCost);

                //- noticeable ness death state -//
                if (PlayerChoiceTracker.NoticableNess > 100)
                {
                    Debug.LogError("YOUR DEAD");
                }

                //- Add Event For Later use to construct Emails -//
                EmailGen.AddCompleatedEvent(ProcessedInputs[1], ProcessedInputs[2]);
            }
            FormattedComputers[ProcessedInputs[1] - 1].EventCompleated[ProcessedInputs[2] - 1] = true;
        }  
        return output;
    }

    public string GetRandomName(string UnformattedInput/*int PcIndex*/)
    {
        int[] ProcessedInputs = ProcessIntInput(UnformattedInput, 1, ',');
        return NameGen.GetName(ProcessedInputs[0] - 1);
    }

    int[] ProcessIntInput(string unformattedInput, int length, char stopCharacter)
    {
        int[] Inputs = new int[length];
        string tempStorage = "";
        int inputCount = 0;

        for (int i = 0; i <= unformattedInput.Length; i++)
        {
            if (unformattedInput.Length == i)
            {
                Inputs[inputCount] = Convert.ToInt32(tempStorage);
            }
            else if (unformattedInput[i] == stopCharacter)
            {
                Inputs[inputCount] = Convert.ToInt32(tempStorage);
                inputCount++;
                tempStorage = "";
            }
            else
            {
                tempStorage += unformattedInput[i];
            }
        }

        return Inputs;
    }
}